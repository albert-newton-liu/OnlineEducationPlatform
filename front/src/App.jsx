import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';

// Import your child route components from their dedicated files
import UserManagement from './pages/DashboardPage/UserManagement'; 
import CourseManagement from './pages/DashboardPage/CourseManagement'; 
import AddCoursePage from './pages/DashboardPage/CourseManagement/AddCoursePage'; // <-- Add this import

// Temporary placeholders (replace with actual imports when you create these files)
const Announcements = () => <div className="content-placeholder"><h2>Announcements Page</h2><p>Content for announcements.</p></div>;
const AppointmentManagement = () => <div className="content-placeholder"><h2>Appointment Management Page</h2><p>Content for managing appointments.</p></div>;
const MyCourses = () => <div className="content-placeholder"><h2>My Courses Page</h2><p>Content for student's courses.</p></div>;

function App() {
  // Use state to manage the login status. Initialize it by checking localStorage once.
  const [isLoggedIn, setIsLoggedIn] = useState(localStorage.getItem('userToken') !== null);

  // This function will be called by LoginPage after successful login.
  const handleLoginSuccess = () => {
    setIsLoggedIn(true); // Update the state, triggering App.jsx to re-render.
  };

  // Optional: A logout handler to clear token and update state
  const handleLogout = () => {
    localStorage.removeItem('userToken');
    localStorage.removeItem('userId');
    setIsLoggedIn(false);
  };

  return (
    <Router>
      <div className="App">
       
        <Routes>
          {/* Pass the handleLoginSuccess function to the LoginPage component */}
          <Route path="/login" element={<LoginPage onLoginSuccess={handleLoginSuccess} />} />

          <Route
            path="/dashboard"
            // Now use the `isLoggedIn` state variable for conditional rendering
            element={isLoggedIn ? <DashboardPage onLogout={handleLogout} /> : <Navigate to="/login" replace />}
          >
            <Route path="users" element={<UserManagement />} />
            <Route path="announcements" element={<Announcements />} />
            <Route path="courses" element={<CourseManagement />} />
            <Route path="add-course" element={<AddCoursePage />} />  {/* <-- Add this new route for the new page */}
            <Route path="appointments" element={<AppointmentManagement />} />
            <Route path="my-courses" element={<MyCourses />} />
            <Route index element={<div className="content-placeholder"><h3>Welcome to your Dashboard!</h3><p>Please select an option from the sidebar.</p></div>} />
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
          </Route>

          <Route path="*" element={isLoggedIn ? <Navigate to="/dashboard" replace /> : <Navigate to="/login" replace />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;