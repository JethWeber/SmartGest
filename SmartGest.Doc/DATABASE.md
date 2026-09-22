# SmartGest — Base de Dados

## Estratégia

| Característica | Server | Local |
|---|---|---|
| Provider | PostgreSQL | SQLite |
| API | Sim | Não |
| Docker | Pode usar | Não necessário |
| Internet | Conforme implantação | Não |
| Uso típico | Centralizado/multiutilizador | Máquina única |

## SQLite

Indicado para:

- uma instalação local;
- poucos processos concorrentes;
- operação offline;
- instalação simples;
- backup por ficheiro.

Não usar um ficheiro SQLite numa partilha de rede como solução de multiutilizador.

## Integridade

A base deve usar:

- chaves primárias;
- foreign keys;
- constraints adequadas;
- índices onde necessário;
- transações para operações críticas;
- migrations versionadas.

## Dados

Valores monetários devem ter representação precisa e consistente. Identificadores e datas devem seguir uma convenção única em todo o sistema.

## Portabilidade

O SmartGest Local deve permitir exportar/importar dados de forma controlada para facilitar migração, suporte e recuperação.
