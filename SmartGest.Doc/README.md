# SmartGest.Doc

Documentação oficial da arquitetura, desenvolvimento, operação e evolução do SmartGest.

## Visão

O SmartGest é projetado para suportar duas modalidades:

- **SmartGest Server/Web** — aplicação orientada a servidor, API e PostgreSQL.
- **SmartGest Local/Desktop** — aplicação local/offline, desktop e SQLite.

A regra central é evitar duplicação da lógica de negócio. O domínio e os casos de uso devem ser compartilhados sempre que possível.

## Estrutura

```
SmartGest
├── SmartGest.Domain
├── SmartGest.Application
├── SmartGest.Infrastructure
├── SmartGest.Web
├── SmartGest.Desktop
└── SmartGest.Doc
```

## Documentos

- [Arquitetura](ARCHITECTURE.md)
- [Domínio](DOMAIN.md)
- [Aplicação](APPLICATION.md)
- [Infraestrutura](INFRASTRUCTURE.md)
- [Base de dados](DATABASE.md)
- [API](API.md)
- [Desktop](DESKTOP.md)
- [Segurança](SECURITY.md)
- [Backup e recuperação](BACKUP.md)
- [Deploy](DEPLOYMENT.md)
- [Testes](TESTING.md)
- [Produto](SMARTGEST_PRODUCT.md)
- [Roadmap](ROADMAP.md)

## Princípios

1. Regras de negócio não dependem de UI, HTTP ou banco.
2. Application coordena casos de uso.
3. Infrastructure implementa detalhes técnicos.
4. Web/API e Desktop são pontos de entrada.
5. SQLite é destinado ao cenário local de máquina única.
6. PostgreSQL continua sendo a opção para cenários server/multiutilizador.
7. Backup e recuperação são requisitos de produção, não extras.

## Estado

Esta documentação define a arquitetura-alvo. A implementação deve avançar incrementalmente sem quebrar a versão funcional existente.
