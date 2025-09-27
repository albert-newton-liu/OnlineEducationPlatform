// src/pages/DashboardPage/CourseManagement/LessonSessionPage.js

import React, { useState, useEffect } from 'react';
import { useParams, useLocation, useNavigate, useOutletContext } from 'react-router-dom';
import ViewCoursePage from '../CourseManagement/ViewCoursePage';
import Chat from '../../../components/Chat';
import './LessonSessionPage.css'; // You'll create this file next

const LessonSessionPage = () => {
    // Both components need access to URL parameters and navigation state.
    // We get them here and pass them down as props.
    const { bookingId } = useParams();

    const location = useLocation();
    const { lessonId, recipientId, recipientName } = location.state || {};

    const { setMenuHidden } = useOutletContext();

     // Use useEffect to manage menu visibility on mount and unmount
    useEffect(() => {
        // Hide the menu when the component first loads
        setMenuHidden(true);

        // Return a cleanup function to show the menu when the user navigates away
        return () => {
            setMenuHidden(false);
        };
    }, [setMenuHidden]);

    return (
        <div className="lesson-session-container">
            <div className="course-viewer-pane">
                {/* The ViewCoursePage needs lessonId from the URL */}
                <ViewCoursePage lessonId={lessonId} started bookingId={bookingId} />
            </div>
            <div className="chat-pane">
                {/* The Chat component needs data from the navigation state */}
                <Chat
                    bookingId={bookingId}
                    recipientId={recipientId}
                    recipientName={recipientName}
                />
            </div>
        </div>
    );
};

export default LessonSessionPage;