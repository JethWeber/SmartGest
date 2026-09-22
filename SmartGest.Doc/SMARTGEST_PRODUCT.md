# SmartGest — Produto

## Visão

O SmartGest será tratado como uma plataforma com modalidades diferentes, compartilhando o máximo possível da lógica de negócio.

## SmartGest Local

- operação offline;
- desktop;
- SQLite;
- instalação simples;
- baixa dependência de infraestrutura.

## SmartGest Server

- API;
- PostgreSQL;
- operação centralizada;
- múltiplos utilizadores;
- infraestrutura de servidor.

## SmartGest Cloud

Possibilidade futura de produto hospedado. Não faz parte do MVP Local.

## Sincronização

Visão futura:

```
SmartGest Local
      ↕
    Sync
      ↕
SmartGest Server/Cloud
```

A sincronização não deve ser introduzida antes de a operação local e a operação server estarem estáveis.

## Regra

Não manter duas implementações independentes das mesmas regras de negócio quando elas podem ser compartilhadas.
