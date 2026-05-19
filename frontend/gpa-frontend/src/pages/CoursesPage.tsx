import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { Pencil, Plus, Save, Trash2, X } from 'lucide-react';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { EmptyState } from '../components/EmptyState';
import { StatusBanner } from '../components/StatusBanner';
import { courseApi, departmentApi, getApiErrorMessage } from '../services/api';
import type { Course, CourseForm, Department } from '../types/models';

type CourseFormState = {
  courseCode: string;
  courseTitle: string;
  creditHours: string;
  departmentId: string;
  description: string;
};

const emptyForm: CourseFormState = {
  courseCode: '',
  courseTitle: '',
  creditHours: '3',
  departmentId: '',
  description: '',
};

export function CoursesPage() {
  const [courses, setCourses] = useState<Course[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [form, setForm] = useState<CourseFormState>(emptyForm);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<Course | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const loadData = async () => {
    setLoading(true);
    try {
      const [coursesData, departmentsData] = await Promise.all([
        courseApi.list(),
        departmentApi.list(),
      ]);
      setCourses(coursesData);
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
        const [coursesData, departmentsData] = await Promise.all([
          courseApi.list(),
          departmentApi.list(),
        ]);

        if (!ignore) {
          setCourses(coursesData);
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
    });
    setEditingId(null);
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess(null);

    const payload: CourseForm = {
      courseCode: form.courseCode || undefined,
      courseTitle: form.courseTitle,
      creditHours: Number(form.creditHours),
      departmentId: Number(form.departmentId),
      description: form.description || undefined,
    };

    try {
      if (editingId) {
        await courseApi.update(editingId, {
          ...payload,
          courseCode: form.courseCode,
        });
        setSuccess('Course updated.');
      } else {
        const course = await courseApi.create(payload);
        setSuccess(`Course ${course.courseCode} created.`);
      }

      resetForm();
      await loadData();
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSaving(false);
    }
  };

  const editCourse = (course: Course) => {
    setEditingId(course.courseId);
    setForm({
      courseCode: course.courseCode,
      courseTitle: course.courseTitle,
      creditHours: String(course.creditHours),
      departmentId: String(course.departmentId),
      description: course.description ?? '',
    });
    setError(null);
    setSuccess(null);
  };

  const deleteCourse = async () => {
    if (!deleteTarget) {
      return;
    }

    try {
      await courseApi.delete(deleteTarget.courseId);
      setDeleteTarget(null);
      setSuccess('Course deleted.');
      await loadData();
    } catch (err) {
      setDeleteTarget(null);
      setError(getApiErrorMessage(err));
    }
  };

  const hasDepartments = departments.length > 0;

  return (
    <section className="page">
      <div className="page__header">
        <div>
          <h1>Courses</h1>
          <p>{courses.length} total</p>
        </div>
      </div>

      {!hasDepartments && !loading && (
        <StatusBanner tone="info">Create a department before adding courses.</StatusBanner>
      )}
      {error && <StatusBanner tone="error">{error}</StatusBanner>}
      {success && <StatusBanner tone="success">{success}</StatusBanner>}

      <form className="form-panel" onSubmit={handleSubmit}>
        <div className="form-panel__header">
          <h2>{editingId ? 'Edit Course' : 'Add Course'}</h2>
          {editingId && (
            <button className="icon-button" type="button" onClick={resetForm} aria-label="Cancel edit">
              <X size={18} />
            </button>
          )}
        </div>
        <fieldset disabled={!hasDepartments || saving}>
          <div className="form-grid">
            <label>
              <span>Code</span>
              <input
                maxLength={20}
                minLength={2}
                required={Boolean(editingId)}
                value={form.courseCode}
                onChange={(event) => setForm({ ...form, courseCode: event.target.value })}
              />
            </label>
            <label>
              <span>Title</span>
              <input
                maxLength={200}
                minLength={2}
                required
                value={form.courseTitle}
                onChange={(event) => setForm({ ...form, courseTitle: event.target.value })}
              />
            </label>
            <label>
              <span>Credit Hours</span>
              <input
                max={255}
                min={1}
                required
                type="number"
                value={form.creditHours}
                onChange={(event) => setForm({ ...form, creditHours: event.target.value })}
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
            <label className="form-grid__wide">
              <span>Description</span>
              <textarea
                maxLength={500}
                rows={3}
                value={form.description}
                onChange={(event) => setForm({ ...form, description: event.target.value })}
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
          <EmptyState title="Loading courses" />
        ) : courses.length === 0 ? (
          <EmptyState title="No courses yet" />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Code</th>
                  <th>Course</th>
                  <th>Department</th>
                  <th>Credits</th>
                  <th className="table-actions">Actions</th>
                </tr>
              </thead>
              <tbody>
                {courses.map((course) => (
                  <tr key={course.courseId}>
                    <td>
                      <strong>{course.courseCode}</strong>
                    </td>
                    <td>
                      {course.courseTitle}
                      {course.description && <span>{course.description}</span>}
                    </td>
                    <td>
                      {course.departmentCode}
                      <span>{course.departmentName}</span>
                    </td>
                    <td>{course.creditHours}</td>
                    <td className="table-actions">
                      <button
                        className="icon-button"
                        type="button"
                        onClick={() => editCourse(course)}
                        aria-label={`Edit ${course.courseCode}`}
                      >
                        <Pencil size={17} />
                      </button>
                      <button
                        className="icon-button icon-button--danger"
                        type="button"
                        onClick={() => setDeleteTarget(course)}
                        aria-label={`Delete ${course.courseCode}`}
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
          title="Delete course"
          message={`Delete ${deleteTarget.courseCode}? This is blocked once offerings or prerequisites reference it.`}
          confirmLabel="Delete"
          onCancel={() => setDeleteTarget(null)}
          onConfirm={deleteCourse}
        />
      )}
    </section>
  );
}
