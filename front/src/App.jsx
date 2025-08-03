import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/LoginPage'; // Assuming LoginPage is in src/pages/LoginPage/index.jsx
import DashboardPage from './pages/DashboardPage'; // Assuming DashboardPage is in src/pages/DashboardPage/index.jsx

import UserManagement from './pages/DashboardPage/UserManagement';


import './App.css';

// Placeholder components for the nested routes (defined temporarily in DashboardPage)
// In a real app, you'd import these from their own files:
// import UserManagement from './pages/Admin/UserManagement';
// import CourseManagement from './pages/CourseManagement'; // Could be shared
// import Announcements from './pages/Admin/Announcements';
// import AppointmentManagement from './pages/Teacher/AppointmentManagement';
// import MyCourses from './pages/Student/MyCourses';

// For this example, isAuthenticated is managed through localStorage presence of token.
const isAuthenticated = () => {
  return localStorage.getItem('userToken') !== null;
};

function App() {
  // IMPORTANT: For nested routes to work properly, you need to pass the actual components
  // that will be rendered inside <Outlet>.
  // We're defining them here temporarily for demonstration.
  // In a real app, these would be separate files (e.g., src/pages/Admin/UserManagement.jsx)
  // and imported.
  const CourseManagement = () => <div className="content-placeholder"><h2>Course Management Page</h2><p>Content for managing courses.</p></div>;
  const Announcements = () => <div className="content-placeholder"><h2>Announcements Page</h2><p>Content for announcements.</p></div>;
  const AppointmentManagement = () => <div className="content-placeholder"><h2>Appointment Management Page</h2><p>Content for managing appointments.</p></div>;
  const MyCourses = () => <div className="content-placeholder"><h2>My Courses Page</h2><p>Content for student's courses.</p></div>;


  return (
    <Router>
      <div className="App">
        <Routes>
          <Route path="/login" element={<LoginPage />} />

          {/* Protected Dashboard Route with Nested Routes */}
          {/* The parent '/dashboard' route renders the DashboardPage layout. */}
          {/* Child routes render their elements within the <Outlet> of DashboardPage. */}
          <Route
            path="/dashboard"
            element={isAuthenticated() ? <DashboardPage /> : <Navigate to="/login" replace />}
          >
            {/* Nested routes are relative to the parent path (/dashboard/) */}
            {/* Admin Routes */}
            <Route path="users" element={<UserManagement />} />
            <Route path="announcements" element={<Announcements />} />

            {/* Teacher/Admin Course Management (could be shared or specific) */}
            <Route path="courses" element={<CourseManagement />} />

            {/* Teacher Routes */}
            <Route path="appointments" element={<AppointmentManagement />} />

            {/* Student Routes */}
            <Route path="my-courses" element={<MyCourses />} />

            {/* Default content for /dashboard itself */}
            {/* This will render if you just go to /dashboard without a sub-path */}
            <Route index element={<div className="content-placeholder"><h3>Welcome to your Dashboard!</h3><p>Please select an option from the sidebar.</p></div>} />

             {/* Catch-all for unknown dashboard sub-paths */}
             <Route path="*" element={<Navigate to="/dashboard" replace />} />

          </Route>

          {/* Fallback for any unmatched paths outside /login or /dashboard */}
          <Route path="*" element={isAuthenticated() ? <Navigate to="/dashboard" replace /> : <Navigate to="/login" replace />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;