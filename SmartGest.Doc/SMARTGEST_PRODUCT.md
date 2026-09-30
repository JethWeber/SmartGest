# SmartGest — Produto

## Primeira entrega

A prioridade comercial e técnica imediata é:

# SmartGest Local

Aplicação Desktop de gestão, totalmente funcional e capaz de operar sem Internet.

## SmartGest Local

Características:

- Desktop;
- offline;
- SQLite;
- instalação simples;
- sem Docker;
- sem PostgreSQL no cliente;
- sem dependência de API externa;
- dados locais;
- backup e restore;
- atualização controlada.

## SmartGest Server

Será uma modalidade posterior.

Características previstas:

- API;
- PostgreSQL;
- operação centralizada;
- múltiplos utilizadores;
- infraestrutura de servidor.

## SmartGest Cloud

Possibilidade futura de serviço hospedado.

Não faz parte da primeira entrega.

## Sincronização

Também é futura:

```
SmartGest Local
      ↕
    Sync
      ↕
SmartGest Server/Cloud
```

Primeiro fazemos o Local funcionar perfeitamente. Depois sincronizamos.
