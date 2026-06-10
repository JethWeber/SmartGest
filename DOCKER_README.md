# SmartGest — Deploy com Docker

## Estrutura dos containers

```
┌─────────────────────────────────────────┐
│  Máquina Host (Ubuntu)                  │
│                                         │
│  ┌─────────────────┐                    │
│  │ SmartGest        │  (app nativa)     │
│  │ Desktop/Avalonia │──────────────┐    │
│  └─────────────────┘              │    │
│                                   ▼    │
│  ┌────────────────────────────────────┐ │
│  │  Docker Network: smartgest_net     │ │
│  │                                    │ │
│  │  ┌──────────────┐  ┌────────────┐  │ │
│  │  │ smartgest_api│  │smartgest   │  │ │
│  │  │ :8080        │──│_postgres   │  │ │
│  │  │ (.NET 10)    │  │ :5432      │  │ │
│  │  └──────────────┘  └────────────┘  │ │
│  └────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

- **SmartGest.Desktop** corre nativamente na máquina (GUI Avalonia não corre em container)
- **SmartGest.API** corre em container, acessível em `http://localhost:8080`
- **PostgreSQL** corre em container, acessível em `localhost:5432`

---

## Pré-requisitos

```bash
# Verificar Docker instalado
docker --version          # >= 24.0
docker compose version    # >= 2.20
```

Se não tiver Docker:
```bash
# Ubuntu/Debian
curl -fsSL https://get.docker.com | bash
sudo usermod -aG docker $USER   # adicionar ao grupo docker
newgrp docker                   # aplicar sem logout
```

---

## Estrutura de ficheiros

Coloque estes ficheiros na **raiz do projecto** (`~/projects/SmartGest/`):

```
SmartGest/
├── SmartGest.API.Dockerfile      ← Dockerfile da API
├── docker-compose.yml            ← Serviços principais
├── docker-compose.override.yml   ← Overrides de desenvolvimento
├── .dockerignore                 ← Exclusões do contexto Docker
├── smartgest.sh                  ← Script de gestão (opcional)
├── SmartGest.API/
├── SmartGest.Desktop/
└── SmartGest.Core/
```

---

## Iniciar (primeira vez)

```bash
cd ~/projects/SmartGest

# Dar permissão ao script de gestão
chmod +x smartgest.sh

# Iniciar tudo (faz build automático na primeira vez)
./smartgest.sh start

# — OU directamente com docker compose —
docker compose up -d --build
```

As **migrations são aplicadas automaticamente** ao arranque da API
(`db.Database.Migrate()` no `Program.cs`).

---

## Comandos do dia a dia

```bash
./smartgest.sh start          # Inicia API + PostgreSQL
./smartgest.sh stop           # Para os serviços
./smartgest.sh restart        # Reinicia
./smartgest.sh logs           # Ver logs em tempo real
./smartgest.sh logs api       # Logs só da API
./smartgest.sh status         # Estado dos containers
./smartgest.sh shell-db       # Abrir psql no container
./smartgest.sh reset-db       # ⚠️  Apaga e recria a BD
```

---

## Configurar o Desktop para apontar ao container

No ficheiro `SmartGest.Desktop/Services/ApiClient.cs`, confirme que o `BaseUrl` aponta para:

```
http://localhost:8080
```

Se o Desktop já usava `localhost:5000` ou `localhost:7000` (Kestrel local),
altere para `localhost:8080`.

---

## Variáveis de ambiente sensíveis (produção)

Para produção, não coloque credenciais no `docker-compose.yml`.
Use um ficheiro `.env` na raiz do projecto:

```bash
# .env  (nunca commitar no git!)
POSTGRES_PASSWORD=SuaSenhaSegura
JWT_KEY=SuaChaveJwtSegura
```

E no `docker-compose.yml` substitua os valores hardcoded por:
```yaml
POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
Jwt__Key: ${JWT_KEY}
```

---

## Verificar que a API está a funcionar

```bash
# Health check básico
curl http://localhost:8080/swagger

# Testar login
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"telefone":"900000000","password":"Admin@2025"}'
```

---

## Resolução de problemas

**Container da API não arranca:**
```bash
docker compose logs api
# Erro comum: BD ainda não pronta — o healthcheck resolve automaticamente
```

**Porta 5432 em uso (PostgreSQL local já instalado):**
```bash
# Opção 1: parar o postgres local
sudo systemctl stop postgresql

# Opção 2: mapear para outra porta no docker-compose.yml
ports:
  - "5433:5432"   # usar 5433 no host
```

**Rebuild da imagem após alterações no código:**
```bash
./smartgest.sh build
./smartgest.sh restart
# — OU —
docker compose up -d --build api
```
