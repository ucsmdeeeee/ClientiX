# ClientiX

> SaaS-платформа для самозанятых бьюти-мастеров с персональными Telegram-ботами.
> Каждый мастер получает собственного бота для записи клиентов с полным циклом: услуги, портфолио, расписание, бронирование, напоминания, статистика. Управление — через главный бот, веб-кабинет и админ-панель.

🌐 **Production:** [clientix-assist.ru](https://clientix-assist.ru)
🤖 **Главный бот:** [@cl1ent1x_bot](https://t.me/cl1ent1x_bot)
💼 **Кабинет мастера:** [clientix-assist.ru/app](https://clientix-assist.ru/app)

---

## Возможности продукта

### Для мастера

#### В Telegram (главный бот + бот мастера)
- Регистрация и личный кабинет через главный бот `@cl1ent1x_bot`
- Подключение собственного бота клиентов через @BotFather (one-tap setup)
- Каталог услуг с ценами и длительностью
- Портфолио (фото работ)
- Гибкое расписание: шаблон недели + исключения на конкретные даты
- Календарь с цветовой раскраской (рабочий/выходной/особый/прошедший)
- Настраиваемые горизонт записи (7/14/30/60 дней) и часовой пояс (11 поясов РФ)
- Управление записями: создание, перенос, отмена, добавление/удаление услуг
- Завершение записи (выполнена / клиент не пришёл)
- Настраиваемые напоминания клиентам (за 24 ч + 1/3/6/12 ч)
- Подписка через ЮKassa с триал-периодом

#### Веб-кабинет (`/app`)
- Авторизация через deep-link `/login` в боте (короткоживущий JWT exchange) или Telegram Login Widget
- Дашборд с реальными метриками: записи и доход за день / 7 дней / 30 дней
- График динамики записей за 30 дней (recharts)
- Premium luxury-дизайн на базе чёрного и платинового цветов

### Для клиента
- Запись через бот мастера в 3 шага: услуга → дата → время
- Календарь с раскраской свободных дат
- Просмотр своих записей
- Перенос и отмена записей
- Добавление и удаление услуг в существующей записи
- Автоматические напоминания
- Опциональная проверка подписки на TG-канал мастера

### Для администратора платформы (`/app/admin`)
- Общая статистика: число мастеров, активных ботов, платных подписок, броней и оборота за 30 дней
- Таблица всех мастеров с поиском по имени/username/городу/нише
- Бейджи статусов подписок
- Прямые ссылки на профили мастеров и их боты

---

## Архитектура

### Стек

#### Backend
- **.NET 8, C#** — Clean Architecture
- **PostgreSQL 16** — основная БД
- **Redis 7** — кэш и FSM состояний бота
- **RabbitMQ 3.13** — message broker (заготовка под микросервисы)
- **Telegram.Bot 22.x** — long polling
- **Yandex.Checkout.V3** — ЮKassa
- **xUnit + FluentAssertions + EF Core InMemory** — тесты

#### Frontend
- **React 18 + TypeScript + Vite** — лендинг и кабинет
- **Tailwind CSS** — стилизация
- **framer-motion** — анимации
- **recharts** — графики статистики
- **react-router-dom** — маршрутизация

#### Инфраструктура
- **Docker Compose** — оркестрация контейнеров
- **Caddy 2** — reverse proxy с автоматическим HTTPS через Let's Encrypt
- **nginx** — раздача статики React-сборок
- **Yandex Cloud** — VM Ubuntu 22.04

### Слои Backend
```
ClientiX.Domain          — Entities, value objects
ClientiX.Application     — (в развитии) Use cases, CQRS
ClientiX.Infrastructure  — EF Core, Repositories, ЮKassa, Redis, BookingSlotService
ClientiX.BotGateway      — Telegram polling, FSM, MasterBotManager, webhook ЮKassa
ClientiX.WebApi          — REST API для веб-кабинета и админки, JWT-auth
```

### Компоненты в проде
```
                     ┌─────────────────────────────────────────┐
                     │       Yandex Cloud VM (46.21.246.30)    │
                     │                                         │
       HTTPS         │  ┌────────────────────────────────────┐ │
clientix-assist.ru ──┼─→│           Caddy :443               │ │
                     │  │  (Let's Encrypt, HSTS, security    │ │
                     │  │   headers, reverse proxy router)   │ │
                     │  └────┬───────┬──────┬──────┬─────────┘ │
                     │       │       │      │      │           │
                     │   /   │   /app│ /api │/yk-w │           │
                     │       ↓       ↓      ↓      ↓           │
                     │ ┌─────────┐┌────────┐┌──────────┐       │
                     │ │ landing ││ master ││ webapi   │       │
                     │ │ (nginx) ││ (nginx)││ (.NET 8) │       │
                     │ │  React  ││  React ││  REST    │       │
                     │ └─────────┘└────────┘└────┬─────┘       │
                     │                           │             │
                     │                  ┌────────┴──────┐      │
                     │                  │   botgateway  │      │
                     │                  │   (.NET 8)    │      │
                     │                  │ Telegram      │      │
                     │                  │ + Yookassa    │      │
                     │                  └─┬───┬───┬─────┘      │
                     │                    │   │   │            │
                     │             ┌──────┴┐ ┌┴──┐┌┴────────┐  │
                     │             │postgres││redis││rabbitmq│  │
                     │             │ (127.0)││(127)│ (127.0)│  │
                     │             └────────┘└────┘└────────┘  │
                     │                                         │
                     └─────────────────────────────────────────┘
```

### Мульти-бот
В одном процессе `BotGateway` работают:
1. **Главный бот** `@cl1ent1x_bot` через `TelegramPollingService` — управление кабинетом мастера
2. **Боты мастеров** через `MasterBotManager` — динамическое подключение/отключение long polling-ов для каждого мастера

### Защита от race condition
В таблице `bookings` — уникальный частичный индекс:
```sql
CREATE UNIQUE INDEX idx_bookings_no_overlap
ON bookings ("UserId", "StartsAt")
WHERE "Status" IN ('pending', 'confirmed');
```
Это гарантирует, что два клиента не смогут одновременно забронировать один слот.

### Безопасность
- **Изоляция сервисов**: PostgreSQL/Redis/RabbitMQ/BotGateway/WebApi не доступны снаружи (только через Docker network или `127.0.0.1`)
- **Файрвол**: ufw + Yandex Cloud Security Group разрешают только 22, 80, 443
- **HTTPS**: TLS 1.3 через Let's Encrypt (auto-renew), HSTS на 1 год
- **Security headers**: `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`
- **SSH**: только по ключу, пароли отключены
- **JWT**: HMAC-SHA256, 30-дневный долгий токен + 5-минутный короткий для deep-link login
- **Telegram Login Widget**: верификация HMAC-подписи + защита от replay (24h окно)
- **Webhook ЮKassa**: проверка подписи и идемпотентность
- **Токены ботов мастеров**: шифруются через ASP.NET Data Protection
- **Rate-limit на бронирование**: 5 операций/мин, max 3 активных записи

---

## Веб-приложения

### Лендинг (`web/landing`)
Маркетинговый сайт с premium luxury-дизайном:
- Hero с анимированным логотипом и орбитами иконок
- Секции: возможности, как это работает, тарифы, FAQ
- Правовые страницы: политика конфиденциальности, публичная оферта, согласие на ОПД (152-ФЗ)
- Адаптивный дизайн

### Кабинет мастера (`web/master`)
React SPA для управления через браузер:
- Авторизация через deep-link `/login` (короткоживущий JWT exchange)
- Backup-вариант: Telegram Login Widget
- Дашборд с реальными метриками из БД
- График записей за 30 дней через recharts
- Админский маршрут `/admin` (доступен только пользователю с `tg_id == AdminTelegramId`)

---

## Локальный запуск

### Требования
- .NET 8 SDK
- Docker Desktop
- Node.js 18+ (для фронтов)
- PowerShell или bash

### Шаги
```powershell
# 1. Склонировать
git clone https://github.com/ucsmdeeeee/Clientix
cd Clientix

# 2. Скопировать пример конфига
cp src/ClientiX.BotGateway/appsettings.Development.json.example `
   src/ClientiX.BotGateway/appsettings.Development.json
# Отредактируй: вставь токен бота, тестовые ключи ЮKassa

# 3. Запустить инфраструктуру (Postgres, Redis, RabbitMQ)
docker compose up -d postgres redis rabbitmq

# 4. Применить миграции
dotnet ef database update `
    --project src/ClientiX.Infrastructure `
    --startup-project src/ClientiX.BotGateway

# 5. Запустить BotGateway
dotnet run --project src/ClientiX.BotGateway

# 6. (Опционально) Запустить WebApi
dotnet run --project src/ClientiX.WebApi

# 7. (Опционально) Запустить лендинг и кабинет в dev-режиме
cd web/landing && npm install && npm run dev
cd web/master && npm install && npm run dev
```

### Создание миграций
```powershell
dotnet ef migrations add НазваниеМиграции `
    --project src/ClientiX.Infrastructure `
    --startup-project src/ClientiX.BotGateway `
    --output-dir Persistence/Migrations
```

---

## Тесты

В `tests/ClientiX.Tests/` — 22 unit-теста на xUnit + FluentAssertions + EF Core InMemory.

### Покрытие
- **JwtServiceTests** (6 тестов) — генерация и валидация JWT, claims, expiry, защита от подделки
- **TelegramAuthServiceTests** (4 теста) — HMAC подпись Login Widget, защита от tampering, expiry
- **UserRepositoryTests** (6 тестов) — CRUD пользователей через InMemory DB, статистика по статусам
- **DomainEntityTests** (6 тестов) — defaults сущностей `Booking` и `User`

### Запуск
```powershell
dotnet test tests/ClientiX.Tests/ClientiX.Tests.csproj
```

Все тесты проходят за **~4 секунды**:
```
Total tests: 22
     Passed: 22
     Failed: 0
```

---

## Production-деплой

### Деплой обновлений
```bash
ssh clientix@46.21.246.30
cd ~/Clientix
git pull

# Backend (если менялся C#-код)
docker compose up -d --build botgateway webapi

# Frontend (если менялся React-код)
docker compose up -d --build landing master

# Логи
docker logs clientix-botgateway --tail 20
docker logs clientix-webapi --tail 20
```

Автомиграции применяются при старте контейнера.

### Caddyfile (production)
```caddyfile
www.clientix-assist.ru {
    redir https://clientix-assist.ru{uri} permanent
}

clientix-assist.ru {
    header Strict-Transport-Security "max-age=31536000; includeSubDomains"
    header X-Content-Type-Options "nosniff"
    header X-Frame-Options "SAMEORIGIN"
    header Referrer-Policy "strict-origin-when-cross-origin"

    handle /api/* { reverse_proxy webapi:5050 }
    handle /yk-webhook { reverse_proxy botgateway:5000 }
    handle /payments/* { reverse_proxy botgateway:5000 }
    redir /app /app/ permanent
    handle_path /app/* { reverse_proxy master:80 }
    handle { reverse_proxy landing:80 }
}
```

---

## Резервное копирование

### Автоматические бэкапы
Cron на ВМ выполняет два скрипта:

**1. Бэкап PostgreSQL** — `/home/clientix/backup-postgres.sh`, ежедневно в 4:00 UTC.
- `pg_dump` → gzip → `/home/clientix/backups/postgres/clientix-db-YYYY-MM-DD_HH-MM-SS.sql.gz`
- Хранятся последние 14 дней

**2. Бэкап конфигов** — `/home/clientix/backup-configs.sh`, еженедельно в 4:30 UTC (по воскресеньям).
- Архивирует `Caddyfile`, `docker-compose.yml`, оба `appsettings.Production.json`
- Хранятся последние 30 архивов

### Ручной запуск
```bash
~/backup-postgres.sh
~/backup-configs.sh
```

### Восстановление из бэкапа
```bash
# Остановить бот, чтобы избежать конфликта записи
docker compose stop botgateway

# Восстановить (укажи нужный файл)
zcat /home/clientix/backups/postgres/clientix-db-2026-05-21.sql.gz | \
    docker exec -i clientix-postgres psql -U clientix -d clientix

# Запустить обратно
docker compose start botgateway
```

---

## Мониторинг

В проде работает **watchdog** — `/home/clientix/clientix-watchdog.sh`, запускается cron каждую минуту. Алертит в Telegram (бот `@clientix_alerts_bot`) при проблемах:

- ❌ Домен не отвечает
- ❌ Один из контейнеров упал
- ❌ PostgreSQL не отвечает на `pg_isready`
- ❌ Свободного места на диске < 1GB
- ❌ Свободной памяти < 200MB
- ❌ Webhook ЮKassa недоступен
- ❌ API кабинета вернул 5xx

Защита от спама: один алерт пока проблема не решится, и `RECOVERED` при восстановлении.

---

## Структура репозитория
```
Clientix/
├── src/
│   ├── ClientiX.Domain/          # Entities, доменная модель
│   ├── ClientiX.Application/     # Use cases (в развитии)
│   ├── ClientiX.Infrastructure/  # EF Core, Repositories, Services, ЮKassa
│   ├── ClientiX.BotGateway/      # Telegram polling, FSM, MasterBotManager
│   └── ClientiX.WebApi/          # REST API + JWT auth (auth, master, admin)
├── tests/
│   └── ClientiX.Tests/           # 22 unit-теста (xUnit)
├── web/
│   ├── landing/                  # Маркетинговый сайт (React + Vite)
│   │   ├── src/
│   │   │   ├── sections/         # Hero, Features, Pricing, Faq, Footer
│   │   │   ├── pages/            # PrivacyPage, OfferPage, ConsentPage
│   │   │   └── components/       # Particles, AnimatedLogo, LegalLayout
│   └── master/                   # Кабинет мастера (React + Vite)
│       ├── src/
│       │   ├── pages/            # Login, Dashboard, AuthCallback, AdminPage
│       │   ├── lib/api.ts        # Axios клиент + типы
│       │   └── components/       # Particles, AnimatedLogo, AnimatedCounter
├── docker-compose.yml
├── Dockerfile               # BotGateway
├── Dockerfile.webapi
├── Dockerfile.landing
├── Dockerfile.master
├── Caddyfile
└── README.md
```

---

## API endpoints WebApi

Базовый URL: `https://clientix-assist.ru/api`

### `/auth`
- `POST /auth/telegram` — авторизация через Telegram Login Widget
- `POST /auth/generate-web-token` — генерация короткого JWT для deep-link (требует `X-Internal-Secret`, вызывается из бота)
- `POST /auth/exchange` — обмен короткого токена на долгий (30 дней)

### `/master` (требует JWT)
- `GET /master/me` — профиль текущего мастера
- `GET /master/stats` — статистика за сегодня / 7 дней / 30 дней
- `GET /master/stats/daily` — массив `{date, count}` за 30 дней для графика

### `/admin` (требует JWT + policy AdminOnly)
- `GET /admin/dashboard` — общая статистика платформы
- `GET /admin/masters` — список всех мастеров с базовой инфой

---

## Юридическая информация

ClientiX — коммерческий сервис самозанятого **Снопова Даниила Руслановича**. Сервис принимает реальные платежи через ЮKassa, обрабатывает персональные данные в соответствии с 152-ФЗ.

- [Политика конфиденциальности](https://clientix-assist.ru/privacy)
- [Публичная оферта](https://clientix-assist.ru/offer)
- [Согласие на обработку ПД](https://clientix-assist.ru/consent)

Все данные хранятся на серверах в РФ (Yandex Cloud, г. Москва).

---

## Roadmap

### Реализовано (MVP + Production)
- [x] Полный цикл записи (создание/перенос/отмена/добавление-удаление услуг)
- [x] Календари с раскраской
- [x] Мульти-таймзоны (11 поясов РФ)
- [x] Напоминания клиентам
- [x] ЮKassa в проде (с боевыми платежами)
- [x] Rate-limit и базовая безопасность
- [x] Автоматические бэкапы PostgreSQL и конфигов
- [x] Лендинг с premium luxury-дизайном
- [x] Веб-кабинет мастера с реальными метриками и графиками
- [x] Админ-панель платформы
- [x] Deep-link авторизация через бот
- [x] HTTPS + HSTS + security headers
- [x] Watchdog с Telegram-алертами
- [x] Изоляция сервисов (только 22, 80, 443 наружу)
- [x] 22 unit-теста на критичные сервисы
- [x] Правовые документы (политика, оферта, согласие)

### Планируется
- [ ] Рефакторинг на полноценные микросервисы (CQRS + RabbitMQ + MediatR)
- [ ] Веб-аналитика для мастеров (расширенная)
- [ ] Чат «клиент ↔ мастер» через бот
- [ ] Отзывы клиентов после визита
- [ ] Реферальная программа для клиентов

---

GitHub: [@ucsmdeeeee](https://github.com/ucsmdeeeee)
Telegram: [@ucsmdeeeee](https://t.me/ucsmdeeeee)

---

## Лицензия

Проприетарный коммерческий проект. Все права защищены © 2026.
