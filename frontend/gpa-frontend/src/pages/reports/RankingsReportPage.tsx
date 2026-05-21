import { useEffect, useState } from 'react';
import { Download, Trophy } from 'lucide-react';
import { EmptyState } from '../../components/EmptyState';
import { StatusBanner } from '../../components/StatusBanner';
import { departmentApi, getApiErrorMessage, reportApi, semesterApi } from '../../services/api';
import type { ClassRankingsReport, Department, Semester } from '../../types/models';

export function RankingsReportPage() {
  const [departments, setDepartments] = useState<Department[]>([]);
  const [semesters, setSemesters] = useState<Semester[]>([]);
  const [departmentId, setDepartmentId] = useState('');
  const [semesterId, setSemesterId] = useState('');
  const [report, setReport] = useState<ClassRankingsReport | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let ignore = false;
    void (async () => {
      try {
        const [d, s] = await Promise.all([departmentApi.list(), semesterApi.list()]);
        if (!ignore) { setDepartments(d); setSemesters(s); }
      } catch (err) {
        if (!ignore) setError(getApiErrorMessage(err));
      } finally {
        if (!ignore) setLoading(false);
      }
    })();
    return () => { ignore = true; };
  }, []);

  useEffect(() => {
    let ignore = false;
    void (async () => {
      setLoading(true);
      try {
        const data = await reportApi.getRankings(
          departmentId ? Number(departmentId) : undefined,
          semesterId ? Number(semesterId) : undefined,
        );
        if (!ignore) { setReport(data); setError(null); }
      } catch (err) {
        if (!ignore) setError(getApiErrorMessage(err));
      } finally {
        if (!ignore) setLoading(false);
      }
    })();
    return () => { ignore = true; };
  }, [departmentId, semesterId]);

  return (
    <section className="page">
      <div className="page__header">
        <h1 style={{ display: 'flex', alignItems: 'center', gap: 10 }}><Trophy size={28} /> Class Rankings</h1>
        <button className="button button--secondary" type="button" onClick={() => reportApi.downloadRankingsCsv(departmentId ? Number(departmentId) : undefined, semesterId ? Number(semesterId) : undefined)}>
          <Download size={17} /> Export CSV
        </button>
      </div>
      {error && <StatusBanner tone="error">{error}</StatusBanner>}
      <div className="form-grid form-grid--two" style={{ marginBottom: '1rem' }}>
        <label><span>Department (optional)</span>
          <select value={departmentId} onChange={(e) => setDepartmentId(e.target.value)}>
            <option value="">All departments</option>
            {departments.map((d) => <option key={d.departmentId} value={d.departmentId}>{d.departmentCode}</option>)}
          </select>
        </label>
        <label><span>Semester (optional)</span>
          <select value={semesterId} onChange={(e) => setSemesterId(e.target.value)}>
            <option value="">Latest CGPA</option>
            {semesters.map((s) => <option key={s.semesterId} value={s.semesterId}>{s.semesterName}</option>)}
          </select>
        </label>
      </div>
      {loading ? (
        <EmptyState title="Loading..." />
      ) : !report || report.rankings.length === 0 ? (
        <EmptyState title="No rankings available" />
      ) : (
        <div className="table-wrap">
          <table>
            <thead><tr><th>Rank</th><th>Student</th><th>Department</th><th>CGPA</th></tr></thead>
            <tbody>
              {report.rankings.map((r) => (
                <tr key={r.studentId}>
                  <td><strong>#{r.rank}</strong></td>
                  <td>{r.fullName}<br /><small>{r.studentNumber}</small></td>
                  <td>{r.departmentCode}</td>
                  <td>{r.cgpa.toFixed(2)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
