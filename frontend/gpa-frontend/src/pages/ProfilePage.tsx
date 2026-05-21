import { useState } from 'react';
import type { FormEvent } from 'react';
import { KeyRound, LogOut, Save } from 'lucide-react';
import { useAuth } from '../auth/AuthContext';
import { StatusBanner } from '../components/StatusBanner';
import { authApi, getApiErrorMessage } from '../services/api';

export function ProfilePage() {
  const { user, logout } = useAuth();
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess(null);
    try {
      await authApi.changePassword({ currentPassword, newPassword });
      setCurrentPassword('');
      setNewPassword('');
      setSuccess('Password changed successfully.');
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSaving(false);
    }
  };

  return (
    <section className="page">
      <div className="page__header">
        <div>
          <h1>Profile</h1>
          <p>{user?.displayName} - {user?.role}</p>
        </div>
        <button className="button button--ghost" type="button" onClick={logout}>
          <LogOut size={17} />
          Sign Out
        </button>
      </div>

      {error && <StatusBanner tone="error">{error}</StatusBanner>}
      {success && <StatusBanner tone="success">{success}</StatusBanner>}

      <form className="form-panel" onSubmit={submit}>
        <div className="form-panel__header">
          <h2>Change Password</h2>
          <KeyRound size={20} aria-hidden="true" />
        </div>

        <div className="form-grid form-grid--two">
          <label>
            <span>Current Password</span>
            <input
              type="password"
              value={currentPassword}
              onChange={(event) => setCurrentPassword(event.target.value)}
              autoComplete="current-password"
              required
            />
          </label>

          <label>
            <span>New Password</span>
            <input
              type="password"
              value={newPassword}
              onChange={(event) => setNewPassword(event.target.value)}
              autoComplete="new-password"
              required
              minLength={8}
            />
          </label>
        </div>

        <div className="form-actions">
          <button className="button button--primary" type="submit" disabled={saving}>
            <Save size={17} />
            Save Password
          </button>
        </div>
      </form>
    </section>
  );
}
