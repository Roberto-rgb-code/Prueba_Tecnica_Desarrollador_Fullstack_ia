import { FormEvent, useState } from 'react';
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
    <div className="login-page">
      <form className="login-card" onSubmit={handleSubmit}>
        <h2>{isRegister ? 'Registrarse' : 'Iniciar sesión'}</h2>
        {error && <div className="error">{error}</div>}
        {isRegister && (
          <input placeholder="Nombre completo" value={fullName} onChange={(e) => setFullName(e.target.value)} required />
        )}
        <input type="email" placeholder="Email" value={email} onChange={(e) => setEmail(e.target.value)} required />
        <input type="password" placeholder="Contraseña (min 6)" value={password} onChange={(e) => setPassword(e.target.value)} minLength={6} required />
        <button type="submit" disabled={loading}>{loading ? 'Cargando...' : isRegister ? 'Registrarse' : 'Entrar'}</button>
        <button type="button" className="link-btn" onClick={() => setIsRegister(!isRegister)}>
          {isRegister ? '¿Ya tienes cuenta? Inicia sesión' : '¿No tienes cuenta? Regístrate'}
        </button>
      </form>
    </div>
  );
}
