# SmartGest.Doc

Documentação oficial da arquitetura, desenvolvimento, operação e evolução do SmartGest.

## Prioridade atual

**Fase atual: SmartGest Desktop Local.**

O objetivo imediato é entregar uma aplicação Desktop **100% funcional e offline**, sem depender de Web/API, Docker, PostgreSQL ou Internet.

A modalidade Web/API fica para uma fase posterior e não bloqueia a primeira entrega.

## Arquitetura-alvo atual

```
SmartGest
├── SmartGest.Domain
├── SmartGest.Application
├── SmartGest.Infrastructure
├── SmartGest.Desktop
└── SmartGest.Doc
```

Posteriormente:

```
SmartGest.Web/API
```

poderá ser adicionada reutilizando Domain, Application e Infrastructure.

## Documentos

- [Arquitetura](ARCHITECTURE.md)
- [Domínio](DOMAIN.md)
- [Aplicação](APPLICATION.md)
- [Infraestrutura](INFRASTRUCTURE.md)
- [Base de dados](DATABASE.md)
- [Desktop](DESKTOP.md)
- [Segurança](SECURITY.md)
- [Backup e recuperação](BACKUP.md)
- [Deploy](DEPLOYMENT.md)
- [Testes](TESTING.md)
- [Produto](SMARTGEST_PRODUCT.md)
- [Roadmap](ROADMAP.md)

## Princípios

1. Desktop Local é a prioridade da primeira entrega.
2. A aplicação essencial deve funcionar sem Internet.
3. Regras de negócio não dependem de UI, HTTP ou banco específico.
4. Application coordena casos de uso.
5. Infrastructure implementa detalhes técnicos.
6. SQLite é o banco da edição Local.
7. PostgreSQL/API/Web ficam para uma fase posterior.
8. Backup e recuperação são requisitos de produção.

## Estratégia

Não criar agora uma Web/API apenas por arquitetura. Primeiro extrair Domain e Application do código existente e fazer o Desktop consumir essas camadas.

A futura API poderá reutilizar os mesmos casos de uso.
