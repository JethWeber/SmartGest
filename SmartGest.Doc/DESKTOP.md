# SmartGest — Desktop Local

## Objetivo

O SmartGest Desktop permite operar o sistema localmente sem depender de Internet, API externa, Docker ou PostgreSQL.

## Arquitetura

```
Desktop UI
    ↓
Application
    ↓
Domain
    ↓
Infrastructure
    ↓
SQLite
```

## Instalação

A distribuição deve fornecer um instalador para a plataforma suportada.

A instalação deve configurar:

- executável;
- diretório de dados;
- banco SQLite;
- logs;
- backups;
- configuração local.

## Operação offline

Nenhuma funcionalidade essencial da modalidade Local deve depender de conexão externa.

## Atualização

Atualizações devem preservar os dados existentes através de migrations e backups prévios.

## Diagnóstico

O utilizador/suporte deve conseguir localizar:

- versão instalada;
- logs;
- estado da base;
- último backup;
- informações básicas da instalação.

## Suporte

A aplicação deve ter mecanismo claro para exportar informações de diagnóstico sem expor dados sensíveis desnecessários.
