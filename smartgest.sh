#!/usr/bin/env bash
# =============================================================================
# SmartGest — Gestão do ambiente Docker
# Uso: ./smartgest.sh [comando]
# =============================================================================

set -e

COMPOSE="docker compose"
PROJECT="SmartGest"

RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'
BLUE='\033[0;34m'; NC='\033[0m'

info()    { echo -e "${BLUE}[INFO]${NC}  $1"; }
success() { echo -e "${GREEN}[OK]${NC}    $1"; }
warn()    { echo -e "${YELLOW}[AVISO]${NC} $1"; }
error()   { echo -e "${RED}[ERRO]${NC}  $1"; exit 1; }

check_docker() {
    command -v docker &>/dev/null || error "Docker não encontrado. Instale em https://docs.docker.com/get-docker/"
    docker info &>/dev/null       || error "Docker daemon não está a correr. Execute: sudo systemctl start docker"
}

cmd_start() {
    check_docker
    info "A iniciar SmartGest (API + PostgreSQL)..."
    $COMPOSE up -d --build
    echo ""
    success "Serviços iniciados!"
    echo ""
    echo -e "  ${GREEN}API:${NC}        http://localhost:8080"
    echo -e "  ${GREEN}Swagger UI:${NC} http://localhost:8080/swagger"
    echo -e "  ${GREEN}PostgreSQL:${NC} localhost:5432  (user: postgres)"
    echo ""
    info "Desktop: aponte o ApiClient para http://localhost:8080"
}

cmd_stop() {
    check_docker
    info "A parar os serviços..."
    $COMPOSE down
    success "Serviços parados."
}

cmd_restart() {
    cmd_stop
    cmd_start
}

cmd_logs() {
    check_docker
    local svc=${1:-""}
    if [ -n "$svc" ]; then
        $COMPOSE logs -f "$svc"
    else
        $COMPOSE logs -f
    fi
}

cmd_status() {
    check_docker
    $COMPOSE ps
}

cmd_reset_db() {
    check_docker
    warn "Isto vai APAGAR todos os dados da base de dados!"
    read -r -p "Tem a certeza? (escreva 'sim' para confirmar): " conf
    if [ "$conf" = "sim" ]; then
        $COMPOSE down -v
        info "Volume da base de dados removido."
        cmd_start
    else
        info "Operação cancelada."
    fi
}

cmd_shell_db() {
    check_docker
    info "A abrir psql na base de dados smartgest..."
    docker exec -it smartgest_postgres psql -U postgres -d smartgest
}

cmd_build() {
    check_docker
    info "A recompilar a imagem da API..."
    $COMPOSE build --no-cache api
    success "Imagem reconstruída."
}

cmd_help() {
    echo ""
    echo -e "${BLUE}SmartGest — Comandos disponíveis:${NC}"
    echo ""
    echo "  ./smartgest.sh start       Inicia API + PostgreSQL (build automático)"
    echo "  ./smartgest.sh stop        Para todos os serviços"
    echo "  ./smartgest.sh restart     Para e reinicia"
    echo "  ./smartgest.sh build       Reconstrói a imagem da API sem cache"
    echo "  ./smartgest.sh logs        Mostra logs de todos os serviços"
    echo "  ./smartgest.sh logs api    Mostra logs apenas da API"
    echo "  ./smartgest.sh logs db     Mostra logs apenas do PostgreSQL"
    echo "  ./smartgest.sh status      Estado dos containers"
    echo "  ./smartgest.sh shell-db    Abre psql no container PostgreSQL"
    echo "  ./smartgest.sh reset-db    Apaga e recria a base de dados (DESTRUTIVO)"
    echo ""
}

# ── Dispatcher ────────────────────────────────────────────────────────────────
case "${1:-help}" in
    start)     cmd_start ;;
    stop)      cmd_stop ;;
    restart)   cmd_restart ;;
    logs)      cmd_logs "${2:-}" ;;
    status)    cmd_status ;;
    build)     cmd_build ;;
    reset-db)  cmd_reset_db ;;
    shell-db)  cmd_shell_db ;;
    *)         cmd_help ;;
esac
