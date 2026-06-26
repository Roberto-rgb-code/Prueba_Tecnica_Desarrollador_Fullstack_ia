import { FormEvent, useEffect, useState } from 'react';
import { User, usersApi } from '../api/client';

export default function UsersTab() {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [form, setForm] = useState({ email: '', firstName: '', lastName: '' });

  const load = async () => {
    setLoading(true);
    setError(null);
    try {
      const { data } = await usersApi.list();
      setUsers(data);
    } catch {
      setError('Error al cargar usuarios');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const handleCreate = async (e: FormEvent) => {
    e.preventDefault();
    try {
      await usersApi.create(form);
      setForm({ email: '', firstName: '', lastName: '' });
      await load();
    } catch {
      setError('Error al crear usuario');
    }
  };

  if (loading) return <div className="loading">Cargando usuarios...</div>;

  return (
    <div>
      <h2>Usuarios</h2>
      {error && <div className="error">{error}</div>}
      <form className="form-row" onSubmit={handleCreate}>
        <input placeholder="Email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
        <input placeholder="Nombre" value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} required />
        <input placeholder="Apellido" value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} required />
        <button type="submit">Crear</button>
      </form>
      <table>
        <thead><tr><th>Email</th><th>Nombre</th><th>Activo</th><th>Acciones</th></tr></thead>
        <tbody>
          {users.map((u) => (
            <tr key={u.id}>
              <td>{u.email}</td>
              <td>{u.fullName}</td>
              <td>{u.isActive ? 'Sí' : 'No'}</td>
              <td>
                <button onClick={async () => { await usersApi.update(u.id, { firstName: u.firstName, lastName: u.lastName, isActive: !u.isActive }); load(); }}>
                  {u.isActive ? 'Desactivar' : 'Activar'}
                </button>
                <button className="danger" onClick={async () => { await usersApi.remove(u.id); load(); }}>Eliminar</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
