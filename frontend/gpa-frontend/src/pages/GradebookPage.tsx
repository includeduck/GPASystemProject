import { useEffect, useState, useCallback, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  BookOpenCheck,
  Plus,
  Save,
  Trash2,
  Lock,
  AlertTriangle,
  CheckCircle2,
  ArrowLeft,
  Loader2,
  PencilLine,
  ShieldAlert,
  X,
} from 'lucide-react';
import { useAuth } from '../auth/AuthContext';
import { StatusBanner } from '../components/StatusBanner';
import { EmptyState } from '../components/EmptyState';
import {
  gradeComponentApi,
  gradeEntryApi,
  courseOfferingApi,
  getApiErrorMessage,
} from '../services/api';
import type {
  GradeComponentResponse,
  RosterGradeResponse,
  RecordGradeEntryRequest,
  CourseOffering,
} from '../types/models';

/* ────────────────────────────────── helpers ────────────────────────────── */

const parseNum = (v: string): number | null => {
  const n = parseFloat(v);
  return Number.isNaN(n) ? null : n;
};

/* ──────────────────────────────── component ────────────────────────────── */

export function GradebookPage() {
  const { offeringId: rawId } = useParams<{ offeringId: string }>();
  const offeringId = Number(rawId);
  const navigate = useNavigate();
  const { user } = useAuth();

  /* ─── state ─── */
  const [offering, setOffering] = useState<CourseOffering | null>(null);
  const [components, setComponents] = useState<GradeComponentResponse[]>([]);
  const [roster, setRoster] = useState<RosterGradeResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Component form
  const [newName, setNewName] = useState('');
  const [newMax, setNewMax] = useState('100');
  const [addingComp, setAddingComp] = useState(false);

  // Mark editing
  const [dirtyMarks, setDirtyMarks] = useState<Record<string, string>>({});
  const [markErrors, setMarkErrors] = useState<Record<string, string>>({});
  const [savingMarks, setSavingMarks] = useState(false);

  // Finalize modal
  const [showFinalize, setShowFinalize] = useState(false);
  const [missingCount, setMissingCount] = useState(0);
  const [finalizing, setFinalizing] = useState(false);

  // fallback offerings selection directory
  const [allOfferings, setAllOfferings] = useState<CourseOffering[]>([]);
  const [searchQuery, setSearchQuery] = useState('');

  const tableRef = useRef<HTMLDivElement>(null);

  /* ─── data loading ─── */
  const loadData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const allOfferings = await courseOfferingApi.list();
      const off = allOfferings.find((o) => o.offeringId === offeringId) ?? null;
      setOffering(off);

      const comps = await gradeComponentApi.list(offeringId);
      setComponents(comps);

      const ros = await gradeEntryApi.getRoster(offeringId);
      setRoster(ros);

      setDirtyMarks({});
      setMarkErrors({});
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }, [offeringId]);

  useEffect(() => {
    let ignore = false;

    const loadInitialData = async () => {
      try {
        if (offeringId) {
          const [allOfferings, comps, ros] = await Promise.all([
            courseOfferingApi.list(),
            gradeComponentApi.list(offeringId),
            gradeEntryApi.getRoster(offeringId),
          ]);

          if (!ignore) {
            setOffering(allOfferings.find((o) => o.offeringId === offeringId) ?? null);
            setComponents(comps);
            setRoster(ros);
            setDirtyMarks({});
            setMarkErrors({});
            setError(null);
          }
        } else {
          const offerings = await courseOfferingApi.list();
          if (!ignore) {
            setAllOfferings(offerings);
            setError(null);
          }
        }
      } catch (err) {
        if (!ignore) {
          setError(getApiErrorMessage(err));
        }
      } finally {
        if (!ignore) {
          setLoading(false);
        }
      }
    };

    void loadInitialData();

    return () => {
      ignore = true;
    };
  }, [offeringId]);

  /* ─── derived ─── */
  const isLocked = offering?.isGradeFinalized ?? false;
  const canEdit = user?.role === 'INSTRUCTOR' && !isLocked;

  /* ────────────────── component CRUD ────────────────── */

  const handleAddComponent = async () => {
    const name = newName.trim();
    const max = parseNum(newMax);
    if (!name || max === null || max <= 0) {
      setError('Provide a valid component name and max points > 0.');
      return;
    }
    setAddingComp(true);
    setError(null);
    try {
      await gradeComponentApi.create(offeringId, {
        componentName: name,
        maxPoints: max,
        sortOrder: components.length + 1,
      });
      setNewName('');
      setNewMax('100');
      setSuccess(`Component "${name}" added.`);
      await loadData();
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setAddingComp(false);
    }
  };

  const handleDeleteComponent = async (comp: GradeComponentResponse) => {
    if (!confirm(`Remove "${comp.componentName}"? All marks for this component will be deleted.`)) return;
    try {
      await gradeComponentApi.delete(offeringId, comp.componentId);
      setSuccess(`Component "${comp.componentName}" removed.`);
      await loadData();
    } catch (err) {
      setError(getApiErrorMessage(err));
    }
  };

  /* ────────────────── mark editing ────────────────── */

  const markKey = (enrollmentId: number, componentId: number) => `${enrollmentId}:${componentId}`;

  const getCurrentValue = (row: RosterGradeResponse, comp: GradeComponentResponse): string => {
    const key = markKey(row.enrollmentId, comp.componentId);
    if (key in dirtyMarks) return dirtyMarks[key];
    const entry = row.entries.find((e) => e.componentId === comp.componentId);
    return entry ? String(entry.obtainedMarks) : '';
  };

  const handleMarkChange = (enrollmentId: number, comp: GradeComponentResponse, value: string) => {
    const key = markKey(enrollmentId, comp.componentId);
    setDirtyMarks((prev) => ({ ...prev, [key]: value }));

    if (value === '') {
      setMarkErrors((prev) => {
        const next = { ...prev };
        delete next[key];
        return next;
      });
      return;
    }

    const num = parseNum(value);
    if (num === null || num < 0 || num > comp.maxPoints) {
      setMarkErrors((prev) => ({ ...prev, [key]: `0 – ${comp.maxPoints}` }));
    } else {
      setMarkErrors((prev) => {
        const next = { ...prev };
        delete next[key];
        return next;
      });
    }
  };

  const handleSaveMarks = async () => {
    if (Object.keys(markErrors).length > 0) {
      setError('Fix validation errors before saving.');
      return;
    }

    const entries: RecordGradeEntryRequest[] = [];
    for (const [key, val] of Object.entries(dirtyMarks)) {
      if (val === '') continue;
      const [eid, cid] = key.split(':').map(Number);
      const num = parseNum(val);
      if (num === null) continue;
      entries.push({ enrollmentId: eid, componentId: cid, obtainedMarks: num });
    }

    if (entries.length === 0) {
      setError('No marks to save.');
      return;
    }

    setSavingMarks(true);
    setError(null);
    try {
      await gradeEntryApi.recordMarks(offeringId, entries);
      setSuccess(`${entries.length} mark(s) saved successfully.`);
      await loadData();
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSavingMarks(false);
    }
  };

  /* ────────────────── finalize ────────────────── */

  const openFinalizeModal = () => {
    let missing = 0;
    for (const row of roster) {
      for (const comp of components) {
        const entry = row.entries.find((e) => e.componentId === comp.componentId);
        if (!entry) missing++;
      }
    }
    setMissingCount(missing);
    setShowFinalize(true);
  };

  const handleFinalize = async (force: boolean) => {
    setFinalizing(true);
    setError(null);
    try {
      await gradeEntryApi.finalize(offeringId, force);
      setSuccess('Grades finalized successfully! GPA/CGPA recalculated for all students.');
      setShowFinalize(false);
      await loadData();
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setFinalizing(false);
    }
  };

  /* ────────────────── render ────────────────── */

  if (!rawId || Number.isNaN(offeringId)) {
    const filtered = allOfferings.filter((o) =>
      o.courseCode.toLowerCase().includes(searchQuery.toLowerCase()) ||
      o.courseTitle.toLowerCase().includes(searchQuery.toLowerCase()) ||
      o.instructorName.toLowerCase().includes(searchQuery.toLowerCase())
    );

    return (
      <section className="page" style={{ maxWidth: 1200 }}>
        <div className="page__header">
          <div>
            <h1 style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              <BookOpenCheck size={30} style={{ color: 'var(--accent)' }} />
              Gradebooks Directory
            </h1>
            <p>Select a course offering to view its gradebook and record student marks.</p>
          </div>
        </div>

        {error && <StatusBanner tone="error">{error}</StatusBanner>}

        <div className="form-panel">
          <label>
            <span>Search Offerings</span>
            <input
              type="text"
              placeholder="Search by course code, title, or instructor..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
            />
          </label>
        </div>

        <div className="table-panel">
          {loading ? (
            <EmptyState title="Loading offerings..." />
          ) : filtered.length === 0 ? (
            <EmptyState
              title="No matching offerings found"
              detail="Try a different search query or select from the Offerings page."
            />
          ) : (
            <div className="table-wrap">
              <table>
                <thead>
                  <tr>
                    <th>Course</th>
                    <th>Semester</th>
                    <th>Instructor</th>
                    <th>Enrollment</th>
                    <th>Status</th>
                    <th className="table-actions">Action</th>
                  </tr>
                </thead>
                <tbody>
                  {filtered.map((off) => (
                    <tr key={off.offeringId}>
                      <td>
                        <strong>{off.courseCode}</strong>
                        <span>{off.courseTitle}</span>
                      </td>
                      <td>{off.semesterName}</td>
                      <td>{off.instructorName}</td>
                      <td>
                        {off.currentEnrollment} / {off.maxCapacity}
                      </td>
                      <td>
                        <span
                          className={`badge ${
                            off.isGradeFinalized ? 'badge--active' : 'badge--inactive'
                          }`}
                          style={{
                            background: off.isGradeFinalized ? 'var(--success-soft)' : undefined,
                            color: off.isGradeFinalized ? 'var(--success)' : undefined,
                          }}
                        >
                          {off.isGradeFinalized ? 'Finalized' : 'Active'}
                        </span>
                      </td>
                      <td className="table-actions">
                        <button
                          className="button button--ghost"
                          onClick={() => navigate(`/gradebook/${off.offeringId}`)}
                          style={{ minHeight: 34, fontSize: 13 }}
                        >
                          Open Gradebook
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

  return (
    <section className="page" style={{ maxWidth: 1400 }}>
      {/* Header */}
      <div className="page__header">
        <div>
          <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 4 }}>
            <button
              className="icon-button"
              onClick={() => navigate(user?.role === 'INSTRUCTOR' ? '/gradebook' : '/offerings')}
              aria-label="Back to offerings"
              style={{ flexShrink: 0 }}
            >
              <ArrowLeft size={18} />
            </button>
            <h1 style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
              <BookOpenCheck size={30} style={{ color: 'var(--accent)' }} />
              Gradebook
            </h1>
          </div>
          {offering && (
            <p style={{ marginLeft: 50 }}>
              {offering.courseCode} &mdash; {offering.courseTitle} &middot; {offering.semesterName} &middot; {offering.instructorName}
            </p>
          )}
        </div>
        {isLocked && (
          <div
            className="badge"
            style={{
              background: 'linear-gradient(135deg, #047857, #065f46)',
              color: '#fff',
              padding: '6px 14px',
              fontSize: 13,
              gap: 6,
              display: 'inline-flex',
              alignItems: 'center',
              borderRadius: 999,
            }}
          >
            <Lock size={14} /> Finalized
          </div>
        )}
      </div>

      {error && <StatusBanner tone="error">{error}</StatusBanner>}
      {success && <StatusBanner tone="success">{success}</StatusBanner>}

      {loading ? (
        <EmptyState title="Loading gradebook..." />
      ) : (
        <>
          {/* ─── Section 1: Component Configurator ─── */}
          <div className="form-panel" style={{ opacity: canEdit ? 1 : 0.7 }}>
            <div className="form-panel__header">
              <h2 style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <PencilLine size={20} style={{ color: 'var(--accent)' }} />
                Grade Components
              </h2>
              {!canEdit && (
                <span style={{ fontSize: 13, color: 'var(--warning)', fontWeight: 600, display: 'flex', alignItems: 'center', gap: 4 }}>
                  <Lock size={14} /> Read only
                </span>
              )}
            </div>

            {/* existing components */}
            {components.length > 0 && (
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: 10 }}>
                {components.map((c) => (
                  <div
                    key={c.componentId}
                    style={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: 8,
                      padding: '8px 14px',
                      background: 'var(--surface-muted)',
                      border: '1px solid var(--border)',
                      borderRadius: 8,
                      fontSize: 14,
                    }}
                  >
                    <strong>{c.componentName}</strong>
                    <span style={{ color: 'var(--text-muted)', fontSize: 12 }}>max {c.maxPoints}</span>
                    {canEdit && (
                      <button
                        className="icon-button icon-button--danger"
                        style={{ width: 26, height: 26 }}
                        onClick={() => handleDeleteComponent(c)}
                        aria-label={`Remove ${c.componentName}`}
                      >
                        <Trash2 size={14} />
                      </button>
                    )}
                  </div>
                ))}
              </div>
            )}

            {/* add new */}
            {canEdit && (
              <div style={{ display: 'flex', alignItems: 'flex-end', gap: 10, flexWrap: 'wrap' }}>
                <label style={{ flex: '1 1 200px' }}>
                  <span>Component Name</span>
                  <input
                    type="text"
                    placeholder="e.g. Midterm, Final, Quiz 1"
                    value={newName}
                    onChange={(e) => setNewName(e.target.value)}
                  />
                </label>
                <label style={{ flex: '0 1 140px' }}>
                  <span>Max Points</span>
                  <input
                    type="number"
                    min={1}
                    step={1}
                    value={newMax}
                    onChange={(e) => setNewMax(e.target.value)}
                  />
                </label>
                <button
                  className="button button--primary"
                  type="button"
                  disabled={addingComp}
                  onClick={handleAddComponent}
                  style={{ minHeight: 42 }}
                >
                  {addingComp ? <Loader2 size={17} className="spin" /> : <Plus size={17} />}
                  Add Component
                </button>
              </div>
            )}
          </div>

          {/* ─── Section 2: Grade Roster Table ─── */}
          <div className="table-panel">
            <div
              style={{
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                padding: '16px 20px',
                borderBottom: '1px solid var(--border)',
              }}
            >
              <h2 style={{ display: 'flex', alignItems: 'center', gap: 8, fontSize: 20 }}>
                <BookOpenCheck size={20} style={{ color: 'var(--accent)' }} />
                Student Roster
                <span style={{ color: 'var(--text-muted)', fontSize: 14, fontWeight: 400 }}>
                  ({roster.length} student{roster.length !== 1 ? 's' : ''})
                </span>
              </h2>

              {canEdit && Object.keys(dirtyMarks).length > 0 && (
                <button
                  className="button button--primary"
                  onClick={handleSaveMarks}
                  disabled={savingMarks || Object.keys(markErrors).length > 0}
                >
                  {savingMarks ? <Loader2 size={17} className="spin" /> : <Save size={17} />}
                  Save Marks ({Object.keys(dirtyMarks).length})
                </button>
              )}
            </div>

            {components.length === 0 ? (
              <EmptyState
                title="No grade components defined"
                detail="Add at least one grade component above before entering marks."
              />
            ) : roster.length === 0 ? (
              <EmptyState
                title="No enrolled students"
                detail="There are no students enrolled in this offering."
              />
            ) : (
              <div className="table-wrap" ref={tableRef}>
                <table style={{ minWidth: Math.max(720, 320 + components.length * 140) }}>
                  <thead>
                    <tr>
                      <th style={{ position: 'sticky', left: 0, background: 'var(--surface-muted)', zIndex: 2, minWidth: 60 }}>#</th>
                      <th style={{ position: 'sticky', left: 60, background: 'var(--surface-muted)', zIndex: 2, minWidth: 180 }}>Student</th>
                      {components.map((c) => (
                        <th key={c.componentId} style={{ minWidth: 130, textAlign: 'center' }}>
                          {c.componentName}
                          <span style={{ display: 'block', fontSize: 10, fontWeight: 400, textTransform: 'none', color: 'var(--text-muted)' }}>
                            / {c.maxPoints}
                          </span>
                        </th>
                      ))}
                      <th style={{ minWidth: 90, textAlign: 'center' }}>Total</th>
                      <th style={{ minWidth: 80, textAlign: 'center' }}>%</th>
                      <th style={{ minWidth: 70, textAlign: 'center' }}>Grade</th>
                      <th style={{ minWidth: 60, textAlign: 'center' }}>GP</th>
                      <th style={{ minWidth: 90, textAlign: 'center' }}>Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {roster.map((row, idx) => (
                      <tr key={row.enrollmentId}>
                        <td style={{ position: 'sticky', left: 0, background: '#fff', zIndex: 1, color: 'var(--text-muted)', fontSize: 13 }}>
                          {idx + 1}
                        </td>
                        <td style={{ position: 'sticky', left: 60, background: '#fff', zIndex: 1 }}>
                          <strong>{row.studentName}</strong>
                          <span>{row.studentNumber}</span>
                        </td>
                        {components.map((comp) => {
                          const key = markKey(row.enrollmentId, comp.componentId);
                          const val = getCurrentValue(row, comp);
                          const hasErr = key in markErrors;

                          return (
                            <td key={comp.componentId} style={{ textAlign: 'center', padding: '8px 6px' }}>
                              {!canEdit ? (
                                <span style={{ fontWeight: 600 }}>{val || '—'}</span>
                              ) : (
                                <div style={{ position: 'relative' }}>
                                  <input
                                    type="number"
                                    min={0}
                                    max={comp.maxPoints}
                                    step="any"
                                    value={val}
                                    placeholder="—"
                                    onChange={(e) => handleMarkChange(row.enrollmentId, comp, e.target.value)}
                                    style={{
                                      width: '100%',
                                      maxWidth: 100,
                                      textAlign: 'center',
                                      padding: '6px 8px',
                                      minHeight: 36,
                                      borderColor: hasErr ? 'var(--danger)' : undefined,
                                      boxShadow: hasErr ? '0 0 0 2px rgba(185, 28, 28, 0.15)' : undefined,
                                    }}
                                  />
                                  {hasErr && (
                                    <span
                                      style={{
                                        position: 'absolute',
                                        bottom: -16,
                                        left: '50%',
                                        transform: 'translateX(-50%)',
                                        fontSize: 10,
                                        color: 'var(--danger)',
                                        whiteSpace: 'nowrap',
                                      }}
                                    >
                                      {markErrors[key]}
                                    </span>
                                  )}
                                </div>
                              )}
                            </td>
                          );
                        })}
                        <td style={{ textAlign: 'center', fontWeight: 600 }}>
                          {row.totalObtained != null ? `${row.totalObtained} / ${row.maxPossible}` : '—'}
                        </td>
                        <td style={{ textAlign: 'center', fontWeight: 600 }}>
                          {row.percentage != null ? `${row.percentage.toFixed(2)}%` : '—'}
                        </td>
                        <td style={{ textAlign: 'center' }}>
                          {row.letterGrade ? (
                            <span
                              className="badge"
                              style={{
                                background:
                                  row.letterGrade === 'F'
                                    ? 'var(--danger-soft)'
                                    : row.letterGrade.startsWith('A')
                                      ? 'var(--success-soft)'
                                      : 'var(--accent-soft)',
                                color:
                                  row.letterGrade === 'F'
                                    ? 'var(--danger)'
                                    : row.letterGrade.startsWith('A')
                                      ? 'var(--success)'
                                      : 'var(--accent-strong)',
                              }}
                            >
                              {row.letterGrade}
                            </span>
                          ) : (
                            '—'
                          )}
                        </td>
                        <td style={{ textAlign: 'center', fontWeight: 600, color: 'var(--text-muted)' }}>
                          {row.gradePoints != null ? row.gradePoints.toFixed(2) : '—'}
                        </td>
                        <td style={{ textAlign: 'center' }}>
                          <span className={`badge badge--${row.enrollmentStatus.toLowerCase()}`}>
                            {row.enrollmentStatus}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>

          {/* ─── Finalize Actions ─── */}
          {canEdit && components.length > 0 && roster.length > 0 && (
            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10 }}>
              <button
                className="button"
                style={{
                  background: 'linear-gradient(135deg, #047857, #065f46)',
                  color: '#fff',
                  border: 'none',
                  fontWeight: 700,
                  gap: 8,
                  padding: '0 20px',
                  minHeight: 44,
                }}
                onClick={openFinalizeModal}
              >
                <CheckCircle2 size={18} />
                Finalize Grades
              </button>
            </div>
          )}
        </>
      )}

      {/* ─── Finalization Modal ─── */}
      {showFinalize && (
        <div className="modal-backdrop" role="presentation" onClick={() => setShowFinalize(false)}>
          <div
            className="modal"
            role="dialog"
            aria-modal="true"
            style={{ width: 'min(520px, 95vw)' }}
            onClick={(e) => e.stopPropagation()}
          >
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
              <div className="modal__icon" style={{ background: missingCount > 0 ? 'var(--warning-soft)' : 'var(--success-soft)', color: missingCount > 0 ? 'var(--warning)' : 'var(--success)' }}>
                {missingCount > 0 ? <AlertTriangle size={20} /> : <CheckCircle2 size={20} />}
              </div>
              <button className="icon-button" onClick={() => setShowFinalize(false)} aria-label="Close">
                <X size={18} />
              </button>
            </div>

            <h2>Finalize Grades</h2>

            <div style={{ display: 'grid', gap: 12 }}>
              <div
                style={{
                  display: 'grid',
                  gridTemplateColumns: '1fr 1fr',
                  gap: 10,
                }}
              >
                <div style={{ padding: '12px 14px', background: 'var(--surface-muted)', borderRadius: 8, textAlign: 'center' }}>
                  <div style={{ fontSize: 22, fontWeight: 700, color: 'var(--accent)' }}>{roster.length}</div>
                  <div style={{ fontSize: 12, color: 'var(--text-muted)' }}>Students</div>
                </div>
                <div style={{ padding: '12px 14px', background: 'var(--surface-muted)', borderRadius: 8, textAlign: 'center' }}>
                  <div style={{ fontSize: 22, fontWeight: 700, color: 'var(--accent)' }}>{components.length}</div>
                  <div style={{ fontSize: 12, color: 'var(--text-muted)' }}>Components</div>
                </div>
              </div>

              {missingCount > 0 && (
                <div
                  style={{
                    padding: '12px 16px',
                    backgroundColor: 'rgba(239, 68, 68, 0.08)',
                    borderLeft: '4px solid #ef4444',
                    borderRadius: 6,
                  }}
                >
                  <div style={{ display: 'flex', gap: 8, alignItems: 'flex-start' }}>
                    <ShieldAlert size={16} style={{ color: '#ef4444', flexShrink: 0, marginTop: 2 }} />
                    <div>
                      <strong style={{ color: '#ef4444' }}>{missingCount} missing mark{missingCount !== 1 ? 's' : ''} detected</strong>
                      <p style={{ margin: '4px 0 0', fontSize: 13, color: 'var(--text-muted)' }}>
                        Missing marks will default to <strong>0</strong> if you force finalization.
                      </p>
                    </div>
                  </div>
                </div>
              )}

              <p style={{ fontSize: 14, color: 'var(--text-muted)', lineHeight: 1.6 }}>
                Finalizing will <strong>lock all grade entries</strong> for this offering, calculate letter grades and grade points for every student, and trigger a full <strong>GPA/CGPA recalculation</strong> including repeated-course resolution. This action <strong>cannot be undone</strong>.
              </p>
            </div>

            <div className="modal__actions">
              <button className="button button--ghost" onClick={() => setShowFinalize(false)} disabled={finalizing}>
                Cancel
              </button>
              {missingCount > 0 ? (
                <button
                  className="button button--danger"
                  onClick={() => handleFinalize(true)}
                  disabled={finalizing}
                >
                  {finalizing ? <Loader2 size={17} className="spin" /> : <AlertTriangle size={17} />}
                  Force Finalize
                </button>
              ) : (
                <button
                  className="button"
                  style={{ background: 'linear-gradient(135deg, #047857, #065f46)', color: '#fff', border: 'none', fontWeight: 700 }}
                  onClick={() => handleFinalize(false)}
                  disabled={finalizing}
                >
                  {finalizing ? <Loader2 size={17} className="spin" /> : <CheckCircle2 size={17} />}
                  Confirm &amp; Finalize
                </button>
              )}
            </div>
          </div>
        </div>
      )}
    </section>
  );
}
