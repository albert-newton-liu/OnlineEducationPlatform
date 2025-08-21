import React, { useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

import { API_BASE_URL } from '../../constant/Constants';

import './LoginPage.css';

// Accept onLoginSuccess as a prop
function LoginPage({ onLoginSuccess }) { // <--- Added onLoginSuccess prop
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const response = await axios.post(`${API_BASE_URL}/api/Users/login`, {
        username,
        password,
      });

      console.log('Login successful!', response.data);

      // Store user authentication token/data
      localStorage.setItem('userToken', response.data.token);
      localStorage.setItem('userId', response.data.userId);
      localStorage.setItem('role', response.data.role);

      // --- CRITICAL CHANGE HERE ---
      // Call the onLoginSuccess function to update the parent App component's state.
      if (onLoginSuccess) {
        onLoginSuccess();
      }

      // Redirect the user to the dashboard
      navigate('/dashboard');

    } catch (err) {
      console.error('Login failed:', err);
      if (err.response) {
        if (err.response.status === 401) {
          setError('Invalid username or password.');
        } else if (err.response.data && err.response.data.message) {
          setError(err.response.data.message);
        } else {
          setError('An unexpected error occurred during login. Please try again.');
        }
      } else if (err.request) {
        setError('No response from server. Check your network connection or API URL.');
      } else {
        setError('Error setting up the request.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <form className="login-form" onSubmit={handleSubmit}>
        <h2>Login</h2>
        {error && <p className="error-message">{error}</p>}
        <div className="form-group">
          <label htmlFor="username">Username:</label>
          <input
            type="text"
            id="username"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
            aria-describedby="username-help"
          />
        </div>
        <div className="form-group">
          <label htmlFor="password">Password:</label>
          <input
            type="password"
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
            aria-describedby="password-help"
          />
        </div>
        <button type="submit" disabled={loading}>
          {loading ? 'Logging in...' : 'Login'}
        </button>
      </form>
    </div>
  );
}

export default LoginPage;