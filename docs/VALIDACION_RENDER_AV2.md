# Validación de PostgreSQL y preparación para Render Free

Fecha: 12/11/2026, hora de Lima. Estas comprobaciones locales no acreditan todavía
un despliegue público en Render.

| Comprobación | Resultado |
|---|---|
| `dotnet build full-tank-platform.sln -c Release` | Correcta: API, pruebas y MCP; 7 advertencias CS9113 preexistentes |
| Pruebas automatizadas del backend | **89 aprobadas**, 0 fallidas |
| Proveedor de persistencia | Npgsql EF Core 10.0.2, EF Core 10.0.8, .NET 10 |
| Migración inicial PostgreSQL | Generada con EF CLI 10.0.8, historial MySQL archivado |
| `dotnet ef migrations has-pending-model-changes` | Sin cambios pendientes |
| `render.yaml` frente al esquema oficial vigente | Válido |
| Prueba MCP con PostgreSQL real | Metadata, identidad, UTC, redacción, insert/update/delete con filtros y rollback: PASS |
| Imagen `fulltank-av2-api:local` | Construcción correcta; proceso sin root |
| PostgreSQL real | PostgreSQL 18 en contenedor desechable, TLS activo, límite 256 MB |
| `python3 deploy/smoke_render.py` | **PASS** |

El smoke test comprobó conectividad TLS, readiness 503 antes de migrar y 200
después, migración desde una base vacía, seed transaccional, OpenAPI, CORS para
Firebase y rechazo de otro origen, protección JWT, acceso de comprador/proveedor,
pago simulado con comprobante y conservación del pago tras reiniciar API y
PostgreSQL. El reinicio no duplicó las tres órdenes de demostración.

Las nuevas pruebas de configuración comprueban URL Render `postgres://` y
`postgresql://`, caracteres reservados en credenciales, puerto predeterminado,
precedencia de configuración y rechazo de conexiones con fallback a texto plano.

El Blueprint usa exclusivamente API Free y PostgreSQL Free. La documentación
oficial vigente indica que la base gratuita tiene 1 GB, vence a los 30 días y no
incluye backups. La cuenta consultada no tenía otra instancia PostgreSQL.

No se eliminó ni importó la base MySQL local. La inicialización de PostgreSQL es
un esquema nuevo con datos académicos de muestra. Para conservar datos reales se
necesita una importación controlada, descrita en [RENDER_AV2.md](RENDER_AV2.md).
