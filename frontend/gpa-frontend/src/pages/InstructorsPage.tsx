import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { Pencil, Plus, Save, UserX, X } from 'lucide-react';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { CredentialsPanel } from '../components/CredentialsPanel';
import { EmptyState } from '../components/EmptyState';
import { StatusBanner } from '../components/StatusBanner';
import { departmentApi, getApiErrorMessage, instructorApi } from '../services/api';
import type { Department, Instructor, InstructorForm, TemporaryCredentials } from '../types/models';
import { formatDate, todayInputValue } from '../utils/dates';

type InstructorFormState = {
  fullName: string;
  email: string;
  departmentId: string;
  hireDate: string;
};

const emptyForm: InstructorFormState = {
  fullName: '',
  email: '',
  departmentId: '',
  hireDate: todayInputValue(),
};

export function InstructorsPage() {
  const [instructors, setInstructors] = useState<Instructor[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [form, setForm] = useState<InstructorFormState>(emptyForm);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [deactivateTarget, setDeactivateTarget] = useState<Instructor | null>(null);
  const [credentials, setCredentials] = useState<TemporaryCredentials | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const loadData = async () => {
    setLoading(true);
    try {
      const [instructorsData, departmentsData] = await Promise.all([
        instructorApi.list(),
        departmentApi.list(),
      ]);
      setInstructors(instructorsData);
      setDepartments(departmentsData);
      setError(null);

      if (!editingId && !form.departmentId && departmentsData[0]) {
        setForm((current) => ({ ...current, departmentId: String(departmentsData[0].departmentId) }));
      }
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    let ignore = false;

    const loadInitialData = async () => {
      try {
        const [instructorsData, departmentsData] = await Promise.all([
          instructorApi.list(),
          departmentApi.list(),
        ]);

        if (!ignore) {
          setInstructors(instructorsData);
          setDepartments(departmentsData);
          setError(null);

          if (departmentsData[0]) {
            setForm((current) => ({
              ...current,
              departmentId: current.departmentId || String(departmentsData[0].departmentId),
            }));
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
  }, []);

  const resetForm = () => {
    setForm({
      ...emptyForm,
      departmentId: departments[0] ? String(departments[0].departmentId) : '',
      hireDate: todayInputValue(),
    });
    setEditingId(null);
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess(null);
    setCredentials(null);

    const payload: InstructorForm = {
      fullName: form.fullName,
      email: form.email,
      departmentId: Number(form.departmentId),
      hireDate: form.hireDate,
    };

    try {
      if (editingId) {
        await instructorApi.update(editingId, payload);
        setSuccess('Instructor updated.');
      } else {
        const response = await instructorApi.create(payload);
        setCredentials(response.credentials);
        setSuccess('Instructor created.');
      }

      resetForm();
      await loadData();
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSaving(false);
    }
  };

  const editInstructor = (instructor: Instructor) => {
    setEditingId(instructor.instructorId);
    setCredentials(null);
    setForm({
      fullName: instructor.fullName,
      email: instructor.email,
      departmentId: String(instructor.departmentId),
      hireDate: instructor.hireDate,
    });
    setError(null);
    setSuccess(null);
  };

  const deactivateInstructor = async () => {
    if (!deactivateTarget) {
      return;
    }

    try {
      await instructorApi.deactivate(deactivateTarget.instructorId);
      setDeactivateTarget(null);
      setSuccess('Instructor deactivated.');
      await loadData();
    } catch (err) {
      setDeactivateTarget(null);
      setError(getApiErrorMessage(err));
    }
  };

  const hasDepartments = departments.length > 0;

  return (
    <section className="page">
      <div className="page__header">
        <div>
          <h1>Instructors</h1>
          <p>{instructors.length} total</p>
        </div>
      </div>

      {!hasDepartments && !loading && (
        <StatusBanner tone="info">Create a department before adding instructors.</StatusBanner>
      )}
      {error && <StatusBanner tone="error">{error}</StatusBanner>}
      {success && <StatusBanner tone="success">{success}</StatusBanner>}
      <CredentialsPanel credentials={credentials} label="New instructor credentials" />

      <form className="form-panel" onSubmit={handleSubmit}>
        <div className="form-panel__header">
          <h2>{editingId ? 'Edit Instructor' : 'Add Instructor'}</h2>
          {editingId && (
            <button className="icon-button" type="button" onClick={resetForm} aria-label="Cancel edit">
              <X size={18} />
            </button>
          )}
        </div>
        <fieldset disabled={!hasDepartments || saving}>
          <div className="form-grid">
            <label>
              <span>Full Name</span>
              <input
                maxLength={100}
                minLength={2}
                required
                value={form.fullName}
                onChange={(event) => setForm({ ...form, fullName: event.target.value })}
              />
            </label>
            <label>
              <span>Email</span>
              <input
                maxLength={100}
                required
                type="email"
                value={form.email}
                onChange={(event) => setForm({ ...form, email: event.target.value })}
              />
            </label>
            <label>
              <span>Department</span>
              <select
                required
                value={form.departmentId}
                onChange={(event) => setForm({ ...form, departmentId: event.target.value })}
              >
                {departments.map((department) => (
                  <option key={department.departmentId} value={department.departmentId}>
                    {department.departmentCode} - {department.departmentName}
                  </option>
                ))}
              </select>
            </label>
            <label>
              <span>Hire Date</span>
              <input
                required
                type="date"
                value={form.hireDate}
                onChange={(event) => setForm({ ...form, hireDate: event.target.value })}
              />
            </label>
          </div>
        </fieldset>
        <div className="form-actions">
          <button className="button button--primary" type="submit" disabled={!hasDepartments || saving}>
            {editingId ? <Save size={17} /> : <Plus size={17} />}
            {editingId ? 'Save' : 'Add'}
          </button>
        </div>
      </form>

      <div className="table-panel">
        {loading ? (
          <EmptyState title="Loading instructors" />
        ) : instructors.length === 0 ? (
          <EmptyState title="No instructors yet" />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Instructor</th>
                  <th>Email</th>
                  <th>Department</th>
                  <th>Hire Date</th>
                  <th>Status</th>
                  <th className="table-actions">Actions</th>
                </tr>
              </thead>
              <tbody>
                {instructors.map((instructor) => (
                  <tr key={instructor.instructorId}>
                    <td>
                      <strong>{instructor.fullName}</strong>
                      <span>{instructor.username}</span>
                    </td>
                    <td>{instructor.email}</td>
                    <td>
                      {instructor.departmentCode}
                      <span>{instructor.departmentName}</span>
                    </td>
                    <td>{formatDate(instructor.hireDate)}</td>
                    <td>
                      <span className={`badge badge--${instructor.isActive ? 'active' : 'inactive'}`}>
                        {instructor.isActive ? 'ACTIVE' : 'INACTIVE'}
                      </span>
                    </td>
                    <td className="table-actions">
                      <button
                        className="icon-button"
                        type="button"
                        onClick={() => editInstructor(instructor)}
                        aria-label={`Edit ${instructor.fullName}`}
                      >
                        <Pencil size={17} />
                      </button>
                      <button
                        className="icon-button icon-button--danger"
                        type="button"
                        onClick={() => setDeactivateTarget(instructor)}
                        aria-label={`Deactivate ${instructor.fullName}`}
                        disabled={!instructor.isActive}
                      >
                        <UserX size={17} />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {deactivateTarget && (
        <ConfirmDialog
          title="Deactivate instructor"
          message={`Deactivate ${deactivateTarget.fullName}? Course history remains intact.`}
          confirmLabel="Deactivate"
          onCancel={() => setDeactivateTarget(null)}
          onConfirm={deactivateInstructor}
        />
      )}
    </section>
  );
}
