// src/pages/Admin/UpdateUserModal.jsx
import React, { useState, useEffect } from 'react';
import axios from 'axios';
import {API_BASE_URL} from '../../../../constant/Constants'

import '../Modal.css'; 

function UpdateUserModal({ user, onClose }) {
  // Common fields
  const [username, setUsername] = useState(user.username || '');
  const [email, setEmail] = useState(user.email || '');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  // Role-specific fields (initialized from existing user data)
  const [parentEmail, setParentEmail] = useState(user.parentEmail || '');
  const [dateOfBirth, setDateOfBirth] = useState(user.dateOfBirth ? new Date(user.dateOfBirth).toISOString().split('T')[0] : '');
  const [avatarUrl, setAvatarUrl] = useState(user.avatarUrl || '');

  const [bio, setBio] = useState(user.bio || '');
  const [profilePictureUrl, setProfilePictureUrl] = useState(user.profilePictureUrl || '');
  const [teachingLanguages, setTeachingLanguages] = useState(user.teachingLanguages?.join(', ') || '');

  const [permissions, setPermissions] = useState(user.permissions?.join(', ') || '');



  useEffect(() => {
    // This effect ensures modal state updates if the 'user' prop changes (e.g., selecting a different user)
    setUsername(user.username || '');
    setEmail(user.email || '');
    setParentEmail(user.parentEmail || '');
    setDateOfBirth(user.dateOfBirth ? new Date(user.dateOfBirth).toISOString().split('T')[0] : '');
    setAvatarUrl(user.avatarUrl || '');
    setBio(user.bio || '');
    setProfilePictureUrl(user.profilePictureUrl || '');
    setTeachingLanguages(user.teachingLanguages?.join(', ') || '');
    setPermissions(user.permissions?.join(', ') || '');
  }, [user]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    let requestData = {
      id: user.id, // Include user ID for update
      username,
      email,
      // NOTE: Password update is usually handled by a separate "change password" flow
      // or requires confirming old password. We are not including it here directly.
    };
    let endpoint = '';

    // Determine endpoint and data based on the user's current role
    switch (user.role) {
      case 0: // Student
        requestData = {
          ...requestData,
          parentEmail: parentEmail || null,
          dateOfBirth: dateOfBirth ? new Date(dateOfBirth).toISOString() : null,
          avatarUrl: avatarUrl || null,
        };
        endpoint = `${API_BASE_URL}/api/Users/update/student`; // Assuming an endpoint for updating students
        break;
      case 1: // Teacher
        requestData = {
          ...requestData,
          bio: bio || null,
          profilePictureUrl: profilePictureUrl || null,
          teachingLanguages: teachingLanguages.split(',').map(lang => lang.trim()).filter(lang => lang !== ''),
        };
        endpoint = `${API_BASE_URL}/api/Users/update/teacher`; // Assuming an endpoint for updating teachers
        break;
      case 2: // Admin
        requestData = {
          ...requestData,
          permissions: permissions.split(',').map(perm => perm.trim()).filter(perm => perm !== ''),
        };
        endpoint = `${API_BASE_URL}/api/Users/update/admin`; // Assuming an endpoint for updating admins
        break;
      default:
        setError('Unknown user role. Cannot update.');
        setLoading(false);
        return;
    }

    try {
      const token = localStorage.getItem('userToken');
      // For PUT or PATCH, typically the ID is in the URL or the body.
      // If your backend expects a PUT request with ID in URL: `${API_BASE_URL}/User/${user.id}`
      await axios.put(endpoint, requestData, { // Assuming PUT method for update
        headers: {
          Authorization: `Bearer ${token}`
        }
      });
      alert('User updated successfully!');
      onClose(true); // Close modal and refresh user list
    } catch (err) {
      console.error('Failed to update user:', err.response?.data || err.message);
      setError(err.response?.data?.message || 'Failed to update user. Please check inputs.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <div className="modal-header">
          <h2>Update User: {user.username}</h2>
          <button className="close-button" onClick={() => onClose(false)}>&times;</button>
        </div>
        <form onSubmit={handleSubmit}>
          {/* Common fields for all roles */}
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
          {/* NOTE: Password update is usually separate and not done directly in user profile update */}

          {/* Role-specific fields based on the user's current role */}
          {user.role === 0 && ( // Student fields
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

          {user.role === 1 && ( // Teacher fields
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

          {user.role === 2 && ( // Admin fields
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
              {loading ? 'Updating User...' : 'Update User'}
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

export default UpdateUserModal;