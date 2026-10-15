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

