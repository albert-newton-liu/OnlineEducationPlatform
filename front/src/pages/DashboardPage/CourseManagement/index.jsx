import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import axios from 'axios';
import { API_BASE_URL } from '../../../constant/Constants';

import './CourseManagement.css';

const CourseManagement = () => {
    const [lessons, setLessons] = useState([]);
    const [isLoading, setIsLoading] = useState(true);
    const [role, setRole] = useState(() => localStorage.getItem('role'));

    // Pagination states
    const [currentPage, setCurrentPage] = useState(1);
    const [pageSize] = useState(5);
    const [totalItems, setTotalItems] = useState(0);
    const [totalPages, setTotalPages] = useState(0);
    const [hasPreviousPage, setHasPreviousPage] = useState(false);
    const [hasNextPage, setHasNextPage] = useState(false);

    const token = localStorage.getItem('userToken') + `,${role},${localStorage.getItem('userId')}`;

    // Function to map the difficulty level byte to a string
    const getDifficultyText = (level) => {
        const levels = ["Beginner", "Intermediate", "Advanced"];
        return levels[level] || "Unknown";
    };

    // Data fetching from the backend API with pagination logic
    const fetchLessons = async () => {
        setIsLoading(true);
        try {


            const response = await axios.get(
                `${API_BASE_URL}/api/Lesson?PageNumber=${currentPage}&PageSize=${pageSize}`,
                {
                    headers: {
                        Authorization: `Bearer ${token}` // Include JWT token
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
    }, [currentPage, pageSize, token]); // Re-fetch when currentPage or pageSize changes

    // Action to approve a lesson.
    const handleApprove = async (lessonId) => {
        try {
            // TODO: In a real app, this would be an API call (e.g., PUT or POST)
            // await axios.put(`${API_BASE_URL}/api/Lesson/${lessonId}/approve`, {}, {
            //     headers: { Authorization: `Bearer ${token}` }
            // });

            const response = await axios.post(
                `${API_BASE_URL}/api/Lesson/approve/${lessonId}`, {},
                {
                    headers: {
                        Authorization: `Bearer ${token}` // Include JWT token
                    }
                }
            );


            console.log(`Approving lesson: ${lessonId}`);

            // After a successful action, re-fetch the data to update the UI
            fetchLessons();

        } catch (error) {
            console.error(`Failed to approve lesson ${lessonId}:`, error);
        }
    };

    // Handle page change
    const handlePageChange = (newPage) => {
        setCurrentPage(newPage);
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
                                         {role == 0 && <button className="book-button">book</button>}
                                        {role == 1 && <button className="edit-button">Edit</button>}

                                        {role == 1 && <button className="delete-button">Delete</button>}
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

            {/* Pagination Controls */}
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
        </div>
    );
};

export default CourseManagement;