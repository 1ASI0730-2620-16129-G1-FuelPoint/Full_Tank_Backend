# FullTank AV2 — preparación para Render

Resultado de las comprobaciones locales: [VALIDACION_RENDER_AV2.md](VALIDACION_RENDER_AV2.md).

La configuración publica la API .NET 10 y MySQL 8.0.46 en el mismo entorno y
región. La API usa `Production`, con un modo académico explícito: pagos y
comprobantes simulados, sin procesar dinero ni emitir comprobantes electrónicos
SUNAT. Swagger identifica ese alcance.

## Recursos y coste

`render.yaml` define el proyecto `fulltank-av2`, entorno `av2`:

| Recurso | Configuración |
|---|---|
| `fulltank-av2-api` | Web Service Docker, plan `free`, puerto `PORT`, usuario sin privilegios |
| `fulltank-av2-mysql` | Private Service Docker, plan **de pago** `0.5c-512mb`, MySQL 8.0.46 |
| Datos MySQL | Disco persistente **de pago**, 1 GB, `/var/lib/mysql` |
| Región | Oregon en ambos servicios, dentro del mismo workspace |

La API gratuita puede suspenderse por inactividad: abrir `/health` y esperar su
respuesta antes de la sustentación. Para evitar esa espera se puede elegir un
plan de pago. Revisar el coste total mostrado por Render antes de crear recursos.
Preparar estos archivos no crea infraestructura ni activa cargos.

MySQL no se publica en Internet. La API recibe host y contraseña con
`fromService` y usa un usuario propio, distinto de root. Render genera las
contraseñas de MySQL y la clave JWT. `Database__SslMode=Required` cifra la conexión
privada, sin verificar la identidad del certificado autofirmado. Con MySQL
externo y CA conocida usar `VerifyFull` y la configuración del proveedor.

## Publicación

1. Subir **el contenido de `backend-main` como raíz del repositorio backend**,
   incluyendo los cambios previos de integración, `DatabaseSeeder.cs`,
   `Dockerfile`, `.dockerignore`, `deploy/`, `render.yaml` y las clases nuevas.
   Esta copia local todavía no tiene remoto Git configurado. No subir archivos
   locales de configuración, `.env` ni secretos. Para un monorepo adaptar
   `dockerContext` y `dockerfilePath` a su raíz.
2. Si se dispone del CLI de Render, ejecutar `render blueprints validate render.yaml`.
   La validación con el esquema JSON público solo comprueba la estructura;
   no acredita permisos del workspace, planes ni acceso al repositorio remoto.
3. En Render elegir **New → Blueprint**, conectar el repositorio y seleccionar
   la rama de entrega que contenga el Blueprint.
4. Revisar servicios, región y coste. Completar **`Database__DemoPassword` con
   una contraseña privada de al menos 12 caracteres** para las dos cuentas demo.
   Esta contraseña es distinta de las credenciales de MySQL.
5. Aplicar el Blueprint. Si la API arranca antes de la primera inicialización de
   MySQL y falla, revisar sus logs y volver a desplegar **la API** cuando la base
   esté lista.
6. Verificar `https://<API-ASIGNADA-POR-RENDER>/health` → HTTP 200 y
   `https://<API-ASIGNADA-POR-RENDER>/swagger/index.html`. Usar la URL que asigne
   Render, sin presuponer un subdominio disponible.

Los despliegues automáticos están desactivados para fijar la versión del hito.
Las correcciones se publican desplegando manualmente el commit revisado.
`sync: false` solicita la contraseña demo al crear el Blueprint; las rotaciones
posteriores se hacen en las variables del servicio, sin escribir secretos en YAML.

## Variables de la API

| Variable | Uso |
|---|---|
| `ASPNETCORE_ENVIRONMENT=Production` | Excluye configuración local y logging sensible de desarrollo |
| `PORT=10000` | Escucha HTTP en `0.0.0.0`; Render gestiona HTTPS en su borde |
| `Database__Host/Port/Name/User/Password` | Conexión MySQL construida con escape correcto de contraseñas |
| `ConnectionStrings__DefaultConnection` | Alternativa para MySQL existente; si se define tiene prioridad |
| `TokenSettings__Secret` | Clave JWT aleatoria privada de al menos 32 caracteres |
| `Cors__AllowedOrigins__0` y `__1` | Orígenes HTTPS exactos de Firebase, sin ruta ni barra final |
| `Database__ApplyMigrationsOnStartup=true` | Aplica migraciones antes de servir; un fallo impide publicar la API |
| `Database__SeedDemoData=true` | Inserta datos demo en una transacción, sin reiniciar la base |
| `Database__DemoPassword` | Contraseña al crear cuentas demo; **no rota** contraseñas ya existentes |
| `AcademicDemo__Enabled=true` | Autoriza expresamente simulación y datos académicos fuera de Development |
| `PaymentSettings__EnableSimulation=true` | Checkout simulado con comprobantes `SIM-INV-*` |
| `Swagger__Enabled=true` | Documentación para evaluar; los endpoints protegidos siguen requiriendo JWT |
| `Hosting__HttpsRedirectionEnabled=false` | Evita redirecciones internas; Render ya redirige HTTP a HTTPS |

Production normal mantiene simulación y Swagger desactivados por defecto.
Desactivar la simulación **no activa una pasarela real**: el checkout rechazará
pagos mientras no se implemente esa integración.

## Cuentas y datos

| Rol | Correo | Contraseña |
|---|---|---|
| Comprador | `logistics@transportesdelsur.com` | La definida en `Database__DemoPassword` |
| Proveedor | `dispatch@petroandes.com` | La definida en `Database__DemoPassword` |

El seeder incluye catálogo, inventario, equipos, flota, solicitudes y pedidos de
ejemplo. Si la cuenta principal ya existe, no vuelve a insertar ni restablece
datos. Usar una base exclusiva de AV2. En Development se conserva la contraseña
de prueba anterior `123456`.

Las migraciones al arrancar se destinan a esta única instancia académica. Antes
de migrar datos que deban conservarse, hacer backup con `mysqldump`, revisar SQL
y comprobar duplicados según el README. El disco no sustituye backups coherentes
de MySQL. Cambiar `MYSQL_PASSWORD` no modifica la cuenta ya existente dentro del
disco: una rotación requiere actualizar MySQL y las variables de la API.

## Conectar el frontend

Tras verificar el backend público, construir **`frontend-backend`** con:

```dotenv
VITE_USE_FAKE_API=false
VITE_FULLTANK_API_URL=https://<API-ASIGNADA-POR-RENDER>/api/v1
```

Usar `npm run build` y publicar su `dist` en Firebase. El workflow actual del
frontend usa `build:demo`: debe cambiarse antes de confiarle la publicación
integrada. Nunca publicar `localhost:5204` como URL de la API.

## Verificación reproducible

Desde la raíz del repositorio backend:

```bash
dotnet test full-tank-platform.sln
docker build -t fulltank-av2-api:local .
docker build -t fulltank-av2-mysql:local -f deploy/mysql/Dockerfile .
python3 deploy/smoke_render.py
```

El smoke test crea red, volumen, MySQL y API **desechables**, con contraseñas
aleatorias, y elimina solo esos recursos al finalizar. Las solicitudes viajan
por la red privada de Docker, sin publicar puertos al host. No toca la base local
existente. Prueba ambos contenedores con límite de 512 MB y un `PORT` distinto
de 10000, falta de migraciones, Production, Swagger, CORS permitido/rechazado,
JWT, ambos roles, checkout simulado y persistencia tras reiniciar MySQL y API
sin duplicar datos iniciales.

`/health` devuelve 503 si MySQL no responde o faltan migraciones. `/health/live`
solo indica que el proceso está vivo. Las respuestas no exponen excepciones ni
credenciales. El health check no sustituye el recorrido completo de negocio ni
las entrevistas de validación AV2.

## Evidencias después de publicar

- URL pública, commit desplegado y captura de ambos servicios.
- Swagger, requests/responses y persistencia al recargar/reiniciar.
- Recorrido comprador → solicitud → proveedor → despacho → recepción → pago
  simulado → comprobante, indicando expresamente el alcance académico.
- Incorporar evidencias en 5.2.3.6 y 5.2.3.7 del informe AV2.

Referencias oficiales consultadas:
[Blueprints](https://render.com/docs/blueprint-spec),
[MySQL](https://render.com/docs/deploy-mysql),
[discos](https://render.com/docs/disks),
[red privada](https://render.com/docs/private-network),
[Web Services](https://render.com/docs/web-services).
