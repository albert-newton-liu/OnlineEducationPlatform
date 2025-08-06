
import React, { useState } from 'react';
import axios from 'axios';
import {API_BASE_URL} from '../../../../constant/Constants'

import '../Modal.css'; 

function AddUserModal({ onClose }) {
  const [username, setUsername] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [role, setRole] = useState(0); // 0: Student, 1: Teacher, 2: Admin
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // Role-specific states
  const [parentEmail, setParentEmail] = useState('');
  const [dateOfBirth, setDateOfBirth] = useState('');
  const [avatarUrl, setAvatarUrl] = useState(''); // For Student

  const [bio, setBio] = useState('');
  const [profilePictureUrl, setProfilePictureUrl] = useState('');
  const [teachingLanguages, setTeachingLanguages] = useState(''); // For Teacher (comma-separated string)

  const [permissions, setPermissions] = useState(''); // For Admin (comma-separated string)


  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    let requestData = {
      username,
      email,
      passwordHash: password, // Assuming backend expects 'passwordHash' field for raw password
    };
    let endpoint = '';

    switch (role) {
      case 0: // Student
        requestData = {
          ...requestData,
          parentEmail: parentEmail || null, // Ensure empty string becomes null if not required by backend
          dateOfBirth: dateOfBirth ? new Date(dateOfBirth).toISOString() : null,
          avatarUrl: avatarUrl || null,
        };
        endpoint = `${API_BASE_URL}/api/Users/register/student`; // Assuming a specific endpoint for adding students
        break;
      case 1: // Teacher
        requestData = {
          ...requestData,
          bio: bio || null,
          profilePictureUrl: profilePictureUrl || null,
          teachingLanguages: teachingLanguages.split(',').map(lang => lang.trim()).filter(lang => lang !== ''),
        };
        endpoint = `${API_BASE_URL}/api/Users/register/teacher`; // Assuming a specific endpoint for adding teachers
        break;
      case 2: // Admin
        requestData = {
          ...requestData,
          permissions: permissions.split(',').map(perm => perm.trim()).filter(perm => perm !== ''),
        };
        endpoint = `${API_BASE_URL}/api/Users/register/admin`; // Assuming a specific endpoint for adding admins
        break;
      default:
        setError('Invalid role selected.');
        setLoading(false);
        return;
    }

    try {
      const token = localStorage.getItem('userToken');
      await axios.post(endpoint, requestData, {
        headers: {
          Authorization: `Bearer ${token}`
        }
      });
      
      onClose(true); // Close modal and refresh user list
    } catch (err) {
      console.error('Failed to add user:', err.response?.data || err.message);
      setError(err.response?.data?.message || 'Failed to add user. Please check inputs.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <div className="modal-header">
          <h2>Add New User</h2>
          <button className="close-button" onClick={() => onClose(false)}>&times;</button>
        </div>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="username">Username:</label>
            <input
              type="text"
              id="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
            />
          </div>
          <div className="form-group">
            <label htmlFor="email">Email:</label>
            <input
              type="email"
              id="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
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
            />
          </div>
          <div className="form-group">
            <label htmlFor="role">Role:</label>
            <select id="role" value={role} onChange={(e) => setRole(parseInt(e.target.value))} required>
              <option value={0}>Student</option>
              <option value={1}>Teacher</option>
              <option value={2}>Admin</option>
            </select>
          </div>

          {/* Role-specific fields */}
          {role === 0 && ( // Student fields
            <>
              <div className="form-group">
                <label htmlFor="parentEmail">Parent Email (Optional):</label>
                <input type="email" id="parentEmail" value={parentEmail} onChange={(e) => setParentEmail(e.target.value)} />
              </div>
              <div className="form-group">
                <label htmlFor="dateOfBirth">Date of Birth:</label>
                <input type="date" id="dateOfBirth" value={dateOfBirth} onChange={(e) => setDateOfBirth(e.target.value)} required />
              </div>
              <div className="form-group">
                <label htmlFor="avatarUrl">Avatar URL (Optional):</label>
                <input type="url" id="avatarUrl" value={avatarUrl} onChange={(e) => setAvatarUrl(e.target.value)} />
              </div>
            </>
          )}

          {role === 1 && ( // Teacher fields
            <>
              <div className="form-group">
                <label htmlFor="bio">Bio (Optional):</label>
                <textarea id="bio" value={bio} onChange={(e) => setBio(e.target.value)} rows="3"></textarea>
              </div>
              <div className="form-group">
                <label htmlFor="profilePictureUrl">Profile Picture URL (Optional):</label>
                <input type="url" id="profilePictureUrl" value={profilePictureUrl} onChange={(e) => setProfilePictureUrl(e.target.value)} />
              </div>
              <div className="form-group">
                <label htmlFor="teachingLanguages">Teaching Languages (comma-separated):</label>
                <input type="text" id="teachingLanguages" value={teachingLanguages} onChange={(e) => setTeachingLanguages(e.target.value)} />
              </div>
            </>
          )}

          {role === 2 && ( // Admin fields
            <>
              <div className="form-group">
                <label htmlFor="permissions">Permissions (comma-separated):</label>
                <input type="text" id="permissions" value={permissions} onChange={(e) => setPermissions(e.target.value)} />
              </div>
            </>
          )}

          {error && <p className="error-message">{error}</p>}
          <div className="modal-actions">
            <button type="submit" disabled={loading}>
              {loading ? 'Adding User...' : 'Add User'}
            </button>
            <button type="button" className="secondary-button" onClick={() => onClose(false)}>
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default AddUserModal;