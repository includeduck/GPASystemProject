import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { Pencil, Plus, Save, Trash2, X } from 'lucide-react';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { EmptyState } from '../components/EmptyState';
import { StatusBanner } from '../components/StatusBanner';
import { departmentApi, getApiErrorMessage } from '../services/api';
import type { Department, DepartmentForm } from '../types/models';

const emptyForm: DepartmentForm = {
  departmentCode: '',
  departmentName: '',
};

export function DepartmentsPage() {
  const [departments, setDepartments] = useState<Department[]>([]);
  const [form, setForm] = useState<DepartmentForm>(emptyForm);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Department | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const loadDepartments = async () => {
    setLoading(true);
    try {
      setDepartments(await departmentApi.list());
      setError(null);
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    let ignore = false;

    const loadInitialDepartments = async () => {
      try {
        const data = await departmentApi.list();
        if (!ignore) {
          setDepartments(data);
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

    void loadInitialDepartments();

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
        await departmentApi.update(editingId, form);
        setSuccess('Department updated.');
      } else {
        await departmentApi.create(form);
        setSuccess('Department created.');
      }

      resetForm();
      await loadDepartments();
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSaving(false);
    }
  };

  const editDepartment = (department: Department) => {
    setEditingId(department.departmentId);
    setForm({
      departmentCode: department.departmentCode,
      departmentName: department.departmentName,
    });
    setError(null);
    setSuccess(null);
  };

  const deleteDepartment = async () => {
    if (!deleteTarget) {
      return;
    }

    try {
      await departmentApi.delete(deleteTarget.departmentId);
      setDeleteTarget(null);
      setSuccess('Department deleted.');
      await loadDepartments();
    } catch (err) {
      setDeleteTarget(null);
      setError(getApiErrorMessage(err));
    }
  };

  return (
    <section className="page">
      <div className="page__header">
        <div>
          <h1>Departments</h1>
          <p>{departments.length} total</p>
        </div>
      </div>

      {error && <StatusBanner tone="error">{error}</StatusBanner>}
      {success && <StatusBanner tone="success">{success}</StatusBanner>}

      <form className="form-panel" onSubmit={handleSubmit}>
        <div className="form-panel__header">
          <h2>{editingId ? 'Edit Department' : 'Add Department'}</h2>
          {editingId && (
            <button className="icon-button" type="button" onClick={resetForm} aria-label="Cancel edit">
              <X size={18} />
            </button>
          )}
        </div>
        <div className="form-grid form-grid--two">
          <label>
            <span>Code</span>
            <input
              maxLength={10}
              minLength={2}
              required
              value={form.departmentCode}
              onChange={(event) => setForm({ ...form, departmentCode: event.target.value })}
            />
          </label>
          <label>
            <span>Name</span>
            <input
              maxLength={100}
              minLength={2}
              required
              value={form.departmentName}
              onChange={(event) => setForm({ ...form, departmentName: event.target.value })}
            />
          </label>
        </div>
        <div className="form-actions">
          <button className="button button--primary" type="submit" disabled={saving}>
            {editingId ? <Save size={17} /> : <Plus size={17} />}
            {editingId ? 'Save' : 'Add'}
          </button>
        </div>
      </form>

      <div className="table-panel">
        {loading ? (
          <EmptyState title="Loading departments" />
        ) : departments.length === 0 ? (
          <EmptyState title="No departments yet" detail="Create one before adding students, instructors, or courses." />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Code</th>
                  <th>Name</th>
                  <th className="table-actions">Actions</th>
                </tr>
              </thead>
              <tbody>
                {departments.map((department) => (
                  <tr key={department.departmentId}>
                    <td>
                      <strong>{department.departmentCode}</strong>
                    </td>
                    <td>{department.departmentName}</td>
                    <td className="table-actions">
                      <button
                        className="icon-button"
                        type="button"
                        onClick={() => editDepartment(department)}
                        aria-label={`Edit ${department.departmentCode}`}
                      >
                        <Pencil size={17} />
                      </button>
                      <button
                        className="icon-button icon-button--danger"
                        type="button"
                        onClick={() => setDeleteTarget(department)}
                        aria-label={`Delete ${department.departmentCode}`}
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
          title="Delete department"
          message={`Delete ${deleteTarget.departmentCode}? This is blocked when records already use it.`}
          confirmLabel="Delete"
          onCancel={() => setDeleteTarget(null)}
          onConfirm={deleteDepartment}
        />
      )}
    </section>
  );
}
