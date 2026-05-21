/* eslint-disable react-refresh/only-export-components */
import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { authApi, authTokenStore } from '../services/api';
import type { AuthUser, LoginResponse } from '../types/models';

const INACTIVITY_TIMEOUT_MS = 15 * 60 * 1000;

interface AuthContextValue {
  user: AuthUser | null;
  token: string | null;
  loading: boolean;
  login: (username: string, password: string) => Promise<LoginResponse>;
  logout: () => void;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(() => authTokenStore.get());
  const [user, setUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(true);

  const logout = useCallback(() => {
    authTokenStore.clear();
    setToken(null);
    setUser(null);
  }, []);

  const refreshUser = useCallback(async () => {
    const currentToken = authTokenStore.get();
    if (!currentToken) {
      setLoading(false);
      return;
    }

    try {
      const profile = await authApi.me();
      setToken(currentToken);
      setUser(profile);
    } catch {
      logout();
    } finally {
      setLoading(false);
    }
  }, [logout]);

  useEffect(() => {
    const timer = window.setTimeout(() => void refreshUser(), 0);
    return () => window.clearTimeout(timer);
  }, [refreshUser]);

  useEffect(() => {
    const handleExpired = () => logout();
    window.addEventListener('gpa-auth-expired', handleExpired);
    return () => window.removeEventListener('gpa-auth-expired', handleExpired);
  }, [logout]);

  useEffect(() => {
    if (!token || !user) {
      return undefined;
    }

    let timer = window.setTimeout(logout, INACTIVITY_TIMEOUT_MS);
    const resetTimer = () => {
      window.clearTimeout(timer);
      timer = window.setTimeout(logout, INACTIVITY_TIMEOUT_MS);
    };

    const events = ['click', 'keydown', 'mousemove', 'scroll', 'touchstart'];
    events.forEach((eventName) => window.addEventListener(eventName, resetTimer, { passive: true }));

    return () => {
      window.clearTimeout(timer);
      events.forEach((eventName) => window.removeEventListener(eventName, resetTimer));
    };
  }, [logout, token, user]);

  const login = useCallback(async (username: string, password: string) => {
    const response = await authApi.login(username, password);
    authTokenStore.set(response.token);
    setToken(response.token);
    setUser(response.user);
    return response;
  }, []);

  const value = useMemo(
    () => ({
      user,
      token,
      loading,
      login,
      logout,
      refreshUser,
    }),
    [loading, login, logout, refreshUser, token, user],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider.');
  }

  return context;
}
