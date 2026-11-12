# FullTank Database MCP

Local STDIO MCP server for the `fulltank_platform` MySQL database. It intentionally does not expose arbitrary SQL.

## Tools

- `db_list_tables`, `db_describe_table`, `db_count_rows`, `db_select_rows`
- `db_insert_row`, `db_update_rows`, `db_delete_rows`

Updates and deletes require a non-empty equality filter and an exact `expectedRows` value. Delete is disabled by default.

## Required environment variables

Set these variables for the Windows user that starts Codex/ChatGPT Desktop, and restart the app afterward:

```powershell
[Environment]::SetEnvironmentVariable("FULLTANK_MCP_DB_USER", "YOUR_MCP_USER", "User")
[Environment]::SetEnvironmentVariable("FULLTANK_MCP_DB_PASSWORD", "YOUR_MCP_PASSWORD", "User")
[Environment]::SetEnvironmentVariable("FULLTANK_MCP_WRITE_TABLES", "provider_products,equipment", "User")
```

Optional variables:

| Variable | Default | Purpose |
|---|---:|---|
| `FULLTANK_MCP_DB_HOST` | `127.0.0.1` | MySQL host |
| `FULLTANK_MCP_DB_PORT` | `3306` | MySQL port |
| `FULLTANK_MCP_DB_NAME` | `fulltank_platform` | Database |
| `FULLTANK_MCP_DB_SSL_MODE` | `Disabled` | MySQL TLS mode; local loopback connections also enable RSA public-key retrieval |
| `FULLTANK_MCP_MAX_ROWS` | `25` | Maximum rows per read or mutation, capped at 100 |
| `FULLTANK_MCP_ALLOW_DELETE` | `false` | Enables `db_delete_rows` |
| `FULLTANK_MCP_ALLOW_SENSITIVE_WRITES` | `false` | Enables writes to password/token/secret-like columns |
| `FULLTANK_MCP_CONNECTION_STRING` | unset | Complete connection string; replaces the individual connection variables |

Do not use the application's `root` connection for the MCP. Use the dedicated account with only `SELECT`, `INSERT`, `UPDATE`, and—if needed—`DELETE` on `fulltank_platform`.

## Build

```powershell
& "C:\Program Files\dotnet\dotnet.exe" build .\TechnoSac.FullTank.Database.Mcp\TechnoSac.FullTank.Database.Mcp.csproj
```

The project-level `.codex/config.toml` starts the server with `dotnet run --no-build` and asks for approval on write tools. Build once before restarting Codex.

## OpenCode V2

The repository-level `opencode.jsonc` starts the same STDIO server. OpenCode inherits the `FULLTANK_MCP_*` variables from the Windows process, so restart OpenCode after setting or changing them.

From the backend root, verify the connection with:

```powershell
opencode mcp list
```

Inside OpenCode, use `/mcps` to inspect the server. The configuration exposes the tools directly and asks for confirmation before insert, update, or delete operations.
