# Validación de preparación para Render — AV2

Fecha: 7 de noviembre de 2026. Alcance: código y contenedores locales;
**no acredita un despliegue público en Render**.

| Comprobación | Resultado |
|---|---|
| `dotnet test full-tank-platform.sln --no-restore --disable-build-servers -m:1 -p:UseSharedCompilation=false` | **82/82**, sin fallos |
| Construcción de `fulltank-av2-api:local` desde `Dockerfile` | Correcta, publicación Release .NET 10 |
| Construcción de `fulltank-av2-mysql:local` desde `deploy/mysql/Dockerfile` | Correcta, MySQL 8.0.46 |
| `render.yaml` frente al esquema público actual `https://render.com/schema/render.yaml.json` | Válido con JSON Schema Draft 2020-12 |
| Diagnósticos Serena de Program, configuración, health check, seeder y pruebas nuevas | Sin errores ni advertencias; únicamente sugerencias de estilo |
| `python3 deploy/smoke_render.py` | **PASS** |
| `git diff --check` | Correcto |

La prueba Docker utiliza una red y un volumen nuevos, contraseñas aleatorias,
`ASPNETCORE_ENVIRONMENT=Production` y un límite de 512 MB para cada contenedor.
Verifica:

1. Escucha en todas las interfaces con `PORT=10001`, diferente del puerto por defecto.
2. Proceso vivo pero readiness HTTP 503 cuando faltan migraciones.
3. Migraciones y seeder sobre MySQL vacío; readiness HTTP 200 al terminar.
4. Swagger accesible e identificación explícita del alcance simulado.
5. CORS para Firebase y rechazo de un origen distinto.
6. JWT obligatorio para consultar órdenes; login de comprador y proveedor.
7. Checkout académico y generación de un comprobante `SIM-INV-*`.
8. Reinicio de MySQL y API, conservación del pago y ausencia de duplicados del seed.

Las solicitudes de la prueba se enviaron dentro de la red Docker, porque el
entorno de ejecución no exponía sus puertos al host. Esto comprueba la imagen y
su comunicación privada; no verifica el proxy, DNS ni TLS públicos de Render.
El script elimina únicamente sus contenedores, volumen y red desechables.
Los servicios locales existentes de FullTank se conservaron.

Se conservaron los cambios previos en `OrderingContextFacade.cs` y la base del
seeder local. No se hicieron commits, push, despliegues ni cambios en recursos
de Render. El CLI de Render no estaba instalado: no se ejecutó su validación
autenticada, distinta de la comprobación del esquema público.

Siguiente paso: seguir [RENDER_AV2.md](RENDER_AV2.md), revisar el coste de MySQL,
subir el repositorio, completar la contraseña privada demo y aplicar el Blueprint.
Después hay que comprobar la URL pública y recoger las evidencias del informe.
