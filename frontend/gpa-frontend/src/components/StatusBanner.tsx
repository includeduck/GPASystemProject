import type { ReactNode } from 'react';

interface StatusBannerProps {
  tone: 'success' | 'error' | 'info';
  children: ReactNode;
}

export function StatusBanner({ tone, children }: StatusBannerProps) {
  return <div className={`status-banner status-banner--${tone}`}>{children}</div>;
}
