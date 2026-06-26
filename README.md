# Toka User Management System

Prueba técnica **Senior Full-Stack Engineer (IA)** — sistema de gestión de usuarios con microservicios (.NET 8), frontend React, agente IA con RAG, auditoría y despliegue Docker.

## Requisitos previos

| Herramienta | Versión mínima |
|-------------|----------------|
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | 4.x |
| RAM disponible | 8 GB recomendado |
| (Opcional) [.NET 8 SDK](https://dotnet.microsoft.com/download) | Para tests locales |
| (Opcional) [Node.js 20+](https://nodejs.org/) | Para frontend en dev |

Opcional: `OPENAI_API_KEY` para respuestas reales del agente IA (sin ella funciona en modo mock).

---

## Inicio rápido (recomendado — un solo contenedor)

Todo el stack en **un único contenedor**: SQL Server, MongoDB, Redis, RabbitMQ, microservicios, gateway y frontend.

```powershell
git clone https://github.com/Roberto-rgb-code/Prueba_Tecnica_Desarrollador_Fullstack_ia.git
cd Prueba_Tecnica_Desarrollador_Fullstack_ia
.\scripts\run-single-container.ps1
```

En Linux/macOS (requiere bash):

```bash
export DOCKER_BUILDKIT=0
docker compose -f docker-compose.single.yml up --build -d
```

**Aplicación:** http://localhost:3000

> La primera build puede tardar 10–15 minutos. El healthcheck tarda ~3 min en pasar a `healthy`.

### Primer uso

1. Abre http://localhost:3000
2. Haz clic en **Regístrate** (no hay usuario admin precargado)
3. Email + contraseña (mín. 6 caracteres) + nombre
4. Explora: **Usuarios**, **Roles**, **Auditoría**, **Agente IA**

---

## Modo multi-contenedor (arquitectura completa con Qdrant)

Incluye Qdrant para RAG vectorial real y un contenedor por servicio:

```powershell
.\scripts\run-docker.ps1
```

| Servicio | URL |
|----------|-----|
| Frontend | http://localhost:3000 |
| API Gateway | http://localhost:5000 |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |

---

## Desarrollo local (sin Docker)

Para desarrollo y depuración rápida con LocalDB y servicios en memoria:

```powershell
.\scripts\run-local.ps1
```

Requiere SQL Server LocalDB instalado (incluido con Visual Studio / Build Tools).

---

## Tests

### Backend (.NET) — 42 tests, coverage ≥ 70%

```powershell
dotnet test TokaUserManagement.sln
```

Con reporte de cobertura (capa Application + servicios IA):

```powershell
.\scripts\run-tests-with-coverage.ps1
```

### Frontend (Vitest)

```powershell
cd frontend
npm install
npm test
```

---

## Arquitectura

```
Frontend (React) → Nginx/Gateway (YARP) → Microservicios
                                              ├── AuthService    → SQL Server
                                              ├── UserService    → SQL Server + Redis
                                              ├── RoleService    → SQL Server
                                              ├── AuditService   → MongoDB ← RabbitMQ
                                              └── AiAgentService → Qdrant + OpenAI
```

Documentación detallada:

- [Arquitectura y DDD](docs/architecture.md)
- [Prompt engineering (IA)](docs/prompt-engineering.md)
- [Ejercicio 4 — Diagnóstico bajo presión](docs/diagnosis.md)

---

## Microservicios

| Servicio | Responsabilidad |
|----------|-----------------|
| **AuthService** | Registro/login JWT |
| **UserService** | CRUD usuarios + cache Redis |
| **RoleService** | CRUD roles + asignación |
| **AuditService** | Consumer RabbitMQ → MongoDB |
| **AiAgentService** | RAG (Qdrant) + OpenAI + métricas |
| **Gateway** | YARP reverse proxy |

Patrón por servicio: **Api → Application → Domain ← Infrastructure** (DDD + Clean Architecture).

---

## API principal

Todas las rutas pasan por el gateway (`/api/...`):

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/auth/register` | Registro |
| POST | `/api/auth/login` | Login |
| GET | `/api/users` | Listar usuarios |
| POST | `/api/users` | Crear usuario |
| GET | `/api/roles` | Listar roles |
| POST | `/api/roles/{roleId}/assign/{userId}` | Asignar rol |
| GET | `/api/audit` | Logs de auditoría |
| POST | `/api/agent/query` | Consulta al agente IA |

---

## Variables de entorno

| Variable | Descripción |
|----------|-------------|
| `OPENAI_API_KEY` | API key OpenAI (opcional; mock si no está) |
| `SA_PASSWORD` | Password SQL Server en contenedor (default en compose) |

Ejemplo con agente IA real:

```powershell
$env:OPENAI_API_KEY = "sk-..."
.\scripts\run-single-container.ps1
```

---

## Estructura del repositorio

```
├── services/           # Microservicios .NET 8
│   ├── AuthService/
│   ├── UserService/
│   ├── RoleService/
│   ├── AuditService/
│   └── AiAgentService/
├── gateway/            # API Gateway (YARP)
├── frontend/           # React + TypeScript + Zustand
├── shared/             # JWT, RabbitMQ, eventos compartidos
├── docker/             # Config all-in-one (nginx, supervisord)
├── docs/               # Arquitectura, diagnóstico, IA
├── scripts/            # Scripts de arranque y tests
├── docker-compose.yml          # Multi-contenedor
├── docker-compose.single.yml   # Un solo contenedor
└── Dockerfile.all-in-one
```

---

## Comandos útiles Docker

```powershell
# Ver logs (contenedor único)
docker compose -f docker-compose.single.yml logs -f

# Detener
docker compose -f docker-compose.single.yml down

# Rebuild forzado
$env:DOCKER_BUILDKIT=0
docker compose -f docker-compose.single.yml up --build -d
```

> **Nota Windows:** si el build falla por caracteres especiales en la ruta del proyecto, clona el repo en una ruta ASCII (ej. `C:\dev\toka`) o usa `DOCKER_BUILDKIT=0`.

---

## Entregables de la prueba técnica

| Ejercicio | Entregable |
|-----------|------------|
| 1 – Arquitectura | `docs/architecture.md` + `docker-compose.yml` |
| 2 – Microservicios | 5 servicios + tests + coverage report |
| 3 – Frontend | React + Zustand + tests Vitest |
| 4 – Diagnóstico | `docs/diagnosis.md` |
| 5 – IA / RAG | `AiAgentService` + `docs/prompt-engineering.md` |

---

## Licencia

Proyecto de prueba técnica — uso educativo/evaluación.
