# SmartGest — Domain

## Objetivo

O Domain representa o negócio do SmartGest independentemente da tecnologia usada para executá-lo.

## Conteúdo esperado

```
SmartGest.Domain/
├── Entities/
├── ValueObjects/
├── Enums/
├── Exceptions/
└── Interfaces/
```

## Entidades

As entidades devem representar conceitos reais do negócio. Exemplos possíveis:

- Produto
- Categoria
- Cliente
- Fornecedor
- Venda
- ItemVenda
- Compra
- ItemCompra
- Stock
- Movimento
- Caixa
- Utilizador

A lista final deve refletir o modelo efetivamente suportado pelo produto.

## Regras

Regras invariantes devem ficar próximas das entidades ou em serviços de domínio quando envolverem múltiplas entidades.

Exemplos:

- quantidade vendida não pode ser negativa;
- documentos devem possuir estado válido;
- operações de stock devem respeitar as regras do negócio;
- totais devem ser consistentes com os itens.

## Independência

O Domain não deve:

- abrir conexões de banco;
- fazer chamadas HTTP;
- ler configurações de Docker;
- conhecer telas;
- depender de PostgreSQL ou SQLite.

## Evolução

Mudanças no modelo de negócio devem começar no Domain e depois ser propagadas para Application, Infrastructure e interfaces.
