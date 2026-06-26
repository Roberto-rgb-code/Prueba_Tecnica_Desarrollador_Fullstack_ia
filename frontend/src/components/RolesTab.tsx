import { FormEvent, useEffect, useState } from 'react';
import { Role, rolesApi } from '../api/client';

export default function RolesTab() {
  const [roles, setRoles] = useState<Role[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState({ name: '', description: '' });
  const [assign, setAssign] = useState({ roleId: '', userId: '' });

  const load = async () => {
    setLoading(true);
    try {
      const { data } = await rolesApi.list();
      setRoles(data);
    } catch {
      setError('Error al cargar roles');
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
      await load();
    } catch {
      setError('Error al crear rol');
    }
  };

  const handleAssign = async (e: FormEvent) => {
    e.preventDefault();
    try {
      await rolesApi.assign(assign.roleId, assign.userId);
      setError(null);
      alert('Rol asignado correctamente');
    } catch {
      setError('Error al asignar rol');
    }
  };

  if (loading) return <div className="loading">Cargando roles...</div>;

  return (
    <div>
      <h2>Roles</h2>
      {error && <div className="error">{error}</div>}
      <form className="form-row" onSubmit={handleCreate}>
        <input placeholder="Nombre del rol" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
        <input placeholder="Descripción" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
        <button type="submit">Crear rol</button>
      </form>
      <form className="form-row" onSubmit={handleAssign}>
        <select value={assign.roleId} onChange={(e) => setAssign({ ...assign, roleId: e.target.value })} required>
          <option value="">Seleccionar rol</option>
          {roles.map((r) => <option key={r.id} value={r.id}>{r.name}</option>)}
        </select>
        <input placeholder="User ID (GUID)" value={assign.userId} onChange={(e) => setAssign({ ...assign, userId: e.target.value })} required />
        <button type="submit">Asignar rol</button>
      </form>
      <table>
        <thead><tr><th>Nombre</th><th>Descripción</th><th>Activo</th></tr></thead>
        <tbody>
          {roles.map((r) => (
            <tr key={r.id}><td>{r.name}</td><td>{r.description}</td><td>{r.isActive ? 'Sí' : 'No'}</td></tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
