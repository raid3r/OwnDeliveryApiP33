# ?? Docker Конфигурація - OwnDeliveryApiP33

## ?? Огляд

Docker конфігурація дозволяє запускати весь проект локально у контейнерах:
- **PostgreSQL Database** - База даних
- **OwnDeliveryAPI** - Основний API
- **PgAdmin** (опціонально) - Управління БД

---

## ?? Швидкий Старт (5 хвилин)

### Крок 1: Встановить Docker

#### Windows
- [Завантажити Docker Desktop for Windows](https://www.docker.com/products/docker-desktop)
- Встановіть та перезагрузитесь

#### macOS
```bash
brew install docker
open /Applications/Docker.app
```

#### Linux
```bash
sudo apt-get install docker.io docker-compose
sudo usermod -aG docker $USER
```

### Крок 2: Налаштуйте .env файл

```bash
# Файл уже створений: .env
# Значення за замовчуванням вже встановлені
# Змініть при необхідності
```

### Крок 3: Запустіть контейнери

```bash
# Запустите Docker контейнеры
docker-compose up -d

# Перевіріть статус
docker-compose ps
```

### Крок 4: Перевіріть API

```bash
# Swagger UI доступний за адресою
http://localhost:5134/swagger/index.html

# Health check
curl http://localhost:5134/health
```

---

## ?? Структура Docker файлів

```
OwnDeliveryApiP33/
??? Dockerfile              ? Інструкції для побудови image
??? docker-compose.yml      ? Основна конфігурація
??? docker-compose.override.yml  ? Перевизначення для розробки
??? .env                    ? Змінні середовища (НЕ коммітити!)
??? .env.example            ? Приклад .env файлу
??? .dockerignore           ? Файли для ігнорування при побудові
```

---

## ?? Детальне Описання Файлів

### Dockerfile
```dockerfile
# Stage 1: Build - компілює код
# Stage 2: Runtime - готова Docker image
```

**Особливості:**
- Multi-stage build для оптимізації розміру image
- .NET 8 SDK для побудови
- .NET 8 Runtime для виконання
- Порт 8080 за замовчуванням

### docker-compose.yml
**Сервіси:**
1. **db** - PostgreSQL 16
   - Портал: 5432
   - Облем: postgres_data
   - Healthcheck включений

2. **api** - ASP.NET Core API
   - Портал: 5134
   - Залежить від db (service_healthy)

### docker-compose.override.yml
**Перевизначення для розробки:**
- Развёртывание зміст коду
- Детальне логування
- Команда для запуску

### .env
**Змінні середовища:**
```
POSTGRES_DB=OwnDeliveryDb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
JWT_KEY=...
ASPNETCORE_ENVIRONMENT=Development
```

---

## ?? Команди Docker

### Запуск

```bash
# Запустити в фоновому режимі
docker-compose up -d

# Запустити з виводом логів
docker-compose up

# Перебудувати image перед запуском
docker-compose up -d --build

# Запустити окремий сервіс
docker-compose up -d api
```

### Стоп і очистка

```bash
# Зупинити контейнеры
docker-compose down

# Зупинити і видалити volumes (БД)
docker-compose down -v

# Видалити всі неиспользуемые контейнеры
docker container prune -f

# Видалити всі неиспользуемые image
docker image prune -a -f
```

### Логи

```bash
# Дивити логи всіх сервісів
docker-compose logs -f

# Дивити логи конкретного сервісу
docker-compose logs -f api
docker-compose logs -f db

# Останні N рядків
docker-compose logs --tail=100 api
```

### Вхід до контейнера

```bash
# Вхід до API контейнера
docker-compose exec api /bin/bash

# Вхід до БД контейнера
docker-compose exec db psql -U postgres -d OwnDeliveryDb

# Запустити команду в контейнері
docker-compose exec api dotnet --version
```

### Статус

```bash
# Перевіріть статус сервісів
docker-compose ps

# Більше інформації
docker-compose ps --services
docker inspect $(docker-compose ps -q api)
```

---

## ?? Доступ до Сервісів

### API Endpoints

| Сервіс | URL | Опис |
|--------|-----|------|
| Swagger UI | http://localhost:5134/swagger | API документація |
| Health Check | http://localhost:5134/health | Статус API |
| Register | POST /api/v1/auth/register | Реєстрація |
| Login | POST /api/v1/auth/login | Вхід |

### Приклади запитів

```bash
# Реєстрація
curl -X POST http://localhost:5134/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john@example.com",
    "password": "Password123!",
    "phoneNumber": "+380501234567"
  }'

# Вхід
curl -X POST http://localhost:5134/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john@example.com",
    "password": "Password123!"
  }'
```

### База Даних

| Сервіс | URL | Логін | Пароль |
|--------|-----|-------|--------|
| PostgreSQL | localhost:5432 | postgres | postgres |
| DB Name | - | - | OwnDeliveryDb |

**Підключення з psql:**
```bash
docker-compose exec db psql -U postgres -d OwnDeliveryDb
```

---

## ?? Розв'язання Проблем

### Проблема: Контейнер не запускається

```bash
# Перевіріть логи
docker-compose logs api

# Перебудуйте image
docker-compose down
docker-compose up -d --build

# Очистіть старі image
docker image prune -a -f
```

### Проблема: Портал вже використовується

```bash
# Знайдіть процес на порту 5134
lsof -i :5134  # macOS/Linux
netstat -ano | findstr :5134  # Windows

# Змініть портал у docker-compose.override.yml
ports:
  - "5135:8080"  # Нова портал
```

### Проблема: БД не підключується

```bash
# Перевіріть, чи БД готова
docker-compose ps db

# Перевіріть логи БД
docker-compose logs db

# Перезавантажте БД
docker-compose down
docker volume rm owndeliveryapip33_postgres_data
docker-compose up -d
```

### Проблема: JWT Key не встановлений

```bash
# Перевіріть .env файл
cat .env

# Переконайтеся, що JWT_KEY містить принаймні 32 символи
# Закодуйте .env файл, якщо використовуєте Windows
```

---

## ?? Моніторинг і Логування

### Real-time логи

```bash
# Всі логи
docker-compose logs -f

# Тільки API
docker-compose logs -f api

# Тільки БД
docker-compose logs -f db

# Останні 50 рядків
docker-compose logs --tail=50 -f
```

### Здоров'я контейнерів

```bash
# Перевіріть стан здоров'я
docker-compose ps

# Детальна інформація
docker inspect $(docker-compose ps -q api) | grep -A 10 "Health"
```

### Використання ресурсів

```bash
# Статистика в реальному часі
docker stats

# Тільки API контейнер
docker stats owndelivery_api
```

---

## ?? Безпека

### Development vs Production

**Development (.env):**
- Просто пароль для розробки
- Логування деталізоване
- Health check увімкнений

**Production (GitHub Secrets):**
- Сильні пароля
- Мінімальне логування
- Масштабування контейнерів

### Змінні середовища

**Важливо:** Ніколи не коммітьте .env файл!

```bash
# .env уже в .gitignore
cat .gitignore | grep env
```

---

## ??? Архітектура

```
???????????????????????????????????????
?         Docker Compose              ?
???????????????????????????????????????
?                                     ?
?  ????????????????  ????????????   ?
?  ?   API (ASP   ?  ?   DB     ?   ?
?  ?   .NET 8)    ???? Postgres ?   ?
?  ?   :8080      ?  ? :5432    ?   ?
?  ????????????????  ????????????   ?
?                                     ?
?  ????????????????????????????????  ?
?  ?   Shared Volume (postgres)   ?  ?
?  ????????????????????????????????  ?
?                                     ?
???????????????????????????????????????

     ? (Host Machine)

???????????????????????????????????????
?   Browser / API Client              ?
?   localhost:5134/swagger            ?
???????????????????????????????????????
```

---

## ?? Performance Tips

### Оптимізація побудови

```bash
# Кешування рівнів
docker-compose build --no-cache

# Паралельна побудова
docker-compose build --parallel
```

### Оптимізація запуску

```bash
# Запустити тільки необхідні сервіси
docker-compose up -d db api

# Не запускати dependent сервіси
docker-compose up -d --no-deps api
```

### Очистка простору

```bash
# Очистіть невиконувані контейнеры
docker container prune -f

# Очистіть невиконувані volumes
docker volume prune -f

# Очистіть всі невиконувані дані
docker system prune -a
```

---

## ?? Обновления і перебудови

### Оновити code

```bash
git pull

# Перебудуйте image
docker-compose down
docker-compose up -d --build
```

### Оновити dependencies

```bash
# Очистіть кеш побудови
docker builder prune

# Перебудуйте з --no-cache
docker-compose up -d --build --no-cache
```

---

## ?? Додаткові Ресурси

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [ASP.NET Core in Containers](https://learn.microsoft.com/en-us/dotnet/core/docker/build-container)
- [PostgreSQL in Docker](https://hub.docker.com/_/postgres)

---

## ? Checklist - Перша установка

- [ ] Docker Desktop встановлений
- [ ] `.env` файл створений
- [ ] `docker-compose up -d` запущений
- [ ] `docker-compose ps` показує 2 running контейнера
- [ ] http://localhost:5134/swagger доступний
- [ ] БД підключена (дивіться логи)

---

## ?? Наступні кроки

1. **Запустити проект:** `docker-compose up -d`
2. **Перевірити API:** http://localhost:5134/swagger
3. **Запустити тести:** `docker-compose exec api dotnet test`
4. **Розглянути логи:** `docker-compose logs -f`

---

**Версія:** 1.0
**Дата:** 2024
**Статус:** ? Production Ready
