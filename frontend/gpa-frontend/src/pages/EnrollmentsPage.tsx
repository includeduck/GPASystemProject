import { useEffect, useState } from 'react';
import { Plus } from 'lucide-react';
import { useAuth } from '../auth/AuthContext';
import { EmptyState } from '../components/EmptyState';
import { StatusBanner } from '../components/StatusBanner';
import { enrollmentApi, getApiErrorMessage, semesterApi, studentApi } from '../services/api';
import type { AvailableOffering, Enrollment, Semester, Student } from '../types/models';
import { formatDateTime } from '../utils/dates';

export function EnrollmentsPage() {
  const { user } = useAuth();
  const isStudent = user?.role === 'STUDENT';
  const [students, setStudents] = useState<Student[]>([]);
  const [semesters, setSemesters] = useState<Semester[]>([]);
  const [enrollments, setEnrollments] = useState<Enrollment[]>([]);
  const [availableOfferings, setAvailableOfferings] = useState<AvailableOffering[]>([]);
  const [selectedStudentId, setSelectedStudentId] = useState('');
  const [selectedSemesterId, setSelectedSemesterId] = useState('');
  const [loading, setLoading] = useState(true);
  const [loadingEnrollment, setLoadingEnrollment] = useState(false);
  const [savingOfferingId, setSavingOfferingId] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  useEffect(() => {
    let ignore = false;

    const loadInitialData = async () => {
      try {
        const semestersData = await semesterApi.list();
        const studentsData = isStudent ? [] : await studentApi.list();

        if (!ignore) {
          const activeStudents = studentsData.filter((student) => student.status === 'ACTIVE' && student.isActive);
          setStudents(studentsData);
          setSemesters(semestersData);
          setSelectedStudentId(
            isStudent
              ? String(user?.studentId ?? '')
              : activeStudents[0] ? String(activeStudents[0].studentId) : '',
          );
          setSelectedSemesterId(
            String(semestersData.find((semester) => semester.isCurrent)?.semesterId ?? semestersData[0]?.semesterId ?? ''),
          );
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

    void loadInitialData();

    return () => {
      ignore = true;
    };
  }, [isStudent, user?.studentId]);

  useEffect(() => {
    if (!selectedStudentId) {
      return;
    }

    let ignore = false;

    const loadEnrollmentData = async () => {
      setLoadingEnrollment(true);
      try {
        const [enrollmentsData, availableData] = await Promise.all([
          enrollmentApi.forStudent(Number(selectedStudentId)),
          enrollmentApi.available(
            Number(selectedStudentId),
            selectedSemesterId ? Number(selectedSemesterId) : undefined,
          ),
        ]);

        if (!ignore) {
          setEnrollments(enrollmentsData);
          setAvailableOfferings(availableData);
          setError(null);
        }
      } catch (err) {
        if (!ignore) {
          setError(getApiErrorMessage(err));
        }
      } finally {
        if (!ignore) {
          setLoadingEnrollment(false);
        }
      }
    };

    void loadEnrollmentData();

    return () => {
      ignore = true;
    };
  }, [selectedStudentId, selectedSemesterId]);

  const refreshEnrollmentData = async () => {
    if (!selectedStudentId) {
      return;
    }

    const [enrollmentsData, availableData] = await Promise.all([
      enrollmentApi.forStudent(Number(selectedStudentId)),
      enrollmentApi.available(
        Number(selectedStudentId),
        selectedSemesterId ? Number(selectedSemesterId) : undefined,
      ),
    ]);
    setEnrollments(enrollmentsData);
    setAvailableOfferings(availableData);
  };

  const enroll = async (offering: AvailableOffering) => {
    setSavingOfferingId(offering.offering.offeringId);
    setError(null);
    setSuccess(null);

    try {
      const enrollment = await enrollmentApi.enroll(Number(selectedStudentId), offering.offering.offeringId);
      setSuccess(`${enrollment.studentName} enrolled in ${enrollment.courseCode}.`);
      await refreshEnrollmentData();
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSavingOfferingId(null);
    }
  };

  const activeStudents = students.filter((student) => student.status === 'ACTIVE' && student.isActive);
  const hasSetup = (isStudent ? Boolean(selectedStudentId) : activeStudents.length > 0) && semesters.length > 0;

  return (
    <section className="page">
      <div className="page__header">
        <div>
          <h1>Enrollments</h1>
          <p>{enrollments.length} current records</p>
        </div>
      </div>

      {!hasSetup && !loading && (
        <StatusBanner tone="info">Create an active student and semester before enrolling courses.</StatusBanner>
      )}
      {error && <StatusBanner tone="error">{error}</StatusBanner>}
      {success && <StatusBanner tone="success">{success}</StatusBanner>}

      <div className="form-panel">
        <div className="form-panel__header">
          <h2>Student Registration</h2>
        </div>
        <fieldset disabled={!hasSetup || loadingEnrollment}>
          <div className="form-grid form-grid--two">
            {isStudent ? (
              <label>
                <span>Student</span>
                <input value={user?.displayName ?? ''} readOnly />
              </label>
            ) : (
              <label>
                <span>Student</span>
                <select
                  value={selectedStudentId}
                  onChange={(event) => setSelectedStudentId(event.target.value)}
                >
                  {activeStudents.map((student) => (
                    <option key={student.studentId} value={student.studentId}>
                      {student.studentNumber} - {student.fullName}
                    </option>
                  ))}
                </select>
              </label>
            )}
            <label>
              <span>Semester</span>
              <select
                value={selectedSemesterId}
                onChange={(event) => setSelectedSemesterId(event.target.value)}
              >
                {semesters.map((semester) => (
                  <option key={semester.semesterId} value={semester.semesterId}>
                    {semester.semesterName}{semester.isCurrent ? ' - Current' : ''}
                  </option>
                ))}
              </select>
            </label>
          </div>
        </fieldset>
      </div>

      <div className="table-panel">
        {loading || loadingEnrollment ? (
          <EmptyState title="Loading available offerings" />
        ) : availableOfferings.length === 0 ? (
          <EmptyState title="No available offerings" />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Offering</th>
                  <th>Instructor</th>
                  <th>Seats</th>
                  <th>Eligibility</th>
                  <th className="table-actions">Actions</th>
                </tr>
              </thead>
              <tbody>
                {availableOfferings.map((available) => (
                  <tr key={available.offering.offeringId}>
                    <td>
                      <strong>{available.offering.courseCode}</strong>
                      <span>{available.offering.courseTitle}</span>
                    </td>
                    <td>{available.offering.instructorName}</td>
                    <td>
                      {available.offering.currentEnrollment} / {available.offering.maxCapacity}
                      <span>{available.offering.seatsAvailable} open</span>
                    </td>
                    <td>
                      <span className={`badge badge--${available.canEnroll ? 'active' : 'inactive'}`}>
                        {available.canEnroll ? 'ELIGIBLE' : available.blockedReason}
                      </span>
                      {available.missingPrerequisites.length > 0 && (
                        <span>
                          {available.missingPrerequisites
                            .map((prerequisite) => prerequisite.courseCode)
                            .join(', ')}
                        </span>
                      )}
                    </td>
                    <td className="table-actions">
                      <button
                        className="icon-button"
                        type="button"
                        onClick={() => enroll(available)}
                        aria-label={`Enroll in ${available.offering.courseCode}`}
                        disabled={!available.canEnroll || savingOfferingId === available.offering.offeringId}
                      >
                        <Plus size={17} />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <div className="table-panel">
        {loading || loadingEnrollment ? (
          <EmptyState title="Loading enrollments" />
        ) : enrollments.length === 0 ? (
          <EmptyState title="No enrollments for this student" />
        ) : (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Course</th>
                  <th>Semester</th>
                  <th>Instructor</th>
                  <th>Enrolled</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {enrollments.map((enrollment) => (
                  <tr key={enrollment.enrollmentId}>
                    <td>
                      <strong>{enrollment.courseCode}</strong>
                      <span>{enrollment.courseTitle}</span>
                    </td>
                    <td>{enrollment.semesterName}</td>
                    <td>{enrollment.instructorName}</td>
                    <td>{formatDateTime(enrollment.enrollmentDate)}</td>
                    <td>
                      <span className={`badge badge--${enrollment.status.toLowerCase()}`}>
                        {enrollment.status}
                      </span>
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
