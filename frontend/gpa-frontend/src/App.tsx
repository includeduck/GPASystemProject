import { BrowserRouter, Navigate, NavLink, Route, Routes } from 'react-router-dom';
import {
  BookOpen,
  Building2,
  CalendarDays,
  ClipboardList,
  GraduationCap,
  Link2,
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

const navigation = [
  { to: '/departments', label: 'Departments', icon: Building2 },
  { to: '/students', label: 'Students', icon: Users },
  { to: '/instructors', label: 'Instructors', icon: UserRoundCog },
  { to: '/courses', label: 'Courses', icon: BookOpen },
  { to: '/semesters', label: 'Semesters', icon: CalendarDays },
  { to: '/offerings', label: 'Offerings', icon: ClipboardList },
  { to: '/prerequisites', label: 'Prerequisites', icon: Link2 },
  { to: '/enrollments', label: 'Enrollments', icon: GraduationCap },
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
            <Route path="*" element={<Navigate to="/departments" replace />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
}

export default App;
