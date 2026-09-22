# SmartGest — Roadmap

## Fase 0 — Baseline

- [ ] Congelar versão atual
- [ ] Mapear arquitetura existente
- [ ] Identificar dependências PostgreSQL/Docker
- [ ] Identificar regras de negócio misturadas com UI/API

## Fase 1 — Domain

- [ ] Criar SmartGest.Domain
- [ ] Migrar entidades
- [ ] Migrar regras
- [ ] Definir interfaces

## Fase 2 — Application

- [ ] Criar SmartGest.Application
- [ ] Casos de uso
- [ ] DTOs
- [ ] Validações
- [ ] Tratamento de erros

## Fase 3 — Infrastructure

- [ ] Criar SmartGest.Infrastructure
- [ ] PostgreSQL
- [ ] SQLite
- [ ] Repositórios
- [ ] Migrations
- [ ] Logs
- [ ] Backup

## Fase 4 — Web/API

- [ ] Isolar Controllers
- [ ] Autenticação
- [ ] Autorização
- [ ] Error handling
- [ ] OpenAPI
- [ ] Health checks

## Fase 5 — Desktop

- [ ] Criar SmartGest.Desktop
- [ ] Integrar Application
- [ ] Integrar SQLite
- [ ] Login local
- [ ] Configuração
- [ ] Backup/restore

## Fase 6 — Produção

- [ ] Installer
- [ ] Atualizações
- [ ] Logs
- [ ] Diagnóstico
- [ ] Documentação operacional

## Fase 7 — Validação

- [ ] Unit tests
- [ ] Integration tests
- [ ] API tests
- [ ] Desktop tests
- [ ] E2E
- [ ] Performance
- [ ] Recovery

## Fase 8 — Futuro

- [ ] Sincronização Local ↔ Server
- [ ] SmartGest Cloud
- [ ] Gestão centralizada

## Critério

Uma fase só deve ser marcada como concluída quando houver implementação funcional e validação correspondente.
