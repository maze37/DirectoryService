# DirectoryService

Сервис организационного справочника платформы: подразделения, локации, должности
и связи между ними. Хранит структуру организации, поддерживает поиск и изменение
иерархии, мягкое удаление и восстановление. Для фотографий использует FileService,
а готовность ассетов отслеживает по событиям RabbitMQ.

## Цели проекта

- Создать единый источник данных об организационной структуре.
- Управлять подразделениями, их иерархией, локациями и должностями с доменными проверками.
- Предоставить API для списков, поиска, дерева, предков и дочерних подразделений.
- Поддержать безопасное изменение связанных данных через транзакции и контроль конкуренции.
- Отделить хранение файлов от справочника: сохранять ID фотографии, а не её содержимое.
- Принимать события FileService и проверять готовность файлов по локальной проекции.
- Отработать разделение слоёв, CQRS-подход, интеграционные тесты и идемпотентных consumers.

DirectoryService владеет локациями и их привязками к фотографиям. FileService
владеет ассетами и содержимым. Таблица `asset_states` — локальная проекция событий,
которая позволяет принимать решения без HTTP-проверки готовности при каждом прикреплении.

## Навигация

- [Архитектура](#архитектура)
- [GitHub-токен](#github-токен-и-приватные-nuget-пакеты)
- [Запуск](#запуск)
- [API](#api)
- [Интеграция с FileService](#интеграция-с-fileservice)
- [Тесты](#тесты)
- [Диагностика](#диагностика)

## Архитектура

Команды в этом README выполняются из каталога `backend` репозитория DirectoryService.

| Проект | Назначение |
| --- | --- |
| `DirectoryService.Presentation` | ASP.NET Core API, Swagger, middleware, композиция приложения |
| `DirectoryService.Application` | Команды, запросы, валидация, интерфейсы, read models, обработчики событий |
| `DirectoryService.Domain` | Подразделения, локации, должности, связи и бизнес-правила |
| `DirectoryService.Infrastructure` | EF Core/PostgreSQL, репозитории, миграции, фоновые задачи |
| `DirectoryService.Contracts` | DTO HTTP API |
| `tests/DirectoryService.IntegrationTests` | Интеграционные сценарии |

Стек: .NET 10, ASP.NET Core, EF Core/Npgsql, Dapper, PostgreSQL с `ltree`,
Redis/HybridCache, Wolverine, RabbitMQ, Serilog/Seq, xUnit, Testcontainers и Respawn.
Версии пакетов задаются централизованно в `Directory.Packages.props`.

### Основные данные

- **Department** — подразделение в иерархии организации.
- **Location** — локация с адресом, часовым поясом и ссылкой на фотографию.
- **Position** — должность.
- **DepartmentLocation / DepartmentPosition** — связи подразделений с локациями и должностями.
- **AssetState** — проекция внешнего события, находится в Application и хранится в PostgreSQL.

Иерархия подразделений использует PostgreSQL `ltree`. Домен содержит правила
изменения сущностей, инфраструктура отвечает за их сохранение.
Для подразделений, локаций и должностей предусмотрены мягкое удаление,
восстановление и фоновые задачи окончательной очистки.

В development-настройках очистка включена: интервал 15 секунд, период хранения
удалённых записей 2 минуты, размер пачки 200. Эти короткие значения предназначены
для локальной проверки; после окончательной очистки восстановить запись нельзя.
Настройки находятся в секциях `Cleanup:Departments`, `Cleanup:Locations`, `Cleanup:Positions`.

```mermaid
flowchart LR
    Client[Клиент] --> API[DirectoryService API]
    API --> DB[(PostgreSQL)]
    API --> Cache[Redis / HybridCache]
    FS[FileService] --> MQ[RabbitMQ file-events]
    MQ --> Queue[directory.asset-events]
    Queue --> Handlers[AssetReady / AssetDeleted handlers]
    Handlers --> State[(asset_states)]
    API -->|Проверка фотографии| State
```

## GitHub-токен и приватные NuGet-пакеты

Оба сервиса используют пакеты `Mazeland.*` из GitHub Packages. Без доступа к ним
`dotnet restore`, сборка Docker-образа и запуск тестов могут завершиться ошибкой.
Источник `mazeland-private` уже задан в [nuget.config](nuget.config):

```text
https://nuget.pkg.github.com/maze37/index.json
```

Для локальной разработки создайте **Personal access token (classic)** с правом
`read:packages`. Владелец токена также должен иметь доступ к приватным пакетам;
само наличие токена этот доступ не выдаёт. Для скачивания не нужны права публикации
или удаления пакетов. См. [документацию GitHub Packages](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-nuget-registry).

### Для Docker Compose

Создайте `.env` рядом с `docker-compose.yml` в каждом проекте:

```dotenv
GITHUB_TOKEN=your_personal_access_token
```

Подставьте собственный токен вместо примера. `.env` исключён из Git.
Compose подставляет переменную в `build.args.GITHUB_TOKEN`; Dockerfile использует
её при восстановлении зависимостей. Токен нужен на этапе сборки.

### Для dotnet CLI и IDE

Compose читает `.env` автоматически, а `dotnet restore` — нет. Перед запуском CLI
передайте токен в окружение текущего терминала. Если `.env` создан вами и содержит
обычные доверенные присваивания переменных:

```sh
set -a
. ./.env
set +a
dotnet restore
```

В `nuget.config` пароль задан как `%GITHUB_TOKEN%`: NuGet подставляет значение
переменной окружения. Для IDE задайте ту же переменную в окружении процесса,
выполняющего restore; настройка только профиля запуска приложения может не влиять
на восстановление пакетов.

В текущем конфиге указан username `maze37`. При использовании другого GitHub-аккаунта
согласуйте username с владельцем токена; в Dockerfile DirectoryService username
также задан явно. Не меняйте адрес feed на свой username: пакеты размещены у `maze37`.

Не записывайте настоящий токен в README, исходники или коммиты. Текущие Dockerfile
передают секрет через build argument, а DirectoryService записывает credentials
на этапе сборки. Для CI и распространяемых build cache стоит перейти на BuildKit
secrets. Имя переменной `GITHUB_TOKEN` в локальном проекте обозначает ваш PAT;
автоматический токен GitHub Actions — отдельный механизм с собственными правами.

При `401/403` проверьте срок действия токена, `read:packages`, доступ аккаунта к
пакету и наличие переменной именно в процессе restore/build.

## Запуск

### Требования и адреса

Нужны Docker с Compose v2, .NET SDK 10 и доступ к приватным NuGet-пакетам.
DirectoryService использует RabbitMQ из compose FileService.

| Компонент | Адрес с компьютера | Адрес внутри Docker |
| --- | --- | --- |
| API / Swagger | `http://localhost:8002/swagger/index.html` | `http://directory-service:8002` |
| PostgreSQL | `localhost:5434` | `postgres:5432` в сети DirectoryService |
| Redis | `localhost:6380` | `redis:6379` |
| Seq UI и приём логов | `http://localhost:8081` | `http://seq:80` |
| FileService API | `http://localhost:8003` | `http://file-service:8003` |
| Общий RabbitMQ | `localhost:5672` | `platform-rabbitmq:5672` |
| RabbitMQ UI | `http://localhost:15672` | — |

PostgreSQL в локальном compose: `postgres / 1234`, база `directory_service_db`.
RabbitMQ в FileService: `guest / guest`. Конфигурация рассчитана на разработку.

### Порядок запуска двух сервисов

1. Подготовьте `.env` с GitHub-токеном в обоих проектах.
2. Создайте общую сеть один раз: `docker network create shared-network`.
3. Из каталога FileService запустите инфраструктуру:

```sh
docker compose up -d postgres rabbitmq minio redis seq
```

4. Из `DirectoryService/backend` запустите свою инфраструктуру:

```sh
docker compose up -d postgres redis seq
```

5. Подготовьте базы данных обоих сервисов, как описано ниже и в README FileService.
6. Запустите приложения из Docker либо IDE.
7. До первой публикации убедитесь, что DirectoryService создал очередь и binding.

Общая сеть объявлена external в обоих compose. В DirectoryService нет отдельного
RabbitMQ: оба приложения должны видеть один брокер и один virtual host.
`depends_on` не связывает два независимых compose, поэтому готовность общего
RabbitMQ проверяется отдельно.

### Локальные настройки

Загрузите `GITHUB_TOKEN` в окружение, затем:

```sh
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__Database='Host=localhost;Port=5434;Database=directory_service_db;Username=postgres;Password=1234'
export ConnectionStrings__RabbitMq='amqp://guest:guest@localhost:5672'
export ConnectionStrings__Redis='localhost:6380'
export Serilog__WriteTo__1__Args__serverUrl='http://localhost:8081'
export FileServiceOptions__Url='http://localhost:8003'
dotnet restore
```

Текущий ключ БД — **`Database`**, а не `DirectoryServiceDb`. В development-конфиге
остались старые значения; приведённые переменные согласованы с compose.
В Docker используйте внутренние имена сервисов, которые уже заданы в compose.

### Миграции и известное ограничение

Приложение не вызывает миграции бизнес-таблиц автоматически. Для обновления
существующей базы используйте `dotnet-ef` 10.x:

```sh
dotnet ef database update \
  --project src/DirectoryService.Infrastructure \
  --startup-project src/DirectoryService.Presentation
```

**Известная проблема текущей истории миграций:** применение всей цепочки на пустой
БД завершается ошибкой `relation "departments" does not exist`. Это было обнаружено
при подготовке тестов. Для чистого развёртывания необходимо исправить/согласовать
начальную историю миграций либо использовать подготовленную базу с корректной
историей. Команда выше не является гарантированным bootstrap пустой базы.
Не удаляйте рабочие данные, чтобы обходить эту проблему.

Миграция `AddAssetStates` создаёт `public.asset_states`. Тесты обработчиков,
как существующая тестовая фабрика проекта, используют `EnsureCreated` в отдельной
одноразовой БД. Этот подход не проверяет историю миграций и не заменяет её
для обычного окружения.

### Docker

После подготовки базы:

```sh
docker compose up -d --build directory-service
docker compose logs -f directory-service
```

Откройте Swagger по адресу `http://localhost:8002/swagger/index.html`.
Для проверки состояния: `docker compose ps`.

### IDE / CLI

```sh
dotnet run --project src/DirectoryService.Presentation --no-launch-profile --urls http://localhost:8002
```

Инфраструктура остаётся в Docker. Контейнер API на том же порту следует остановить:
`docker compose stop directory-service`.

## API

Подробные модели запросов, параметры фильтрации и примеры доступны в Swagger.

| Область | Основные операции |
| --- | --- |
| `/api/departments` | Создание, список, получение по ID, удаление, восстановление |
| `/api/departments/{id}/parent` | Изменение родителя |
| `/api/departments/{id}/locations` | Изменение связанных локаций |
| `/api/departments/{deptId}/positions/{posId}` | Прикрепление и удаление связи с должностью |
| `/api/departments/tree` | Дерево подразделений |
| `/api/departments/tree/search` | Поиск по дереву |
| `/api/departments/{id}/children` | Дочерние подразделения |
| `/api/departments/{id}/ancestors` | Предки подразделения |
| `/api/locations` | Создание, список, получение, удаление, восстановление |
| `/api/locations/top` | Выборка топ-локаций |
| `/api/positions` | Создание, обновление, удаление, восстановление |

Фотографии локаций:

| Метод | Маршрут | Тело |
| --- | --- | --- |
| PUT | `/api/locations/{id}/attach-photo` | `{ "photoAssetId": "GUID" }` |
| PUT | `/api/locations/{id}/update-photo` | `{ "newPhotoAssetId": "GUID" }` |
| PUT | `/api/locations/{id}/remove-photo` | См. Swagger |

## Интеграция с FileService

### Кто за что отвечает

FileService публикует `AssetReady` после полной загрузки и `AssetDeleted` после
логического удаления. Контракты поступают из NuGet-пакета
`Mazeland.FileService.Shared.Messaging`, namespace `IntegrationEvents.Files.Events`.
HTTP-контракты подключены через `Mazeland.FileService.Contracts`.
Изменения контрактов в другом репозитории не попадут сюда автоматически: требуется
обновление используемого пакета и совместимость формата сообщений.

Конфигурация Wolverine находится в `Application/Messaging`:

- exchange `file-events`, тип `topic`, durable;
- очередь `directory.asset-events`, quorum;
- binding `asset.*.*`;
- durable inbox в PostgreSQL, схема `public`;
- обработчики `AssetReadyHandler` и `AssetDeletedHandler` в Application;
- SQL-реализация `IAssetStateRepository` в Infrastructure.

Binding получает все события с тремя сегментами `asset.<действие>.<контекст>`.
Несколько экземпляров DirectoryService, слушающих одну очередь, разделяют сообщения.
Другому независимому сервису для своей копии событий нужна собственная очередь.

### Локальная проекция asset_states

| Поле | Назначение |
| --- | --- |
| `asset_id` | Первичный ключ, ID ассета FileService |
| `entity_id` | ID владельца |
| `entity_type` | Контекст владельца, например `location` |
| `asset_type` | Назначение ассета |
| `status` | `Ready` или `Deleted` |
| `occurred_at` | Время события |

CHECK constraint ограничивает допустимые статусы. Первичный ключ `asset_id`
обеспечивает уникальность строки даже при конкурентной обработке.

| Текущее состояние | Пришёл Ready | Пришёл Deleted |
| --- | --- | --- |
| Строки нет | Создать Ready | Создать Deleted |
| Ready | Оставить без изменений | Перевести в Deleted |
| Deleted | Оставить Deleted | Оставить без изменений |

Репозиторий использует атомарные `INSERT ... ON CONFLICT` в PostgreSQL.
Повторный Ready не обновляет строку; повторный Deleted также ничего не меняет.
Поздний Ready не восстанавливает удалённый ассет. Для новой загрузки нужен новый AssetId.

Inbox Wolverine отслеживает обработку envelope. Бизнес-идемпотентность SQL нужна
дополнительно: один факт может прийти в другом envelope или повториться после сбоя
между изменением проекции и фиксацией статуса обработки. Создание нового envelope
не является проверкой дедупликации прежнего envelope по message ID.

Обработчики не вызывают ACK/NACK вручную. Исключение должно дойти до Wolverine,
чтобы механизм доставки мог применить политику обработки ошибок. Не следует
перехватывать ошибку БД и возвращать успех. При durable inbox подтверждение брокеру
и завершение бизнес-обработки — разные этапы, поэтому пустая RabbitMQ-очередь
сама по себе не доказывает обновление локации или проекции.

### Проверка фотографии

`AttachPhoto` и `UpdatePhoto` читают `asset_states` и проверяют:

1. Запись уже существует — готовность подтверждена событием.
2. Статус `Ready`, а не `Deleted`.
3. `entity_type` соответствует `location`.
4. `entity_id` совпадает с ID локации.

После этого ID файла записывается в локацию в транзакции. Событие само по себе
не выбирает фотографию локации: прикрепление — отдельная пользовательская операция.
Текущие handlers проверяют состояние и владельца; дополнительной проверки
`asset_type` как типа изображения в них нет.

Если запись ещё не пришла, возвращается `photo.asset.state_unknown`.
Это ожидаемое следствие асинхронной доставки: можно повторить действие позже.
`photo.asset.deleted` означает, что ассет уже удалён; повтор запроса его не восстановит.

Событие удаления меняет проекцию. Автоматическая очистка уже записанного
`PhotoAssetId` у локации отдельным обработчиком сейчас не выполняется.

### Проверка полного пути вручную

1. Запустите оба приложения и убедитесь, что очередь `directory.asset-events` создана.
2. Создайте локацию через Swagger DirectoryService и сохраните её ID.
3. В FileService начните загрузку изображения с `context = "location"`,
   `entityId = ID локации`, `assetType = "preview"`.
4. Выполните PUT содержимого по presigned URL и вызовите `complete`.
5. Проверьте строку `Ready` в `public.asset_states`.
6. Вызовите прикрепление:

```sh
curl -X PUT http://localhost:8002/api/locations/LOCATION_ID/attach-photo \
  -H 'Content-Type: application/json' \
  -d '{"photoAssetId":"ASSET_ID"}'
```

7. Удалите ассет через `DELETE http://localhost:8003/api/files/ASSET_ID`.
8. Дождитесь `Deleted` в проекции. Попытка прикрепить этот ассет должна вернуть
   конфликт `photo.asset.deleted`.

Проверка состояния в БД DirectoryService:

```sql
SELECT asset_id, entity_id, entity_type, asset_type, status, occurred_at
FROM public.asset_states
ORDER BY occurred_at DESC;

SELECT * FROM public.wolverine_incoming_envelopes LIMIT 20;
SELECT * FROM public.wolverine_dead_letters LIMIT 20;
```

Статус `Handled` означает, что Wolverine завершил обработку сообщения. Inbox —
служебное хранилище, а не постоянный журнал: не стройте аудит на вечном наличии этих строк.

## Тесты

Новые проверки consumer-логики:

```sh
dotnet test tests/DirectoryService.IntegrationTests/DirectoryService.IntegrationTests.csproj \
  --filter FullyQualifiedName~AssetStateHandlerTests
```

Они поднимают отдельный PostgreSQL, вызывают настоящие обработчики и репозиторий
и проверяют повтор Ready, повтор Deleted, Deleted до Ready и конкурентную обработку.
Последний проверенный запуск: **4 успешных теста**.
Это проверка идемпотентности бизнес-операций; RabbitMQ и inbox здесь не участвуют.

Полный существующий набор:

```sh
dotnet test tests/DirectoryService.IntegrationTests/DirectoryService.IntegrationTests.csproj
```

В нём также есть сценарии подразделений, локаций и должностей. Полный набор не
перезапускался при добавлении тестов событий. Его `DirectoryTestWebFactory` требует
дополнительной адаптации к Wolverine: сейчас она заменяет DbContext, но не изолирует
конфигурацию брокера и хранилища сообщений так, как фабрика FileService. Для безопасной
изолированной проверки новых обработчиков используйте фильтр выше.

## Диагностика

```sh
docker compose ps
docker compose logs --tail=150 directory-service postgres
```

| Симптом | Проверка |
| --- | --- |
| 401/403 при restore | PAT, права на пакеты, username и окружение сборки |
| Не разрешается platform-rabbitmq | Общая сеть, запущен ли RabbitMQ из FileService |
| Очередь не появилась | Успешен ли старт Wolverine, вызван ли AddWolverine, доступны ли PostgreSQL и RabbitMQ |
| Очередь пуста, но нет Ready | Логи обработчика, inbox/dead letters, правильная БД |
| `photo.asset.state_unknown` | Завершена ли загрузка, доставлено ли событие, совпадает ли AssetId |
| `photo.asset.owner_mismatch` | Контекст location и ID владельца из события |
| `photo.asset.deleted` | Терминальное состояние Deleted; нужен другой ассет |
| Нет логов в Seq | С компьютера `localhost:8081`, из контейнера `seq:80` |
| Миграция на пустой БД падает | Известная проблема истории миграций, см. раздел выше |

RabbitMQ UI: `http://localhost:15672`, Seq DirectoryService: `http://localhost:8081`.
При диагностике различайте outbox FileService и inbox DirectoryService: это разные БД.
RabbitMQ dead-letter очередь и `wolverine_dead_letters` в PostgreSQL — разные
хранилища ошибок; наличие одного не означает автоматического наличия записи в другом.

Остановка: `docker compose down`. Данные именованных volumes сохраняются.
Не добавляйте `-v`, если не намерены удалить локальные данные.

## Границы текущего сценария

Реализована реакция на готовность и удаление ассетов. Файлы остаются в FileService,
а URL содержимого при необходимости запрашивается синхронно. Проекция обновляется
асинхронно: краткая задержка между загрузкой и возможностью прикрепления допустима.
События завершения/ошибки обработки видео в согласованный объём не входят.
