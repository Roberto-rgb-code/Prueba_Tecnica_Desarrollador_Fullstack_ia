import { FormEvent, useEffect, useState } from 'react';
import { Loader2, Shield, UserCog } from 'lucide-react';
import { toast } from 'sonner';
import { Role, User, rolesApi, usersApi } from '../api/client';

export default function RolesTab() {
  const [roles, setRoles] = useState<Role[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState({ name: '', description: '' });
  const [assign, setAssign] = useState({ roleId: '', userId: '' });

  const load = async () => {
    setLoading(true);
    try {
      const [rolesRes, usersRes] = await Promise.all([rolesApi.list(), usersApi.list()]);
      setRoles(Array.isArray(rolesRes.data) ? rolesRes.data : []);
      setUsers(Array.isArray(usersRes.data) ? usersRes.data : []);
    } catch {
      toast.error('Error al cargar datos');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleCreate = async (e: FormEvent) => {
    e.preventDefault();
    try {
      await rolesApi.create(form);
      setForm({ name: '', description: '' });
      toast.success('Rol creado');
      await load();
    } catch {
      toast.error('Error al crear rol');
    }
  };

  const handleAssign = async (e: FormEvent) => {
    e.preventDefault();
    try {
      await rolesApi.assign(assign.roleId, assign.userId);
      toast.success('Rol asignado correctamente');
    } catch {
      toast.error('Error al asignar rol');
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-24 text-slate-500">
        <Loader2 className="mr-2 h-6 w-6 animate-spin text-toka-600" />
        Cargando roles...
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="flex items-center gap-2 text-2xl font-bold text-slate-900">
          <Shield className="h-7 w-7 text-toka-600" />
          Roles
        </h2>
        <p className="mt-1 text-sm text-slate-500">Administra roles y asignaciones</p>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <form className="card space-y-4" onSubmit={handleCreate}>
          <h3 className="font-semibold text-slate-900">Crear rol</h3>
          <input className="input-field" placeholder="Nombre del rol" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
          <input className="input-field" placeholder="Descripción" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
          <button type="submit" className="btn-primary w-full">Crear rol</button>
        </form>

        <form className="card space-y-4" onSubmit={handleAssign}>
          <h3 className="flex items-center gap-2 font-semibold text-slate-900">
            <UserCog className="h-5 w-5 text-toka-600" />
            Asignar rol
          </h3>
          <select className="input-field" value={assign.roleId} onChange={(e) => setAssign({ ...assign, roleId: e.target.value })} required>
            <option value="">Seleccionar rol</option>
            {roles.map((r) => <option key={r.id} value={r.id}>{r.name}</option>)}
          </select>
          <select className="input-field" value={assign.userId} onChange={(e) => setAssign({ ...assign, userId: e.target.value })} required>
            <option value="">Seleccionar usuario</option>
            {users.map((u) => <option key={u.id} value={u.id}>{u.fullName} ({u.email})</option>)}
          </select>
          {users.length === 0 && (
            <p className="text-xs text-amber-600">Crea usuarios en la pestaña Usuarios primero.</p>
          )}
          <button type="submit" className="btn-primary w-full" disabled={users.length === 0}>Asignar rol</button>
        </form>
      </div>

      <div className="card overflow-hidden !p-0">
        <table className="w-full text-left text-sm">
          <thead className="border-b border-slate-100 bg-slate-50/80">
            <tr>
              <th className="px-6 py-4 font-semibold text-slate-700">Nombre</th>
              <th className="px-6 py-4 font-semibold text-slate-700">Descripción</th>
              <th className="px-6 py-4 font-semibold text-slate-700">Estado</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {roles.map((r) => (
              <tr key={r.id} className="hover:bg-slate-50/50">
                <td className="px-6 py-4 font-medium text-slate-900">{r.name}</td>
                <td className="px-6 py-4 text-slate-600">{r.description}</td>
                <td className="px-6 py-4">
                  <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium ${r.isActive ? 'bg-emerald-100 text-emerald-700' : 'bg-slate-100 text-slate-600'}`}>
                    {r.isActive ? 'Activo' : 'Inactivo'}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
