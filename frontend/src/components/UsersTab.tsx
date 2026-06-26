import { FormEvent, useEffect, useState } from 'react';
import { Loader2, Plus, Trash2, UserCheck, UserX, Users } from 'lucide-react';
import { toast } from 'sonner';
import { User, usersApi } from '../api/client';

export default function UsersTab() {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState({ email: '', firstName: '', lastName: '' });
  const [submitting, setSubmitting] = useState(false);

  const load = async () => {
    setLoading(true);
    try {
      const { data } = await usersApi.list();
      setUsers(Array.isArray(data) ? data : []);
    } catch {
      toast.error('Error al cargar usuarios');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleCreate = async (e: FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      await usersApi.create(form);
      setForm({ email: '', firstName: '', lastName: '' });
      toast.success('Usuario creado');
      await load();
    } catch {
      toast.error('Error al crear usuario');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-24 text-slate-500">
        <Loader2 className="mr-2 h-6 w-6 animate-spin text-toka-600" />
        Cargando usuarios...
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="flex items-center gap-2 text-2xl font-bold text-slate-900">
          <Users className="h-7 w-7 text-toka-600" />
          Usuarios
        </h2>
        <p className="mt-1 text-sm text-slate-500">Gestiona los usuarios del sistema</p>
      </div>

      <form className="card grid gap-4 sm:grid-cols-2 lg:grid-cols-4" onSubmit={handleCreate}>
        <input className="input-field" placeholder="Email" type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
        <input className="input-field" placeholder="Nombre" value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} required />
        <input className="input-field" placeholder="Apellido" value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} required />
        <button type="submit" className="btn-primary" disabled={submitting}>
          <Plus className="h-4 w-4" />
          {submitting ? 'Creando...' : 'Crear usuario'}
        </button>
      </form>

      <div className="card overflow-hidden !p-0">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-slate-100 bg-slate-50/80">
              <tr>
                <th className="px-6 py-4 font-semibold text-slate-700">Email</th>
                <th className="px-6 py-4 font-semibold text-slate-700">Nombre</th>
                <th className="px-6 py-4 font-semibold text-slate-700">Estado</th>
                <th className="px-6 py-4 font-semibold text-slate-700">Acciones</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {users.length === 0 ? (
                <tr>
                  <td colSpan={4} className="px-6 py-12 text-center text-slate-500">
                    No hay usuarios. Crea el primero con el formulario de arriba.
                  </td>
                </tr>
              ) : (
                users.map((u) => (
                  <tr key={u.id} className="transition hover:bg-slate-50/50">
                    <td className="px-6 py-4 font-medium text-slate-900">{u.email}</td>
                    <td className="px-6 py-4 text-slate-600">{u.fullName}</td>
                    <td className="px-6 py-4">
                      <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium ${u.isActive ? 'bg-emerald-100 text-emerald-700' : 'bg-slate-100 text-slate-600'}`}>
                        {u.isActive ? 'Activo' : 'Inactivo'}
                      </span>
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex gap-2">
                        <button
                          className="btn-secondary !px-3 !py-2"
                          onClick={async () => {
                            await usersApi.update(u.id, { firstName: u.firstName, lastName: u.lastName, isActive: !u.isActive });
                            toast.success(u.isActive ? 'Usuario desactivado' : 'Usuario activado');
                            load();
                          }}
                        >
                          {u.isActive ? <UserX className="h-4 w-4" /> : <UserCheck className="h-4 w-4" />}
                        </button>
                        <button
                          className="btn-danger"
                          onClick={async () => {
                            await usersApi.remove(u.id);
                            toast.success('Usuario eliminado');
                            load();
                          }}
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </div>
                    </td>
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
