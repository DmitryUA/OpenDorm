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

## 🖼️ Основные разделы

<table>
  <tr>
    <td align="center"><b>Главная панель</b></td>
    <td align="center"><b>Список общежитий</b></td>
  </tr>
  <tr>
    <td><img src="images/Главная%20страница%20-%20менеджер.png" alt="Главная панель менеджера"></td>
    <td><img src="images/Страница%20общежитий%20-%20менеджер.png" alt="Список общежитий менеджера"></td>
  </tr>
</table>

<table>
  <tr>
    <td align="center"><b>Карточка общежития и комнаты</b></td>
  </tr>
  <tr>
    <td><img src="images/Страница%20общежития%20-%20менеджер.png" alt="Страница общежития менеджера"></td>
  </tr>
</table>

<table>
  <tr>
    <td align="center"><b>Страница комнаты</b></td>
    <td align="center"><b>Страница пользователей</b></td>
  </tr>
  <tr>
    <td><img src="images/Страница%20комнаты%20-%20менеджер.png" alt="Страница комнаты менеджера"></td>
    <td><img src="images/Страница%20жильцов%20-%20менеджер.png" alt="Страница пользователей менеджера"></td>
  </tr>
</table>

### Приложение в Docker

Для запуска требуются Docker Engine и Docker Compose plugin. На сервере выполните:

```bash
cp .env.example .env
./scripts/create-secrets.sh
./scripts/deploy.sh
```

Подробные инструкции по настройке сервера, HTTPS, учётным записям администратора и резервному копированию приведены в **[руководстве администратора](docs/Руководство%20администратора.odt)**.

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

## 📖 Руководство пользователя

**[Скачать руководство пользователя](docs/Руководство%20пользователя.odt)** — описание разделов системы и работы менеджера.

**[Скачать руководство администратора](docs/Руководство%20администратора.odt)** — инструкции по развёртыванию приложения, управлению секретами, резервному копированию и восстановлению базы данных.

## Контакты

- **Email:** dreamysnail0921@gmail.com
