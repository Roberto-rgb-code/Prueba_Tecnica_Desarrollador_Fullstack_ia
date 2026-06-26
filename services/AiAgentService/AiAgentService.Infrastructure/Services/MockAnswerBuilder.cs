using System.Text;
using System.Text.RegularExpressions;

namespace AiAgentService.Infrastructure.Services;

public static class MockAnswerBuilder
{
    public static string Build(string userPrompt)
    {
        var question = ExtractQuestion(userPrompt);
        var q = question.ToLowerInvariant();
        var context = ExtractContext(userPrompt);
        var liveData = ExtractLiveData(userPrompt);

        if (IsGreeting(q))
            return "¡Hola! Soy **Toka Assistant**. Puedo ayudarte con gestión de usuarios, roles, autenticación JWT y auditoría del sistema. ¿Qué te gustaría saber?";

        if (q.Contains("login") || q.Contains("iniciar") || q.Contains("autentic") || q.Contains("jwt") || q.Contains("registr"))
            return FormatAnswer(
                "Autenticación en Toka",
                "1. **Registro:** POST `/api/auth/register` con email, contraseña (mín. 6) y nombre.\n" +
                "2. **Login:** POST `/api/auth/login` — recibes un token JWT.\n" +
                "3. **Uso:** envía el token en el header `Authorization: Bearer {token}`.\n" +
                "4. Los eventos de login quedan registrados en auditoría.",
                context);

        if (q.Contains("rol") || q.Contains("role") || q.Contains("permiso") || q.Contains("admin"))
            return FormatAnswer(
                "Roles en Toka",
                "Existen roles como **Admin** (acceso completo) y **User** (acceso estándar).\n" +
                "• Listar: GET `/api/roles`\n" +
                "• Crear rol: POST `/api/roles`\n" +
                "• Asignar: POST `/api/roles/{roleId}/assign/{userId}`",
                context);

        if (q.Contains("usuario") || q.Contains("user") || q.Contains("crud") || q.Contains("crear"))
            return FormatAnswer(
                "Gestión de usuarios",
                "• **Listar:** GET `/api/users`\n" +
                "• **Crear:** POST `/api/users` (email, nombre, apellido)\n" +
                "• **Actualizar / desactivar:** PUT `/api/users/{id}`\n" +
                "• **Eliminar:** DELETE `/api/users/{id}`\n" +
                (string.IsNullOrWhiteSpace(liveData) ? "" : $"\n**Datos actuales:** {liveData}"),
                context);

        if (q.Contains("audit") || q.Contains("auditor") || q.Contains("log") || q.Contains("evento"))
            return FormatAnswer(
                "Auditoría",
                "Los eventos (`user.created`, `user.updated`, `user.logged_in`, `role.assigned`) se publican en **RabbitMQ** y los consume AuditService, guardándolos en **MongoDB**.\n" +
                "Consulta los logs en GET `/api/audit`.",
                context);

        if (!string.IsNullOrWhiteSpace(context))
            return FormatAnswer(
                "Respuesta según la base de conocimiento",
                SummarizeContext(context) + (string.IsNullOrWhiteSpace(liveData) ? "" : $"\n\n**Datos en vivo:** {liveData}"),
                null);

        return "No encontré información suficiente en la base de conocimiento para esa pregunta. " +
               "Prueba preguntar sobre usuarios, roles, autenticación o auditoría.";
    }

    private static bool IsGreeting(string q) =>
        q is "hola" or "hi" or "hello" or "buenas" or "hey" ||
        q.StartsWith("hola ") || q.StartsWith("buenos") || q.Contains("qué tal") || q.Contains("como estas");

    private static string ExtractQuestion(string prompt)
    {
        const string marker = "User question:";
        var idx = prompt.LastIndexOf(marker, StringComparison.Ordinal);
        return idx >= 0 ? prompt[(idx + marker.Length)..].Trim() : prompt.Trim();
    }

    private static string ExtractLiveData(string prompt)
    {
        const string marker = "Live system data:";
        var start = prompt.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0) return string.Empty;
        start += marker.Length;
        var end = prompt.IndexOf("User question:", start, StringComparison.Ordinal);
        var block = end > start ? prompt[start..end] : prompt[start..];
        return block.Trim();
    }

    private static string ExtractContext(string prompt)
    {
        const string startMarker = "Context from knowledge base:";
        const string endMarker = "Live system data:";
        var start = prompt.IndexOf(startMarker, StringComparison.Ordinal);
        if (start < 0) return string.Empty;
        start += startMarker.Length;
        var end = prompt.IndexOf(endMarker, start, StringComparison.Ordinal);
        return (end > start ? prompt[start..end] : prompt[start..]).Trim();
    }

    private static string SummarizeContext(string context)
    {
        var sections = Regex.Matches(context, @"\[(?<title>[^\]]+)\]\s*(?<body>[^\[]*)")
            .Select(m => $"**{m.Groups["title"].Value.Trim()}:** {m.Groups["body"].Value.Trim()}")
            .Where(s => s.Length > 4)
            .Take(3);

        return string.Join("\n\n", sections);
    }

    private static string FormatAnswer(string title, string body, string? context)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"### {title}");
        sb.AppendLine();
        sb.AppendLine(body);
        if (!string.IsNullOrWhiteSpace(context))
        {
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine(SummarizeContext(context));
        }
        return sb.ToString().Trim();
    }
}
