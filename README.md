# FullTank Backend

REST API académica para compra, venta y entrega de combustible. Implementada con
ASP.NET Core / C# en .NET 10, Entity Framework Core 10, Npgsql y PostgreSQL 18.
El frontend Vue consume `/api/v1`; OpenAPI está disponible en `/swagger`.

## Módulos

- Catalog: productos de proveedores, favoritos y valoraciones.
- Inventory: existencias y movimientos.
- Ordering: solicitudes y órdenes.
- Equipment: equipos y recargas.
- Fulfillment: conductores, vehículos y entregas.
- Payment: checkout simulado y comprobantes académicos; no procesa pagos reales.
- Notification: notificaciones de la aplicación.
- ReportingAndAnalytics: indicadores de compras y ventas.
- Iam: registro, autenticación JWT y autorización por roles.

Cada bounded context separa dominio, aplicación, infraestructura e interfaces.
La persistencia se comparte mediante `AppDbContext`; el interceptor de auditoría
almacena marcas de tiempo UTC. El proyecto `TechnoSac.FullTank.Database.Mcp` es una
herramienta auxiliar local y no se incluye en la imagen de la API.

## Ejecutar localmente

Requisitos: .NET SDK 10 y Docker con Compose.

1. Establecer `POSTGRES_PASSWORD` como variable privada local. No guardar su valor
   en archivos versionados.
2. Ejecutar `docker compose up -d postgres` y esperar al estado saludable.
3. Crear `TechnoSac.FullTank.Platform/appsettings.Local.json` (ignorado por Git):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=127.0.0.1;Port=5432;Database=fulltank_platform;Username=fulltank;Password=<contraseña-local>;SSL Mode=Disable"
  },
  "Database": { "ApplyMigrationsOnStartup": true, "SeedDemoData": true },
  "TokenSettings": { "Secret": "<clave-local-aleatoria-de-al-menos-32-caracteres>" }
}
```

4. Ejecutar `dotnet run --project TechnoSac.FullTank.Platform` en Development.
   El perfil del proyecto publica la API en `http://localhost:5204`.
5. Consultar `http://localhost:5204/swagger`. Las cuentas demo locales son
   `logistics@transportesdelsur.com` (comprador) y `dispatch@petroandes.com`
   (proveedor), con contraseña `123456` si no se configura `Database:DemoPassword`.
   Production requiere una contraseña demo privada de al menos 12 caracteres.

No usar `docker compose down -v` si se desea conservar el volumen PostgreSQL.
Las variables de entorno tienen prioridad sobre los archivos de configuración.

## Pruebas y migraciones

```bash
dotnet build full-tank-platform.sln --configuration Release
dotnet test full-tank-platform.sln --configuration Release
dotnet tool restore
dotnet ef migrations has-pending-model-changes --project TechnoSac.FullTank.Platform
docker build -t fulltank-av2-api:local .
python3 deploy/smoke_render.py
```

La migración inicial activa es PostgreSQL y crea todo el esquema. El historial
MySQL anterior está archivado en `deploy/legacy-mysql-migrations`. Esta migración
no importa ni borra datos de una base MySQL existente.

## Render Free

[Configuración y pasos de despliegue](docs/RENDER_AV2.md).
[Resultados de validación](docs/VALIDACION_RENDER_AV2.md).
El Blueprint define API y PostgreSQL en Free, conectadas por la red interna.
La base gratuita vence a los 30 días. Las credenciales de DB, JWT y cuentas demo
se mantienen en las variables privadas de Render.

El frontend debe compilarse con la URL pública real y `VITE_USE_FAKE_API=false`.
La compilación `build:demo` usa datos simulados en memoria.
