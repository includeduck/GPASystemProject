import { useEffect, useState, useCallback } from 'react';
import type { FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { Pencil, Plus, Save, UserX, X, GraduationCap } from 'lucide-react';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { CredentialsPanel } from '../components/CredentialsPanel';
import { EmptyState } from '../components/EmptyState';
import { StatusBanner } from '../components/StatusBanner';
import { departmentApi, getApiErrorMessage, studentApi } from '../services/api';
import type { Department, Student, StudentForm, StudentListItem, TemporaryCredentials } from '../types/models';
import { formatDate, todayInputValue } from '../utils/dates';

type StudentFormState = {
  fullName: string;
  email: string;
  phone: string;
  departmentId: string;
  enrollmentDate: string;
  status: Student['status'];
};

const emptyForm: StudentFormState = {
  fullName: '',
  email: '',
  phone: '',
  departmentId: '',
  enrollmentDate: todayInputValue(),
  status: 'ACTIVE',
};

export function StudentsPage() {
  const navigate = useNavigate();
  const [students, setStudents] = useState<StudentListItem[]>([]);
  const [search, setSearch] = useState('');
  const [filterDept, setFilterDept] = useState('');
  const [sortBy, setSortBy] = useState<'name' | 'studentNumber' | 'cgpa'>('name');
  const [sortDir, setSortDir] = useState<'asc' | 'desc'>('asc');
  const [totalCount, setTotalCount] = useState(0);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [form, setForm] = useState<StudentFormState>(emptyForm);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [deactivateTarget, setDeactivateTarget] = useState<Student | null>(null);
  const [credentials, setCredentials] = useState<TemporaryCredentials | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const fetchStudents = useCallback(async () => {
    setLoading(true);
    try {
      const result = await studentApi.search({
        search: search.trim() || undefined,
        departmentId: filterDept ? Number(filterDept) : undefined,
        sortBy,
        sortDir,
        page: 1,
        pageSize: 100,
      });
      setStudents(result.items);
      setTotalCount(result.totalCount);
      setError(null);
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setLoading(false);
    }
  }, [search, filterDept, sortBy, sortDir]);

  useEffect(() => {
    let ignore = false;
    void (async () => {
      try {
        const departmentsData = await departmentApi.list();
        if (!ignore) {
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
        if (!ignore) setError(getApiErrorMessage(err));
      }
    })();
    return () => { ignore = true; };
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => {
      void fetchStudents();
    }, 300);
    return () => clearTimeout(timer);
  }, [fetchStudents]);

  const resetForm = () => {
    setForm({
      ...emptyForm,
      departmentId: departments[0] ? String(departments[0].departmentId) : '',
      enrollmentDate: todayInputValue(),
    });
    setEditingId(null);
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess(null);
    setCredentials(null);

    const basePayload: StudentForm = {
      fullName: form.fullName,
      email: form.email,
      phone: form.phone || undefined,
      departmentId: Number(form.departmentId),
      enrollmentDate: form.enrollmentDate || undefined,
    };

    try {
      if (editingId) {
        await studentApi.update(editingId, {
          ...basePayload,
          enrollmentDate: form.enrollmentDate,
          status: form.status,
        });
        setSuccess('Student updated.');
      } else {
        const response = await studentApi.create(basePayload);
        setCredentials(response.credentials);
        setSuccess(`Student ${response.student.studentNumber} created.`);
      }

      resetForm();
      await fetchStudents();
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSaving(false);
    }
  };

  const editStudent = (student: Student) => {
    setEditingId(student.studentId);
    setCredentials(null);
    setForm({
      fullName: student.fullName,
      email: student.email,
      phone: student.phone ?? '',
      departmentId: String(student.departmentId),
      enrollmentDate: student.enrollmentDate,
      status: student.status,
    });
    setError(null);
    setSuccess(null);
  };

  const deactivateStudent = async () => {
    if (!deactivateTarget) {
      return;
    }

    try {
      await studentApi.deactivate(deactivateTarget.studentId);
      setDeactivateTarget(null);
      setSuccess('Student deactivated.');
      await fetchStudents();
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
          <h1>Students</h1>
          <p>{students.length} total</p>
        </div>
      </div>

      {!hasDepartments && !loading && (
        <StatusBanner tone="info">Create a department before adding students.</StatusBanner>
      )}
      {error && <StatusBanner tone="error">{error}</StatusBanner>}
      {success && <StatusBanner tone="success">{success}</StatusBanner>}
      <CredentialsPanel credentials={credentials} label="New student credentials" />

      <form className="form-panel" onSubmit={handleSubmit}>
        <div className="form-panel__header">
          <h2>{editingId ? 'Edit Student' : 'Add Student'}</h2>
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
              <span>Phone</span>
              <input
                maxLength={20}
                type="tel"
                value={form.phone}
                onChange={(event) => setForm({ ...form, phone: event.target.value })}
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
              <span>Enrollment Date</span>
              <input
                required
                type="date"
                value={form.enrollmentDate}
                onChange={(event) => setForm({ ...form, enrollmentDate: event.target.value })}
              />
            </label>
            {editingId && (
              <label>
                <span>Status</span>
                <select
                  value={form.status}
                  onChange={(event) =>
                    setForm({ ...form, status: event.target.value as Student['status'] })
                  }
                >
                  <option value="ACTIVE">Active</option>
                  <option value="INACTIVE">Inactive</option>
                  <option value="GRADUATED">Graduated</option>
                </select>
              </label>
            )}
          </div>
        </fieldset>
        <div className="form-actions">
          <button className="button button--primary" type="submit" disabled={!hasDepartments || saving}>
            {editingId ? <Save size={17} /> : <Plus size={17} />}
            {editingId ? 'Save' : 'Add'}
          </button>
        </div>
      </form>

      <div className="form-panel" style={{ marginBottom: '1rem' }}>
        <div className="form-grid form-grid--two">
          <label>
            <span>Search (name or ID)</span>
            <input
              type="search"
              placeholder="Search students..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </label>
          <label>
            <span>Department</span>
            <select value={filterDept} onChange={(e) => setFilterDept(e.target.value)}>
              <option value="">All departments</option>
              {departments.map((d) => (
                <option key={d.departmentId} value={d.departmentId}>{d.departmentCode}</option>
              ))}
            </select>
          </label>
          <label>
            <span>Sort by</span>
            <select value={sortBy} onChange={(e) => setSortBy(e.target.value as typeof sortBy)}>
              <option value="name">Name</option>
              <option value="studentNumber">Student ID</option>
              <option value="cgpa">CGPA</option>
            </select>
          </label>
          <label>
            <span>Order</span>
            <select value={sortDir} onChange={(e) => setSortDir(e.target.value as typeof sortDir)}>
              <option value="asc">Ascending</option>
              <option value="desc">Descending</option>
            </select>
          </label>
        </div>
        <p style={{ margin: 0, fontSize: '0.9rem', color: 'var(--text-muted)' }}>
          Showing {students.length} of {totalCount} students
        </p>
      </div>

      <div className="table-panel">
        {loading ? (
          <EmptyState title="Loading students" />
        ) : students.length === 0 ? (
          <EmptyState title="No students match your search" />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Student</th>
                  <th>Email</th>
                  <th>Department</th>
                  <th>CGPA</th>
                  <th>Enrollment</th>
                  <th>Status</th>
                  <th className="table-actions">Actions</th>
                </tr>
              </thead>
              <tbody>
                {students.map((student) => (
                  <tr key={student.studentId}>
                    <td>
                      <strong>{student.fullName}</strong>
                      <span>{student.studentNumber}</span>
                    </td>
                    <td>
                      {student.email}
                      <span>{student.username}</span>
                    </td>
                    <td>
                      {student.departmentCode}
                      <span>{student.departmentName}</span>
                    </td>
                    <td>
                      <strong>{student.cgpa.toFixed(2)}</strong>
                      {student.latestSemesterName && (
                        <span>{student.latestSemesterName}</span>
                      )}
                    </td>
                    <td>{formatDate(student.enrollmentDate)}</td>
                    <td>
                      <span className={`badge badge--${student.status.toLowerCase()}`}>
                        {student.status}
                      </span>
                    </td>
                    <td className="table-actions" style={{ width: 180 }}>
                      <button
                        className="icon-button"
                        type="button"
                        onClick={() => navigate(`/student-results/${student.studentId}`)}
                        title="View Academic Record"
                        aria-label={`View Academic Record for ${student.fullName}`}
                      >
                        <GraduationCap size={17} />
                      </button>
                      <button
                        className="icon-button"
                        type="button"
                        onClick={() => editStudent(student)}
                        aria-label={`Edit ${student.fullName}`}
                      >
                        <Pencil size={17} />
                      </button>
                      <button
                        className="icon-button icon-button--danger"
                        type="button"
                        onClick={() => setDeactivateTarget(student)}
                        aria-label={`Deactivate ${student.fullName}`}
                        disabled={student.status === 'INACTIVE'}
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
          title="Deactivate student"
          message={`Deactivate ${deactivateTarget.fullName}? Academic history is preserved.`}
          confirmLabel="Deactivate"
          onCancel={() => setDeactivateTarget(null)}
          onConfirm={deactivateStudent}
        />
      )}
    </section>
  );
}
