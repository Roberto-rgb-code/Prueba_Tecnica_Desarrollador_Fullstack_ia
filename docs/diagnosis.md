# Ejercicio 4 – Diagnóstico bajo presión

## Escenario

> Los usuarios no pueden guardar registros, algunos microservicios responden con errores 500, y hay reportes de alta latencia en las respuestas de agentes de IA.

## Hipótesis (priorizadas)

1. **SQL Server caído o sin conexión** – UserService/AuthService no pueden persistir (500)
2. **RabbitMQ saturado o caído** – Eventos fallan, posible timeout en cascada
3. **Redis no disponible** – UserService podría fallar al cachear
4. **Pool de conexiones agotado** – Alta concurrencia en SQL Server
5. **Agente IA: rate limiting OpenAI** – Latencia alta, no errores 500 en users
6. **Qdrant caído** – Solo afecta AI Agent, no guardado de usuarios

## Plan de diagnóstico

### Fase 1 – Confirmar alcance (5 min)

```bash
docker compose ps
curl http://localhost:5000/health
curl http://localhost:5000/api/users -H "Authorization: Bearer $TOKEN"
```

Revisar logs estructurados JSON por servicio:

```bash
docker compose logs user-service --tail=100
docker compose logs sqlserver --tail=50
docker compose logs ai-agent-service --tail=50
```

### Fase 2 – Infraestructura (10 min)

1. Verificar health de SQL Server, Redis, RabbitMQ, MongoDB
2. Buscar en logs: `Connection refused`, `Timeout`, `Login failed`
3. Comprobar espacio en disco de volúmenes Docker

### Fase 3 – Aplicación (10 min)

1. Filtrar logs por `"level":"Error"` y `"Service":"UserService"`
2. Revisar stack traces de EF Core / SQL
3. Verificar si AuditService bloquea (no debería afectar sync path)

### Fase 4 – Agente IA (5 min)

1. Revisar latencia en métricas del agente (`latencyMs`)
2. Buscar HTTP 429 (rate limit) en logs de OpenAI
3. Verificar conectividad a Qdrant

## Logs centralizados

Con ELK/Datadog/CloudWatch se filtraría:

```
service:UserService AND level:Error AND @timestamp:[now-15m TO now]
```

Correlacionar por `traceId` si está implementado en headers.

## Comunicación a stakeholders

**T+0 min:** "Detectamos incidente en guardado de usuarios. Investigando infraestructura y UserService. ETA update en 15 min."

**T+15 min:** "Causa probable: SQL Server no responde. Reiniciando contenedor / escalando. Guardado de usuarios afectado; login y consultas pueden funcionar parcialmente."

**T+30 min:** "Servicio restaurado. Causa raíz: [X]. Acciones preventivas: health checks, alertas, retry policies."

## Acciones correctivas

- Health checks con `depends_on: condition: service_healthy`
- Retry con Polly en conexiones EF Core
- Circuit breaker en llamadas OpenAI
- Monitoreo de latencia p95 del agente IA
- Límites de tokens para control de costos
