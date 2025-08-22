import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import axios from 'axios';
import { API_BASE_URL } from '../../../constant/Constants';

import './CourseManagement.css';

const CourseManagement = () => {
    const [lessons, setLessons] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [role, setRole] = useState(() => localStorage.getItem('role'));

    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize] = useState(5);
    const [totalItems, setTotalItems] = useState(0);
    const [totalPages, setTotalPages] = useState(0);
    const [hasPreviousPage, setHasPreviousPage] = useState(false);
    const [hasNextPage, setHasNextPage] = useState(false);

    // New states for the booking modal
    const [showModal, setShowModal] = useState(false);
    const [bookableSlots, setBookableSlots] = useState([]);
    const [modalLoading, setModalLoading] = useState(false);
    const [modalError, setModalError] = useState(null);
    const [currentLessonId, setCurrentLessonId] = useState(null);

    // States for the success modal
    const [showSuccessModal, setShowSuccessModal] = useState(false);
    const [successMessage, setSuccessMessage] = useState('');

    const [showDeleteModal, setShowDeleteModal] = useState(false);
    const [lessonToDelete, setLessonToDelete] = useState(null);

    const token = localStorage.getItem('userToken') + `,${role},${localStorage.getItem('userId')}`;

    const getDayName = (dayOfWeek) => {
        const days = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
        return days[dayOfWeek] || 'Unknown Day';
    };

    const getDifficultyText = (level) => {
        const levels = ["Beginner", "Intermediate", "Advanced"];
        return levels[level] || "Unknown";
    };

    const fetchLessons = async () => {
        setIsLoading(true);
        try {
            const response = await axios.get(
                `${API_BASE_URL}/api/Lesson?PageNumber=${currentPage}&PageSize=${pageSize}`,
                {
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                }
            );
            const paginatedResult = response.data;
            setLessons(paginatedResult.items);
            setTotalItems(paginatedResult.totalCount);
            setTotalPages(paginatedResult.totalPages);
            setHasPreviousPage(paginatedResult.hasPreviousPage);
            setHasNextPage(paginatedResult.hasNextPage);
        } catch (error) {
            console.error("Failed to fetch lessons:", error);
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchLessons();
    }, [currentPage, pageSize, token]);

    const handleApprove = async (lessonId) => {
        try {
            const response = await axios.post(
                `${API_BASE_URL}/api/Lesson/approve/${lessonId}`, {},
                {
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                }
            );
            console.log(`Approving lesson: ${lessonId}`);
            fetchLessons();
        } catch (error) {
            console.error(`Failed to approve lesson ${lessonId}:`, error);
        }
    };

    const handleBookLesson = async (teacherId, lessonId) => {
        setModalLoading(true);
        setModalError(null);
        setShowModal(true);
        setCurrentLessonId(lessonId);

        try {
            const response = await axios.get(
                `${API_BASE_URL}/api/Booking/getBookableSlot/${teacherId}?studentId=${localStorage.getItem('userId')}`
            );
            setBookableSlots(response.data.items);
        } catch (error) {
            console.error("Failed to fetch bookable slots:", error);
            setModalError("Failed to load available slots. Please try again later.");
            setBookableSlots([]);
        } finally {
            setModalLoading(false);
        }
    };

    const handleBookNow = async (bookableSlotId) => {
        const studentId = localStorage.getItem('userId');
        if (!studentId || !currentLessonId) {
            setModalError('Missing user or lesson information.');
            return;
        }

        try {
            const requestBody = {
                studentId: studentId,
                bookableSlotId: bookableSlotId,
                lessonId: currentLessonId
            };

            await axios.post(
                `${API_BASE_URL}/api/Booking/book`,
                requestBody
            );

            setShowModal(false);
            setSuccessMessage("Booking successful!");
            setShowSuccessModal(true);

        } catch (error) {
            console.error("Failed to book slot:", error);
            setModalError("Failed to book slot. Please try again.");
        }
    };

    const handleCloseModal = () => {
        setShowModal(false);
        setBookableSlots([]);
        setModalError(null);
        setCurrentLessonId(null);
    };

    const handleCloseSuccessModal = () => {
        setShowSuccessModal(false);
        fetchLessons();
    };

    const handleDeleteClick = (lessonId) => {
        setLessonToDelete(lessonId);
        setShowDeleteModal(true);
    };

    const handlePageChange = (newPage) => {
        setCurrentPage(newPage);
    };

    const handleConfirmDelete = async () => {
        try {
            await axios.delete(
                `${API_BASE_URL}/api/Lesson/delete/${lessonToDelete}`,
                {
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                }
            );
            console.log(`Successfully deleted lesson: ${lessonToDelete}`);
            fetchLessons();
        } catch (error) {
            console.error("Failed to delete lesson:", error);
            alert("Failed to delete lesson. Please try again.");
        } finally {
            handleCancelDelete();
        }
    };

    const handleCancelDelete = () => {
        setShowDeleteModal(false);
        setLessonToDelete(null);
    };

    if (isLoading) {
        return <div className="loading">Loading...</div>;
    }

    return (
        <div className="course-management-container">
            <header className="course-management-header">
                {role == 1 ? (
                    <>
                        <h2>Course Management</h2>
                        <Link to="/dashboard/add-course" className="add-course-button">
                            Add New Course
                        </Link>
                    </>
                ) : (
                    <h2>Course Information</h2>
                )}
            </header>

            <div className="table-container">
                <table className="lesson-table">
                    <thead>
                        <tr>
                            <th>Title</th>
                            <th>Description</th>
                            <th>Difficulty</th>
                            <th>Status</th>
                            {role != 1 && <th>Creator</th>}
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {lessons.map(lesson => (
                            <tr key={lesson.lessonId}>
                                <td>{lesson.title}</td>
                                <td>{lesson.description}</td>
                                <td>{getDifficultyText(lesson.difficultyLevel)}</td>
                                <td>
                                    <span className={`status-tag ${lesson.isPublished ? 'published' : 'unpublished'}`}>
                                        {lesson.isPublished ? "Published" : "Unpublished"}
                                    </span>
                                </td>
                                {role != 1 && <td>{lesson.creator}</td>}
                                <td>
                                    <div className="actions-buttons vertical-buttons">
                                        <Link to={`/dashboard/view-course/${lesson.lessonId}`} className="view-details-button">
                                            View
                                        </Link>
                                        {role == 0 &&
                                            <button className="book-button"
                                                onClick={() => handleBookLesson(lesson.creatorId, lesson.lessonId)}
                                            >
                                                Book
                                            </button>
                                        }

                                        {role == 1 &&
                                            <button
                                                className="delete-button"
                                                onClick={() => handleDeleteClick(lesson.lessonId)}
                                            >
                                                Delete
                                            </button>
                                        }
                                        {role == 2 && !lesson.isPublished && (
                                            <button
                                                className="approve-button"
                                                onClick={() => handleApprove(lesson.lessonId)}
                                            >
                                                Approve
                                            </button>
                                        )}
                                    </div>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            <div className="pagination-controls">
                <button
                    onClick={() => handlePageChange(currentPage - 1)}
                    disabled={!hasPreviousPage}
                >
                    Previous
                </button>
                <span>Page {currentPage} of {totalPages}</span>
                <button
                    onClick={() => handlePageChange(currentPage + 1)}
                    disabled={!hasNextPage}
                >
                    Next
                </button>
            </div>

            {showModal && (
                <div className="modal-overlay">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h2>Book a Slot</h2>
                            <button className="close-button" onClick={handleCloseModal}>&times;</button>
                        </div>
                        <div className="modal-body">
                            {modalLoading ? (
                                <p>Loading available slots...</p>
                            ) : modalError ? (
                                <p className="error-message">{modalError}</p>
                            ) : bookableSlots.length > 0 ? (
                                <ul className="slot-list">
                                    {bookableSlots.map(slot => (
                                        <li key={slot.bookableSlotId} className="slot-item">
                                            <p><strong>Day:</strong> {getDayName(slot.dayOfWeek)}</p>
                                            <p><strong>Date:</strong> {slot.dateOnly}</p>
                                            <p><strong>Time:</strong> {slot.startTime.substring(0, 5)} - {slot.endTime.substring(0, 5)}</p>
                                            <button
                                                className={`book-now-button ${slot.isBooked ? 'booked-button' : ''}`}
                                                onClick={() => handleBookNow(slot.bookableSlotId)}
                                                disabled={slot.isBooked}
                                            >
                                                {slot.isBooked ? 'Booked' : 'Book Now'}
                                            </button>
                                        </li>
                                    ))}
                                </ul>
                            ) : (
                                <p>No bookable slots found for this teacher.</p>
                            )}
                        </div>
                    </div>
                </div>
            )}

            {showSuccessModal && (
                <div className="modal-overlay">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h2>Success!</h2>
                        </div>
                        <div className="modal-body">
                            <p className="success-message">{successMessage}</p>
                            <button className="close-button-center" onClick={handleCloseSuccessModal}>OK</button>
                        </div>
                    </div>
                </div>
            )}

            {/* Delete Confirmation Modal */}
            {showDeleteModal && (
                <div className="modal-overlay">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h2>Confirm Deletion</h2>
                            <button className="close-button" onClick={handleCancelDelete}>&times;</button>
                        </div>
                        <div className="modal-body">
                            <p>Are you sure you want to delete this course? This action cannot be undone.</p>
                            <div className="delete-buttons">
                                <button className="cancel-button" onClick={handleCancelDelete}>Cancel</button>
                                <button className="confirm-delete-button" onClick={handleConfirmDelete}>Delete</button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default CourseManagement;