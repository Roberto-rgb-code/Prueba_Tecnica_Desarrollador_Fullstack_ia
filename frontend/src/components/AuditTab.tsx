import { useEffect, useState } from 'react';
import { AuditLog, auditApi } from '../api/client';

export default function AuditTab() {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    auditApi.list()
      .then(({ data }) => setLogs(data))
      .catch(() => setError('Error al cargar auditoría'))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div className="loading">Cargando auditoría...</div>;

  return (
    <div>
      <h2>Auditoría</h2>
      {error && <div className="error">{error}</div>}
      <table>
        <thead><tr><th>Fecha</th><th>Evento</th><th>Servicio</th><th>Acción</th><th>Detalle</th></tr></thead>
        <tbody>
          {logs.map((l) => (
            <tr key={l.id}>
              <td>{new Date(l.occurredAtUtc).toLocaleString()}</td>
              <td>{l.eventType}</td>
              <td>{l.serviceName}</td>
              <td>{l.action}</td>
              <td>{l.details}</td>
            </tr>
          ))}
        </tbody>
      </table>
      {logs.length === 0 && <p>Sin registros de auditoría aún.</p>}
    </div>
  );
}
