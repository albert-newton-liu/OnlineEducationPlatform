import React from 'react';
import './CourseInfoSection.css'

const CourseInfoSection = ({ courseInfo, onInfoChange ,isReadOnly = false}) => {
    return (
        <div className="course-info-section">
            <h2>Course Details</h2>
            {isReadOnly ? (
                <div className="read-only-field">
                    <strong>Title:</strong> {courseInfo.title}
                </div>
            ) : (
                <input
                    type="text"
                    name="title"
                    placeholder="Course Title"
                    value={courseInfo.title}
                    onChange={onInfoChange}
                    disabled={isReadOnly}
                />
            )}
            
            {isReadOnly ? (
                <div className="read-only-field">
                    <strong>Description:</strong> {courseInfo.description}
                </div>
            ) : (
                <textarea
                    name="description"
                    placeholder="Course Description"
                    value={courseInfo.description}
                    onChange={onInfoChange}
                    disabled={isReadOnly}
                />
            )}

            {isReadOnly ? (
                <div className="read-only-field">
                    <strong>Difficulty:</strong> {['Beginner', 'Intermediate', 'Advanced'][courseInfo.difficultyLevel]}
                </div>
            ) : (
                <select
                    name="difficultyLevel"
                    value={courseInfo.difficultyLevel}
                    onChange={onInfoChange}
                    disabled={isReadOnly}
                >
                    <option value={0}>Beginner</option>
                    <option value={1}>Intermediate</option>
                    <option value={2}>Advanced</option>
                </select>
            )}
        </div>
    );
};

export default CourseInfoSection;