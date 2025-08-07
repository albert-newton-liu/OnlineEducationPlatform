import React, { useEffect, useState } from 'react';
import { useNavigate, Outlet, Link, useLocation } from 'react-router-dom'; // Import useLocation
import axios from 'axios';
import { API_BASE_URL } from '../../constant/Constants'

import './DashboardPage.css'; // We'll update this CSS

function DashboardPage() {
  const navigate = useNavigate();
  const location = useLocation(); // Get the current location object
  const [userData, setUserData] = useState(null);
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

  const getMenuItems = (role) => {
    switch (role) {
      case 2: // Admin role
        return [
          { name: 'User Management', path: 'users' },
          { name: 'Course Management', path: 'courses' },
          { name: 'Announcements', path: 'announcements' }
        ];
      case 1: // Teacher role
        return [
          { name: 'Course Management', path: 'courses' },
          { name: 'Appointment Management', path: 'appointments' },
        ];
      case 0: // Student role
        return [
          { name: 'My Courses', path: 'my-courses' },
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
    return null; 
  }

  const menuItems = getMenuItems(userData.role);
  // Check if the current path is exactly the dashboard index page
  const isDashboardIndex = location.pathname === '/dashboard' || location.pathname === '/dashboard/';


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
                {/* Add a dynamic active class to the Link */}
                <Link 
                  to={item.path} 
                  className={location.pathname.endsWith(item.path) ? 'active' : ''}
                >
                  {item.name}
                </Link>
              </li>
            ))}
          </ul>
        </nav>

        {/* Page Content Area - Nested Routes will render here */}
        <main className="dashboard-content">
          {/* Outlet is where the child route components will be rendered */}
          <Outlet />
          
          {/* Corrected welcome message: only render if on the dashboard index page */}
          {/* {isDashboardIndex && (
             <div className="welcome-message">
               <h3>Welcome to your Dashboard!</h3>
               <p>Select an option from the sidebar.</p>
             </div>
          )} */}
        </main>
      </div>
    </div>
  );
}

export default DashboardPage;