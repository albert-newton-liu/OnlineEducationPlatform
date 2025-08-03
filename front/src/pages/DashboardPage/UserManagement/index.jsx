// src/pages/Admin/UserManagement.jsx (or src/components/UserManagement.jsx if you prefer)
import React, { useState, useEffect, useCallback } from 'react'; // Add useCallback
import axios from 'axios';
import AddUserModal from './AddUserModal'; // We'll create this next
import UpdateUserModal from './UpdateUserModal'; // We'll create this next

import {API_BASE_URL} from '../../../constant/Constants'


import './UserManagement.css'; // Create this CSS file for styling

function UserManagement() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Pagination states
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10); // Default page size
  const [totalUsers, setTotalUsers] = useState(0);
  const [totalPages, setTotalPages] = useState(1);

  // Modal states
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [isUpdateModalOpen, setIsUpdateModalOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState(null);


  // Memoize fetchUsers with useCallback to prevent unnecessary re-renders in useEffect
  const fetchUsers = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const token = localStorage.getItem('userToken');
      // Make the request to the paginated endpoint
      const response = await axios.get(
        `${API_BASE_URL}/api/Users?PageNumber=${currentPage}&PageSize=${pageSize}`,
        {
          headers: {
            Authorization: `Bearer ${token}` // Include JWT token
          }
        }
      );
      // Update state with paginated data and metadata
      setUsers(response.data.items);
      setTotalUsers(response.data.totalCount);
      setTotalPages(response.data.totalPages);
    } catch (err) {
      console.error('Failed to fetch users:', err.response?.data || err.message);
      setError('Failed to load user data. Please try again.');
      // Optionally handle specific errors, e.g., redirect to login if unauthorized
    } finally {
      setLoading(false);
    }
  }, [currentPage, pageSize]); // Dependency array: re-run fetchUsers if currentPage or pageSize changes

  // Trigger fetchUsers when component mounts or pagination states change
  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]); // fetchUsers is a dependency here because it's memoized by useCallback

  const handleDeleteUser = async (userId) => {
    if (window.confirm(`Are you sure you want to delete user with ID: ${userId}?`)) {
      try {
        const token = localStorage.getItem('userToken');
        await axios.delete(`${API_BASE_URL}/User/${userId}`, { // Assuming DELETE /api/User/{id}
          headers: {
            Authorization: `Bearer ${token}`
          }
        });
        alert('User deleted successfully!');
        // After deletion, refresh the current page's data
        // If the last item on a page was deleted, consider moving to the previous page
        if (users.length === 1 && currentPage > 1) {
            setCurrentPage(prevPage => prevPage - 1);
        } else {
            fetchUsers(); // Refresh current page
        }
      } catch (err) {
        console.error('Failed to delete user:', err.response?.data || err.message);
        setError('Failed to delete user.');
      }
    }
  };

  const handleUpdateUser = (user) => {
    setSelectedUser(user);
    setIsUpdateModalOpen(true);
  };

  // Callback function to close the modal and refresh data
  const handleModalClose = (shouldRefresh = false) => {
    setIsAddModalOpen(false);
    setIsUpdateModalOpen(false);
    setSelectedUser(null);
    if (shouldRefresh) {
      fetchUsers(); // Refresh the user list after add/update
    }
  };

  // Pagination Handlers
  const handlePreviousPage = () => {
    if (currentPage > 1) {
      setCurrentPage(prevPage => prevPage - 1);
    }
  };

  const handleNextPage = () => {
    if (currentPage < totalPages) {
      setCurrentPage(prevPage => prevPage + 1);
    }
  };

  const handlePageSizeChange = (e) => {
      setPageSize(parseInt(e.target.value));
      setCurrentPage(1); // Reset to first page when page size changes
  };

  if (loading) {
    return (
      <div className="user-management-container">
        <p>Loading users...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="user-management-container">
        <p className="error-message">{error}</p>
      </div>
    );
  }

  return (
    <div className="user-management-container">
      <div className="user-management-header">
        <h2>User Management</h2>
        <button className="add-user-button" onClick={() => setIsAddModalOpen(true)}>
          Add User
        </button>
      </div>

      {users.length === 0 && totalUsers === 0 ? (
        <p>No users found. Click "Add User" to get started.</p>
      ) : (
        <>
          <div className="table-responsive">
            <table>
              <thead>
                <tr>
                  <th>Username</th>
                  <th>Email</th>
                  <th>Role</th>
                  <th>Created At</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {users.map((user) => (
                  <tr key={user.userId}>
                    <td>{user.username}</td>
                    <td>{user.email}</td>
                    <td>
                      {user.role === 0 ? 'Student' :
                       user.role === 1 ? 'Teacher' :
                       user.role === 2 ? 'Admin' : 'Unknown'}
                    </td>
                    <td>{new Date(user.createdAt).toLocaleDateString()}</td>
                    <td className="actions-cell">
                      <button className="action-button update-button" onClick={() => handleUpdateUser(user)}>
                        Update
                      </button>
                      <button className="action-button delete-button" onClick={() => handleDeleteUser(user.id)}>
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination Controls */}
          <div className="pagination-controls">
            <button
              onClick={handlePreviousPage}
              disabled={currentPage === 1}
              className="pagination-button"
            >
              Previous
            </button>
            <span>
              Page {currentPage} of {totalPages} (Total Users: {totalUsers})
            </span>
            <button
              onClick={handleNextPage}
              disabled={currentPage === totalPages}
              className="pagination-button"
            >
              Next
            </button>

            <div className="page-size-selector">
                <label htmlFor="pageSize">Items per page:</label>
                <select id="pageSize" value={pageSize} onChange={handlePageSizeChange}>
                    <option value={2}>2</option>
                    <option value={5}>5</option>
                    <option value={10}>10</option>
                    <option value={20}>20</option>
                    <option value={50}>50</option>
                </select>
            </div>
          </div>
        </>
      )}

      {isAddModalOpen && (
        <AddUserModal onClose={handleModalClose} />
      )}

      {isUpdateModalOpen && selectedUser && (
        <UpdateUserModal user={selectedUser} onClose={handleModalClose} />
      )}
    </div>
  );
}

export default UserManagement;