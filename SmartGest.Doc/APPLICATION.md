# SmartGest — Application

## Objetivo

A camada Application transforma ações do utilizador em casos de uso executáveis.

## Organização sugerida

```
SmartGest.Application/
├── DTOs/
├── Services/
├── UseCases/
├── Validators/
└── Interfaces/
```

## Casos de uso

Exemplos:

- Cadastrar produto
- Alterar produto
- Consultar stock
- Registrar venda
- Cancelar venda
- Registrar compra
- Cadastrar cliente
- Cadastrar fornecedor
- Fechar caixa
- Consultar movimentos
- Gerar relatórios

## Fluxo

```
Entrada → Validação → Caso de uso → Domain → Persistência → Resultado
```

A Application não deve assumir se o destino final é PostgreSQL ou SQLite.

## Validação

Validações de entrada devem acontecer antes de executar operações críticas. Regras de negócio permanecem no Domain quando forem invariantes do negócio.

## Erros

Os casos de uso devem produzir erros previsíveis e tratáveis. A camada Web traduz esses erros para HTTP; a Desktop traduz para feedback de interface.
