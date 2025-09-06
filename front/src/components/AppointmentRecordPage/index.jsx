import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { API_BASE_URL } from '../../constant/Constants';
import './AppointmentRecordPage.css';

const AppointmentRecordPage = () => {
    const navigate = useNavigate();
    const [bookingRecords, setBookingRecords] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);
    const [selectedStatus, setSelectedStatus] = useState('0');

    const role = localStorage.getItem('role');
    const userId = localStorage.getItem('userId');
    const token = localStorage.getItem('userToken');

    
    const fetchBookingRecords = async () => {
        setIsLoading(true);
        setError(null);
        try {
            if (!userId || !token) {
                setError("Authentication information is missing. Please log in again.");
                setIsLoading(false);
                return;
            }

            const queryParams = role === '1'
                ? `teacherId=${userId}`
                : `studentId=${userId}`;
            
            const response = await axios.get(
                `${API_BASE_URL}/api/Booking/getBookingList?${queryParams}`,
                {
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                }
            );
            setBookingRecords(response.data.items);
        } catch (err) {
            console.error('Failed to fetch booking records:', err);
            setError("Failed to load booking records. Please try again.");
            setBookingRecords([]);
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchBookingRecords();
    }, [userId, token, role]);

    const handleStatusChange = (event) => {
        setSelectedStatus(event.target.value);
    };

    const filteredRecords = bookingRecords.filter(record => {
        if (selectedStatus === '0') {
            return record.status === 0;
        }
        if (selectedStatus === '1') {
            return record.status === 2;
        }
        if (selectedStatus === '2') {
            return record.status === 3;
        }
        return true;
    });

    const handleView = (lessonId) => {
        navigate(`/dashboard/view-course/${lessonId}`);
    };

    const handleCancel = async (bookingId) => {
        if (window.confirm("Are you sure you want to cancel this booking?")) {
            try {
                await axios.post(
                    `${API_BASE_URL}/api/Booking/cancel/${bookingId}`,
                    {},
                    {
                        headers: {
                            Authorization: `Bearer ${token}`
                        }
                    }
                );
                alert("Booking cancelled successfully.");
                fetchBookingRecords();
            } catch (error) {
                console.error("Failed to cancel booking:", error);
                alert("Failed to cancel booking. Please try again.");
            }
        }
    };

    const handleStart = (bookingId) => {
        console.log(`Starting class for booking ID: ${bookingId}`);
        alert(`Class for booking ${bookingId} has started!`);
    };

    if (isLoading) {
        return <div className="loading">Loading booking records...</div>;
    }

    if (error) {
        return <div className="error-message">{error}</div>;
    }

    return (
        <div className="appointment-records-container">
           
            <div className="filter-controls">
                <label htmlFor="status-filter">Filter by Status:</label>
                <select id="status-filter" value={selectedStatus} onChange={handleStatusChange}>
                    <option value="0">Upcoming</option>
                    <option value="1">Completed</option>
                    <option value="2">Canceled</option>
                </select>
            </div>

            {filteredRecords.length > 0 ? (
                <div className="table-container">
                    <table className="booking-table">
                        <thead>
                            <tr>
                                <th>Lesson</th>
                                {role === '0' && <th>Teacher</th>}
                                {role === '1' && <th>Student</th>}
                                <th>Start Time</th>
                                <th>End Time</th>
                                <th>Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {filteredRecords.map(record => (
                                <tr key={record.bookingId}>
                                    <td>{record.lessonTitle}</td>
                                    {role === '0' && <td>{record.teacherName}</td>}
                                    {role === '1' && <td>{record.studentName}</td>}
                                    <td>{new Date(record.startTime).toLocaleString()}</td>
                                    <td>{new Date(record.endTime).toLocaleString()}</td>
                                    <td>
                                        <div className="actions-buttons vertical-buttons">
                                            <button 
                                                className="action-button"
                                                onClick={() => handleView(record.lessonId)}
                                            >
                                                View
                                            </button>
                                             {record.status === 0 && role === '1' && <button className="action-button" onClick={() => handleStart(record.bookingId)}>Start</button>}
                                             {role === '0' && record.status === 0 && <button className="action-button" onClick={() => handleCancel(record.bookingId)}>Cancel</button>}
                                        </div>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            ) : (
                <p>No appointments found with the selected filter.</p>
            )}
        </div>
    );
};

export default AppointmentRecordPage;