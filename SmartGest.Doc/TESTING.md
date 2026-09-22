# SmartGest — Testes

## Objetivo

Validar comportamento, persistência, segurança e recuperação antes de produção.

## Camadas

### Unit
Regras de negócio, cálculos, validações e serviços puros.

### Integration
Application + Infrastructure, repositories, migrations, transações, SQLite e PostgreSQL.

### API
Autenticação, autorização, endpoints, validações, erros e contratos.

### Desktop
Instalação, login, operações principais, funcionamento sem Internet, atualização, backup e restore.

### E2E

```
Produto → Venda → Stock → Caixa → Relatório
```

## Recuperação

Testar restauração de backup, falhas, migrations de versões antigas e exportação/importação.

## Desempenho

Avaliar abertura, pesquisas, vendas, relatórios e crescimento da base com dados realistas.

## Regra

Testes propostos no roadmap não são considerados concluídos até serem executados e validados.
