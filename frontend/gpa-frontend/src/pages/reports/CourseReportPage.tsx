import { useEffect, useState } from 'react';
import { BookOpen, Download } from 'lucide-react';
import { EmptyState } from '../../components/EmptyState';
import { StatusBanner } from '../../components/StatusBanner';
import { courseApi, getApiErrorMessage, reportApi, semesterApi } from '../../services/api';
import type { Course, CoursePerformanceReport, Semester } from '../../types/models';

export function CourseReportPage() {
  const [courses, setCourses] = useState<Course[]>([]);
  const [semesters, setSemesters] = useState<Semester[]>([]);
  const [courseId, setCourseId] = useState('');
  const [semesterId, setSemesterId] = useState('');
  const [report, setReport] = useState<CoursePerformanceReport | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let ignore = false;
    void (async () => {
      try {
        const [c, s] = await Promise.all([courseApi.list(), semesterApi.list()]);
        if (!ignore) {
          setCourses(c);
          setSemesters(s);
          if (c[0]) setCourseId(String(c[0].courseId));
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
    if (!courseId) return;
    let ignore = false;
    void (async () => {
      setLoading(true);
      try {
        const data = await reportApi.getCoursePerformance(
          Number(courseId),
          semesterId ? Number(semesterId) : undefined,
        );
        if (!ignore) {
          setReport(data);
          setError(null);
        }
      } catch (err) {
        if (!ignore) setError(getApiErrorMessage(err));
      } finally {
        if (!ignore) setLoading(false);
      }
    })();
    return () => { ignore = true; };
  }, [courseId, semesterId]);

  return (
    <section className="page">
      <div className="page__header">
        <h1 style={{ display: 'flex', alignItems: 'center', gap: 10 }}><BookOpen size={28} /> Course Performance</h1>
        {courseId && (
          <button className="button button--secondary" type="button" onClick={() => reportApi.downloadCourseCsv(Number(courseId), semesterId ? Number(semesterId) : undefined)}>
            <Download size={17} /> Export CSV
          </button>
        )}
      </div>
      {error && <StatusBanner tone="error">{error}</StatusBanner>}
      <div className="form-grid form-grid--two" style={{ marginBottom: '1rem' }}>
        <label><span>Course</span>
          <select value={courseId} onChange={(e) => setCourseId(e.target.value)}>
            {courses.map((c) => <option key={c.courseId} value={c.courseId}>{c.courseCode} - {c.courseTitle}</option>)}
          </select>
        </label>
        <label><span>Semester (optional)</span>
          <select value={semesterId} onChange={(e) => setSemesterId(e.target.value)}>
            <option value="">All semesters</option>
            {semesters.map((s) => <option key={s.semesterId} value={s.semesterId}>{s.semesterName}</option>)}
          </select>
        </label>
      </div>
      {loading || !report ? (
        <EmptyState title="Loading..." />
      ) : (
        <>
          <div className="form-panel" style={{ marginBottom: '1rem' }}>
            <p><strong>{report.courseCode}</strong> — {report.courseTitle}</p>
            <p>Enrollments: {report.totalEnrollments} | Passed: {report.passedCount} | Failed: {report.failedCount} | Avg: {report.averagePercentage.toFixed(1)}%</p>
          </div>
          {report.offerings.length === 0 ? (
            <EmptyState title="No finalized offerings" />
          ) : (
            <div className="table-wrap">
              <table>
                <thead><tr><th>Semester</th><th>Instructor</th><th>Enrollments</th><th>Avg %</th></tr></thead>
                <tbody>
                  {report.offerings.map((o) => (
                    <tr key={o.offeringId}>
                      <td>{o.semesterName}</td><td>{o.instructorName}</td><td>{o.enrollmentCount}</td><td>{o.averagePercentage.toFixed(1)}%</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}
    </section>
  );
}
