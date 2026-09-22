# SmartGest — Segurança

## Objetivo

Proteger dados, operações e credenciais sem complicar desnecessariamente a operação local.

## Autenticação

A modalidade Server deve possuir autenticação robusta. A modalidade Local também deve proteger o acesso à aplicação quando houver dados empresariais.

## Autorização

Permissões devem ser baseadas em funções/capacidades.

Exemplos:

- Administrador
- Gerente
- Operador
- Contabilista

A lista final depende dos requisitos do produto.

## Passwords

Nunca armazenar passwords em texto puro. Usar hashing apropriado e política consistente.

## Auditoria

Operações relevantes devem registrar:

- utilizador;
- operação;
- data/hora;
- entidade/documento afetado;
- valores relevantes quando necessário.

## Segredos

Nunca versionar:

- passwords;
- tokens;
- connection strings com credenciais;
- chaves privadas;
- ficheiros de autenticação.

## Desktop

O modo offline não significa ausência de segurança. A base local e os ficheiros de configuração devem possuir permissões adequadas ao sistema operativo.
