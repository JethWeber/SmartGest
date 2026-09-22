# SmartGest — API

## Objetivo

A API é a fronteira HTTP da modalidade Server.

## Princípio

Controllers devem ser finos:

```
HTTP → Controller → Application → Domain → Infrastructure
```

## Requisitos

- autenticação;
- autorização;
- validação;
- tratamento global de exceções;
- respostas HTTP consistentes;
- logging;
- versionamento quando necessário;
- documentação OpenAPI;
- health checks.

## Endpoints

Os endpoints reais devem ser documentados a partir da implementação efetiva. Exemplos de recursos esperados:

```
/api/auth
/api/products
/api/customers
/api/suppliers
/api/sales
/api/purchases
/api/stock
/api/cash
/api/reports
```

## Erros

A API deve retornar erros estruturados, sem expor stack traces ou segredos ao cliente.

## Compatibilidade

Alterações incompatíveis devem possuir estratégia explícita de versionamento ou migração.
