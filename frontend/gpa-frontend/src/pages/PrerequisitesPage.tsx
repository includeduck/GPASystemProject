import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { Link2, Trash2 } from 'lucide-react';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { EmptyState } from '../components/EmptyState';
import { StatusBanner } from '../components/StatusBanner';
import { courseApi, getApiErrorMessage, prerequisiteApi } from '../services/api';
import type { Course, Prerequisite } from '../types/models';

export function PrerequisitesPage() {
  const [courses, setCourses] = useState<Course[]>([]);
  const [prerequisites, setPrerequisites] = useState<Prerequisite[]>([]);
  const [selectedCourseId, setSelectedCourseId] = useState('');
  const [selectedPrerequisiteId, setSelectedPrerequisiteId] = useState('');
  const [deleteTarget, setDeleteTarget] = useState<Prerequisite | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  useEffect(() => {
    let ignore = false;

    const loadInitialCourses = async () => {
      try {
        const data = await courseApi.list();
        if (!ignore) {
          setCourses(data);
          setSelectedCourseId(data[0] ? String(data[0].courseId) : '');
          setError(null);
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

    void loadInitialCourses();

    return () => {
      ignore = true;
    };
  }, []);

  useEffect(() => {
    if (!selectedCourseId) {
      return;
    }

    let ignore = false;

    const loadPrerequisites = async () => {
      try {
        const data = await prerequisiteApi.list(Number(selectedCourseId));
        if (!ignore) {
          setPrerequisites(data);
          setSelectedPrerequisiteId('');
          setError(null);
        }
      } catch (err) {
        if (!ignore) {
          setError(getApiErrorMessage(err));
        }
      }
    };

    void loadPrerequisites();

    return () => {
      ignore = true;
    };
  }, [selectedCourseId]);

  const reloadPrerequisites = async () => {
    if (!selectedCourseId) {
      return;
    }

    setPrerequisites(await prerequisiteApi.list(Number(selectedCourseId)));
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (!selectedCourseId || !selectedPrerequisiteId) {
      return;
    }

    setSaving(true);
    setError(null);
    setSuccess(null);

    try {
      await prerequisiteApi.add(Number(selectedCourseId), Number(selectedPrerequisiteId));
      setSuccess('Prerequisite added.');
      setSelectedPrerequisiteId('');
      await reloadPrerequisites();
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSaving(false);
    }
  };

  const removePrerequisite = async () => {
    if (!deleteTarget) {
      return;
    }

    try {
      await prerequisiteApi.remove(deleteTarget.courseId, deleteTarget.prerequisiteCourseId);
      setDeleteTarget(null);
      setSuccess('Prerequisite removed.');
      await reloadPrerequisites();
    } catch (err) {
      setDeleteTarget(null);
      setError(getApiErrorMessage(err));
    }
  };

  const availablePrerequisiteCourses = courses.filter((course) => {
    const courseId = Number(selectedCourseId);
    return (
      course.courseId !== courseId &&
      !prerequisites.some((prerequisite) => prerequisite.prerequisiteCourseId === course.courseId)
    );
  });

  const selectedCourse = courses.find((course) => course.courseId === Number(selectedCourseId));
  const hasCourses = courses.length > 1;

  return (
    <section className="page">
      <div className="page__header">
        <div>
          <h1>Prerequisites</h1>
          <p>{selectedCourse ? `${selectedCourse.courseCode} selected` : 'No course selected'}</p>
        </div>
      </div>

      {!hasCourses && !loading && (
        <StatusBanner tone="info">Create at least two courses before defining prerequisites.</StatusBanner>
      )}
      {error && <StatusBanner tone="error">{error}</StatusBanner>}
      {success && <StatusBanner tone="success">{success}</StatusBanner>}

      <form className="form-panel" onSubmit={handleSubmit}>
        <div className="form-panel__header">
          <h2>Add Prerequisite</h2>
        </div>
        <fieldset disabled={!hasCourses || saving}>
          <div className="form-grid">
            <label>
              <span>Course</span>
              <select value={selectedCourseId} onChange={(event) => setSelectedCourseId(event.target.value)}>
                {courses.map((course) => (
                  <option key={course.courseId} value={course.courseId}>
                    {course.courseCode} - {course.courseTitle}
                  </option>
                ))}
              </select>
            </label>
            <label>
              <span>Prerequisite</span>
              <select
                required
                value={selectedPrerequisiteId}
                onChange={(event) => setSelectedPrerequisiteId(event.target.value)}
              >
                <option value="">Select course</option>
                {availablePrerequisiteCourses.map((course) => (
                  <option key={course.courseId} value={course.courseId}>
                    {course.courseCode} - {course.courseTitle}
                  </option>
                ))}
              </select>
            </label>
          </div>
        </fieldset>
        <div className="form-actions">
          <button
            className="button button--primary"
            type="submit"
            disabled={!hasCourses || !selectedPrerequisiteId || saving}
          >
            <Link2 size={17} />
            Add
          </button>
        </div>
      </form>

      <div className="table-panel">
        {loading ? (
          <EmptyState title="Loading prerequisites" />
        ) : prerequisites.length === 0 ? (
          <EmptyState title="No prerequisites yet" />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Course</th>
                  <th>Required Course</th>
                  <th className="table-actions">Actions</th>
                </tr>
              </thead>
              <tbody>
                {prerequisites.map((prerequisite) => (
                  <tr key={`${prerequisite.courseId}-${prerequisite.prerequisiteCourseId}`}>
                    <td>
                      <strong>{prerequisite.courseCode}</strong>
                      <span>{prerequisite.courseTitle}</span>
                    </td>
                    <td>
                      {prerequisite.prerequisiteCourseCode}
                      <span>{prerequisite.prerequisiteCourseTitle}</span>
                    </td>
                    <td className="table-actions">
                      <button
                        className="icon-button icon-button--danger"
                        type="button"
                        onClick={() => setDeleteTarget(prerequisite)}
                        aria-label={`Remove ${prerequisite.prerequisiteCourseCode}`}
                      >
                        <Trash2 size={17} />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {deleteTarget && (
        <ConfirmDialog
          title="Remove prerequisite"
          message={`Remove ${deleteTarget.prerequisiteCourseCode} from ${deleteTarget.courseCode}?`}
          confirmLabel="Remove"
          onCancel={() => setDeleteTarget(null)}
          onConfirm={removePrerequisite}
        />
      )}
    </section>
  );
}
