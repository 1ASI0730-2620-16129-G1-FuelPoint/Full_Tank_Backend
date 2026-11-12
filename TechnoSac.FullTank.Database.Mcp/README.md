# FullTank Database MCP

Servidor STDIO local para la base PostgreSQL `fulltank_platform`, esquema `public`.
Es una herramienta de desarrollo y no forma parte de la API ni de su imagen Docker.
No permite ejecutar SQL arbitrario.

## Herramientas

- `db_list_tables`, `db_describe_table`, `db_count_rows`, `db_select_rows`.
- `db_insert_row`, `db_update_rows`, `db_delete_rows`.

Las lecturas ocultan columnas sensibles. Las escrituras requieren una lista explícita
de tablas permitidas. Actualizar o eliminar exige filtros no vacíos y un número exacto
`expectedRows`; la operación usa una transacción serializable y bloquea las filas.
Eliminar y escribir columnas sensibles están desactivados por defecto.

## Configuración privada

Definir `FULLTANK_MCP_DB_USER` y `FULLTANK_MCP_DB_PASSWORD` en el entorno del proceso.
Usar una cuenta PostgreSQL dedicada, sin privilegios de superusuario, con permiso
`USAGE` del esquema y únicamente los permisos necesarios sobre tablas y secuencias.
No utilizar las credenciales administrativas de la aplicación.

| Variable | Predeterminado | Uso |
|---|---|---|
| `FULLTANK_MCP_DB_HOST` | `127.0.0.1` | Host PostgreSQL |
| `FULLTANK_MCP_DB_PORT` | `5432` | Puerto |
| `FULLTANK_MCP_DB_NAME` | `fulltank_platform` | Base |
| `FULLTANK_MCP_DB_SSL_MODE` | `Require` | TLS; usar `Disable` solo en desarrollo local sin TLS |
| `FULLTANK_MCP_CONNECTION_STRING` | Sin definir | Conexión completa en formato Npgsql |
| `FULLTANK_MCP_WRITE_TABLES` | Vacío | Tablas de escritura, separadas por comas |
| `FULLTANK_MCP_MAX_ROWS` | `25` | Límite máximo, acotado a 100 |
| `FULLTANK_MCP_ALLOW_DELETE` | `false` | Habilita eliminación |
| `FULLTANK_MCP_ALLOW_SENSITIVE_WRITES` | `false` | Habilita escrituras sensibles |

```bash
dotnet build TechnoSac.FullTank.Database.Mcp
dotnet run --no-build --project TechnoSac.FullTank.Database.Mcp
```

El cliente MCP debe arrancar ese proceso con transporte STDIO y las variables de
entorno configuradas. No publicar contraseñas ni archivos de configuración del cliente.
