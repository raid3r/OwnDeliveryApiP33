# ?? OwnDeliveryApiP33 - Docker Ready!

> **API для управління доставками курьєрів з Docker конфігурацією**

## ? Швидкий Старт (3 кроки)

```bash
# 1. Запустити контейнери
docker-compose up -d

# 2. Перевірити статус
docker-compose ps

# 3. Відкрити API
# Браузер: http://localhost:5134/swagger
```

**Ось і все! ?? Проект запущений локально.**

---

## ?? Що Запускається?

| Сервіс | Портал | URL |
|--------|--------|-----|
| ?? API (ASP.NET 8) | 5134 | http://localhost:5134/swagger |
| ??? PostgreSQL | 5432 | localhost:5432 |

---

## ?? Docker - Одна Команда

```bash
# Запустити
docker-compose up -d

# Логи
docker-compose logs -f api

# Зупинити
docker-compose down
```

---

## ?? Документація

### Новим розробникам?

Почніть з цього:

1. **[DOCKER_QUICKSTART.md](DOCKER_QUICKSTART.md)** - 5 хвилин (ПОЧНІТЬ ЗВІДСИ!)
2. **[DOCKER_SETUP.md](DOCKER_SETUP.md)** - Повна документація
3. **[DOCKER_DEVELOPMENT.md](DOCKER_DEVELOPMENT.md)** - Щоденна розробка

### Скорочена версія всіх команд:

```bash
# Основні операції
docker-compose up -d          # Запустити
docker-compose down           # Зупинити
docker-compose logs -f        # Логи
docker-compose ps             # Статус

# Тестування
docker-compose exec api dotnet test

# Вхід до контейнерів
docker-compose exec api /bin/bash         # API shell
docker-compose exec db psql -U postgres   # Database CLI
```

---

## ??? Helper Скрипти

### Windows (PowerShell)

```powershell
.\docker.ps1 -Command up
.\docker.ps1 -Command logs
.\docker.ps1 -Command help
```

### Linux / macOS

```bash
./docker.sh up
./docker.sh logs
./docker.sh help
```

### Все Платформи (Make)

```bash
make up       # Запустити
make down     # Зупинити  
make logs     # Логи
make test     # Тести
make help     # Всі команди
```

---

## ?? Конфігурація

### .env Файл

Автоматично створено з стандартними значеннями:

```env
POSTGRES_DB=OwnDeliveryDb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
JWT_KEY=SuperSecretKeyForJwtTokenGenerationAtLeast32Chars!
ASPNETCORE_ENVIRONMENT=Development
```

**Для Production:** Змініть паролі перед deployment!

---

## ?? Архітектура

```
???????????????????????????????????????
?      Docker Compose Stack           ?
???????????????????????????????????????
?                                     ?
?  ????????????????  ????????????   ?
?  ?   API        ?  ?   DB     ?   ?
?  ?   (8080)     ?  ? (5432)   ?   ?
?  ????????????????  ????????????   ?
?         ?                ?          ?
?   Swagger UI      Auto Migrate     ?
?   Health Check                     ?
?                                     ?
???????????????????????????????????????
```

---

## ? Можливості

? **Multi-stage Docker build** - Оптимізований розмір image  
? **Health checks** - Моніторинг стану контейнерів  
? **Auto-migration** - Автоматичні міграції БД при старті  
? **Development overrides** - Спеціальна конфігурація для розробки  
? **Helper скрипти** - Для всіх платформ (Windows, Linux, macOS)  
? **Повна документація** - 3 гайди для різних потреб  

---

## ?? Проблеми?

### Контейнер не запускається
```bash
docker-compose logs api  # Дивіть помилку
docker-compose rebuild   # Перебудуйте
```

### Портал 5134 зайнятий
```bash
# Змініть у docker-compose.override.yml
ports:
  - "5135:8080"  # Нова портал
```

### БД не підключується
```bash
docker-compose logs db        # Дивіть логи БД
docker-compose restart db     # Перезавантажте БД
```

Детальна інформація: [DOCKER_SETUP.md ? Розв'язання проблем](DOCKER_SETUP.md)

---

## ?? Структура Проекту

```
OwnDeliveryApiP33/
??? ?? Docker:
?   ??? Dockerfile
?   ??? docker-compose.yml
?   ??? docker-compose.override.yml
?   ??? .env
??? ?? Документація:
?   ??? DOCKER_QUICKSTART.md      ? START HERE
?   ??? DOCKER_SETUP.md
?   ??? DOCKER_DEVELOPMENT.md
??? ??? Помічники:
?   ??? docker.sh
?   ??? docker.ps1
?   ??? Makefile
??? ?? Проект:
?   ??? OwnDeliveryApiP33/
?   ?   ??? Program.cs (Updated for Docker)
?   ?   ??? appsettings.Docker.json
?   ?   ??? ...
?   ??? Tests/
??? ... інші файли
```

---

## ?? Налаштування (для розробників)

### Перший раз запускаєте?

1. **Встановіть Docker**
   - [Windows/macOS](https://www.docker.com/products/docker-desktop)
   - Linux: `sudo apt-get install docker.io docker-compose`

2. **Запустіть проект**
   ```bash
   docker-compose up -d
   ```

3. **Перевірте статус**
   ```bash
   docker-compose ps
   ```

4. **Відкрийте API**
   - Браузер: http://localhost:5134/swagger
   - API здоров'я: curl http://localhost:5134/health

---

## ?? Безпека

- ? `.env` в `.gitignore` (не коммітяться паролі)
- ? Non-root user у контейнері
- ? Health checks для моніторингу
- ? Для production використовуйте [GitHub Secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets)

---

## ?? Залежності (Requirement)

- Docker Desktop або Docker + Docker Compose
- Інше: встановлюється в контейнері

---

## ?? Готово!

```bash
# Запустіть одну команду:
docker-compose up -d

# Все готово до роботи! ??
```

API доступна за адресою: **http://localhost:5134/swagger**

---

## ?? Додаткова Інформація

**Повна документація:**
- [DOCKER_QUICKSTART.md](DOCKER_QUICKSTART.md) - 5 хвилин 
- [DOCKER_SETUP.md](DOCKER_SETUP.md) - Детальний гайд
- [DOCKER_DEVELOPMENT.md](DOCKER_DEVELOPMENT.md) - Для розробників

**Helper скрипти:**
- Windows: `.\docker.ps1 -Command help`
- Linux/macOS: `./docker.sh help`
- Make: `make help`

---

## ?? Корисні Команди

```bash
# Основні
docker-compose up -d              # Запустити
docker-compose down               # Зупинити
docker-compose logs -f api        # Логи API
docker-compose exec api dotnet test   # Тести

# Керування контейнерами  
docker-compose ps                 # Статус
docker-compose restart api        # Перезавантажити
docker-compose exec api /bin/bash # Shell у контейнері

# Очистка
docker-compose down -v            # Видалити (включно БД)
docker system prune -a            # Очистити все
```

---

## ?? Внесок

Дивіться [Contributing Guide](CONTRIBUTING.md)

---

## ?? Ліцензія

MIT

---

**Версія:** 1.0  
**Статус:** ? Production Ready  
**Останнє оновлення:** 2024

**?? Готові запустити проект? Виконайте:** `docker-compose up -d`
