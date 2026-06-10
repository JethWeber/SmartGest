# SmartGest V2 — Próximo Passo: Ligar Bancos às Contas PGC e Validar Relatórios

## Estado actual

| Componente              | Estado         | Observação                                          |
|-------------------------|----------------|-----------------------------------------------------|
| Motor Contabilístico    | ✅ Funcional   | Gera partidas dobradas via CategoriaId              |
| 25 Categorias           | ✅ Seedadas    | GET /api/categorias a responder                     |
| Lançamentos V2          | ✅ Funcional   | Exige CategoriaId obrigatório                       |
| Relatórios (estrutura)  | ✅ Funcional   | Lêem LancamentoDetalhe correctamente                |
| Bancos → Conta PGC      | ❌ Incompleto  | ContaContabilId não é gravado ao criar banco        |
| Relatórios com dados    | ⚠️ Dependente | Só correctos se lançamentos tiverem detalhes        |

---

## Passo 1 — Actualizar ContasBancariasController

O controller actual não grava `ContaContabilId`. Sem este campo, o motor
usa sempre a conta genérica `45` (Depósitos Bancários) em vez da conta
real do banco.

### 1.1 Actualizar ContaBancariaRequest

Em `DTOs/Requests/Requests.cs`, adicionar o campo:

```csharp
public record ContaBancariaRequest(
    string Banco, string NIB, string Tipo, string Moeda,
    decimal SaldoAtual, string Agencia, string Titular, string CorAccent,
    int? ContaContabilId = null);   // <-- adicionar este campo
```

### 1.2 Actualizar ContasBancariasController

No método `Criar`, adicionar:
```csharp
var conta = new ContaBancaria
{
    Banco           = req.Banco,
    NIB             = req.NIB,
    Tipo            = req.Tipo,
    Moeda           = req.Moeda,
    SaldoAtual      = req.SaldoAtual,
    SaldoOntem      = req.SaldoAtual,
    Agencia         = req.Agencia,
    Titular         = req.Titular,
    CorAccent       = req.CorAccent,
    ContaContabilId = req.ContaContabilId,   // <-- adicionar
};
```

No método `Atualizar`, adicionar:
```csharp
conta.ContaContabilId = req.ContaContabilId;   // <-- adicionar
```

### 1.3 Associar os bancos existentes à conta 45

Se já tens bancos criados na base de dados sem `ContaContabilId`, associa
via SQL directo (o Id 13 é o Id da conta "45 — Depósitos Bancários" no seed):

```sql
UPDATE "ContasBancarias"
SET "ContaContabilId" = 13
WHERE "ContaContabilId" IS NULL;
```

Ou via endpoint após actualizar o controller:
```http
PUT /api/contas-bancarias/{id}
{
  "banco": "Nome do banco",
  ...campos existentes...,
  "contaContabilId": 13
}
```

---

## Passo 2 — Corrigir MaxLength no campo Categoria

Em `Models/Entities.cs`, o campo `Categoria` tem `MaxLength(50)`.
O motor grava `categoria.Nome` que pode exceder 50 caracteres.

```csharp
// Alterar de:
[MaxLength(50)]
public string Categoria { get; set; } = string.Empty;

// Para:
[MaxLength(200)]
public string Categoria { get; set; } = string.Empty;
```

Depois criar migration:
```bash
dotnet ef migrations add Fix_Lancamento_Categoria_MaxLength
docker compose down -v
docker compose build api
docker compose up
```

---

## Passo 3 — Teste end-to-end completo

### 3.1 Criar um lançamento de teste

```http
POST /api/lancamentos
Authorization: Bearer {token}
Content-Type: application/json

{
  "data": "2025-06-10T00:00:00",
  "descricao": "Venda teste motor V2",
  "tipo": "Entrada",
  "valor": 150000,
  "categoriaId": 1,
  "beneficiario": "Cliente Teste",
  "metodoPagamento": "Transferência",
  "caminhoDocumento": "",
  "observacoes": "",
  "centroCusto": "",
  "referenciaInterna": "REF-001",
  "contaBancariaId": null
}
```

### 3.2 Verificar detalhes gerados pelo motor

```http
GET /api/lancamentos/{id}
```

Confirmar que o motor gerou 2 linhas em `LancamentoDetalhe`:
- Linha 1: ContaId = 13 (45 — Depósitos Bancários), Débito = 150000, Crédito = 0
- Linha 2: ContaId = 25 (71 — Vendas), Débito = 0, Crédito = 150000

### 3.3 Verificar Balancete

```http
GET /api/balancete
```

Deve aparecer:
- Conta 45: MovDebito = 150000
- Conta 71: MovCredito = 150000
- `"equilibrado": true`

### 3.4 Verificar DRE

```http
GET /api/dre
```

Deve aparecer:
- Linha com código 71, ValorRealizado = 150000
- TotalReceitas = 150000, ResultadoLiquido = 150000

### 3.5 Verificar Balanço

```http
GET /api/balanco
```

Deve aparecer:
- Depósitos Bancários (45): 150000 no Activo Corrente

---

## Passo 4 — Validação de equilíbrio

O Balancete devolve o campo `"equilibrado": true/false`.
Se vier `false`, há lançamentos com detalhes desalinhados.

Para re-processar lançamentos antigos sem detalhes:

```http
POST /api/lancamentos/recalcular
Authorization: Bearer {token} (Administrador ou Contabilista)
```

---

## Resumo dos endpoints a testar por ordem

```
1. POST   /api/auth/login
2. GET    /api/categorias
3. POST   /api/lancamentos          (com categoriaId obrigatório)
4. GET    /api/balancete
5. GET    /api/dre
6. GET    /api/balanco
7. GET    /api/fluxo-caixa
```

---

## Quando a API está "pronta"

A API está pronta quando o teste do Passo 3 passar completamente:
um lançamento criado com `categoriaId` aparece correctamente no
Balancete, DRE e Balanço com `"equilibrado": true`.
