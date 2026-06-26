import { NavLink, useLocation } from 'react-router-dom';
import { Bot, ClipboardList, LogOut, Shield, Users } from 'lucide-react';
import { useAuthStore } from '../store/authStore';
import { cn } from '../lib/utils';

const tabs = [
  { id: 'users', label: 'Usuarios', icon: Users },
  { id: 'roles', label: 'Roles', icon: Shield },
  { id: 'audit', label: 'Auditoría', icon: ClipboardList },
  { id: 'agent', label: 'Agente IA', icon: Bot },
];

export default function Layout({ children }: { children: React.ReactNode }) {
  const logout = useAuthStore((s) => s.logout);
  const user = useAuthStore((s) => s.user);
  const location = useLocation();
  const currentTab = new URLSearchParams(location.search).get('tab') ?? 'users';

  return (
    <div className="min-h-screen bg-slate-50">
      <header className="sticky top-0 z-40 border-b border-slate-200/80 bg-white/90 backdrop-blur-md">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6">
          <div className="flex items-center gap-3">
            <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-gradient-to-br from-toka-600 to-indigo-600 text-white shadow-sm">
              <Users className="h-5 w-5" />
            </div>
            <div>
              <h1 className="text-lg font-bold text-slate-900">Toka</h1>
              <p className="text-xs text-slate-500">User Management</p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <div className="hidden text-right sm:block">
              <p className="text-sm font-medium text-slate-900">{user?.fullName ?? user?.email}</p>
              <p className="text-xs text-slate-500">{user?.email}</p>
            </div>
            <button onClick={logout} className="btn-danger !rounded-xl">
              <LogOut className="h-4 w-4" />
              <span className="hidden sm:inline">Salir</span>
            </button>
          </div>
        </div>

        <nav className="mx-auto flex max-w-7xl gap-1 overflow-x-auto px-4 pb-3 sm:px-6">
          {tabs.map(({ id, label, icon: Icon }) => (
            <NavLink
              key={id}
              to={id === 'users' ? '/' : `/?tab=${id}`}
              className={cn(
                'inline-flex shrink-0 items-center gap-2 rounded-xl px-4 py-2.5 text-sm font-medium transition',
                currentTab === id
                  ? 'bg-toka-600 text-white shadow-sm'
                  : 'text-slate-600 hover:bg-slate-100 hover:text-slate-900'
              )}
            >
              <Icon className="h-4 w-4" />
              {label}
            </NavLink>
          ))}
        </nav>
      </header>

      <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6">{children}</main>
    </div>
  );
}
