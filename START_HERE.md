# ?? DOCKER КОНФІГУРАЦІЯ - ГОТОВА ДО ВИКОРИСТАННЯ

## ? ЧИМ РОЗПОЧАТИ?

### Крок 1??: Установити Docker

- **Windows/macOS**: [Docker Desktop](https://www.docker.com/products/docker-desktop)
- **Linux**: `sudo apt-get install docker.io docker-compose`

### Крок 2??: Запустити Проект

```bash
docker-compose up -d
```

### Крок 3??: Перевірити

```bash
docker-compose ps
```

Повинно показати 2 контейнера: `api` та `db`

### Крок 4??: Відкрити API

Браузер: **http://localhost:5134/swagger**

---

## ?? ФАЙЛИ СТВОРЕНІ

```
? .env                         - Змінні середовища
? docker-compose.override.yml  - Розробничі налаштування
? docker.sh                    - Скрипт для Linux/macOS
? docker.ps1                   - Скрипт для Windows
? Makefile                     - Make команди
? Program.cs (оновлено)        - Health checks & CORS
? appsettings.Docker.json      - Docker конфіг
? 7 гайдів документації        - Повна інформація
```

---

## ?? НАЙПОПУЛЯРНІШІ КОМАНДИ

```bash
# Запустити все
docker-compose up -d

# Дивити логи
docker-compose logs -f api

# Зупинити
docker-compose down

# Тести
docker-compose exec api dotnet test

# Статус
docker-compose ps
```

---

## ?? ДОКУМЕНТАЦІЯ (ЧИТАЙТЕ В ЦЬОМУ ПОРЯДКУ)

1. **DOCKER_QUICKSTART.md** - 5 хвилин ? START HERE
2. **DOCKER_SETUP.md** - Повна інформація
3. **DOCKER_DEVELOPMENT.md** - Для розробників

---

## ?? ?ПИ (HELPER СКРИПТИ)

### Windows

```powershell
.\docker.ps1 -Command up
.\docker.ps1 -Command logs
.\docker.ps1 -Command test
.\docker.ps1 -Command help
```

### Linux/macOS

```bash
./docker.sh up
./docker.sh logs
./docker.sh test
./docker.sh help
```

### Make (все платформи)

```bash
make up
make logs
make test
make help
```

---

## ?? СЕРВІСИ

| Сервіс | Портал | URL |
|--------|--------|-----|
| API | 5134 | http://localhost:5134/swagger |
| БД | 5432 | localhost:5432 |

---

## ? ОДИН РАЗ - ГОТОВО ДО ЗАПУСКУ!

```bash
docker-compose up -d && docker-compose ps
```

---

## ?? ПРОБЛЕМИ?

| Проблема | Рішення |
|----------|---------|
| API не запускається | `docker-compose logs api` |
| Портал зайнятий | `docker-compose down` |
| БД не підключена | `docker-compose logs db` |

---

## ? ГОТОВО!

Проект налаштований для локального запуску через Docker.

**Виконайте одну команду:** `docker-compose up -d`

**Все готово! ??**

---

**Для повної документації читайте:** DOCKER_QUICKSTART.md
