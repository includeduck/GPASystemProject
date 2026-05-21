import { Link } from 'react-router-dom';
import { BarChart3, BookOpen, Building2, FileText, ShieldAlert, Trophy } from 'lucide-react';

const cards = [
  { to: '/reports/semester', label: 'Semester Results', icon: FileText, detail: 'All students for a term' },
  { to: '/reports/course', label: 'Course Performance', icon: BookOpen, detail: 'Grades and pass rates by course' },
  { to: '/reports/department', label: 'Department Performance', icon: Building2, detail: 'Department GPA rollups' },
  { to: '/reports/warnings', label: 'Warning List', icon: ShieldAlert, detail: 'Students below GPA threshold' },
  { to: '/reports/rankings', label: 'Class Rankings', icon: Trophy, detail: 'CGPA-based rankings' },
  { to: '/student-results', label: 'Student Transcripts', icon: BarChart3, detail: 'Individual academic records' },
];

export function ReportsHubPage() {
  return (
    <section className="page">
      <div className="page__header">
        <div>
          <h1>Reports</h1>
          <p>Generate and export academic performance reports</p>
        </div>
      </div>
      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fill, minmax(260px, 1fr))',
          gap: 16,
        }}
      >
        {cards.map((card) => {
          const Icon = card.icon;
          return (
            <Link
              key={card.to}
              to={card.to}
              className="form-panel"
              style={{ textDecoration: 'none', color: 'inherit', padding: '1.25rem' }}
            >
              <div style={{ display: 'flex', gap: 12, alignItems: 'flex-start' }}>
                <Icon size={22} style={{ color: 'var(--accent)', flexShrink: 0 }} />
                <div>
                  <strong>{card.label}</strong>
                  <p style={{ margin: '0.35rem 0 0', fontSize: '0.9rem', color: 'var(--text-muted)' }}>
                    {card.detail}
                  </p>
                </div>
              </div>
            </Link>
          );
        })}
      </div>
    </section>
  );
}
