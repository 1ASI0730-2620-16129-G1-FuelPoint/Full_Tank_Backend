#!/usr/bin/env python3
"""Verify the AV2 images against a NEW disposable MySQL volume, then remove all test resources."""
import json
from email import message_from_string
import os
import secrets
import subprocess
import time


def docker(*args, env=None, check=True):
    result = subprocess.run(["docker", *args], env=env, capture_output=True, text=True)
    if check and result.returncode:
        raise RuntimeError(f"docker {args[0]} failed: {result.stderr.strip()}")
    return (result.stdout + (result.stderr if args[0] == "logs" else "")).strip()


def request(path, data=None, token=None, method=None, headers=None):
    request_headers = {"Content-Type": "application/json", **(headers or {})}
    if token:
        request_headers["Authorization"] = "Bearer " + token
    # Use the curl already included in MySQL's image. Requests cross the private
    # Docker network, so this also works on hosts without container port forwarding.
    config = ["url = " + json.dumps(f"http://{api}:10001{path}"), "include", "silent",
              "show-error", "max-time = 10", 'noproxy = "*"',
              "request = " + json.dumps(method or ("POST" if data is not None else "GET"))]
    for key, value in request_headers.items():
        config.append("header = " + json.dumps(key + ": " + value))
    if data is not None:
        config.append("data = " + json.dumps(json.dumps(data)))
    result = subprocess.run(["docker", "exec", "-i", db, "curl", "--config", "-"],
                            input="\n".join(config), capture_output=True, text=True)
    if result.returncode:
        raise OSError("Container HTTP probe failed: " + result.stderr.strip())
    head, _, content = result.stdout.partition("\n\n")
    status_line, _, headers = head.partition("\n")
    try:
        content = json.loads(content)
    except ValueError:
        pass
    return int(status_line.split()[1]), content, message_from_string(headers)


def wait_for(path, status, seconds=120):
    deadline = time.monotonic() + seconds
    while time.monotonic() < deadline:
        if docker("inspect", "--format", "{{.State.Running}}", api, check=False) == "false":
            raise RuntimeError("API container exited before becoming ready")
        try:
            if request(path)[0] == status:
                return
        except OSError:
            pass
        time.sleep(1)
    raise RuntimeError(f"{path} did not return HTTP {status} within {seconds}s")


prefix = "fulltank-av2-check-" + secrets.token_hex(4)
db, api, network, volume = [prefix + suffix for suffix in ("-db", "-api", "-net", "-data")]
db_password, demo_password = secrets.token_urlsafe(32), secrets.token_urlsafe(20)
environment = dict(os.environ, MYSQL_DATABASE="fulltank_av2", MYSQL_USER="fulltank",
                   MYSQL_PASSWORD=db_password, MYSQL_ROOT_PASSWORD=secrets.token_urlsafe(32))
runtime_settings = dict(ASPNETCORE_ENVIRONMENT="Production", PORT="10001",
    Database__Host="mysql", Database__Name="fulltank_av2", Database__User="fulltank",
    Database__Password=db_password, Database__DemoPassword=demo_password,
    TokenSettings__Secret=secrets.token_urlsafe(48),
    Cors__AllowedOrigins__0="https://full-tank-964e2.web.app",
    Cors__AllowedOrigins__1="https://full-tank-964e2.firebaseapp.com",
    AcademicDemo__Enabled="true", PaymentSettings__EnableSimulation="true",
    Swagger__Enabled="true", Hosting__HttpsRedirectionEnabled="false")


def start_api(initialize):
    settings = dict(runtime_settings, Database__ApplyMigrationsOnStartup=str(initialize).lower(),
                    Database__SeedDemoData=str(initialize).lower())
    env = dict(os.environ, **settings)
    arguments = ["run", "-d", "--name", api, "--network", network, "--memory", "512m"]
    for key in settings:
        arguments += ["-e", key]
    docker(*arguments, "fulltank-av2-api:local", env=env)


try:
    docker("network", "create", network)
    docker("volume", "create", volume)
    arguments = ["run", "-d", "--name", db, "--network", network, "--network-alias", "mysql",
                 "--memory", "512m", "-v", volume + ":/var/lib/mysql"]
    for key in ("MYSQL_DATABASE", "MYSQL_USER", "MYSQL_PASSWORD", "MYSQL_ROOT_PASSWORD"):
        arguments += ["-e", key]
    docker(*arguments, "fulltank-av2-mysql:local", env=environment)
    print("Started isolated MySQL (512 MB) and a disposable persistent volume.", flush=True)
    for _ in range(120):
        ready = docker("exec", db, "sh", "-c",
            'MYSQL_PWD="$MYSQL_PASSWORD" mysql -h 127.0.0.1 -u "$MYSQL_USER" -D "$MYSQL_DATABASE" -e "SELECT 1"',
            check=False)
        if ready:
            break
        time.sleep(1)
    else:
        raise RuntimeError("MySQL did not initialize")

    start_api(False)
    wait_for("/health/live", 200)
    assert request("/health")[0] == 503, "An unmigrated database must not be ready"
    docker("rm", "-f", api)
    print("Readiness correctly rejects an empty, unmigrated database.", flush=True)
    start_api(True)
    wait_for("/health", 200)
    status, swagger, _ = request("/swagger/v1/swagger.json")
    assert status == 200 and "simulated" in swagger["info"]["description"]
    assert request("/api/v1/orders")[0] == 401
    for origin, allowed in [("https://full-tank-964e2.web.app", True), ("https://untrusted.example", False)]:
        status, _, headers = request("/api/v1/authentication/sign-in", method="OPTIONS", headers={
            "Origin": origin, "Access-Control-Request-Method": "POST"})
        assert (headers.get("Access-Control-Allow-Origin") == origin) == allowed
    tokens = {}
    for role, email in [("buyer", "logistics@transportesdelsur.com"), ("provider", "dispatch@petroandes.com")]:
        status, session, _ = request("/api/v1/authentication/sign-in", {"email": email, "password": demo_password})
        assert status == 200 and session["token"], f"{role} login failed"
        tokens[role] = session["token"]
    status, orders, _ = request("/api/v1/orders", token=tokens["buyer"])
    assert status == 200 and len(orders) == 3
    order = next(item for item in orders if item["status"] == "PENDING_PAYMENT")
    payload = {
        "payment": {"orderId": order["id"], "companyId": order["buyerCompanyId"],
            "providerId": order["providerId"], "method": "CARD", "amount": order["totalAmount"],
            "status": "COMPLETED", "maskedCard": "4111********1111", "cardHolder": "Academic Demo"},
        "invoice": {"orderId": order["id"], "invoiceNumber": "SIM-TEST", "providerRuc": "20512345671",
            "providerName": "Demo provider", "buyerRuc": "20100111222", "buyerName": "Demo buyer",
            "fuelType": "DIESEL", "quantity": 460, "unit": "L", "unitPrice": 4.5,
            "subtotal": 1, "igv": 1, "total": 2, "status": "PAID"}}
    status, checkout, _ = request("/api/v1/payment-checkout", payload, tokens["buyer"])
    assert status == 200 and checkout["invoice"]["invoiceNumber"].startswith("SIM-INV-"), f"Checkout returned {status}"
    payment_id = checkout["payment"]["id"]
    print("Migrations, seed, Swagger, CORS, both roles, JWT protection and simulated checkout passed.", flush=True)
    docker("restart", db)
    docker("restart", api)
    wait_for("/health", 200)
    status, orders, _ = request("/api/v1/orders", token=tokens["buyer"])
    assert status == 200 and len(orders) == 3, "Restart must not duplicate demo orders"
    status, payments, _ = request("/api/v1/payments", token=tokens["buyer"])
    assert status == 200 and any(item["id"] == payment_id for item in payments)
    print("API/MySQL restart preserved the payment and did not duplicate seed data. PASS", flush=True)
except Exception:
    print("API diagnostic log:\n" + docker("logs", "--tail", "35", api, check=False), flush=True)
    raise
finally:
    # Only resources created by this invocation are removed. Existing local services are untouched.
    docker("rm", "-f", api, db, check=False)
    docker("volume", "rm", volume, check=False)
    docker("network", "rm", network, check=False)
