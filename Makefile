.PHONY: help up down restart logs logs-api logs-db status build rebuild clean shell-api shell-db test

# Docker Compose commands for OwnDeliveryApiP33

help:
	@echo "================================"
	@echo "OwnDeliveryApiP33 Docker Helper"
	@echo "================================"
	@echo ""
	@echo "Usage: make [command]"
	@echo ""
	@echo "Commands:"
	@echo "  up              - Start containers"
	@echo "  down            - Stop containers"
	@echo "  restart         - Restart containers"
	@echo "  logs            - Show all logs (streaming)"
	@echo "  logs-api        - Show API logs (streaming)"
	@echo "  logs-db         - Show Database logs (streaming)"
	@echo "  status          - Show container status"
	@echo "  build           - Build Docker image"
	@echo "  rebuild         - Rebuild image and start containers"
	@echo "  clean           - Remove containers and volumes"
	@echo "  shell-api       - Enter API container shell"
	@echo "  shell-db        - Enter Database shell"
	@echo "  test            - Run tests in container"
	@echo ""
	@echo "Examples:"
	@echo "  make up"
	@echo "  make logs-api"
	@echo "  make shell-db"
	@echo "  make rebuild"

up:
	@echo "Starting Docker containers..."
	docker-compose up -d
	@echo "? Containers started"
	docker-compose ps

down:
	@echo "Stopping Docker containers..."
	docker-compose down
	@echo "? Containers stopped"

restart:
	@echo "Restarting containers..."
	docker-compose restart
	@echo "? Containers restarted"

logs:
	docker-compose logs -f

logs-api:
	docker-compose logs -f api

logs-db:
	docker-compose logs -f db

status:
	@echo "Container status:"
	docker-compose ps

build:
	@echo "Building Docker image..."
	docker-compose build --no-cache
	@echo "? Image built"

rebuild: down build up

clean:
	@echo "Cleaning Docker resources..."
	docker-compose down -v
	docker system prune -f
	@echo "? Cleanup completed"

shell-api:
	@echo "Entering API container shell..."
	docker-compose exec api /bin/bash

shell-db:
	@echo "Entering Database shell..."
	docker-compose exec db psql -U postgres -d OwnDeliveryDb

test:
	@echo "Running tests in container..."
	docker-compose exec api dotnet test
