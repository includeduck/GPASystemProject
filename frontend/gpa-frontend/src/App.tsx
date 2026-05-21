import { BrowserRouter, Navigate, NavLink, Route, Routes } from 'react-router-dom';
import type { ReactNode } from 'react';
import {
  BookOpen,
  BookOpenCheck,
  Building2,
  CalendarDays,
  ClipboardList,
  GraduationCap,
  Link2,
  Sliders,
  BarChart3,
  FileBarChart,
  LogOut,
  UserRoundCog,
  UserCircle,
  Users,
} from 'lucide-react';
import './App.css';
import { AuthProvider, useAuth } from './auth/AuthContext';
import { ProtectedRoute } from './components/ProtectedRoute';
import { CoursesPage } from './pages/CoursesPage';
import { DepartmentsPage } from './pages/DepartmentsPage';
import { EnrollmentsPage } from './pages/EnrollmentsPage';
import { InstructorsPage } from './pages/InstructorsPage';
import { OfferingsPage } from './pages/OfferingsPage';
import { PrerequisitesPage } from './pages/PrerequisitesPage';
import { SemestersPage } from './pages/SemestersPage';
import { StudentsPage } from './pages/StudentsPage';
import { GradingPolicyPage } from './pages/GradingPolicyPage';
import { GradebookPage } from './pages/GradebookPage';
import { StudentDashboardPage } from './pages/StudentDashboardPage';
import { ReportsHubPage } from './pages/reports/ReportsHubPage';
import { SemesterReportPage } from './pages/reports/SemesterReportPage';
import { CourseReportPage } from './pages/reports/CourseReportPage';
import { DepartmentReportPage } from './pages/reports/DepartmentReportPage';
import { WarningsReportPage } from './pages/reports/WarningsReportPage';
import { RankingsReportPage } from './pages/reports/RankingsReportPage';
import { LoginPage } from './pages/LoginPage';
import { ProfilePage } from './pages/ProfilePage';
import type { AuthRole } from './types/models';

const navigation = [
  { to: '/departments', label: 'Departments', icon: Building2, roles: ['ADMIN'] },
  { to: '/students', label: 'Students', icon: Users, roles: ['ADMIN'] },
  { to: '/instructors', label: 'Instructors', icon: UserRoundCog, roles: ['ADMIN'] },
  { to: '/courses', label: 'Courses', icon: BookOpen, roles: ['ADMIN'] },
  { to: '/semesters', label: 'Semesters', icon: CalendarDays, roles: ['ADMIN'] },
  { to: '/offerings', label: 'Offerings', icon: ClipboardList, roles: ['ADMIN'] },
  { to: '/prerequisites', label: 'Prerequisites', icon: Link2, roles: ['ADMIN'] },
  { to: '/enrollments', label: 'Enrollments', icon: GraduationCap, roles: ['ADMIN', 'STUDENT'] },
  { to: '/grading-policy', label: 'Grading Policy', icon: Sliders, roles: ['ADMIN'] },
  { to: '/gradebook', label: 'Gradebook', icon: BookOpenCheck, roles: ['ADMIN', 'INSTRUCTOR'] },
  { to: '/reports', label: 'Reports', icon: FileBarChart, roles: ['ADMIN'] },
  { to: '/student-results', label: 'Academic Records', icon: BarChart3, roles: ['ADMIN', 'STUDENT'] },
  { to: '/profile', label: 'Profile', icon: UserCircle, roles: ['ADMIN', 'INSTRUCTOR', 'STUDENT'] },
] satisfies Array<{
  to: string;
  label: string;
  icon: typeof Building2;
  roles: AuthRole[];
}>;

const defaultPathForRole = (role?: AuthRole) => {
  if (role === 'STUDENT') return '/enrollments';
  if (role === 'INSTRUCTOR') return '/gradebook';
  return '/departments';
};

function RoleRoute({ roles, children }: { roles: AuthRole[]; children: ReactNode }) {
  return <ProtectedRoute roles={roles}>{children}</ProtectedRoute>;
}

function StudentResultsEntry() {
  const { user } = useAuth();

  if (user?.role === 'STUDENT') {
    return <Navigate to={`/student-results/${user.studentId}`} replace />;
  }

  return <StudentDashboardPage />;
}

function AppShell() {
  const { user, logout } = useAuth();
  const visibleNavigation = navigation.filter((item) => user && item.roles.includes(user.role));
  const homePath = defaultPathForRole(user?.role);

  return (
    <div className="app-shell">
      <aside className="sidebar" aria-label="Primary">
        <div className="brand">
          <span className="brand__mark">
            <GraduationCap size={24} aria-hidden="true" />
          </span>
          <div>
            <strong>GPA System</strong>
            <span>{user?.role ?? 'Secure'}</span>
          </div>
        </div>
        <nav className="nav-list">
          {visibleNavigation.map((item) => {
            const Icon = item.icon;
            return (
              <NavLink key={item.to} to={item.to}>
                <Icon size={18} aria-hidden="true" />
                {item.label}
              </NavLink>
            );
          })}
        </nav>
        <div className="sidebar-account">
          <div>
            <strong>{user?.displayName}</strong>
            <span>{user?.username}</span>
          </div>
          <button className="icon-button" type="button" onClick={logout} aria-label="Sign out">
            <LogOut size={17} />
          </button>
        </div>
      </aside>

      <main className="workspace">
        <Routes>
          <Route path="/" element={<Navigate to={homePath} replace />} />
          <Route path="/departments" element={<RoleRoute roles={['ADMIN']}><DepartmentsPage /></RoleRoute>} />
          <Route path="/students" element={<RoleRoute roles={['ADMIN']}><StudentsPage /></RoleRoute>} />
          <Route path="/instructors" element={<RoleRoute roles={['ADMIN']}><InstructorsPage /></RoleRoute>} />
          <Route path="/courses" element={<RoleRoute roles={['ADMIN']}><CoursesPage /></RoleRoute>} />
          <Route path="/semesters" element={<RoleRoute roles={['ADMIN']}><SemestersPage /></RoleRoute>} />
          <Route path="/offerings" element={<RoleRoute roles={['ADMIN']}><OfferingsPage /></RoleRoute>} />
          <Route path="/prerequisites" element={<RoleRoute roles={['ADMIN']}><PrerequisitesPage /></RoleRoute>} />
          <Route path="/enrollments" element={<RoleRoute roles={['ADMIN', 'STUDENT']}><EnrollmentsPage /></RoleRoute>} />
          <Route path="/grading-policy" element={<RoleRoute roles={['ADMIN']}><GradingPolicyPage /></RoleRoute>} />
          <Route path="/gradebook" element={<RoleRoute roles={['ADMIN', 'INSTRUCTOR']}><GradebookPage /></RoleRoute>} />
          <Route path="/gradebook/:offeringId" element={<RoleRoute roles={['ADMIN', 'INSTRUCTOR']}><GradebookPage /></RoleRoute>} />
          <Route path="/reports" element={<RoleRoute roles={['ADMIN']}><ReportsHubPage /></RoleRoute>} />
          <Route path="/reports/semester" element={<RoleRoute roles={['ADMIN']}><SemesterReportPage /></RoleRoute>} />
          <Route path="/reports/course" element={<RoleRoute roles={['ADMIN']}><CourseReportPage /></RoleRoute>} />
          <Route path="/reports/department" element={<RoleRoute roles={['ADMIN']}><DepartmentReportPage /></RoleRoute>} />
          <Route path="/reports/warnings" element={<RoleRoute roles={['ADMIN']}><WarningsReportPage /></RoleRoute>} />
          <Route path="/reports/rankings" element={<RoleRoute roles={['ADMIN']}><RankingsReportPage /></RoleRoute>} />
          <Route path="/student-results" element={<RoleRoute roles={['ADMIN', 'STUDENT']}><StudentResultsEntry /></RoleRoute>} />
          <Route path="/student-results/:studentId" element={<RoleRoute roles={['ADMIN', 'STUDENT']}><StudentDashboardPage /></RoleRoute>} />
          <Route path="/profile" element={<RoleRoute roles={['ADMIN', 'INSTRUCTOR', 'STUDENT']}><ProfilePage /></RoleRoute>} />
          <Route path="*" element={<Navigate to={homePath} replace />} />
        </Routes>
      </main>
    </div>
  );
}

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route
            path="/*"
            element={
              <ProtectedRoute>
                <AppShell />
              </ProtectedRoute>
            }
          />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
