// Announcements.jsx

import React, { useState, useEffect, useCallback } from 'react';
import axios from 'axios';
import { API_BASE_URL } from '../../../constant/Constants';

import './Announcements.css';

const PAGE_SIZE_DEFAULT = 5;

const TOAST_DISPLAY_TIME = 3000;

// Main Announcements Component
const Announcements = () => {
    // --- State Management ---

    const [data, setData] = useState([]);
    const [loading, setLoading] = useState(false);
    const [isModalVisible, setIsModalVisible] = useState(false);
    const [formData, setFormData] = useState({ title: '', content: '' });
    const [error, setError] = useState(null);
    const [showSuccessToast, setShowSuccessToast] = useState(false);
    const [pagination, setPagination] = useState({
        current: 1,
        pageSize: PAGE_SIZE_DEFAULT,
        total: 0,
        totalPages: 1,
    });

    const [role] = useState(localStorage.getItem('role'));

    // --- Data Fetching Function ---
    const fetchAnnouncements = useCallback(async (pageNumber, pageSize) => {
        setLoading(true);
        setError(null);
        try {
            const response = await axios.get(`${API_BASE_URL}/api/Announcement/getByPage`, {
                params: {
                    PageNumber: pageNumber,
                    PageSize: pageSize,
                },
            });

            const result = response.data;

            if (result && result.items) {
                setData(result.items);
                setPagination({
                    current: result.pageNumber,
                    pageSize: result.pageSize,
                    total: result.totalCount,
                    totalPages: result.totalPages,
                });
            } else {
                setData([]);
                setPagination(prev => ({ ...prev, total: 0, totalPages: 1 }));
            }
        } catch (err) {
            console.error('Error fetching announcements:', err);
            setError('Failed to fetch announcement list. Check server status.');
        } finally {
            setLoading(false);
        }
    }, []);

    // Initial load and pagination changes trigger refetch
    useEffect(() => {
        fetchAnnouncements(pagination.current, pagination.pageSize);
    }, [fetchAnnouncements, pagination.current, pagination.pageSize]);


    // --- Add Modal/Form Handlers ---
    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    // Handle form submission for adding a new announcement
    const handleAddSubmit = async (e) => {
        e.preventDefault();
        setError(null);

        if (!formData.title || !formData.content) {
            setError('Title and Content are required.');
            return;
        }

        try {
            await axios.post(`${API_BASE_URL}/api/Announcement/addAnnouncement`, formData);

            setShowSuccessToast(true);

            setFormData({ title: '', content: '' });
            setIsModalVisible(false);

            fetchAnnouncements(1, pagination.pageSize);

        } catch (err) {
            console.error('Add announcement failed:', err);
            setError('Failed to add announcement. Check server response.');
        }
    };

    useEffect(() => {
        if (showSuccessToast) {
            const timer = setTimeout(() => {
                setShowSuccessToast(false);
            }, TOAST_DISPLAY_TIME);

            return () => clearTimeout(timer);
        }
    }, [showSuccessToast]);


    // --- Pagination Rendering ---
    const renderPagination = () => {
        const pages = [];
        for (let i = 1; i <= pagination.totalPages; i++) {
            const isActive = pagination.current === i;
            pages.push(
                <button
                    key={i}
                    className={`page-button ${isActive ? 'active' : ''}`}
                    onClick={() => setPagination(prev => ({ ...prev, current: i }))}
                >
                    {i}
                </button>
            );
        }

        return (
            <div className="pagination">
                <div className="pagination-info">{`Total ${pagination.total} items`}</div>
                <div className="pagination-controls">{pages}</div>
            </div>
        );
    };

    // --- Main Component Render ---
    return (
        <div className="container">
            <h2 className="header">Announcement Management</h2>

            {/* Add Button */}
            {role == '2' &&
                <button
                    className="btn primary-btn"
                    onClick={() => {
                        setIsModalVisible(true);
                        setError(null);
                    }}
                >
                    + Add Announcement
                </button>}

            {/* Error Display (for general/fetch errors) */}
            {error && !isModalVisible && <div className="error-message">{error}</div>}

            {showSuccessToast && (
                <div className="success-toast">
                    ✅ Announcement added successfully!
                </div>
            )}

            {/* Announcement List Table */}
            {loading ? (
                <div className="loading">Loading announcements...</div>
            ) : (
                <table className="data-table">
                    <thead>
                        <tr>
                            <th className="th">Announcement ID</th>
                            <th className="th">Title</th>
                            <th className="th">Content</th>
                        </tr>
                    </thead>
                    <tbody>
                        {data.length > 0 ? (
                            data.map((item, index) => (
                                <tr key={item.announcementId} className="td-row">
                                    <td className="td">{index + 1}</td>
                                    <td className="td">{item.title}</td>
                                    <td className="td">{item.content}</td>
                                </tr>
                            ))
                        ) : (
                            <tr>
                                <td colSpan="3" className="td no-data">No announcements found.</td>
                            </tr>
                        )}
                    </tbody>
                </table>
            )}

            {/* Pagination */}
            {renderPagination()}

            {/* --- Add Announcement Modal --- */}
            {isModalVisible && (
                <div className="modal-overlay">
                    <div className="modal-content">
                        <h3>Add New Announcement</h3>
                        <form onSubmit={handleAddSubmit}>
                            {/* Title Field */}
                            <div className="form-group">
                                <label className="label" htmlFor="title">Title</label>
                                <input
                                    className="input"
                                    type="text"
                                    id="title"
                                    name="title"
                                    value={formData.title}
                                    onChange={handleInputChange}
                                    required
                                />
                            </div>

                            {/* Content Field */}
                            <div className="form-group">
                                <label className="label" htmlFor="content">Content</label>
                                <textarea
                                    className="input textarea"
                                    id="content"
                                    name="content"
                                    value={formData.content}
                                    onChange={handleInputChange}
                                    required
                                />
                            </div>

                            {/* Error Message in Modal */}
                            {error && <div className="error-message">{error}</div>}

                            {/* Buttons */}
                            <div className="button-group">
                                <button type="submit" className="btn primary-btn submit-btn">
                                    Submit
                                </button>
                                <button type="button" onClick={() => setIsModalVisible(false)} className="btn secondary-btn cancel-btn">
                                    Cancel
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            )}
        </div>
    );
};

export default Announcements;