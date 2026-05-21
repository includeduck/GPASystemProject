import { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  ArrowLeft,
  BarChart3,
  BookOpen,
  ChevronDown,
  ChevronRight,
  GraduationCap,
  Medal,
  RefreshCcw,
  TrendingUp,
  AlertTriangle,
  Award,
} from 'lucide-react';
import { StatusBanner } from '../components/StatusBanner';
import { EmptyState } from '../components/EmptyState';
import { studentResultsApi, getApiErrorMessage, studentApi } from '../services/api';
import type { StudentDashboardResponse, SemesterResultResponse, Student } from '../types/models';

/* ────────────────────────────── helpers ────────────────────────────── */

const gradeColor = (letter: string): { bg: string; fg: string } => {
  if (letter === 'F') return { bg: 'var(--danger-soft)', fg: 'var(--danger)' };
  if (letter.startsWith('A')) return { bg: 'var(--success-soft)', fg: 'var(--success)' };
  if (letter.startsWith('B')) return { bg: '#dbeafe', fg: '#1d4ed8' };
  if (letter.startsWith('C')) return { bg: '#fef3c7', fg: '#b45309' };
  return { bg: 'var(--surface-muted)', fg: 'var(--text-muted)' };
};

const gpaGradient = (gpa: number): string => {
  if (gpa >= 3.5) return 'linear-gradient(135deg, #047857, #065f46)';
  if (gpa >= 3.0) return 'linear-gradient(135deg, #2563eb, #1d4ed8)';
  if (gpa >= 2.0) return 'linear-gradient(135deg, #b45309, #92400e)';
  return 'linear-gradient(135deg, #b91c1c, #991b1b)';
};

/* ────────────────────────────── component ────────────────────────────── */

export function StudentDashboardPage() {
  const { studentId: rawId } = useParams<{ studentId: string }>();
  const studentId = Number(rawId);
  const navigate = useNavigate();

  const [data, setData] = useState<StudentDashboardResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [expandedSemesters, setExpandedSemesters] = useState<Record<number, boolean>>({});

  // fallback student selection directory
  const [allStudents, setAllStudents] = useState<Student[]>([]);
  const [searchQuery, setSearchQuery] = useState('');

  const loadData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await studentResultsApi.getDashboard(studentId);
      setData(result);
      // Auto-expand latest semester
      if (result.semesters.length > 0) {
        const latest = result.semesters[result.semesters.length - 1];
        setExpandedSemesters({ [latest.semesterId]: true });
      }
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }, [studentId]);

  useEffect(() => {
    if (studentId) {
      void loadData();
    } else {
      setLoading(true);
      studentApi.list()
        .then(setAllStudents)
        .catch((err) => setError(getApiErrorMessage(err)))
        .finally(() => setLoading(false));
    }
  }, [studentId, loadData]);

  const toggleSemester = (semId: number) => {
    setExpandedSemesters((prev) => ({ ...prev, [semId]: !prev[semId] }));
  };

  if (!rawId || Number.isNaN(studentId)) {
    const filtered = allStudents.filter((s) =>
      s.fullName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      s.studentNumber.toLowerCase().includes(searchQuery.toLowerCase()) ||
      s.departmentName.toLowerCase().includes(searchQuery.toLowerCase())
    );

    return (
      <section className="page" style={{ maxWidth: 1200 }}>
        <div className="page__header">
          <div>
            <h1 style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              <GraduationCap size={30} style={{ color: 'var(--accent)' }} />
              Academic Records Directory
            </h1>
            <p>Select a student to view their semester GPAs, CGPA, and complete academic transcript.</p>
          </div>
        </div>

        {error && <StatusBanner tone="error">{error}</StatusBanner>}

        <div className="form-panel">
          <label>
            <span>Search Students</span>
            <input
              type="text"
              placeholder="Search by student number, full name, or department..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
            />
          </label>
        </div>

        <div className="table-panel">
          {loading ? (
            <EmptyState title="Loading students..." />
          ) : filtered.length === 0 ? (
            <EmptyState
              title="No matching students found"
              detail="Try a different search query or select from the Students page."
            />
          ) : (
            <div className="table-wrap">
              <table>
                <thead>
                  <tr>
                    <th>Student</th>
                    <th>Email / Username</th>
                    <th>Department</th>
                    <th>Status</th>
                    <th className="table-actions">Action</th>
                  </tr>
                </thead>
                <tbody>
                  {filtered.map((std) => (
                    <tr key={std.studentId}>
                      <td>
                        <strong>{std.fullName}</strong>
                        <span>{std.studentNumber}</span>
                      </td>
                      <td>
                        {std.email}
                        <span>{std.username}</span>
                      </td>
                      <td>
                        {std.departmentCode} &mdash; {std.departmentName}
                      </td>
                      <td>
                        <span className={`badge badge--${std.status.toLowerCase()}`}>
                          {std.status}
                        </span>
                      </td>
                      <td className="table-actions">
                        <button
                          className="button button--ghost"
                          onClick={() => navigate(`/student-results/${std.studentId}`)}
                          style={{ minHeight: 34, fontSize: 13 }}
                        >
                          View Transcript
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </section>
    );
  }

  /* Count failed and repeated courses */
  const failedCourses =
    data?.semesters.flatMap((s) => s.courses.filter((c) => c.letterGrade === 'F' && !c.isRepeatedAttempt)) ?? [];
  const repeatedCourses =
    data?.semesters.flatMap((s) => s.courses.filter((c) => c.isRepeatedAttempt)) ?? [];

  return (
    <section className="page" style={{ maxWidth: 1200 }}>
      {/* Header */}
      <div className="page__header">
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 4 }}>
            <button
              className="icon-button"
              onClick={() => navigate('/students')}
              aria-label="Back to students"
              style={{ flexShrink: 0 }}
            >
              <ArrowLeft size={18} />
            </button>
            <h1 style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              <GraduationCap size={30} style={{ color: 'var(--accent)' }} />
              Academic Transcript
            </h1>
          </div>
          {data && (
            <p style={{ marginLeft: 50 }}>
              {data.fullName} &middot; {data.studentNumber}
            </p>
          )}
        </div>
      </div>

      {error && <StatusBanner tone="error">{error}</StatusBanner>}

      {loading ? (
        <EmptyState title="Loading academic records..." />
      ) : !data ? (
        <EmptyState title="No data available" detail="Could not load student academic records." />
      ) : (
        <>
          {/* ─── KPI Cards ─── */}
          <div
            style={{
              display: 'grid',
              gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
              gap: 14,
            }}
          >
            {/* CGPA Card */}
            <div
              style={{
                background: gpaGradient(data.cgpa),
                color: '#fff',
                borderRadius: 12,
                padding: '22px 20px',
                display: 'grid',
                gap: 4,
                position: 'relative',
                overflow: 'hidden',
              }}
            >
              <div style={{ position: 'absolute', top: 14, right: 16, opacity: 0.2 }}>
                <Medal size={48} />
              </div>
              <span style={{ fontSize: 13, opacity: 0.85, fontWeight: 600 }}>Cumulative CGPA</span>
              <span style={{ fontSize: 36, fontWeight: 800, lineHeight: 1.1 }}>{data.cgpa.toFixed(2)}</span>
              <span style={{ fontSize: 12, opacity: 0.7 }}>/ 4.00 scale</span>
            </div>

            {/* Latest GPA */}
            <div
              style={{
                background: 'var(--surface)',
                border: '1px solid var(--border)',
                borderRadius: 12,
                padding: '22px 20px',
                display: 'grid',
                gap: 4,
                position: 'relative',
                overflow: 'hidden',
              }}
            >
              <div style={{ position: 'absolute', top: 14, right: 16, opacity: 0.1 }}>
                <TrendingUp size={48} />
              </div>
              <span style={{ fontSize: 13, color: 'var(--text-muted)', fontWeight: 600 }}>Latest Semester GPA</span>
              <span style={{ fontSize: 36, fontWeight: 800, lineHeight: 1.1, color: 'var(--accent)' }}>
                {data.semesters.length > 0 ? data.semesters[data.semesters.length - 1].gpa.toFixed(2) : '—'}
              </span>
              <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>
                {data.semesters.length > 0 ? data.semesters[data.semesters.length - 1].semesterName : ''}
              </span>
            </div>

            {/* Credits Attempted */}
            <div
              style={{
                background: 'var(--surface)',
                border: '1px solid var(--border)',
                borderRadius: 12,
                padding: '22px 20px',
                display: 'grid',
                gap: 4,
                position: 'relative',
                overflow: 'hidden',
              }}
            >
              <div style={{ position: 'absolute', top: 14, right: 16, opacity: 0.1 }}>
                <BarChart3 size={48} />
              </div>
              <span style={{ fontSize: 13, color: 'var(--text-muted)', fontWeight: 600 }}>Credits Attempted</span>
              <span style={{ fontSize: 36, fontWeight: 800, lineHeight: 1.1 }}>{data.totalCreditsAttempted}</span>
              <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>total credit hours</span>
            </div>

            {/* Credits Earned */}
            <div
              style={{
                background: 'var(--surface)',
                border: '1px solid var(--border)',
                borderRadius: 12,
                padding: '22px 20px',
                display: 'grid',
                gap: 4,
                position: 'relative',
                overflow: 'hidden',
              }}
            >
              <div style={{ position: 'absolute', top: 14, right: 16, opacity: 0.1 }}>
                <Award size={48} />
              </div>
              <span style={{ fontSize: 13, color: 'var(--text-muted)', fontWeight: 600 }}>Credits Earned</span>
              <span style={{ fontSize: 36, fontWeight: 800, lineHeight: 1.1, color: 'var(--success)' }}>{data.totalCreditsEarned}</span>
              <span style={{ fontSize: 12, color: 'var(--text-muted)' }}>
                {data.totalCreditsAttempted > 0
                  ? `${((data.totalCreditsEarned / data.totalCreditsAttempted) * 100).toFixed(0)}% completion`
                  : '—'}
              </span>
            </div>
          </div>

          {/* ─── Warning Cards ─── */}
          {failedCourses.length > 0 && (
            <div
              style={{
                padding: '14px 18px',
                background: 'rgba(239, 68, 68, 0.06)',
                borderLeft: '4px solid #ef4444',
                borderRadius: 8,
              }}
            >
              <div style={{ display: 'flex', gap: 10, alignItems: 'flex-start' }}>
                <AlertTriangle size={18} style={{ color: '#ef4444', flexShrink: 0, marginTop: 2 }} />
                <div>
                  <strong style={{ color: '#ef4444' }}>
                    {failedCourses.length} Failed Course{failedCourses.length !== 1 ? 's' : ''}
                  </strong>
                  <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6, marginTop: 6 }}>
                    {failedCourses.map((c, i) => (
                      <span
                        key={i}
                        style={{
                          display: 'inline-flex',
                          alignItems: 'center',
                          gap: 4,
                          padding: '3px 10px',
                          background: 'var(--danger-soft)',
                          color: 'var(--danger)',
                          borderRadius: 6,
                          fontSize: 13,
                          fontWeight: 600,
                        }}
                      >
                        {c.courseCode}
                      </span>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          )}

          {repeatedCourses.length > 0 && (
            <div
              style={{
                padding: '14px 18px',
                background: 'rgba(180, 83, 9, 0.06)',
                borderLeft: '4px solid var(--warning)',
                borderRadius: 8,
              }}
            >
              <div style={{ display: 'flex', gap: 10, alignItems: 'flex-start' }}>
                <RefreshCcw size={18} style={{ color: 'var(--warning)', flexShrink: 0, marginTop: 2 }} />
                <div>
                  <strong style={{ color: 'var(--warning)' }}>
                    {repeatedCourses.length} Repeated Attempt{repeatedCourses.length !== 1 ? 's' : ''}
                  </strong>
                  <p style={{ margin: '4px 0 0', fontSize: 13, color: 'var(--text-muted)' }}>
                    These older attempts are excluded from GPA/CGPA calculations. Only the highest-scoring attempt is active.
                  </p>
                  <div style={{ display: 'flex', flexWrap: 'wrap', gap: 6, marginTop: 6 }}>
                    {repeatedCourses.map((c, i) => (
                      <span
                        key={i}
                        style={{
                          display: 'inline-flex',
                          alignItems: 'center',
                          gap: 4,
                          padding: '3px 10px',
                          background: 'var(--warning-soft)',
                          color: 'var(--warning)',
                          borderRadius: 6,
                          fontSize: 13,
                          fontWeight: 600,
                        }}
                      >
                        <RefreshCcw size={12} /> {c.courseCode} ({c.letterGrade})
                      </span>
                    ))}
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* ─── Semester Accordions ─── */}
          {data.semesters.length === 0 ? (
            <EmptyState title="No academic records" detail="This student has no finalized course grades yet." />
          ) : (
            <div style={{ display: 'grid', gap: 12 }}>
              {[...data.semesters].reverse().map((sem: SemesterResultResponse) => {
                const isOpen = expandedSemesters[sem.semesterId] ?? false;
                return (
                  <div
                    key={sem.semesterId}
                    style={{
                      background: 'var(--surface)',
                      border: '1px solid var(--border)',
                      borderRadius: 10,
                      overflow: 'hidden',
                      boxShadow: 'var(--shadow)',
                    }}
                  >
                    {/* Accordion header */}
                    <button
                      type="button"
                      onClick={() => toggleSemester(sem.semesterId)}
                      style={{
                        width: '100%',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'space-between',
                        padding: '16px 20px',
                        background: isOpen ? 'var(--surface-muted)' : 'transparent',
                        border: 'none',
                        borderBottom: isOpen ? '1px solid var(--border)' : 'none',
                        cursor: 'pointer',
                        textAlign: 'left',
                        transition: 'background 0.15s ease',
                      }}
                    >
                      <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                        {isOpen ? <ChevronDown size={18} /> : <ChevronRight size={18} />}
                        <div>
                          <strong style={{ fontSize: 16 }}>{sem.semesterName}</strong>
                          <span style={{ display: 'block', fontSize: 13, color: 'var(--text-muted)' }}>
                            {sem.courses.length} course{sem.courses.length !== 1 ? 's' : ''} &middot; {sem.creditsAttempted} credits
                          </span>
                        </div>
                      </div>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 18, textAlign: 'right' }}>
                        <div>
                          <div style={{ fontSize: 11, color: 'var(--text-muted)', fontWeight: 600 }}>GPA</div>
                          <div style={{ fontSize: 18, fontWeight: 800, color: 'var(--accent)' }}>{sem.gpa.toFixed(2)}</div>
                        </div>
                        <div>
                          <div style={{ fontSize: 11, color: 'var(--text-muted)', fontWeight: 600 }}>CGPA</div>
                          <div style={{ fontSize: 18, fontWeight: 800 }}>{sem.cgpa.toFixed(2)}</div>
                        </div>
                      </div>
                    </button>

                    {/* Accordion body */}
                    {isOpen && (
                      <div style={{ padding: 0 }}>
                        <table style={{ minWidth: 600 }}>
                          <thead>
                            <tr>
                              <th>Course</th>
                              <th style={{ textAlign: 'center' }}>Credits</th>
                              <th style={{ textAlign: 'center' }}>Marks</th>
                              <th style={{ textAlign: 'center' }}>%</th>
                              <th style={{ textAlign: 'center' }}>Grade</th>
                              <th style={{ textAlign: 'center' }}>GP</th>
                              <th style={{ textAlign: 'center' }}>Status</th>
                            </tr>
                          </thead>
                          <tbody>
                            {sem.courses.map((course, idx) => {
                              const gc = gradeColor(course.letterGrade);
                              return (
                                <tr
                                  key={idx}
                                  style={{
                                    opacity: course.isRepeatedAttempt ? 0.55 : 1,
                                    background: course.isRepeatedAttempt ? 'rgba(180, 83, 9, 0.04)' : undefined,
                                  }}
                                >
                                  <td>
                                    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                                      <BookOpen size={15} style={{ color: 'var(--text-muted)', flexShrink: 0 }} />
                                      <div>
                                        <strong>{course.courseCode}</strong>
                                        <span>{course.courseTitle}</span>
                                      </div>
                                      {course.isRepeatedAttempt && (
                                        <span
                                          style={{
                                            display: 'inline-flex',
                                            alignItems: 'center',
                                            gap: 3,
                                            padding: '2px 7px',
                                            background: 'var(--warning-soft)',
                                            color: 'var(--warning)',
                                            borderRadius: 4,
                                            fontSize: 10,
                                            fontWeight: 700,
                                            flexShrink: 0,
                                          }}
                                        >
                                          <RefreshCcw size={10} /> REPEATED
                                        </span>
                                      )}
                                    </div>
                                  </td>
                                  <td style={{ textAlign: 'center', fontWeight: 600 }}>{course.creditHours}</td>
                                  <td style={{ textAlign: 'center', fontSize: 13, color: 'var(--text-muted)' }}>
                                    {course.totalObtained} / {course.maxPossible}
                                  </td>
                                  <td style={{ textAlign: 'center', fontWeight: 600 }}>{course.percentage.toFixed(2)}%</td>
                                  <td style={{ textAlign: 'center' }}>
                                    <span
                                      className="badge"
                                      style={{ background: gc.bg, color: gc.fg }}
                                    >
                                      {course.letterGrade}
                                    </span>
                                  </td>
                                  <td style={{ textAlign: 'center', fontWeight: 700 }}>{course.gradePoints.toFixed(2)}</td>
                                  <td style={{ textAlign: 'center' }}>
                                    <span className={`badge badge--${course.status.toLowerCase()}`}>{course.status}</span>
                                  </td>
                                </tr>
                              );
                            })}
                          </tbody>
                        </table>

                        {/* Semester summary row */}
                        <div
                          style={{
                            display: 'flex',
                            justifyContent: 'flex-end',
                            gap: 24,
                            padding: '12px 20px',
                            background: 'var(--surface-muted)',
                            borderTop: '1px solid var(--border)',
                            fontSize: 13,
                          }}
                        >
                          <span>
                            <strong style={{ color: 'var(--text-muted)' }}>Credits Attempted:</strong>{' '}
                            <strong>{sem.creditsAttempted}</strong>
                          </span>
                          <span>
                            <strong style={{ color: 'var(--text-muted)' }}>Credits Earned:</strong>{' '}
                            <strong style={{ color: 'var(--success)' }}>{sem.creditsEarned}</strong>
                          </span>
                          <span>
                            <strong style={{ color: 'var(--text-muted)' }}>Semester GPA:</strong>{' '}
                            <strong style={{ color: 'var(--accent)' }}>{sem.gpa.toFixed(2)}</strong>
                          </span>
                          <span>
                            <strong style={{ color: 'var(--text-muted)' }}>CGPA:</strong>{' '}
                            <strong>{sem.cgpa.toFixed(2)}</strong>
                          </span>
                        </div>
                      </div>
                    )}
                  </div>
                );
              })}
            </div>
          )}
        </>
      )}
    </section>
  );
}
