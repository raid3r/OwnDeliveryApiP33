# ?? Docker - Швидкий Старт

## ? 5 Хвилин на Запуск

### Крок 1: Установить Docker

**Windows / macOS:**
1. Завантажте [Docker Desktop](https://www.docker.com/products/docker-desktop)
2. Встановіть та перезавантажте комп'ютер
3. Перевірте: `docker --version`

**Linux:**
```bash
sudo apt-get install docker.io docker-compose
sudo usermod -aG docker $USER
```

### Крок 2: Запустити Проект

#### ??? Windows (PowerShell)
```powershell
# Запустити все необхідне
.\docker.ps1 -Command up

# або простіше
docker-compose up -d
```

#### ?? Linux / macOS
```bash
# Дати дозволу на виконання скрипту
chmod +x docker.sh

# Запустити все необхідне
./docker.sh up

# або простіше
docker-compose up -d
```

### Крок 3: Перевірити Проект

```bash
# Перевирити статус контейнерів
docker-compose ps

# Дивити логи
docker-compose logs -f

# Перевірити API
curl http://localhost:5134/health
```

### Крок 4: Відкрити API у браузері

Перейдіть на: **http://localhost:5134/swagger**

---

## ?? Що запускається

| Сервіс | Портал | Статус |
|--------|--------|--------|
| ?? API | 5134 | http://localhost:5134/swagger |
| ??? PostgreSQL | 5432 | localhost:5432 |

---

## ??? Корисні Команди

### Запуск / Зупинка

```bash
# Запустити
docker-compose up -d

# Зупинити
docker-compose down

# Перезавантажити
docker-compose restart

# Перебудувати та запустити
docker-compose up -d --build
```

### Логи

```bash
# Всі логи
docker-compose logs -f

# Тільки API
docker-compose logs -f api

# Тільки БД
docker-compose logs -f db
```

### Вхід до контейнерів

```bash
# Оболонка API
docker-compose exec api /bin/bash

# PostgreSQL командний рядок
docker-compose exec db psql -U postgres -d OwnDeliveryDb
```

### Очистка

```bash
# Видалити все (контейнери, volumes)
docker-compose down -v

# Видалити невиконувані ресурси
docker system prune -f
```

---

## ?? Helper Скрипти

### Linux / macOS - docker.sh

```bash
./docker.sh up           # Запустити
./docker.sh down         # Зупинити
./docker.sh logs         # Дивити логи
./docker.sh logs-api     # Логи API
./docker.sh status       # Статус
./docker.sh rebuild      # Перебудувати
./docker.sh test         # Запустити тести
./docker.sh help         # Довідка
```

### Windows - docker.ps1

```powershell
.\docker.ps1 -Command up
.\docker.ps1 -Command down
.\docker.ps1 -Command logs
.\docker.ps1 -Command status
.\docker.ps1 -Command rebuild
.\docker.ps1 -Command test
.\docker.ps1 -Command help
```

---

## ?? Конфігурація

### .env Файл

Файл `.env` вже створений с значення за замовчуванням:

```env
POSTGRES_DB=OwnDeliveryDb
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
JWT_KEY=SuperSecretKeyForJwtTokenGenerationAtLeast32Chars!
ASPNETCORE_ENVIRONMENT=Development
```

**Важливо:** Змініть пароль в production!

---

## ?? Повна Документація

Дивіться **DOCKER_SETUP.md** для:
- Детальних інструкцій
- Вирішення проблем
- Розширені команди
- Безпека та оптимізація

---

## ?? Проблеми?

### Контейнер не запускається
```bash
docker-compose logs api  # Подивитися помилку
docker-compose down -v   # Очистити
docker-compose up -d --build  # Перебудувати
```

### Портал 5134 вже використовується
```bash
# Змініть портал в docker-compose.override.yml
# Змініть "5134:8080" на "5135:8080"
```

### БД не підключується
```bash
docker-compose ps  # Перевірити статус
docker-compose logs db  # Дивити логи БД
docker-compose exec db psql -U postgres  # Перевірити БД
```

---

## ? Чек-лист

- [ ] Docker встановлений (`docker --version`)
- [ ] `.env` файл створений
- [ ] `docker-compose up -d` виконаний
- [ ] `docker-compose ps` показує 2 контейнера
- [ ] http://localhost:5134/swagger доступний
- [ ] Можна реєструватися та логіниться

---

## ?? Готово!

Проект запущений локально з усіма необхідними сервісами. Час розробки! ??

---

Детальна інформація: [DOCKER_SETUP.md](DOCKER_SETUP.md)
