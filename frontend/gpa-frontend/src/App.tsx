import { BrowserRouter, Navigate, NavLink, Route, Routes } from 'react-router-dom';
import { BookOpen, Building2, GraduationCap, UserRoundCog, Users } from 'lucide-react';
import './App.css';
import { CoursesPage } from './pages/CoursesPage';
import { DepartmentsPage } from './pages/DepartmentsPage';
import { InstructorsPage } from './pages/InstructorsPage';
import { StudentsPage } from './pages/StudentsPage';

const navigation = [
  { to: '/departments', label: 'Departments', icon: Building2 },
  { to: '/students', label: 'Students', icon: Users },
  { to: '/instructors', label: 'Instructors', icon: UserRoundCog },
  { to: '/courses', label: 'Courses', icon: BookOpen },
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
            <Route path="*" element={<Navigate to="/departments" replace />} />
          </Routes>
        </main>
      </div>
    </BrowserRouter>
  );
}

export default App;
