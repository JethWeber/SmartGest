# SmartGest V2 — Motor Contabilístico: Guia de Integração

## 1. Registar o MotorContabil em Program.cs

```csharp
// Program.cs — adicionar antes de builder.Build()
builder.Services.AddScoped<IMotorContabil, MotorContabil>();
builder.Services.AddScoped<RelatoriosService>();
builder.Services.AddScoped<ContabilidadeService>(); // manter por retrocompatibilidade
```

---

## 2. AppDbContext — adicionar DbSet e seed

```csharp
// Data/AppDbContext.cs

// Adicionar DbSet
public DbSet<CategoriaContabil> CategoriaContabeis { get; set; }

// Em OnModelCreating, adicionar seed:
modelBuilder.Entity<CategoriaContabil>().HasData(CategoriaContabilSeed.Obter());

// Relação Lancamento -> CategoriaContabil (já existe, confirmar):
modelBuilder.Entity<Lancamento>()
    .HasOne(l => l.CategoriaContabil)
    .WithMany()
    .HasForeignKey(l => l.CategoriaContabilId)
    .OnDelete(DeleteBehavior.SetNull);

// Relação ContaBancaria -> ContaContabil (nova — para resolução dinâmica do banco):
modelBuilder.Entity<ContaBancaria>()
    .HasOne(b => b.ContaContabil)
    .WithMany()
    .HasForeignKey(b => b.ContaContabilId)
    .OnDelete(DeleteBehavior.SetNull);
```

---

## 3. Models/Entities.cs — acrescentar ContaContabilId à ContaBancaria

```csharp
// Dentro da classe ContaBancaria
public int?          ContaContabilId { get; set; }
public ContaContabil? ContaContabil  { get; set; }
```

---

## 4. Criar e aplicar migration

```bash
dotnet ef migrations add V2_MotorContabil_CategoriaContabil
dotnet ef database update
```

A migration irá:
- Criar tabela `CategoriaContabeis` com as 40 categorias seed
- Adicionar coluna `ContaContabilId` (nullable) em `ContasBancarias`

---

## 5. Actualizar seed das ContasBancarias (opcional mas recomendado)

Associar cada banco à conta PGC correspondente:

```csharp
// No seed das ContasBancarias, adicionar ContaContabilId
// O Id da conta "45" (Depósitos Bancários) no seed das ContasContabeis
// deve ser confirmado — por defeito é o Id da conta com Codigo = "45"
{ 1, ..., ContaContabilId = <id_da_conta_45> },
{ 2, ..., ContaContabilId = <id_da_conta_45> },
{ 3, ..., ContaContabilId = <id_da_conta_45> },
{ 4, ..., ContaContabilId = <id_da_conta_45> },
```

---

## 6. DTOs/Requests/Requests.cs — ContaBancariaRequest

```csharp
// Adicionar campo opcional
public record ContaBancariaRequest(
    string Banco, string NIB, string Tipo, string Moeda,
    decimal SaldoAtual, string Agencia, string Titular, string CorAccent,
    int? ContaContabilId = null);   // <-- novo campo V2
```

---

## 7. ContasBancariasController — guardar ContaContabilId

```csharp
// No método Criar:
var conta = new ContaBancaria
{
    // ... campos existentes ...
    ContaContabilId = req.ContaContabilId,
};

// No método Atualizar:
conta.ContaContabilId = req.ContaContabilId;
```

---

## 8. Remover ContabilidadeService (fase futura)

O `ContabilidadeService` e o `MapeamentoContabil` (dicionário de texto) podem ser
removidos assim que todos os lançamentos existentes forem migrados para `CategoriaContabilId`.

Use o endpoint de recalculo para processar os lançamentos antigos:

```http
POST /api/lancamentos/recalcular
Authorization: Bearer <token>
```

---

## 9. Resumo dos endpoints novos

| Método | Endpoint                  | Descrição                              |
|--------|---------------------------|----------------------------------------|
| GET    | /api/categorias           | Lista as 40 categorias                 |
| GET    | /api/categorias?tipo=Entrada | Só entradas                         |
| GET    | /api/categorias?tipo=Saída   | Só saídas                           |
| POST   | /api/categorias           | Criar categoria (Administrador)        |
| PUT    | /api/categorias/{id}      | Editar categoria (Administrador)       |
| DELETE | /api/categorias/{id}      | Desactivar categoria (Administrador)   |
| GET    | /api/fluxo-caixa          | Fluxo de Caixa do período              |
| POST   | /api/lancamentos/recalcular | Recalcular lançamentos sem detalhes  |

---

## 10. Regras que o motor garante automaticamente

- Débito = Crédito em todos os lançamentos
- Imposto de Selo calculado e lançado nas contas 65/34 quando `AplicaImpostoSelo = true`
- Conta bancária real substituída dinamicamente via `ContaBancaria.ContaContabilId`
- Lançamentos anulados excluídos de todos os relatórios
- Nenhum relatório calcula regras contabilísticas (consome apenas `LancamentoDetalhe`)
