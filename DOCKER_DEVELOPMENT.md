# ?? Docker - Повний Гайд для Розробників

## ?? Структура Проекту

```
OwnDeliveryApiP33/
?
??? ?? Docker Файли:
?   ??? Dockerfile                      # Інструкції побудови image
?   ??? docker-compose.yml              # Основна конфігурація
?   ??? docker-compose.override.yml     # Перевизначення для розробки
?   ??? .dockerignore                   # Файли для ігнорування
?   ??? .env                            # Змінні середовища (локально)
?   ??? .env.example                    # Приклад .env
?
??? ??? Helper Скрипти:
?   ??? docker.sh                       # Shell скрипт (Linux/macOS)
?   ??? docker.ps1                      # PowerShell скрипт (Windows)
?   ??? Makefile                        # Make команди
?
??? ?? Документація:
?   ??? DOCKER_SETUP.md                 # Повний гайд
?   ??? DOCKER_QUICKSTART.md            # Швидкий старт
?   ??? DOCKER_DEVELOPMENT.md           # Цей файл
?
??? ?? Проект:
?   ??? OwnDeliveryApiP33/
?   ?   ??? OwnDeliveryApiP33.csproj
?   ?   ??? Program.cs                  # Оновлено для Docker
?   ?   ??? appsettings.json
?   ?   ??? appsettings.Development.json
?   ?   ??? appsettings.Docker.json     # Новий Docker конфіг
?   ?   ??? ...
?   ??? OwnDeliveryApiP33.Tests.Unit/
?   ??? OwnDeliveryApiP33.Tests.Integration/
?
??? .gitignore                          # Git ігнорування (.env тощо)
```

---

## ?? Стартуємо Проект

### Перший Раз? Давайте Почнемо!

```bash
# 1. Клонуйте репозиторій
git clone https://github.com/raid3r/OwnDeliveryApiP33
cd OwnDeliveryApiP33

# 2. Переконайтеся, що Docker встановлений
docker --version
docker-compose --version

# 3. Запустіть проект
docker-compose up -d

# 4. Перевірте
docker-compose ps

# 5. Отримайте доступ до API
# Браузер: http://localhost:5134/swagger
# Terminal: curl http://localhost:5134/health
```

---

## ?? Сервісна Архітектура

### docker-compose.yml

```yaml
services:
  db:                    # PostgreSQL База Даних
    image: postgres:16-alpine
    container_name: owndelivery_db
    environment:
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:         # Перевірка здоров'я
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      
  api:                   # ASP.NET Core Application
    build: Dockerfile
    container_name: owndelivery_api
    ports:
      - "5134:8080"
    depends_on:
      db:
        condition: service_healthy
```

---

## ?? Dockerfile Пояснення

```dockerfile
# Stage 1: Build
# - Використовує .NET 8 SDK
# - Відновлює залежності
# - Компілює код

# Stage 2: Runtime
# - Менший образ (тільки runtime)
# - Копіює скомпільований код
# - Готово до запуску
```

**Переваги:**
- ? Малий розмір image (~200MB vs 500MB+)
- ? Швидка побудова через кешування
- ? Безпека (видалені SDK та дизайнерські утиліти)

---

## ?? Файли Конфігурації

### .env (НЕ КОММІТИТИ!)

```bash
POSTGRES_DB=OwnDeliveryDb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres

JWT_KEY=SuperSecretKeyForJwtTokenGenerationAtLeast32Chars!
ASPNETCORE_ENVIRONMENT=Development
```

**?? ВАЖЛИВО:**
- Кожен розробник має свій `.env`
- `.env` в `.gitignore`
- Ніколи не коммітьте паролі!
- Для production используйте GitHub Secrets

### appsettings.Docker.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=db;Port=5432;..."  // db = контейнер
  },
  "Jwt": {
    "Key": "..."  // З .env через docker-compose
  }
}
```

---

## ?? Вхід до Контейнерів

### API Container

```bash
# Bash shell
docker-compose exec api /bin/bash

# Запустити команду
docker-compose exec api dotnet --version

# Дивити версію
docker-compose exec api cat /etc/os-release
```

### Database Container

```bash
# PostgreSQL CLI
docker-compose exec db psql -U postgres -d OwnDeliveryDb

# Команди в psql:
# \dt                    - Таблиці
# \d table_name          - Структура таблиці
# SELECT * FROM users;   - SQL запит
# \q                     - Вихід
```

---

## ?? Цикл Розробки

### 1?? Код Скопійовано / Підтягнуто

```bash
# Перевірте наявність змін
git status

# Перетягніть останні зміни
git pull

# Перебудуйте контейнер (якщо змінилися залежності)
docker-compose down
docker-compose up -d --build
```

### 2?? Розробляєте Функціонал

```bash
# Запустіть в розробці
docker-compose up -d

# Дивіть логи в реальному часі
docker-compose logs -f api

# Модифікуйте код у своєму IDE
# (Контейнер автоматично перестартує якщо дозволено)
```

### 3?? Тестування

```bash
# Unit тести
docker-compose exec api dotnet test OwnDeliveryApiP33.Tests.Unit

# Integration тести
docker-compose exec api dotnet test OwnDeliveryApiP33.Tests.Integration

# Всі тести
docker-compose exec api dotnet test
```

### 4?? Commit & Push

```bash
git add .
git commit -m "feat: new feature"
git push

# Docker автоматично запуститься в CI/CD
```

---

## ?? Різні Сценарії

### Сценарій 1: Локальна Розробка

```bash
# Використовуйте docker-compose.override.yml
# (Автоматично застосовується)

# Вам потрібно:
docker-compose up -d

# Це запустить:
# - api з розробничим конфігом
# - db з даними
```

### Сценарій 2: Вийти з Docker (локально без контейнерів)

```bash
# Встановіть PostgreSQL локально
# Змініть appsettings.Development.json на localhost

# Запустіть без Docker
dotnet run --project OwnDeliveryApiP33

# або з Watch (для HotReload)
dotnet watch run --project OwnDeliveryApiP33
```

### Сценарій 3: Production-like (локально)

```bash
# Без override файлу (production конфіг)
docker-compose -f docker-compose.yml up -d

# або
docker-compose up -d --profile production
```

### Сценарій 4: Налагодження (Debug)

```bash
# Запустіть з розробничим конфігом
docker-compose up -d

# Дивіть детальні логи
docker-compose logs -f --timestamps api

# Вхід в контейнер
docker-compose exec api /bin/bash

# Перевірте змінні середовища
docker-compose exec api env | grep -i jwt

# Тестуйте БД
docker-compose exec db psql -U postgres -d OwnDeliveryDb
```

---

## ?? Моніторинг

### Логи

```bash
# Real-time логи (останні 100 рядків)
docker-compose logs -f --tail=100 api

# З часовими мітками
docker-compose logs -f --timestamps api

# Тільки помилки
docker-compose logs api | grep -i error

# За діапазоном часу
docker-compose logs --until 5m api
```

### Ресурси

```bash
# CPU/Memory використання
docker stats

# Тільки api контейнер
docker stats owndelivery_api

# Дискове місце
docker system df

# Детальна інформація про контейнер
docker inspect owndelivery_api
```

---

## ?? Очистка

### Видалити Контейнери

```bash
# Зупинити (can restart)
docker-compose stop

# Видалити (cannot restart)
docker-compose down

# Видалити все + volumes (БД)
docker-compose down -v

# Видалити все + все (volumes + orphans)
docker-compose down -v --remove-orphans
```

### Видалити Image

```bash
# Видалити неиспользуемые
docker image prune -a

# Видалити конкретний image
docker image rm owndeliveryapip33-api

# Видалити всі (ОСТОРОЖНО!)
docker image rm $(docker images -q)
```

### Видалити Volumes

```bash
# Видалити неиспользуемые
docker volume prune

# Видалити конкретний volume
docker volume rm owndeliveryapip33_postgres_data

# Список volumes
docker volume ls
```

### Загальна Очистка

```bash
# Видалити ВСЕ неиспользуемое
docker system prune -a

# З перепитуванням
docker system prune -a --volumes
```

---

## ?? Безпека

### Development

```env
# .env (НЕ КОММІТИТИ!)
POSTGRES_PASSWORD=postgres
JWT_KEY=SimpleKeyForDevelopmentOnly
```

### Production (GitHub Actions)

```yaml
# .github/workflows/deploy.yml
env:
  POSTGRES_PASSWORD: ${{ secrets.POSTGRES_PASSWORD }}
  JWT_KEY: ${{ secrets.JWT_KEY }}
```

### Secrets в Docker

```bash
# Docker Secrets (для swarm mode)
echo "password123" | docker secret create postgres_pwd -

# Reference in compose
secrets:
  postgres_pwd:
    external: true
```

---

## ?? CI/CD Integration

### GitHub Actions

```yaml
name: Docker Build
on: [push, pull_request]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: docker/setup-buildx-action@v2
      - run: docker-compose build
      - run: docker-compose up -d
      - run: docker-compose exec -T api dotnet test
```

---

## ?? Performance Tips

### Caching

```dockerfile
# ? ДОБРЕ (кешуємо залежності)
COPY OwnDeliveryApiP33.csproj .
RUN dotnet restore
COPY . .

# ? ПОГАНО (не кешуємо)
COPY . .
RUN dotnet restore
```

### Image Size

```bash
# Перевірте розмір
docker images

# Оптимізуйте
docker image prune -a
```

### Build Speed

```bash
# Паралельна побудова
docker-compose build --parallel

# Без кешу
docker-compose build --no-cache api
```

---

## ?? Проблеми й Рішення

| Проблема | Причина | Рішення |
|----------|---------|---------|
| **Container не запускається** | Помилка в коді | `docker-compose logs api` |
| **Port вже зайнятий** | Інший процес | `docker-compose down` або змініть port |
| **БД не підключується** | БД не готова | `docker-compose logs db` |
| **Out of memory** | Багато контейнерів | `docker system prune -a` |
| **Slow build** | Великі файли | Додайте до `.dockerignore` |

---

## ?? Документація

| Файл | Призначення |
|------|-----------|
| **DOCKER_QUICKSTART.md** | 5 хвилин до старту |
| **DOCKER_SETUP.md** | Повна документація |
| **DOCKER_DEVELOPMENT.md** | Цей файл (розробка) |

---

## ? Checklist для Розробника

### Перший день:
- [ ] Docker встановлений
- [ ] Проект запущений локально (`docker-compose up -d`)
- [ ] API доступний (http://localhost:5134/swagger)
- [ ] БД підключена (тести пройшли)

### Щоденно:
- [ ] Перед початком: `git pull && docker-compose up -d --build`
- [ ] Розробляю функціонал
- [ ] Тестую локально: `docker-compose exec api dotnet test`
- [ ] Коммітю код: `git push`

### Перед PR:
- [ ] Тести пройшли
- [ ] Логи чисті (без помилок)
- [ ] `.env` не коммітив
- [ ] Код відповідає стилю проекту

---

## ?? Наступні Кроки

1. **Запустіть:** `docker-compose up -d`
2. **Тестуйте:** `docker-compose logs -f`
3. **Розробляйте:** Modifty code in IDE
4. **Коммітьте:** `git push`

---

**Успіху в розробці! ??**

Детальна інформація: [DOCKER_SETUP.md](DOCKER_SETUP.md)
