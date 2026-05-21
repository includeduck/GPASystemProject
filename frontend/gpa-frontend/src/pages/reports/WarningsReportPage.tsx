import { useEffect, useState } from 'react';
import { Download, ShieldAlert } from 'lucide-react';
import { EmptyState } from '../../components/EmptyState';
import { StatusBanner } from '../../components/StatusBanner';
import { getApiErrorMessage, reportApi, semesterApi } from '../../services/api';
import type { Semester, WarningListReport } from '../../types/models';

export function WarningsReportPage() {
  const [semesters, setSemesters] = useState<Semester[]>([]);
  const [semesterId, setSemesterId] = useState('');
  const [threshold, setThreshold] = useState('2.0');
  const [report, setReport] = useState<WarningListReport | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let ignore = false;
    void (async () => {
      try {
        const data = await semesterApi.list();
        if (!ignore) {
          setSemesters(data);
          const current = data.find((s) => s.isCurrent) ?? data[0];
          if (current) setSemesterId(String(current.semesterId));
        }
      } catch (err) {
        if (!ignore) setError(getApiErrorMessage(err));
      } finally {
        if (!ignore) setLoading(false);
      }
    })();
    return () => { ignore = true; };
  }, []);

  useEffect(() => {
    if (!semesterId) return;
    let ignore = false;
    void (async () => {
      setLoading(true);
      try {
        const data = await reportApi.getWarnings(Number(semesterId), Number(threshold));
        if (!ignore) { setReport(data); setError(null); }
      } catch (err) {
        if (!ignore) setError(getApiErrorMessage(err));
      } finally {
        if (!ignore) setLoading(false);
      }
    })();
    return () => { ignore = true; };
  }, [semesterId, threshold]);

  return (
    <section className="page">
      <div className="page__header">
        <h1 style={{ display: 'flex', alignItems: 'center', gap: 10 }}><ShieldAlert size={28} /> Warning List</h1>
        {semesterId && (
          <button className="button button--secondary" type="button" onClick={() => reportApi.downloadWarningsCsv(Number(semesterId), Number(threshold))}>
            <Download size={17} /> Export CSV
          </button>
        )}
      </div>
      {error && <StatusBanner tone="error">{error}</StatusBanner>}
      <div className="form-grid form-grid--two" style={{ marginBottom: '1rem' }}>
        <label><span>Semester</span>
          <select value={semesterId} onChange={(e) => setSemesterId(e.target.value)}>
            {semesters.map((s) => <option key={s.semesterId} value={s.semesterId}>{s.semesterName}</option>)}
          </select>
        </label>
        <label><span>GPA Threshold</span>
          <input type="number" step="0.1" min={0} max={4} value={threshold} onChange={(e) => setThreshold(e.target.value)} />
        </label>
      </div>
      {loading ? (
        <EmptyState title="Loading..." />
      ) : !report || report.students.length === 0 ? (
        <EmptyState title="No students below threshold" detail={`All students meet the ${threshold} GPA minimum.`} />
      ) : (
        <div className="table-wrap">
          <table>
            <thead><tr><th>Student</th><th>Department</th><th>Semester GPA</th><th>CGPA</th></tr></thead>
            <tbody>
              {report.students.map((s) => (
                <tr key={s.studentId}>
                  <td>{s.fullName}<br /><small>{s.studentNumber}</small></td>
                  <td>{s.departmentCode}</td>
                  <td style={{ color: 'var(--danger)' }}>{s.semesterGpa.toFixed(2)}</td>
                  <td>{s.cumulativeGpa.toFixed(2)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
