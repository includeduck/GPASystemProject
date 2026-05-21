import { useCallback, useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { Pencil, Plus, Save, Trash2, X, BookOpenCheck } from 'lucide-react';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { EmptyState } from '../components/EmptyState';
import { StatusBanner } from '../components/StatusBanner';
import {
  courseApi,
  courseOfferingApi,
  getApiErrorMessage,
  instructorApi,
  semesterApi,
} from '../services/api';
import type { Course, CourseOffering, CourseOfferingForm, Instructor, Semester } from '../types/models';

type OfferingFormState = {
  courseId: string;
  semesterId: string;
  instructorId: string;
  maxCapacity: string;
  status: CourseOffering['status'];
};

const emptyForm: OfferingFormState = {
  courseId: '',
  semesterId: '',
  instructorId: '',
  maxCapacity: '30',
  status: 'ACTIVE',
};

export function OfferingsPage() {
  const navigate = useNavigate();
  const [offerings, setOfferings] = useState<CourseOffering[]>([]);
  const [courses, setCourses] = useState<Course[]>([]);
  const [semesters, setSemesters] = useState<Semester[]>([]);
  const [instructors, setInstructors] = useState<Instructor[]>([]);
  const [form, setForm] = useState<OfferingFormState>(emptyForm);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<CourseOffering | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const applyDefaults = useCallback((
    coursesData: Course[],
    semestersData: Semester[],
    instructorsData: Instructor[],
  ) => {
    setForm((current) => ({
      ...current,
      courseId: current.courseId || String(coursesData[0]?.courseId ?? ''),
      semesterId:
        current.semesterId ||
        String(semestersData.find((semester) => semester.isCurrent)?.semesterId ?? semestersData[0]?.semesterId ?? ''),
      instructorId: current.instructorId || String(instructorsData.find((instructor) => instructor.isActive)?.instructorId ?? ''),
    }));
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [offeringsData, coursesData, semestersData, instructorsData] = await Promise.all([
        courseOfferingApi.list(),
        courseApi.list(),
        semesterApi.list(),
        instructorApi.list(),
      ]);
      setOfferings(offeringsData);
      setCourses(coursesData);
      setSemesters(semestersData);
      setInstructors(instructorsData);
      setError(null);

      if (!editingId) {
        applyDefaults(coursesData, semestersData, instructorsData);
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
        const [offeringsData, coursesData, semestersData, instructorsData] = await Promise.all([
          courseOfferingApi.list(),
          courseApi.list(),
          semesterApi.list(),
          instructorApi.list(),
        ]);

        if (!ignore) {
          setOfferings(offeringsData);
          setCourses(coursesData);
          setSemesters(semestersData);
          setInstructors(instructorsData);
          setError(null);
          applyDefaults(coursesData, semestersData, instructorsData);
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
  }, [applyDefaults]);

  const resetForm = () => {
    setForm({
      ...emptyForm,
      courseId: courses[0] ? String(courses[0].courseId) : '',
      semesterId: String(semesters.find((semester) => semester.isCurrent)?.semesterId ?? semesters[0]?.semesterId ?? ''),
      instructorId: String(instructors.find((instructor) => instructor.isActive)?.instructorId ?? ''),
    });
    setEditingId(null);
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSuccess(null);

    const payload: CourseOfferingForm = {
      courseId: Number(form.courseId),
      semesterId: Number(form.semesterId),
      instructorId: Number(form.instructorId),
      maxCapacity: Number(form.maxCapacity),
      status: form.status,
    };

    try {
      if (editingId) {
        await courseOfferingApi.update(editingId, payload);
        setSuccess('Course offering updated.');
      } else {
        await courseOfferingApi.create(payload);
        setSuccess('Course offering created.');
      }

      resetForm();
      await loadData();
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSaving(false);
    }
  };

  const editOffering = (offering: CourseOffering) => {
    setEditingId(offering.offeringId);
    setForm({
      courseId: String(offering.courseId),
      semesterId: String(offering.semesterId),
      instructorId: String(offering.instructorId),
      maxCapacity: String(offering.maxCapacity),
      status: offering.status,
    });
    setError(null);
    setSuccess(null);
  };

  const deleteOffering = async () => {
    if (!deleteTarget) {
      return;
    }

    try {
      await courseOfferingApi.delete(deleteTarget.offeringId);
      setDeleteTarget(null);
      setSuccess('Course offering deleted.');
      await loadData();
    } catch (err) {
      setDeleteTarget(null);
      setError(getApiErrorMessage(err));
    }
  };

  const activeInstructors = instructors.filter((instructor) => instructor.isActive);
  const hasSetup = courses.length > 0 && semesters.length > 0 && activeInstructors.length > 0;

  return (
    <section className="page">
      <div className="page__header">
        <div>
          <h1>Offerings</h1>
          <p>{offerings.length} total</p>
        </div>
      </div>

      {!hasSetup && !loading && (
        <StatusBanner tone="info">Create courses, semesters, and active instructors before adding offerings.</StatusBanner>
      )}
      {error && <StatusBanner tone="error">{error}</StatusBanner>}
      {success && <StatusBanner tone="success">{success}</StatusBanner>}

      <form className="form-panel" onSubmit={handleSubmit}>
        <div className="form-panel__header">
          <h2>{editingId ? 'Edit Offering' : 'Add Offering'}</h2>
          {editingId && (
            <button className="icon-button" type="button" onClick={resetForm} aria-label="Cancel edit">
              <X size={18} />
            </button>
          )}
        </div>
        <fieldset disabled={!hasSetup || saving}>
          <div className="form-grid">
            <label>
              <span>Course</span>
              <select value={form.courseId} onChange={(event) => setForm({ ...form, courseId: event.target.value })}>
                {courses.map((course) => (
                  <option key={course.courseId} value={course.courseId}>
                    {course.courseCode} - {course.courseTitle}
                  </option>
                ))}
              </select>
            </label>
            <label>
              <span>Semester</span>
              <select value={form.semesterId} onChange={(event) => setForm({ ...form, semesterId: event.target.value })}>
                {semesters.map((semester) => (
                  <option key={semester.semesterId} value={semester.semesterId}>
                    {semester.semesterName}{semester.isCurrent ? ' - Current' : ''}
                  </option>
                ))}
              </select>
            </label>
            <label>
              <span>Instructor</span>
              <select
                value={form.instructorId}
                onChange={(event) => setForm({ ...form, instructorId: event.target.value })}
              >
                {activeInstructors.map((instructor) => (
                  <option key={instructor.instructorId} value={instructor.instructorId}>
                    {instructor.fullName}
                  </option>
                ))}
              </select>
            </label>
            <label>
              <span>Max Capacity</span>
              <input
                min={1}
                required
                type="number"
                value={form.maxCapacity}
                onChange={(event) => setForm({ ...form, maxCapacity: event.target.value })}
              />
            </label>
            <label>
              <span>Status</span>
              <select
                value={form.status}
                onChange={(event) =>
                  setForm({ ...form, status: event.target.value as CourseOffering['status'] })
                }
              >
                <option value="ACTIVE">Active</option>
                <option value="COMPLETED">Completed</option>
                <option value="CANCELLED">Cancelled</option>
              </select>
            </label>
          </div>
        </fieldset>
        <div className="form-actions">
          <button className="button button--primary" type="submit" disabled={!hasSetup || saving}>
            {editingId ? <Save size={17} /> : <Plus size={17} />}
            {editingId ? 'Save' : 'Add'}
          </button>
        </div>
      </form>

      <div className="table-panel">
        {loading ? (
          <EmptyState title="Loading offerings" />
        ) : offerings.length === 0 ? (
          <EmptyState title="No offerings yet" />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Course</th>
                  <th>Semester</th>
                  <th>Instructor</th>
                  <th>Seats</th>
                  <th>Status</th>
                  <th className="table-actions">Actions</th>
                </tr>
              </thead>
              <tbody>
                {offerings.map((offering) => (
                  <tr key={offering.offeringId}>
                    <td>
                      <strong>{offering.courseCode}</strong>
                      <span>{offering.courseTitle}</span>
                    </td>
                    <td>
                      {offering.semesterName}
                      <span>{offering.isCurrentSemester ? 'Current semester' : offering.departmentCode}</span>
                    </td>
                    <td>{offering.instructorName}</td>
                    <td>
                      {offering.currentEnrollment} / {offering.maxCapacity}
                      <span>{offering.seatsAvailable} open</span>
                    </td>
                    <td>
                      <span className={`badge badge--${offering.status.toLowerCase()}`}>
                        {offering.status}
                      </span>
                    </td>
                    <td className="table-actions" style={{ width: 180 }}>
                      <button
                        className="icon-button"
                        type="button"
                        onClick={() => navigate(`/gradebook/${offering.offeringId}`)}
                        title="Open Gradebook"
                        aria-label={`Open Gradebook for ${offering.courseCode}`}
                      >
                        <BookOpenCheck size={17} />
                      </button>
                      <button
                        className="icon-button"
                        type="button"
                        onClick={() => editOffering(offering)}
                        aria-label={`Edit ${offering.courseCode} offering`}
                      >
                        <Pencil size={17} />
                      </button>
                      <button
                        className="icon-button icon-button--danger"
                        type="button"
                        onClick={() => setDeleteTarget(offering)}
                        aria-label={`Delete ${offering.courseCode} offering`}
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
          title="Delete offering"
          message={`Delete ${deleteTarget.courseCode} for ${deleteTarget.semesterName}? This is blocked once dependent records exist.`}
          confirmLabel="Delete"
          onCancel={() => setDeleteTarget(null)}
          onConfirm={deleteOffering}
        />
      )}
    </section>
  );
}
