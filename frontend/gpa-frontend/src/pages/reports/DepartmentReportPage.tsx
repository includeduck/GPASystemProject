import { useEffect, useState } from 'react';
import { Building2, Download } from 'lucide-react';
import { EmptyState } from '../../components/EmptyState';
import { StatusBanner } from '../../components/StatusBanner';
import { departmentApi, getApiErrorMessage, reportApi, semesterApi } from '../../services/api';
import type { Department, DepartmentPerformanceReport, Semester } from '../../types/models';

export function DepartmentReportPage() {
  const [departments, setDepartments] = useState<Department[]>([]);
  const [semesters, setSemesters] = useState<Semester[]>([]);
  const [departmentId, setDepartmentId] = useState('');
  const [semesterId, setSemesterId] = useState('');
  const [report, setReport] = useState<DepartmentPerformanceReport | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let ignore = false;
    void (async () => {
      try {
        const [d, s] = await Promise.all([departmentApi.list(), semesterApi.list()]);
        if (!ignore) {
          setDepartments(d);
          setSemesters(s);
          if (d[0]) setDepartmentId(String(d[0].departmentId));
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
    if (!departmentId) return;
    let ignore = false;
    void (async () => {
      setLoading(true);
      try {
        const data = await reportApi.getDepartmentPerformance(
          Number(departmentId),
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
        <h1 style={{ display: 'flex', alignItems: 'center', gap: 10 }}><Building2 size={28} /> Department Performance</h1>
        {departmentId && (
          <button className="button button--secondary" type="button" onClick={() => reportApi.downloadDepartmentCsv(Number(departmentId), semesterId ? Number(semesterId) : undefined)}>
            <Download size={17} /> Export CSV
          </button>
        )}
      </div>
      {error && <StatusBanner tone="error">{error}</StatusBanner>}
      <div className="form-grid form-grid--two" style={{ marginBottom: '1rem' }}>
        <label><span>Department</span>
          <select value={departmentId} onChange={(e) => setDepartmentId(e.target.value)}>
            {departments.map((d) => <option key={d.departmentId} value={d.departmentId}>{d.departmentCode}</option>)}
          </select>
        </label>
        <label><span>Semester (optional)</span>
          <select value={semesterId} onChange={(e) => setSemesterId(e.target.value)}>
            <option value="">Latest records</option>
            {semesters.map((s) => <option key={s.semesterId} value={s.semesterId}>{s.semesterName}</option>)}
          </select>
        </label>
      </div>
      {loading || !report ? (
        <EmptyState title="Loading..." />
      ) : (
        <>
          <div className="form-panel" style={{ marginBottom: '1rem' }}>
            <p>{report.departmentName} — Students: {report.studentCount} | Avg GPA: {report.averageSemesterGpa.toFixed(2)} | Pass rate: {report.passRate.toFixed(1)}%</p>
          </div>
          <div className="table-wrap">
            <table>
              <thead><tr><th>Student</th><th>Semester GPA</th><th>CGPA</th></tr></thead>
              <tbody>
                {report.students.map((s) => (
                  <tr key={s.studentId}>
                    <td>{s.fullName}<br /><small>{s.studentNumber}</small></td>
                    <td>{s.semesterGpa.toFixed(2)}</td>
                    <td>{s.cumulativeGpa.toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </section>
  );
}
