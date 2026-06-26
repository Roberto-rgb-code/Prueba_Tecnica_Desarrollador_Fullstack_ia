import { FormEvent, useState } from 'react';
import { DotLottieReact } from '@lottiefiles/dotlottie-react';
import { LogIn, UserPlus, Sparkles } from 'lucide-react';
import { Navigate } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';

export default function LoginPage() {
  const { token, login, register, loading, error } = useAuthStore();
  const [isRegister, setIsRegister] = useState(false);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [fullName, setFullName] = useState('');

  if (token) return <Navigate to="/" replace />;

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    if (isRegister) await register(email, password, fullName);
    else await login(email, password);
  };

  return (
    <div className="flex min-h-screen">
      {/* Lottie — mitad izquierda / centro en desktop */}
      <div className="relative hidden w-1/2 items-center justify-center overflow-hidden bg-gradient-to-br from-toka-950 via-toka-800 to-indigo-900 lg:flex">
        <div className="absolute inset-0 bg-[radial-gradient(circle_at_30%_20%,rgba(255,255,255,0.12),transparent_50%)]" />
        <div className="relative z-10 flex flex-col items-center px-12 text-center">
          <div className="mb-6 flex h-14 w-14 items-center justify-center rounded-2xl bg-white/10 backdrop-blur-sm">
            <Sparkles className="h-7 w-7 text-indigo-200" />
          </div>
          <h1 className="mb-2 text-3xl font-bold text-white">Toka</h1>
          <p className="mb-8 max-w-sm text-indigo-200/90">
            Gestión de usuarios con microservicios e inteligencia artificial
          </p>
          <div className="h-72 w-72">
            <DotLottieReact
              src="https://lottie.host/1dd66271-eb51-4ee0-87f6-2214a21baca6/pjhGQt3fjn.lottie"
              loop
              autoplay
            />
          </div>
        </div>
      </div>

      {/* Formulario */}
      <div className="flex flex-1 flex-col items-center justify-center px-6 py-12">
        {/* Lottie centrado en móvil */}
        <div className="mb-8 h-48 w-48 lg:hidden">
          <DotLottieReact
            src="https://lottie.host/1dd66271-eb51-4ee0-87f6-2214a21baca6/pjhGQt3fjn.lottie"
            loop
            autoplay
          />
        </div>

        <div className="w-full max-w-md">
          <div className="mb-8 text-center lg:text-left">
            <h2 className="text-2xl font-bold text-slate-900">
              {isRegister ? 'Crea tu cuenta' : 'Bienvenido de nuevo'}
            </h2>
            <p className="mt-2 text-sm text-slate-500">
              {isRegister
                ? 'Regístrate para acceder al panel de administración'
                : 'Inicia sesión en Toka User Management'}
            </p>
          </div>

          <form className="card space-y-4" onSubmit={handleSubmit}>
            {error && (
              <div className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                {error}
              </div>
            )}

            {isRegister && (
              <div>
                <label className="mb-1.5 block text-sm font-medium text-slate-700">Nombre completo</label>
                <input
                  className="input-field"
                  placeholder="Kevin Torres"
                  value={fullName}
                  onChange={(e) => setFullName(e.target.value)}
                  required
                />
              </div>
            )}

            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700">Email</label>
              <input
                type="email"
                className="input-field"
                placeholder="tu@email.com"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>

            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700">Contraseña</label>
              <input
                type="password"
                className="input-field"
                placeholder="Mínimo 6 caracteres"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                minLength={6}
                required
              />
            </div>

            <button type="submit" className="btn-primary w-full" disabled={loading}>
              {loading ? (
                'Procesando...'
              ) : isRegister ? (
                <>
                  <UserPlus className="h-4 w-4" /> Registrarse
                </>
              ) : (
                <>
                  <LogIn className="h-4 w-4" /> Entrar
                </>
              )}
            </button>

            <button
              type="button"
              className="w-full text-center text-sm font-medium text-toka-600 hover:text-toka-700"
              onClick={() => setIsRegister(!isRegister)}
            >
              {isRegister ? '¿Ya tienes cuenta? Inicia sesión' : '¿No tienes cuenta? Regístrate'}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
