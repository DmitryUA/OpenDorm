# OpenDorm

Система администрирования общежитий для образовательных организаций.

[![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![EF Core](https://img.shields.io/badge/EF_Core-10-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![Scalar](https://img.shields.io/badge/Scalar-API_Reference-1a1a1a?logo=scalar&logoColor=white)](https://scalar.com)
[![Vue.js](https://img.shields.io/badge/Vue.js-3-4FC08D?logo=vuedotjs&logoColor=white)](https://vuejs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-6-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Vite](https://img.shields.io/badge/Vite-8-646CFF?logo=vite&logoColor=white)](https://vite.dev/)
[![Pinia](https://img.shields.io/badge/Pinia-State_Management-FADA5E?logo=pinia&logoColor=black)](https://pinia.vuejs.org/)
[![Vue Router](https://img.shields.io/badge/Vue_Router-5-4FC08D?logo=vuedotjs&logoColor=white)](https://router.vuejs.org/)
[![Element Plus](https://img.shields.io/badge/Element_Plus-UI-409EFF?logo=element&logoColor=white)](https://element-plus.org/)
[![Orval](https://img.shields.io/badge/Orval-OpenAPI_Client-6E56CF)](https://orval.dev/)

[![Architecture](https://img.shields.io/badge/Architecture-DDD%20%7C%20Clean%20%7C%20Vertical%20Slice-007ACC)]()
[![Pattern](https://img.shields.io/badge/Pattern-CQRS-007ACC)]()
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Status](https://img.shields.io/badge/Status-WIP-orange)]()

[![xUnit](https://img.shields.io/badge/Tests-xUnit-239120?logo=dotnet&logoColor=white)](https://xunit.net/)
[![Testcontainers](https://img.shields.io/badge/Testcontainers-PostgreSQL-239120)](https://testcontainers.com/)

Проект находится в активной разработке.

**Frontend:** клиентское SPA на Vue 3 и TypeScript находится в каталоге `client/`. API-клиент и типы генерируются из OpenAPI-спецификации с помощью Orval.

**Backend:** серверная часть на .NET 10 и PostgreSQL находится в каталоге `server/`.

### Приложение в Docker

```bash
cp .env.example .env
# Создайте секреты по инструкции ниже
docker compose up -d --build
docker compose ps
```

Создайте локальную директорию с файлами секретов (она добавлена в `.gitignore`). Например, на Linux-сервере:

```bash
mkdir -p secrets
openssl rand -hex 32 > secrets/postgres_password
openssl rand -base64 32 > secrets/search_index_hmac_key
openssl rand -base64 48 > secrets/jwt_signing_key
openssl rand -base64 32 > secrets/encryption_key
cat secrets/postgres_password | awk '{ print "Host=db;Port=5432;Database=open_dorm_db;Username=open_dorm;Password=" $0 }' \
  > secrets/open_dorm_connection_string
chmod 0700 secrets
chmod 0444 secrets/*
```

Файлы соответствуют конфигурации `SearchIndex:HmacKey`, `Jwt:SigningKey`, `Encryption:Key` и `ConnectionStrings:OpenDormConnectionString`. Контейнер API читает их из Docker secrets; пароль PostgreSQL передаётся образу базы отдельно. В Linux Compose монтирует локальные secret-файлы с исходными правами, поэтому файлы доступны на чтение внутри контейнеров, а доступ на хосте ограничен правами `0700` каталога. При изменении имени пользователя или базы в `.env` обновите строку подключения в `secrets/open_dorm_connection_string`.

`SearchIndex:HmacKey` должен декодироваться из Base64 как минимум в 32 байта и отличаться от ключа шифрования. Его смена делает сохранённые индексы недействительными: очистите значения `last_name_search_hash`, `first_name_search_hash` и `patronymic_search_hash`, после чего API заполнит их повторно. `Encryption:Key` должен декодироваться ровно в 32 байта для AES-256; его нельзя менять после появления зашифрованных данных. `Jwt:SigningKey` должен содержать не менее 32 случайных байт. При старте API автоматически применяет миграции, инициализирует индексы и начальные данные.

Frontend раздаётся Nginx на HTTP-порту `5140`; `/api` проксируется к backend внутри Docker-сети. Backend напрямую наружу не опубликован. База доступна только локально на `5433`. Порты можно изменить через `APP_HTTP_PORT` и `POSTGRES_PORT` в `.env`. HTTP не шифрует трафик: не передавайте через него реальные учётные данные или персональные данные из интернета. Перед публичным использованием настройте HTTPS на порту `7170` через reverse proxy и перенаправление HTTP на HTTPS. `docker compose down` останавливает контейнеры, но сохраняет данные в именованном volume; не используйте `docker compose down -v`, если хотите сохранить базу.

Локальные Docker Compose secrets — read-only файлы, смонтированные в контейнер, а не зашифрованное централизованное хранилище. Для одного сервера это проще и безопаснее, чем переменные окружения (они видны в метаданных контейнера); ограничьте доступ к каталогу `secrets/` и защитите резервные копии шифрованием. Для более сложной инфраструктуры рассмотрите Vault или системное хранилище секретов ОС. Для локальной разработки используйте .NET User Secrets.

## 📚 Документация API

🌐 **[Открыть интерактивную документацию API (Scalar)](https://DmitryUA.github.io/OpenDorm/)**

Онлайн-документация содержит полное описание всех эндпоинтов, схем запросов/ответов и примеров JSON.

<details>
<summary><b>💻 Локальный запуск (для разработчиков)</b></summary>
<br>
После запуска проекта (<code>OpenDorm.API</code>) в режиме <code>Development</code> интерактивная документация также доступна в браузере по адресам:

- **HTTP:** [http://localhost:5140/scalar](http://localhost:5140/scalar)
- **HTTPS:** [https://localhost:7170/scalar](https://localhost:7170/scalar)
</details>

## Контакты

- **Email:** dreamysnail0921@gmail.com
