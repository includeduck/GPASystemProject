import { KeyRound } from 'lucide-react';
import type { TemporaryCredentials } from '../types/models';

interface CredentialsPanelProps {
  credentials: TemporaryCredentials | null;
  label: string;
}

export function CredentialsPanel({ credentials, label }: CredentialsPanelProps) {
  if (!credentials) {
    return null;
  }

  return (
    <div className="credentials-panel">
      <div className="credentials-panel__heading">
        <KeyRound size={18} aria-hidden="true" />
        <strong>{label}</strong>
      </div>
      <dl>
        <div>
          <dt>Username</dt>
          <dd>{credentials.username}</dd>
        </div>
        <div>
          <dt>Temporary Password</dt>
          <dd>{credentials.temporaryPassword}</dd>
        </div>
      </dl>
    </div>
  );
}
