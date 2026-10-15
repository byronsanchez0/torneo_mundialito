# INFATLAN Mundialito

Base inicial del sistema del Mundialito, organizada con Clean Architecture.

## Estructura

```text
src/
├── backend/
│   ├── Mundialito.Domain/          Entidades y reglas de negocio
│   ├── Mundialito.Application/     Contratos de la aplicación
│   ├── Mundialito.Infrastructure/  PostgreSQL, EF Core y repositorios
│   └── Mundialito.Api/             Configuración y endpoints HTTP
└── web/                             Frontend Next.js
```

Dependencias del backend:

```text
Domain
  ↑
Application
  ↑
Infrastructure
  ↑
Api

Web ──HTTP──> Api
```

`Goal` pertenece al agregado `Match`. Por eso solo existen contratos de
repositorio para `Team`, `Player` y `Match`.

El diagrama completo, incluyendo los flujos CQRS de lectura y escritura, está en
[docs/architecture.md](docs/architecture.md).

## API

Las operaciones de escritura pasan por un Command Handler y confirman los
cambios mediante un único `IUnitOfWork`. Los repositorios no guardan cambios
directamente. Las lecturas usan exclusivamente Dapper, devuelven DTOs y se
paginan desde PostgreSQL.

| Método | Ruta |
| --- | --- |
| `GET` | `/api/teams?pageNumber=1&pageSize=10` |
| `GET` | `/api/teams/{id}` |
| `POST` | `/api/teams` |
| `PUT` | `/api/teams/{id}` |
| `DELETE` | `/api/teams/{id}` |
| `GET` | `/api/players?pageNumber=1&pageSize=10&teamId={teamId}` |
| `GET` | `/api/players/{id}` |
| `POST` | `/api/players` |
| `PUT` | `/api/players/{id}` |
| `DELETE` | `/api/players/{id}` |
| `GET` | `/api/matches?pageNumber=1&pageSize=10` |
| `GET` | `/api/matches/{id}` |
| `POST` | `/api/matches` |
| `PUT` | `/api/matches/{id}` |
| `DELETE` | `/api/matches/{id}` |
| `POST` | `/api/matches/{matchId}/goals` |
| `GET` | `/api/statistics` |

### Paginación y filtros

Los GET de colección aceptan `pageNumber`, `pageSize`, `sortBy` y
`sortDirection` (`asc` o `desc`). El tamaño máximo de página es 100.

- Equipos: `name`, `coach`.
- Jugadores: `teamId`, `name`, `position`, `shirtNumber`.
- Partidos: `teamId`, `status`, `fromDate`, `toDate`.

La respuesta contiene `data`, `pageNumber`, `pageSize`, `totalRecords` y
`totalPages`. Los filtros, conteo, ordenamiento y paginación se ejecutan en
PostgreSQL mediante Dapper.

### Idempotencia

Todos los endpoints `POST` requieren el header `Idempotency-Key`:

```text
Idempotency-Key: 0c5eccee-1f08-4c37-9bee-bf612b87701f
```

- La primera solicitud crea el recurso y responde `201 Created`.
- Repetir la misma key con el mismo body devuelve la respuesta original sin
  crear otro registro.
- Reutilizar la key con un body diferente devuelve `409 Conflict`.
- Omitir el header devuelve `400 Bad Request`.
- El recurso y la key se guardan dentro de la misma transacción del Unit of
  Work.

Los demás resultados HTTP se mapean como `200 OK`, `400 Bad Request`,
`404 Not Found` o `409 Conflict`, según el resultado del caso de uso.

## Colección Postman

La carpeta [`postman`](postman) contiene:

- `Mundialito.postman_collection.json`: endpoints organizados y pruebas de CRUD,
  idempotencia, filtros, paginación y errores.
- `Mundialito.local.postman_environment.json`: variables para la API local de
  Docker y los identificadores generados durante la ejecución.

Para ejecutarla:

1. Inicia el proyecto con Docker y espera a que `/health` responda.
2. Importa ambos archivos en Postman.
3. Selecciona el entorno **Mundialito - Local Docker**.
4. Ejecuta la colección completa en orden desde el Collection Runner.

La primera carpeta inicializa claves únicas y obtiene los identificadores del
seed. Las pruebas crean y eliminan sus propios equipos, jugadores y partidos.
La base debe contener el seed actual para encontrar `Liverpool` y `Barcelona`.
Si se ejecuta una petición individual, ejecuta antes la carpeta
`00 - Preparación`.

## Base de datos

La API espera PostgreSQL en `localhost:5455`. La cadena predeterminada es solo
para desarrollo:

```text
Host=localhost;Port=5455;Database=mundialito;Username=postgres;Password=postgres
```

Puede reemplazarse con la variable:

```powershell
$env:ConnectionStrings__PostgreSql = 'Host=localhost;Port=5455;Database=mundialito;Username=postgres;Password=tu_clave'
```

## Ejecución

Configurar primero los directorios de trabajo en `E:`:

```powershell
$env:TEMP = 'E:\INFATLA_MUNDIALITO\.tmp'
$env:TMP = 'E:\INFATLA_MUNDIALITO\.tmp'
$env:NUGET_PACKAGES = 'E:\INFATLA_MUNDIALITO\.packages\nuget'
$env:DOTNET_CLI_HOME = 'E:\INFATLA_MUNDIALITO\.cache\dotnet'
$env:NPM_CONFIG_CACHE = 'E:\INFATLA_MUNDIALITO\.cache\npm'
```

Backend:

```powershell
dotnet restore .\Mundialito.slnx
dotnet run --project .\src\backend\Mundialito.Api
```

Frontend, en otra terminal:

```powershell
Set-Location .\src\web
npm install
npm run dev
```

- Frontend: `http://localhost:3001`
- API: `http://localhost:8081`
- Salud de la API: `http://localhost:8081/health`

El frontend ofrece vistas de equipos y partidos con filtros y paginación,
registro de goles, tabla de posiciones y ranking de goleadores. Toda la
comunicación con el backend se realiza mediante HTTP a través de un cliente API
centralizado.

## Docker

Antes de iniciar, comprueba que el almacenamiento de Docker Desktop esté ubicado
en la unidad `E:`. Compose guarda los datos de PostgreSQL en
`.docker/postgres-data`, pero la ubicación de imágenes y cachés depende de la
configuración global de Docker Desktop.

Desde la raíz del proyecto ejecuta:

```powershell
docker-compose up
```

El comando construye e inicia PostgreSQL, la API y el frontend. En este entorno
Docker, la API crea el esquema inicial de la base de datos si todavía no existe.
Si la tabla de equipos está vacía, también agrega automáticamente 4 equipos,
5 jugadores por equipo y 6 partidos, de los cuales 3 incluyen resultados.

