#!/bin/bash

# Docker compose helper script for OwnDeliveryApiP33
# Usage: ./docker.sh [command]

set -e

COMPOSE_FILE="docker-compose.yml"
COMPOSE_OVERRIDE="docker-compose.override.yml"
ENV_FILE=".env"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Functions
print_header() {
    echo -e "${BLUE}================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}================================${NC}"
}

print_success() {
    echo -e "${GREEN}? $1${NC}"
}

print_error() {
    echo -e "${RED}? $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}? $1${NC}"
}

# Check if .env file exists
check_env() {
    if [ ! -f "$ENV_FILE" ]; then
        print_error ".env file not found!"
        echo "Creating .env from .env.example..."
        cp .env.example .env
        print_warning "Please edit .env file with your configuration"
        exit 1
    fi
    print_success ".env file found"
}

# Main commands
cmd_up() {
    print_header "Starting Docker containers"
    check_env
    docker-compose up -d
    print_success "Containers started"
    docker-compose ps
}

cmd_down() {
    print_header "Stopping Docker containers"
    docker-compose down
    print_success "Containers stopped"
}

cmd_logs() {
    print_header "Showing logs"
    docker-compose logs -f
}

cmd_logs_api() {
    print_header "Showing API logs"
    docker-compose logs -f api
}

cmd_logs_db() {
    print_header "Showing Database logs"
    docker-compose logs -f db
}

cmd_restart() {
    print_header "Restarting containers"
    docker-compose restart
    print_success "Containers restarted"
}

cmd_build() {
    print_header "Building Docker image"
    check_env
    docker-compose build --no-cache
    print_success "Image built"
}

cmd_rebuild() {
    print_header "Rebuilding and starting"
    cmd_down
    cmd_build
    cmd_up
}

cmd_status() {
    print_header "Container status"
    docker-compose ps
}

cmd_clean() {
    print_header "Cleaning Docker resources"
    docker-compose down -v
    docker system prune -f
    print_success "Cleanup completed"
}

cmd_shell_api() {
    print_header "Entering API container shell"
    docker-compose exec api /bin/bash
}

cmd_shell_db() {
    print_header "Entering Database shell"
    docker-compose exec db psql -U postgres -d OwnDeliveryDb
}

cmd_test() {
    print_header "Running tests in container"
    docker-compose exec api dotnet test
}

cmd_help() {
    echo "OwnDeliveryApiP33 Docker Helper"
    echo ""
    echo "Usage: ./docker.sh [command]"
    echo ""
    echo "Commands:"
    echo "  up              - Start containers"
    echo "  down            - Stop containers"
    echo "  restart         - Restart containers"
    echo "  logs            - Show all logs"
    echo "  logs-api        - Show API logs"
    echo "  logs-db         - Show Database logs"
    echo "  status          - Show container status"
    echo "  build           - Build Docker image"
    echo "  rebuild         - Rebuild and start (clean)"
    echo "  clean           - Remove all containers and volumes"
    echo "  shell-api       - Enter API container"
    echo "  shell-db        - Enter Database shell"
    echo "  test            - Run tests in container"
    echo "  help            - Show this help message"
}

# Main
case "${1:-help}" in
    up)
        cmd_up
        ;;
    down)
        cmd_down
        ;;
    restart)
        cmd_restart
        ;;
    logs)
        cmd_logs
        ;;
    logs-api)
        cmd_logs_api
        ;;
    logs-db)
        cmd_logs_db
        ;;
    status)
        cmd_status
        ;;
    build)
        cmd_build
        ;;
    rebuild)
        cmd_rebuild
        ;;
    clean)
        cmd_clean
        ;;
    shell-api)
        cmd_shell_api
        ;;
    shell-db)
        cmd_shell_db
        ;;
    test)
        cmd_test
        ;;
    help|--help|-h)
        cmd_help
        ;;
    *)
        print_error "Unknown command: $1"
        cmd_help
        exit 1
        ;;
esac
