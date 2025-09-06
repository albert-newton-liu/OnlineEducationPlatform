import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';

// Import your child route components
import UserManagement from './pages/DashboardPage/UserManagement';
import CourseManagement from './pages/DashboardPage/CourseManagement';
import AddCoursePage from './pages/DashboardPage/CourseManagement/AddCoursePage';
import ViewCoursePage from './pages/DashboardPage/CourseManagement/ViewCoursePage';
import AppointmentManagement from './pages/DashboardPage/AppointmentManagement';
import ScheduleManagementPage from './pages/DashboardPage/AppointmentManagement/ScheduleManagementPage';
import AppointmentRecordPage from './components/AppointmentRecordPage';
import MyCourseManagement from './pages/DashboardPage/MyCourseManagement'

// Temporary placeholders (replace with actual imports)
const Announcements = () => <div className="content-placeholder"><h2>Announcements Page</h2><p>Content for announcements.</p></div>;

function App() {
  // Use state to manage the login status. Initialize it by checking localStorage once.
  const [isLoggedIn, setIsLoggedIn] = useState(localStorage.getItem('userToken') !== null);

  return (
    <Router>
      <div className="App">
        <Routes>
          {/* Public route for the login page */}
          <Route path="/login" element={<LoginPage />} />

          {/* Protected route for the dashboard and its nested pages */}
          <Route
            path="/dashboard"
            // Redirect to login if the user is not authenticated
            element={isLoggedIn ? <DashboardPage /> : <Navigate to="/login" replace />}
          >
            {/* Direct child routes matching the sidebar menu items */}
            <Route path="users" element={<UserManagement />} />
            <Route path="announcements" element={<Announcements />} />
            <Route path="my-courses" element={<MyCourseManagement />} />

            {/* Nested routes for Course Management (teacher/admin) */}
            <Route path="courses" element={<CourseManagement />} />
            <Route path="add-course" element={<AddCoursePage />} />
            <Route path="view-course/:lessonId" element={<ViewCoursePage />} />


            {/* Nested routes for Appointment Management (teacher) */}
            <Route path="appointments" element={<AppointmentManagement />} >
              {/* Direct child routes for the appointment sub-menu */}
              <Route path="schedule" element={<ScheduleManagementPage />} />
              <Route path="records" element={<AppointmentRecordPage />} />
              {/* Pathless route for the default content of the appointments page */}
              <Route index element={<h3>Please select an appointment management option from the sub-menu.</h3>} />
            </Route>

            {/* Default content for the /dashboard path */}
            <Route index element={<div className="content-placeholder"><h3>Welcome to your Dashboard!</h3><p>Please select an option from the sidebar.</p></div>} />

            {/* Catch-all route for any undefined dashboard path, redirects to the index */}
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
          </Route>

          {/* Catch-all route for any undefined path, redirects to dashboard or login */}
          <Route path="*" element={isLoggedIn ? <Navigate to="/dashboard" replace /> : <Navigate to="/login" replace />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;