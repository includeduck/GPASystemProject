import { BrowserRouter, Navigate, NavLink, Route, Routes } from 'react-router-dom';
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
  UserRoundCog,
  Users,
} from 'lucide-react';
import './App.css';
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

const navigation = [
  { to: '/departments', label: 'Departments', icon: Building2 },
  { to: '/students', label: 'Students', icon: Users },
  { to: '/instructors', label: 'Instructors', icon: UserRoundCog },
  { to: '/courses', label: 'Courses', icon: BookOpen },
  { to: '/semesters', label: 'Semesters', icon: CalendarDays },
  { to: '/offerings', label: 'Offerings', icon: ClipboardList },
  { to: '/prerequisites', label: 'Prerequisites', icon: Link2 },
  { to: '/enrollments', label: 'Enrollments', icon: GraduationCap },
  { to: '/grading-policy', label: 'Grading Policy', icon: Sliders },
  { to: '/gradebook', label: 'Gradebook', icon: BookOpenCheck },
  { to: '/reports', label: 'Reports', icon: FileBarChart },
  { to: '/student-results', label: 'Academic Records', icon: BarChart3 },
];

function App() {
  return (
    <BrowserRouter>
      <div className="app-shell">
        <aside className="sidebar" aria-label="Primary">
          <div className="brand">
            <span className="brand__mark">
              <GraduationCap size={24} aria-hidden="true" />
            </span>
            <div>
              <strong>GPA System</strong>
              <span>Admin</span>
            </div>
          </div>
          <nav className="nav-list">
            {navigation.map((item) => {
              const Icon = item.icon;
              return (
                <NavLink key={item.to} to={item.to}>
                  <Icon size={18} aria-hidden="true" />
                  {item.label}
                </NavLink>
              );
            })}
          </nav>
        </aside>

        <main className="workspace">
          <Routes>
            <Route path="/" element={<Navigate to="/departments" replace />} />
            <Route path="/departments" element={<DepartmentsPage />} />
            <Route path="/students" element={<StudentsPage />} />
            <Route path="/instructors" element={<InstructorsPage />} />
            <Route path="/courses" element={<CoursesPage />} />
            <Route path="/semesters" element={<SemestersPage />} />
            <Route path="/offerings" element={<OfferingsPage />} />
            <Route path="/prerequisites" element={<PrerequisitesPage />} />
            <Route path="/enrollments" element={<EnrollmentsPage />} />
            <Route path="/grading-policy" element={<GradingPolicyPage />} />
            <Route path="/gradebook/:offeringId" element={<GradebookPage />} />
            <Route path="/reports" element={<ReportsHubPage />} />
            <Route path="/reports/semester" element={<SemesterReportPage />} />
            <Route path="/reports/course" element={<CourseReportPage />} />
            <Route path="/reports/department" element={<DepartmentReportPage />} />
            <Route path="/reports/warnings" element={<WarningsReportPage />} />
            <Route path="/reports/rankings" element={<RankingsReportPage />} />
            <Route path="/student-results" element={<StudentDashboardPage />} />
            <Route path="/student-results/:studentId" element={<StudentDashboardPage />} />
            <Route path="*" element={<Navigate to="/departments" replace />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
}

export default App;
