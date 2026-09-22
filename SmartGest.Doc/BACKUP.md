# SmartGest — Backup e Recuperação

## Objetivo

Garantir que uma falha de hardware, erro humano ou corrupção de dados não destrua o histórico empresarial.

## Requisitos

- backup manual;
- backup automático configurável;
- retenção;
- identificação por data/versão;
- validação;
- restauração;
- exportação para armazenamento externo.

## Local

Um backup SQLite pode ser armazenado como ficheiro independente, mas não deve ser considerado seguro apenas por existir.

Exemplo:

```
backup/
├── smartgest_2026-09-23_1200.db
└── smartgest_2026-09-24_1200.db
```

## Restore

Fluxo recomendado:

```
Selecionar backup
      ↓
Validar
      ↓
Criar backup do estado atual
      ↓
Restaurar
      ↓
Executar verificação
      ↓
Abrir aplicação
```

## Server

PostgreSQL deve possuir estratégia de backup própria, com testes periódicos de restauração.

## Regra

**Backup que nunca foi restaurado é uma hipótese, não uma garantia.**
