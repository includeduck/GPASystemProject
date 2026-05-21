import { useEffect, useState } from 'react';
import { Download, FileText } from 'lucide-react';
import { EmptyState } from '../../components/EmptyState';
import { StatusBanner } from '../../components/StatusBanner';
import { getApiErrorMessage, reportApi, semesterApi } from '../../services/api';
import type { Semester, SemesterResultsReport } from '../../types/models';

export function SemesterReportPage() {
  const [semesters, setSemesters] = useState<Semester[]>([]);
  const [semesterId, setSemesterId] = useState('');
  const [report, setReport] = useState<SemesterResultsReport | null>(null);
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
      setError(null);
      try {
        const data = await reportApi.getSemesterResults(Number(semesterId));
        if (!ignore) setReport(data);
      } catch (err) {
        if (!ignore) setError(getApiErrorMessage(err));
      } finally {
        if (!ignore) setLoading(false);
      }
    })();
    return () => { ignore = true; };
  }, [semesterId]);

  return (
    <section className="page">
      <div className="page__header">
        <div>
          <h1 style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
            <FileText size={28} /> Semester Results
          </h1>
        </div>
        {semesterId && (
          <button className="button button--secondary" type="button" onClick={() => reportApi.downloadSemesterCsv(Number(semesterId))}>
            <Download size={17} /> Export CSV
          </button>
        )}
      </div>
      {error && <StatusBanner tone="error">{error}</StatusBanner>}
      <div className="form-panel" style={{ marginBottom: '1rem' }}>
        <label>
          <span>Semester</span>
          <select value={semesterId} onChange={(e) => setSemesterId(e.target.value)}>
            {semesters.map((s) => (
              <option key={s.semesterId} value={s.semesterId}>{s.semesterName}</option>
            ))}
          </select>
        </label>
      </div>
      {loading ? (
        <EmptyState title="Loading report..." />
      ) : !report || report.students.length === 0 ? (
        <EmptyState title="No semester results" detail="Finalize grades to populate academic records." />
      ) : (
        <div className="table-panel">
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Student</th>
                  <th>Department</th>
                  <th>Semester GPA</th>
                  <th>CGPA</th>
                  <th>Credits</th>
                </tr>
              </thead>
              <tbody>
                {report.students.map((s) => (
                  <tr key={s.studentId}>
                    <td>{s.fullName}<br /><small>{s.studentNumber}</small></td>
                    <td>{s.departmentCode}</td>
                    <td>{s.semesterGpa.toFixed(2)}</td>
                    <td>{s.cumulativeGpa.toFixed(2)}</td>
                    <td>{s.creditsAttempted}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </section>
  );
}
