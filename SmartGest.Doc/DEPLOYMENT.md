# SmartGest — Deployment

## Server

Arquitetura típica:

```
Internet/LAN
    ↓
Reverse Proxy
    ↓
SmartGest Web/API
    ↓
PostgreSQL
```

Docker pode ser usado para tornar o ambiente reproduzível.

Requisitos:

- configuração por ambiente;
- secrets fora do código;
- migrations controladas;
- health checks;
- logs;
- backup;
- estratégia de atualização;
- rollback.

## Local

Arquitetura:

```
Instalador
   ↓
SmartGest Desktop
   ↓
SQLite
```

O cliente não deve precisar instalar Docker ou PostgreSQL para operar a edição Local.

## Atualizações

Antes de atualizar:

1. verificar versão;
2. criar backup;
3. aplicar migration;
4. validar base;
5. iniciar nova versão.

## Ambiente alvo

A edição Local deve ser testada em computadores reais semelhantes aos usados pelos clientes, incluindo máquinas de baixo/médio desempenho.
