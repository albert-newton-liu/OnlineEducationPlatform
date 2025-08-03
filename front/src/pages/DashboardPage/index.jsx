import React, { useEffect, useState } from 'react';
import { useNavigate, Outlet, Link } from 'react-router-dom'; // Import Outlet and Link
import axios from 'axios';
import {API_BASE_URL} from '../../constant/Constants'
import UserManagement from './UserManagement'

import './DashboardPage.css'; // We'll update this CSS

// Placeholder components for different sections
// These will be defined in separate files later (e.g., src/pages/Admin/UserManagement.jsx)
const CourseManagement = () => <div className="content-placeholder"><h2>Course Management Page</h2><p>Content for managing courses.</p></div>;
const Announcements = () => <div className="content-placeholder"><h2>Announcements Page</h2><p>Content for announcements.</p></div>;
const AppointmentManagement = () => <div className="content-placeholder"><h2>Appointment Management Page</h2><p>Content for managing appointments.</p></div>;
const MyCourses = () => <div className="content-placeholder"><h2>My Courses Page</h2><p>Content for student's courses.</p></div>;


function DashboardPage() {
  const navigate = useNavigate();
  const [userData, setUserData] = useState(null); // To store user details
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');



  useEffect(() => {
    const fetchUserData = async () => {
      const token = localStorage.getItem('userToken');
      const userId = localStorage.getItem('userId');

      if (!token || !userId) {
        console.log("No token or userId found, redirecting to login.");
        navigate('/login');
        return;
      }

      try {
        setLoading(true);
        // Fetch detailed user profile based on userId and token
        const response = await axios.get(`${API_BASE_URL}/api/Users/${userId}`, {
          headers: {
            Authorization: `Bearer ${token}`
          }
        });
        setUserData(response.data);
      } catch (err) {
        console.error('Failed to fetch user data:', err);
        setError('Failed to load user data. Please try logging in again.');
        if (err.response && (err.response.status === 401 || err.response.status === 403)) {
          handleLogout();
        }
      } finally {
        setLoading(false);
      }
    };

    fetchUserData();
  }, [navigate]);

  const handleLogout = () => {
    localStorage.removeItem('userToken');
    localStorage.removeItem('userId');
    navigate('/login');
  };

  // Define menu items based on user role
  const getMenuItems = (role) => {
    switch (role) {
      case 2: // Admin role (assuming 2 for Admin)
        return [
          { name: 'User Management', path: 'users', component: UserManagement },
          { name: 'Course Management', path: 'courses', component: CourseManagement },
          { name: 'Announcements', path: 'announcements', component: Announcements },
        ];
      case 1: // Teacher role (assuming 1 for Teacher)
        return [
          { name: 'Course Management', path: 'courses', component: CourseManagement },
          { name: 'Appointment Management', path: 'appointments', component: AppointmentManagement },
        ];
      case 0: // Student role (assuming 0 for Student)
        return [
          { name: 'My Courses', path: 'my-courses', component: MyCourses },
        ];
      default:
        return [];
    }
  };

  if (loading) {
    return (
      <div className="dashboard-layout">
        <p>Loading user data...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="dashboard-layout">
        <p className="error-message">{error}</p>
        <button onClick={handleLogout} className="logout-button">Go to Login</button>
      </div>
    );
  }

  if (!userData) {
    return null; // Should ideally redirect to login via useEffect
  }

  const menuItems = getMenuItems(userData.role);

  return (
    <div className="dashboard-layout">
      {/* Header */}
      <header className="dashboard-header">
        <h1>Online Education Platform</h1>
        <div className="user-info">
          <span>Welcome, {userData.username || 'User'} ({['Student', 'Teacher', 'Admin'][userData.role]})</span>
          <button onClick={handleLogout} className="logout-button">Logout</button>
        </div>
      </header>

      {/* Main Content Area: Sidebar and Page Content */}
      <div className="dashboard-main">
        {/* Sidebar */}
        <nav className="dashboard-sidebar">
          <h3>Menu</h3>
          <ul>
            {menuItems.map((item) => (
              <li key={item.path}>
                {/* Link to nested routes */}
                <Link to={item.path}>{item.name}</Link>
              </li>
            ))}
          </ul>
        </nav>

        {/* Page Content Area - Nested Routes will render here */}
        <main className="dashboard-content">
          {/* Outlet is where the child route components will be rendered */}
          <Outlet />
          {/* Default content if no specific sub-route is matched */}
          {!window.location.pathname.endsWith('/dashboard') && !menuItems.find(item => window.location.pathname.includes(item.path)) && (
             <div className="welcome-message">
               <p>Select an option from the sidebar.</p>
             </div>
           )}
        </main>
      </div>
    </div>
  );
}

export default DashboardPage;