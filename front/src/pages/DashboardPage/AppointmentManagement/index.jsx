import React, { useEffect } from 'react';
import { useNavigate, Outlet, useLocation, Link } from 'react-router-dom';

/**
 * The AppointmentManagement component acts as a layout and redirection hub
 * for its nested routes (e.g., 'schedule' and 'records').
 * It redirects to the first sub-menu item by default.
 */
export default function AppointmentManagement() {
  const navigate = useNavigate();
  const location = useLocation();

  // Redirect to the first child route when the parent route is accessed.
  useEffect(() => {
    // Check if the current URL matches the parent path exactly.
    if (location.pathname === '/dashboard/appointments' || location.pathname === '/dashboard/appointments/') {
      // Navigate to the 'schedule' sub-route.
      // `replace: true` prevents a new entry in the browser history,
      // so the back button will go to the previous main page.
      navigate('schedule', { replace: true });
    }
  }, [location.pathname, navigate]);

  return (
    <div>
      {/* This is the placeholder where the child routes will be rendered */}
      <Outlet />
    </div>
  );
}