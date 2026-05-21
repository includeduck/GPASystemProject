import { Navigate, useLocation } from 'react-router-dom';
import type { ReactNode } from 'react';
import { EmptyState } from './EmptyState';
import { useAuth } from '../auth/AuthContext';
import type { AuthRole } from '../types/models';

interface ProtectedRouteProps {
  children: ReactNode;
  roles?: AuthRole[];
}

export function ProtectedRoute({ children, roles }: ProtectedRouteProps) {
  const { user, loading } = useAuth();
  const location = useLocation();

  if (loading) {
    return <EmptyState title="Checking session..." />;
  }

  if (!user) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  if (roles && !roles.includes(user.role)) {
    return <EmptyState title="Access denied" detail="Your account does not have access to this area." />;
  }

  return <>{children}</>;
}
