# SmartGest — Arquitetura

## Prioridade da primeira fase

A primeira entrega é **Desktop Local 100% funcional e offline**.

Portanto, a arquitetura de produção imediata é:

```
┌──────────────────────────────┐
│       SmartGest.Desktop      │
└──────────────┬───────────────┘
               ↓
┌──────────────────────────────┐
│    SmartGest.Application     │
└──────────────┬───────────────┘
               ↓
┌──────────────────────────────┐
│       SmartGest.Domain       │
└──────────────┬───────────────┘
               ↓
┌──────────────────────────────┐
│   SmartGest.Infrastructure   │
│          SQLite              │
└──────────────────────────────┘
```

## Projetos

### SmartGest.Domain

Coração do negócio.

Contém:

- entidades;
- value objects;
- enums;
- regras;
- exceções;
- contratos essenciais.

Não conhece UI, HTTP, EF Core, SQLite, PostgreSQL ou Docker.

### SmartGest.Application

Casos de uso do sistema.

Exemplos:

- cadastrar produto;
- registrar venda;
- registrar compra;
- atualizar stock;
- gerir clientes;
- gerir fornecedores;
- fechar caixa;
- gerar relatórios.

### SmartGest.Infrastructure

Implementações técnicas da edição Local:

- EF Core;
- SQLite;
- repositórios;
- migrations;
- logs;
- backup;
- persistência;
- serviços do sistema.

### SmartGest.Desktop

Interface e composição da aplicação.

```
View
 ↓
ViewModel
 ↓
Application
 ↓
Domain
 ↓
Infrastructure
 ↓
SQLite
```

## Web/API

**Não faz parte da primeira fase.**

Quando necessário, será adicionada como outra entrada:

```
SmartGest.Web/API
       ↓
Application
       ↓
Domain
       ↓
Infrastructure
       ↓
PostgreSQL
```

O Desktop não deve esperar pela API para ficar pronto.

## Regra de dependência

```
Desktop ────────→ Application ────────→ Domain
                      ↑
                      │
               Infrastructure
```

A implementação concreta de persistência permanece em Infrastructure.

## Objetivo arquitetural

Construir primeiro um produto local completo e depois reutilizar o mesmo núcleo para outros modos de distribuição.
