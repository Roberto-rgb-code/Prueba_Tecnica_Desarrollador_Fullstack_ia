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

        if (q.Contains("rol") || q.Contains("role") || q.Contains("permiso") || q.Contains("admin") || q.Contains("existen"))
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
        foreach (var marker in new[] { "PREGUNTA DEL USUARIO:", "User question:" })
        {
            var idx = prompt.LastIndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                var rest = prompt[(idx + marker.Length)..];
                var line = rest.Split('\n')[0].Trim();
                if (!string.IsNullOrWhiteSpace(line)) return line;
            }
        }
        return prompt.Trim();
    }

    private static string ExtractLiveData(string prompt)
    {
        foreach (var marker in new[] { "DATOS EN VIVO DEL SISTEMA:", "Live system data:" })
        {
            var start = prompt.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (start < 0) continue;
            start += marker.Length;
            var endMarkers = new[] { "PREGUNTA DEL USUARIO:", "User question:", "Instrucción:" };
            var end = prompt.Length;
            foreach (var em in endMarkers)
            {
                var i = prompt.IndexOf(em, start, StringComparison.OrdinalIgnoreCase);
                if (i > start) end = Math.Min(end, i);
            }
            return prompt[start..end].Trim();
        }
        return string.Empty;
    }

    private static string ExtractContext(string prompt)
    {
        foreach (var (startMarker, endMarker) in new[]
        {
            ("CONTEXTO (base de conocimiento RAG", "DATOS EN VIVO"),
            ("Context from knowledge base:", "Live system data:")
        })
        {
            var start = prompt.IndexOf(startMarker, StringComparison.OrdinalIgnoreCase);
            if (start < 0) continue;
            var lineEnd = prompt.IndexOf('\n', start);
            start = lineEnd >= 0 ? lineEnd + 1 : start + startMarker.Length;
            var end = prompt.IndexOf(endMarker, start, StringComparison.OrdinalIgnoreCase);
            return (end > start ? prompt[start..end] : prompt[start..]).Trim();
        }
        return string.Empty;
    }

    private static string SummarizeContext(string context)
    {
        var bulletSections = Regex.Matches(context, @"•\s*(?<title>[^:]+):\s*(?<body>[^\n•]+)")
            .Select(m => $"**{m.Groups["title"].Value.Trim()}:** {m.Groups["body"].Value.Trim()}");
        var bracketSections = Regex.Matches(context, @"\[(?<title>[^\]]+)\]\s*(?<body>[^\[]*)")
            .Select(m => $"**{m.Groups["title"].Value.Trim()}:** {m.Groups["body"].Value.Trim()}");
        var sections = bulletSections.Concat(bracketSections).Where(s => s.Length > 4).Take(3);
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
