# SmartGest — Infrastructure

## Objetivo

Infrastructure contém implementações técnicas.

## Responsabilidades

- EF Core
- PostgreSQL
- SQLite
- Repositórios
- Migrations
- Logs
- Backups
- Persistência de configurações
- Serviços do sistema
- Implementações de autenticação

## Banco

```
IRepository / interfaces
          |
          v
Infrastructure
     /          \
PostgreSQL     SQLite
```

A Application deve depender de abstrações, não de providers específicos.

## PostgreSQL

Usado na modalidade server, especialmente quando existe API, múltiplos utilizadores ou dados centralizados.

## SQLite

Usado na modalidade Desktop/Local, quando a base pertence a uma instalação local e a operação precisa continuar sem Internet.

## Migrations

Cada provider deve ter estratégia de migration compatível com as suas limitações. Não assumir que uma migration PostgreSQL pode ser aplicada diretamente em SQLite.

## Cuidados

Evitar SQL específico de PostgreSQL em código compartilhado. Quando SQL específico for inevitável, isolá-lo na Infrastructure.
