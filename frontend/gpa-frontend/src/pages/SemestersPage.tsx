import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { CheckCircle2, Pencil, Plus, Save, Trash2, X } from 'lucide-react';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { EmptyState } from '../components/EmptyState';
import { StatusBanner } from '../components/StatusBanner';
import { getApiErrorMessage, semesterApi } from '../services/api';
import type { Semester, SemesterForm } from '../types/models';
import { formatDate, futureInputValue, todayInputValue } from '../utils/dates';

const emptyForm: SemesterForm = {
  semesterName: '',
  startDate: todayInputValue(),
  endDate: futureInputValue(120),
  isCurrent: false,
};

export function SemestersPage() {
  const [semesters, setSemesters] = useState<Semester[]>([]);
  const [form, setForm] = useState<SemesterForm>(emptyForm);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Semester | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const loadSemesters = async () => {
    setLoading(true);
    try {
      setSemesters(await semesterApi.list());
      setError(null);
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    let ignore = false;

    const loadInitialSemesters = async () => {
      try {
        const data = await semesterApi.list();
        if (!ignore) {
          setSemesters(data);
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

    void loadInitialSemesters();

    return () => {
      ignore = true;
    };
  }, []);

  const resetForm = () => {
    setForm(emptyForm);
    setEditingId(null);
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess(null);

    try {
      if (editingId) {
        await semesterApi.update(editingId, form);
        setSuccess('Semester updated.');
      } else {
        await semesterApi.create(form);
        setSuccess('Semester created.');
      }

      resetForm();
      await loadSemesters();
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSaving(false);
    }
  };

  const editSemester = (semester: Semester) => {
    setEditingId(semester.semesterId);
    setForm({
      semesterName: semester.semesterName,
      startDate: semester.startDate,
      endDate: semester.endDate,
      isCurrent: semester.isCurrent,
    });
    setError(null);
    setSuccess(null);
  };

  const setCurrent = async (semester: Semester) => {
    try {
      await semesterApi.setCurrent(semester.semesterId);
      setSuccess(`${semester.semesterName} is now current.`);
      await loadSemesters();
    } catch (err) {
      setError(getApiErrorMessage(err));
    }
  };

  const deleteSemester = async () => {
    if (!deleteTarget) {
      return;
    }

    try {
      await semesterApi.delete(deleteTarget.semesterId);
      setDeleteTarget(null);
      setSuccess('Semester deleted.');
      await loadSemesters();
    } catch (err) {
      setDeleteTarget(null);
      setError(getApiErrorMessage(err));
    }
  };

  return (
    <section className="page">
      <div className="page__header">
        <div>
          <h1>Semesters</h1>
          <p>{semesters.length} total</p>
        </div>
      </div>

      {error && <StatusBanner tone="error">{error}</StatusBanner>}
      {success && <StatusBanner tone="success">{success}</StatusBanner>}

      <form className="form-panel" onSubmit={handleSubmit}>
        <div className="form-panel__header">
          <h2>{editingId ? 'Edit Semester' : 'Add Semester'}</h2>
          {editingId && (
            <button className="icon-button" type="button" onClick={resetForm} aria-label="Cancel edit">
              <X size={18} />
            </button>
          )}
        </div>
        <fieldset disabled={saving}>
          <div className="form-grid">
            <label>
              <span>Name</span>
              <input
                maxLength={50}
                minLength={2}
                required
                value={form.semesterName}
                onChange={(event) => setForm({ ...form, semesterName: event.target.value })}
              />
            </label>
            <label>
              <span>Start Date</span>
              <input
                required
                type="date"
                value={form.startDate}
                onChange={(event) => setForm({ ...form, startDate: event.target.value })}
              />
            </label>
            <label>
              <span>End Date</span>
              <input
                required
                type="date"
                value={form.endDate}
                onChange={(event) => setForm({ ...form, endDate: event.target.value })}
              />
            </label>
            <label className="checkbox-field">
              <input
                checked={form.isCurrent}
                type="checkbox"
                onChange={(event) => setForm({ ...form, isCurrent: event.target.checked })}
              />
              <span>Current semester</span>
            </label>
          </div>
        </fieldset>
        <div className="form-actions">
          <button className="button button--primary" type="submit" disabled={saving}>
            {editingId ? <Save size={17} /> : <Plus size={17} />}
            {editingId ? 'Save' : 'Add'}
          </button>
        </div>
      </form>

      <div className="table-panel">
        {loading ? (
          <EmptyState title="Loading semesters" />
        ) : semesters.length === 0 ? (
          <EmptyState title="No semesters yet" />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Semester</th>
                  <th>Dates</th>
                  <th>Status</th>
                  <th className="table-actions">Actions</th>
                </tr>
              </thead>
              <tbody>
                {semesters.map((semester) => (
                  <tr key={semester.semesterId}>
                    <td>
                      <strong>{semester.semesterName}</strong>
                    </td>
                    <td>
                      {formatDate(semester.startDate)}
                      <span>{formatDate(semester.endDate)}</span>
                    </td>
                    <td>
                      <span className={`badge badge--${semester.isCurrent ? 'active' : 'inactive'}`}>
                        {semester.isCurrent ? 'CURRENT' : 'PLANNED'}
                      </span>
                    </td>
                    <td className="table-actions">
                      <button
                        className="icon-button"
                        type="button"
                        onClick={() => setCurrent(semester)}
                        aria-label={`Set ${semester.semesterName} current`}
                        disabled={semester.isCurrent}
                      >
                        <CheckCircle2 size={17} />
                      </button>
                      <button
                        className="icon-button"
                        type="button"
                        onClick={() => editSemester(semester)}
                        aria-label={`Edit ${semester.semesterName}`}
                      >
                        <Pencil size={17} />
                      </button>
                      <button
                        className="icon-button icon-button--danger"
                        type="button"
                        onClick={() => setDeleteTarget(semester)}
                        aria-label={`Delete ${semester.semesterName}`}
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
          title="Delete semester"
          message={`Delete ${deleteTarget.semesterName}? This is blocked once offerings or academic records reference it.`}
          confirmLabel="Delete"
          onCancel={() => setDeleteTarget(null)}
          onConfirm={deleteSemester}
        />
      )}
    </section>
  );
}
