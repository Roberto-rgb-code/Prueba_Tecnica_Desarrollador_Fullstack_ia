import { Link, useLocation } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';

export default function Layout({ children }: { children: React.ReactNode }) {
  const logout = useAuthStore((s) => s.logout);
  const user = useAuthStore((s) => s.user);
  const location = useLocation();

  const tabs = [
    { id: 'users', label: 'Usuarios' },
    { id: 'roles', label: 'Roles' },
    { id: 'audit', label: 'Auditoría' },
    { id: 'agent', label: 'Agente IA' },
  ];

  return (
    <div className="app">
      <header className="header">
        <h1>Toka User Management</h1>
        <div className="header-actions">
          <span>{user?.fullName ?? user?.email}</span>
          <button onClick={logout}>Salir</button>
        </div>
      </header>
      <nav className="nav">
        {tabs.map((tab) => (
          <Link key={tab.id} to={`/?tab=${tab.id}`} className={location.search.includes(tab.id) ? 'active' : ''}>
            {tab.label}
          </Link>
        ))}
      </nav>
      <main className="main">{children}</main>
    </div>
  );
}
