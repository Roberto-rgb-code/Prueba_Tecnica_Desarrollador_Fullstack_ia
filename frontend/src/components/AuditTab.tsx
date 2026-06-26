import { useEffect, useState } from 'react';
import { ClipboardList, Loader2 } from 'lucide-react';
import { toast } from 'sonner';
import { AuditLog, auditApi } from '../api/client';

const eventColors: Record<string, string> = {
  UserCreated: 'bg-blue-100 text-blue-700',
  UserUpdated: 'bg-violet-100 text-violet-700',
  UserLoggedIn: 'bg-emerald-100 text-emerald-700',
  RoleAssigned: 'bg-amber-100 text-amber-700',
};

export default function AuditTab() {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    auditApi.list()
      .then(({ data }) => setLogs(Array.isArray(data) ? data : []))
      .catch(() => toast.error('Error al cargar auditoría'))
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center py-24 text-slate-500">
        <Loader2 className="mr-2 h-6 w-6 animate-spin text-toka-600" />
        Cargando auditoría...
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="flex items-center gap-2 text-2xl font-bold text-slate-900">
          <ClipboardList className="h-7 w-7 text-toka-600" />
          Auditoría
        </h2>
        <p className="mt-1 text-sm text-slate-500">Eventos del sistema vía RabbitMQ → MongoDB</p>
      </div>

      <div className="card overflow-hidden !p-0">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-slate-100 bg-slate-50/80">
              <tr>
                <th className="px-6 py-4 font-semibold text-slate-700">Fecha</th>
                <th className="px-6 py-4 font-semibold text-slate-700">Evento</th>
                <th className="px-6 py-4 font-semibold text-slate-700">Servicio</th>
                <th className="px-6 py-4 font-semibold text-slate-700">Acción</th>
                <th className="px-6 py-4 font-semibold text-slate-700">Detalle</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {logs.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-6 py-12 text-center text-slate-500">
                    Sin registros aún. Registra usuarios o asigna roles para generar eventos.
                  </td>
                </tr>
              ) : (
                logs.map((l) => (
                  <tr key={l.id} className="hover:bg-slate-50/50">
                    <td className="whitespace-nowrap px-6 py-4 text-slate-600">
                      {new Date(l.occurredAtUtc).toLocaleString()}
                    </td>
                    <td className="px-6 py-4">
                      <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium ${eventColors[l.eventType] ?? 'bg-slate-100 text-slate-600'}`}>
                        {l.eventType}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-slate-600">{l.serviceName}</td>
                    <td className="px-6 py-4 text-slate-600">{l.action}</td>
                    <td className="max-w-xs truncate px-6 py-4 text-slate-500" title={l.details}>{l.details}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
