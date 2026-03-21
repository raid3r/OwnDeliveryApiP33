# Docker compose helper script for OwnDeliveryApiP33 (Windows PowerShell)
# Usage: .\docker.ps1 -Command up

param(
    [Parameter(Position = 0)]
    [ValidateSet('up', 'down', 'restart', 'logs', 'logs-api', 'logs-db', 'status', 'build', 'rebuild', 'clean', 'shell-api', 'shell-db', 'test', 'help')]
    [string]$Command = 'help'
)

$COMPOSE_FILE = "docker-compose.yml"
$ENV_FILE = ".env"

# Colors
function Write-Header {
    param([string]$Message)
    Write-Host "================================" -ForegroundColor Cyan
    Write-Host $Message -ForegroundColor Cyan
    Write-Host "================================" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "? $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "? $Message" -ForegroundColor Red
}

function Write-Warning {
    param([string]$Message)
    Write-Host "? $Message" -ForegroundColor Yellow
}

# Check .env file
function Check-Env {
    if (-not (Test-Path $ENV_FILE)) {
        Write-Error ".env file not found!"
        if (Test-Path ".env.example") {
            Write-Host "Creating .env from .env.example..."
            Copy-Item ".env.example" ".env"
            Write-Warning "Please edit .env file with your configuration"
            exit 1
        }
    }
    Write-Success ".env file found"
}

# Commands
function Cmd-Up {
    Write-Header "Starting Docker containers"
    Check-Env
    docker-compose up -d
    Write-Success "Containers started"
    docker-compose ps
}

function Cmd-Down {
    Write-Header "Stopping Docker containers"
    docker-compose down
    Write-Success "Containers stopped"
}

function Cmd-Logs {
    Write-Header "Showing logs"
    docker-compose logs -f
}

function Cmd-Logs-Api {
    Write-Header "Showing API logs"
    docker-compose logs -f api
}

function Cmd-Logs-Db {
    Write-Header "Showing Database logs"
    docker-compose logs -f db
}

function Cmd-Restart {
    Write-Header "Restarting containers"
    docker-compose restart
    Write-Success "Containers restarted"
}

function Cmd-Build {
    Write-Header "Building Docker image"
    Check-Env
    docker-compose build --no-cache
    Write-Success "Image built"
}

function Cmd-Rebuild {
    Write-Header "Rebuilding and starting"
    Cmd-Down
    Cmd-Build
    Cmd-Up
}

function Cmd-Status {
    Write-Header "Container status"
    docker-compose ps
}

function Cmd-Clean {
    Write-Header "Cleaning Docker resources"
    docker-compose down -v
    docker system prune -f
    Write-Success "Cleanup completed"
}

function Cmd-Shell-Api {
    Write-Header "Entering API container shell"
    docker-compose exec api /bin/bash
}

function Cmd-Shell-Db {
    Write-Header "Entering Database shell"
    docker-compose exec db psql -U postgres -d OwnDeliveryDb
}

function Cmd-Test {
    Write-Header "Running tests in container"
    docker-compose exec api dotnet test
}

function Cmd-Help {
    Write-Host "OwnDeliveryApiP33 Docker Helper" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Usage: .\docker.ps1 -Command [command]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Commands:" -ForegroundColor Cyan
    Write-Host "  up           - Start containers"
    Write-Host "  down         - Stop containers"
    Write-Host "  restart      - Restart containers"
    Write-Host "  logs         - Show all logs"
    Write-Host "  logs-api     - Show API logs"
    Write-Host "  logs-db      - Show Database logs"
    Write-Host "  status       - Show container status"
    Write-Host "  build        - Build Docker image"
    Write-Host "  rebuild      - Rebuild and start (clean)"
    Write-Host "  clean        - Remove all containers and volumes"
    Write-Host "  shell-api    - Enter API container"
    Write-Host "  shell-db     - Enter Database shell"
    Write-Host "  test         - Run tests in container"
    Write-Host "  help         - Show this help message"
}

# Main
switch ($Command) {
    'up' { Cmd-Up; break }
    'down' { Cmd-Down; break }
    'restart' { Cmd-Restart; break }
    'logs' { Cmd-Logs; break }
    'logs-api' { Cmd-Logs-Api; break }
    'logs-db' { Cmd-Logs-Db; break }
    'status' { Cmd-Status; break }
    'build' { Cmd-Build; break }
    'rebuild' { Cmd-Rebuild; break }
    'clean' { Cmd-Clean; break }
    'shell-api' { Cmd-Shell-Api; break }
    'shell-db' { Cmd-Shell-Db; break }
    'test' { Cmd-Test; break }
    'help' { Cmd-Help; break }
    default {
        Write-Error "Unknown command: $Command"
        Cmd-Help
        exit 1
    }
}
