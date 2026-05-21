import { useState } from 'react';
import type { FormEvent } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import { GraduationCap, KeyRound, Loader2, ShieldCheck } from 'lucide-react';
import { useAuth } from '../auth/AuthContext';
import { CredentialsPanel } from '../components/CredentialsPanel';
import { StatusBanner } from '../components/StatusBanner';
import { authApi, getApiErrorMessage } from '../services/api';
import type { TemporaryCredentials } from '../types/models';

function defaultPathForRole(role: string) {
  if (role === 'STUDENT') return '/enrollments';
  if (role === 'INSTRUCTOR') return '/gradebook';
  return '/departments';
}

export function LoginPage() {
  const { user, login } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [credentials, setCredentials] = useState<TemporaryCredentials | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [bootstrapping, setBootstrapping] = useState(false);

  if (user) {
    return <Navigate to={defaultPathForRole(user.role)} replace />;
  }

  const from = (location.state as { from?: { pathname?: string } } | null)?.from?.pathname;

  const handleLogin = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setBusy(true);
    setError(null);
    try {
      const result = await login(username, password);
      navigate(from && from !== '/login' ? from : defaultPathForRole(result.user.role), { replace: true });
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setBusy(false);
    }
  };

  const bootstrapAdmin = async () => {
    setBootstrapping(true);
    setError(null);
    try {
      const result = await authApi.bootstrapAdmin();
      setCredentials(result.credentials);
      setUsername(result.credentials.username);
      setPassword(result.credentials.temporaryPassword);
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setBootstrapping(false);
    }
  };

  return (
    <main className="auth-page">
      <section className="auth-panel">
        <div className="auth-brand">
          <span className="brand__mark">
            <GraduationCap size={24} aria-hidden="true" />
          </span>
          <div>
            <strong>GPA System</strong>
            <span>Secure access</span>
          </div>
        </div>

        <form className="form-panel auth-form" onSubmit={handleLogin}>
          <div className="form-panel__header">
            <h1>Sign in</h1>
            <ShieldCheck size={22} aria-hidden="true" />
          </div>

          {error && <StatusBanner tone="error">{error}</StatusBanner>}
          <CredentialsPanel credentials={credentials} label="Development admin credentials" />

          <label>
            <span>Username or Email</span>
            <input value={username} onChange={(event) => setUsername(event.target.value)} autoComplete="username" />
          </label>

          <label>
            <span>Password</span>
            <input
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              autoComplete="current-password"
            />
          </label>

          <div className="form-actions auth-actions">
            <button className="button button--ghost" type="button" onClick={bootstrapAdmin} disabled={bootstrapping}>
              {bootstrapping ? <Loader2 size={17} className="spin" /> : <KeyRound size={17} />}
              Bootstrap Admin
            </button>
            <button className="button button--primary" type="submit" disabled={busy}>
              {busy ? <Loader2 size={17} className="spin" /> : <ShieldCheck size={17} />}
              Sign In
            </button>
          </div>
        </form>
      </section>
    </main>
  );
}
