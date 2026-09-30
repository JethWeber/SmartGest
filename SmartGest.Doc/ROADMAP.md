# SmartGest — Roadmap

## 🚨 Prioridade atual

**Desktop Local primeiro. Web/API depois.**

Estamos a trabalhar contra o relógio. Nenhuma funcionalidade Web/API deve bloquear a primeira versão produtiva.

---

## Fase 0 — Baseline

- [ ] Congelar versão atual
- [ ] Mapear arquitetura existente
- [ ] Identificar entidades
- [ ] Identificar regras de negócio
- [ ] Identificar persistência atual
- [ ] Identificar dependências externas
- [ ] Identificar funcionalidades já funcionais no Desktop

---

## Fase 1 — SmartGest.Domain

**Objetivo:** separar o negócio da interface e da infraestrutura.

- [ ] Criar projeto SmartGest.Domain
- [ ] Migrar entidades existentes
- [ ] Migrar enums
- [ ] Criar value objects necessários
- [ ] Extrair regras de negócio
- [ ] Criar exceções de domínio
- [ ] Definir contratos essenciais
- [ ] Remover dependências de UI
- [ ] Remover dependências de banco específico
- [ ] Compilar Domain isoladamente

**Critério de conclusão:**

`SmartGest.Domain` deve representar o negócio sem precisar de Desktop, API ou banco específico.

---

## Fase 2 — SmartGest.Application

**Objetivo:** transformar as funcionalidades atuais em casos de uso reutilizáveis pelo Desktop.

- [ ] Criar projeto SmartGest.Application
- [ ] Criar DTOs necessários
- [ ] Criar interfaces de persistência
- [ ] Criar serviços/casos de uso
- [ ] Migrar operações de negócio do Desktop
- [ ] Centralizar validações
- [ ] Centralizar tratamento de erros
- [ ] Garantir transações nas operações críticas
- [ ] Fazer Desktop consumir Application
- [ ] Remover lógica empresarial dos ViewModels
- [ ] Testar casos de uso

**Critério de conclusão:**

O Desktop deve executar as operações principais através de Application, sem duplicar regras de negócio nos ViewModels.

---

## Fase 3 — SmartGest.Infrastructure Local

- [ ] SQLite
- [ ] EF Core
- [ ] DbContext
- [ ] Migrations
- [ ] Repositórios
- [ ] Seed inicial quando necessário
- [ ] Logs
- [ ] Backup
- [ ] Restore
- [ ] Integridade da base

---

## Fase 4 — SmartGest.Desktop

- [ ] Integrar Domain
- [ ] Integrar Application
- [ ] Integrar Infrastructure
- [ ] Garantir funcionamento 100% offline
- [ ] Corrigir fluxos existentes
- [ ] Persistência completa
- [ ] Estados de loading/erro/vazio
- [ ] Impressão/exportação quando aplicável
- [ ] Configurações
- [ ] Backup/restore

---

## Fase 5 — Produção Local

- [ ] Installer
- [ ] Configuração de diretórios
- [ ] Logs de diagnóstico
- [ ] Atualização
- [ ] Migração de versões
- [ ] Testes em máquinas reais
- [ ] Teste de recuperação
- [ ] Checklist de release

---

## Fase 6 — Validação

- [ ] Unit tests
- [ ] Integration tests
- [ ] E2E
- [ ] Performance
- [ ] Backup/restore
- [ ] Teste offline
- [ ] Teste de atualização

---

## Fase 7 — Web/API

**Só depois da edição Desktop estar estável.**

- [ ] SmartGest.Web/API
- [ ] PostgreSQL
- [ ] Autenticação server
- [ ] Autorização
- [ ] OpenAPI
- [ ] Health checks
- [ ] Deployment server

---

## Fase 8 — Futuro

- [ ] SmartGest Server
- [ ] SmartGest Cloud
- [ ] Sincronização Local ↔ Server
- [ ] Gestão centralizada

## Regra

Não marcar uma fase como concluída apenas porque os projetos compilam. A funcionalidade precisa estar integrada e validada.


## Estado da migração Desktop Local — 2026-09-30

### Fase 4 — Desktop Integration
Concluída na base funcional. O Desktop utiliza SQLite local para autenticação, categorias, lançamentos, caixa, contas bancárias, dashboard, balancete, balanço, DRE e configurações persistidas. O API client permanece no projeto apenas para compatibilidade/futura sincronização.

### Fase 5 — Production Local
Infraestrutura operacional implementada: backups diários automáticos, backup manual, verificação de integridade SQLite, WAL, foreign keys, sessão local com BCrypt, projecto dedicado SmartGest.Migrations e script para gerar migrations EF Core. A aplicação aceita uma base criada anteriormente com EnsureCreated e cria um baseline antes de aplicar migrations futuras.

> Antes de uma distribuição de produção, deve ser gerada e revista a migration inicial com ScriptsDeLancamento/gerar-migracao-local.sh. O EF Core recomenda migrations para bases persistentes; EnsureCreated é apropriado sobretudo para prototipagem/testes e não deve ser combinado com migrations no mesmo fluxo. SQLite também possui limitações específicas de migrations.
