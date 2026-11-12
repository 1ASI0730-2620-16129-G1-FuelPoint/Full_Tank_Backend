# FullTank AV2: Render Free y PostgreSQL

La API usa .NET 10, Entity Framework Core 10 y Npgsql con PostgreSQL 18.
El frontend sigue en Firebase Hosting. El pago es simulado y no procesa dinero real.

## Recursos

`render.yaml` define el proyecto `fulltank-av2`, entorno `av2`:

| Recurso | Plan | Región |
|---|---|---|
| `fulltank-av2-api` | Web Service Docker, **free** | Oregon |
| `fulltank-av2-postgres` | Render PostgreSQL, **free**, 1 GB | Oregon |

No se crean servicios privados MySQL ni discos de pago. La base gratuita de Render
**vence 30 días después de su creación**, no incluye backups y solo se admite una
base gratuita por workspace. La API se suspende tras 15 minutos sin tráfico y
puede tardar aproximadamente un minuto en arrancar con la primera petición.
Revisar las cuotas gratuitas de compilación, horas y transferencia en el Dashboard.

## Conexión y secretos

La API recibe la URL interna de PostgreSQL mediante `DATABASE_URL` y
`fromDatabase.connectionString`. El parser convierte esa URL al formato Npgsql,
decodifica usuario y contraseña, exige TLS en Production y limita el pool a 20.
La API y la base deben estar en el mismo workspace y región.
`ipAllowList: []` deshabilita acceso externo a la base; la API usa la red interna.

Render genera `TokenSettings__Secret` y `Database__DemoPassword` como valores
privados. La contraseña demo permite acceder a las cuentas del seed:
`logistics@transportesdelsur.com` (comprador) y `dispatch@petroandes.com` (proveedor).
Consultar la contraseña en las variables privadas del servicio y compartirla por un
canal privado con los evaluadores. No guardarla en GitHub ni en el frontend.

También se admiten `ConnectionStrings__DefaultConnection` en formato Npgsql o URL
PostgreSQL, y las variables separadas `Database__Host`, `Database__Port` (5432),
`Database__Name`, `Database__User`, `Database__Password`, `Database__SslMode`.
La precedencia es conexión completa, `DATABASE_URL`, variables separadas.

## Despliegue desde GitHub

1. Ejecutar pruebas, construir la imagen y ejecutar `python3 deploy/smoke_render.py`.
2. Integrar por GitFlow la rama de trabajo en `develop`, crear una release y llevar
   la versión validada a `main`. Conservar los commits originales.
3. Validar el YAML con `render blueprints validate render.yaml` si está disponible
   el CLI; la validación con el esquema JSON comprueba solamente su estructura.
4. Crear o volver a planificar el Blueprint desde el repositorio backend y `main`.
   Revisar que **API y PostgreSQL muestran Free** antes de aplicar el plan.
5. La API aplica la migración PostgreSQL y después el seed académico en una
   transacción. Un fallo de inicialización impide que se publique como saludable.
   Si PostgreSQL aún está aprovisionándose, revisar los logs y reintentar la API.
6. Esperar al estado `live` y verificar `/health` (200), `/health/live` (200),
   `/swagger/index.html`, el acceso con ambos roles y el pago simulado.
7. En el repositorio frontend configurar la URL pública real:
   `VITE_USE_FAKE_API=false` y `VITE_FULLTANK_API_URL=https://<API>/api/v1`.
   Compilar con `npm run build`; `build:demo` usa la API simulada y no verifica la
   integración con el backend. Publicar `dist` desde GitHub Actions en Firebase.

Los orígenes permitidos son `https://full-tank-964e2.web.app` y
`https://full-tank-964e2.firebaseapp.com`. El Blueprint configura el despliegue de la
API tras superar CI en `main`. `/health` comprueba conectividad y migraciones;
`/health/live` solo comprueba que el proceso está activo.

## Migración desde MySQL

Esta versión crea un **esquema PostgreSQL nuevo**. Las migraciones MySQL antiguas se
conservan en `deploy/legacy-mysql-migrations` como referencia y no se compilan.
No se deben ejecutar sobre PostgreSQL ni copiar su tabla `__EFMigrationsHistory`.
La base MySQL existente no se elimina ni se transforma automáticamente.

Para datos de negocio que deban conservarse: realizar un backup MySQL, diseñar y
probar la importación en una base PostgreSQL de staging, verificar recuentos,
identificadores, relaciones, decimales y fechas; ajustar las secuencias de identidad
tras importar IDs explícitos. No habilitar el seed sobre una importación parcial.
En un despliegue académico nuevo se utiliza el seed reproducible incluido.

## Verificación local del contenedor

```bash
docker build -t fulltank-av2-api:local .
python3 deploy/smoke_render.py
```

La prueba crea PostgreSQL 18 con TLS, API, cliente HTTP, red y volumen **desechables**.
Usa contraseñas aleatorias y elimina exclusivamente sus propios recursos.
Verifica migraciones, seed, Swagger, CORS, JWT, ambos roles, pago académico,
persistencia y ausencia de duplicados tras reiniciar API y PostgreSQL.

Referencias: [Render Free](https://render.com/docs/free),
[PostgreSQL](https://render.com/docs/postgresql-creating-connecting),
[Blueprint](https://render.com/docs/blueprint-spec),
[Npgsql](https://www.npgsql.org/efcore/).
