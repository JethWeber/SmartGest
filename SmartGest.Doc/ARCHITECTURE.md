# SmartGest — Arquitetura

## Arquitetura-alvo

```
                         SmartGest
                             |
              +--------------+--------------+
              |                             |
       SmartGest.Web                 SmartGest.Desktop
              |                             |
              +--------------+--------------+
                             |
                  SmartGest.Application
                             |
                     SmartGest.Domain
                             |
                  SmartGest.Infrastructure
                       /             \
                PostgreSQL          SQLite
```

## Responsabilidades

### Domain

Contém entidades, regras de negócio, value objects, enums e exceções de domínio.

**Não depende de:** ASP.NET, Avalonia, EF Core, PostgreSQL, SQLite, Docker ou HTTP.

### Application

Implementa casos de uso e coordena o domínio através de interfaces.

Exemplos:

- Registrar venda
- Registrar compra
- Atualizar stock
- Cadastrar produto
- Gerir clientes e fornecedores
- Fechar caixa
- Consultar movimentos

### Infrastructure

Implementa persistência, repositórios, banco de dados, autenticação, logs, backups e integração com sistema operativo.

### Web/API

Expõe os casos de uso por HTTP. Controllers devem ser finos e não conter regras empresariais complexas.

### Desktop

Interface local para operação offline. Executa Application diretamente e usa infraestrutura SQLite.

## Regra de dependência

```
Web ────────────────┐
                    ├──> Application ───> Domain
Desktop ────────────┘
                         ^
                         |
                   Infrastructure
```

O objetivo é impedir que o Domain conheça detalhes externos.

## Cenários

### Server

```
Cliente → Web/API → Application → Domain → Infrastructure → PostgreSQL
```

### Local

```
Desktop → Application → Domain → Infrastructure → SQLite
```

## Multi-PC local

SQLite não deve ser tratado como banco compartilhado através de uma pasta de rede. Se uma instalação precisar de vários terminais simultâneos, deve ser criada uma arquitetura de serviço local/LAN apropriada.
