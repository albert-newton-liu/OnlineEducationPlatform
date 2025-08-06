import React from 'react';

const CourseInfoSection = ({ courseInfo, onInfoChange }) => {
    return (
        <div className="course-info-section">
            <h2>Course Details</h2>
            <input
                type="text"
                name="title"
                placeholder="Course Title"
                value={courseInfo.title}
                onChange={onInfoChange}
            />
            <textarea
                name="description"
                placeholder="Course Description"
                value={courseInfo.description}
                onChange={onInfoChange}
            />
            <select
                name="difficultyLevel"
                value={courseInfo.difficultyLevel}
                onChange={onInfoChange}
            >
                <option value={0}>Beginner</option>
                <option value={1}>Intermediate</option>
                <option value={2}>Advanced</option>
            </select>
        </div>
    );
};

export default CourseInfoSection;