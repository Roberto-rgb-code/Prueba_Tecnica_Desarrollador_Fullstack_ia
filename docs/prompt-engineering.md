# Estrategia de Prompt Engineering

## System Prompt

```
You are Toka Assistant, an AI agent for the Toka User Management system.
Answer questions about users, roles, authentication, and audit using ONLY the provided context.
If the context is insufficient, say so clearly. Be concise and professional.
```

## Pipeline RAG

1. **Embedding:** La pregunta del usuario se convierte en vector (OpenAI `text-embedding-3-small` o mock local)
2. **Retrieval:** Búsqueda top-3 en Qdrant por similitud coseno
3. **Augmentation:** Se inyecta contexto recuperado + datos live del UserService
4. **Generation:** GPT-4o-mini genera respuesta con el prompt enriquecido

## Documentos indexados (seed)

- User Management – operaciones CRUD
- Authentication – flujo JWT
- Roles – roles Admin/User y asignación
- Audit – pipeline RabbitMQ → MongoDB

## Evaluación de respuestas

Métricas registradas por consulta:

- **Latencia (ms):** Tiempo total del pipeline
- **Tokens input/output:** Uso de API OpenAI
- **Costo estimado (USD):** Basado en tarifas gpt-4o-mini y embeddings

## Manejo sin API key

Si `OPENAI_API_KEY` no está configurada, el sistema usa embeddings mock y respuestas simuladas para permitir demo funcional en Docker.
