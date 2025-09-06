import React, { useEffect, useState } from 'react';
import { useNavigate, Outlet, Link, useLocation } from 'react-router-dom';
import axios from 'axios';
import { API_BASE_URL } from '../../constant/Constants';
import * as signalR from '@microsoft/signalr';

import './DashboardPage.css';

function DashboardPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const [userData, setUserData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [collapsedItems, setCollapsedItems] = useState({});

  const [connection, setConnection] = useState(null);
  const [notifications, setNotifications] = useState([]);
  const [isDropdownVisible, setIsDropdownVisible] = useState(false);

  // Use a single useEffect for user data and SignalR connection
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

        // --- SignalR Connection Logic (Moved Here) ---
        const newConnection = new signalR.HubConnectionBuilder()
          .withUrl(`${API_BASE_URL}/notificationHub`, {
            accessTokenFactory: () => token 
          })
          .withAutomaticReconnect()
          .build();
        
        setConnection(newConnection);

        newConnection.start()
          .then(() => {
            console.log('Connected to SignalR hub!');
            newConnection.on('ReceiveNotification', (message) => {
              setNotifications(prevNotifications => [...prevNotifications, message]);
              console.log("message", message)
            });
          })
          .catch(err => {
            console.error('SignalR connection failed: ', err);
            if (err.statusCode === 401) {
              handleLogout(); // Log out on unauthorized connection
            }
          });

        // Cleanup function for the SignalR connection
        return () => {
          newConnection.stop();
        };
        // --- End of SignalR Logic ---

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
  }, [navigate]); // The dependency is on 'navigate' only, as the fetchUserData logic is self-contained.

  const handleLogout = () => {
    localStorage.removeItem('userToken');
    localStorage.removeItem('userId');
    // Stop the SignalR connection before navigating
    if (connection) {
        connection.stop();
    }
    navigate('/login');
  };

  // Function to toggle the collapse state of a menu item
  const toggleCollapse = (path) => {
    setCollapsedItems(prevState => ({
      ...prevState,
      [path]: !prevState[path]
    }));
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
          {
            name: 'Appointment Management',
            path: 'appointments',
            subItems: [
              { name: 'Schedule Management', path: 'schedule' },
              { name: 'Appointment Records', path: 'records' }
            ]
          }
        ];
      case 0: // Student role
        return [
          { name: 'My Courses', path: 'my-courses' },
          { name: 'View Courses', path: 'courses' },
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

  return (
    <div className="dashboard-layout">
      {/* Header */}
    
      <header className="dashboard-header">
        <h1>Online Education Platform</h1>
        <div className="user-info">
          <span>Welcome, {userData?.username || 'User'} ({['Student', 'Teacher', 'Admin'][userData?.role]})</span>

          {/* Notifications Icon and Count */}
          <div className="notifications-container">
            <button
              className="notification-icon-button"
              onClick={() => setIsDropdownVisible(!isDropdownVisible)}
            >
              🔔
              {/* Show a badge with the count of new notifications */}
              {notifications.length > 0 && (
                <span className="notification-badge">{notifications.length}</span>
              )}
            </button>

            {/* Notifications Dropdown/Modal */}
            {isDropdownVisible && (
              <div className="notifications-dropdown">
                <h3>Notifications</h3>
                {notifications.length > 0 ? (
                  <ul>
                    {notifications.map((msg, index) => (
                      <li key={index} className="notification-item">
                        {msg}
                      </li>
                    ))}
                  </ul>
                ) : (
                  <p>No new notifications.</p>
                )}
              </div>
            )}
          </div>

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
              <li key={item.path} className="menu-item-with-submenu">
                {/* A button to act as a collapsible trigger for submenus, or a direct link for regular items */}
                <button
                  onClick={() => item.subItems ? toggleCollapse(item.path) : navigate(item.path)}
                  className={`menu-toggle-button ${location.pathname.startsWith(`/dashboard/${item.path}`) ? 'active' : ''} ${item.subItems ? 'has-submenu' : ''}`}
                >
                  {item.name}
                </button>
                {/* Only render the submenu if subItems exist and the item is not collapsed */}
                {item.subItems && !collapsedItems[item.path] && (
                  <ul className="submenu">
                    {item.subItems.map((subItem) => (
                      <li key={subItem.path}>
                        <Link
                          to={`${item.path}/${subItem.path}`}
                          className={location.pathname.endsWith(subItem.path) ? 'active' : ''}
                        >
                          {subItem.name}
                        </Link>
                      </li>
                    ))}
                  </ul>
                )}
              </li>
            ))}
          </ul>
        </nav>

        <main className="dashboard-content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

export default DashboardPage;