import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import axios from 'axios';
import CourseInfoSection from '../AddCoursePage/CourseInfoSection';
import PageEditorSection from '../AddCoursePage/PageEditorSection';
import { API_BASE_URL } from '../../../../constant/Constants'
import {TextType, ImageType} from '../../../../constant/Constants'

import '../AddCoursePage/AddCoursePage.css'
import './ViewCoursePage.css'

const ViewCoursePage = () => {
    const { lessonId } = useParams();
    const navigate = useNavigate();

    const [courseInfo, setCourseInfo] = useState(null);
    const [pages, setPages] = useState([]);
    const [currentPageIndex, setCurrentPageIndex] = useState(0);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);

    // Fetch course data from the API
    useEffect(() => {
        const fetchCourseDetails = async () => {
            const token = localStorage.getItem('userToken');
            if (!token) {
                navigate('/login');
                return;
            }

            try {
                const response = await axios.get(
                    `${API_BASE_URL}/api/Lesson/${lessonId}`,
                    {
                        headers: {
                            Authorization: `Bearer ${token}`
                        }
                    }
                );

                const lessonData = response.data;
                console.log(lessonData);

                setCourseInfo({
                    title: lessonData.title,
                    description: lessonData.description,
                    difficultyLevel: lessonData.difficultyLevel,
                });

                const formattedPages = lessonData.pages.map(page => {
                   
                    const pageContent = {};
                    if (page.pageLayout.templateId === 1) {
                         pageContent.backgroundImage = `url(${page.elements.find(e => e.elementType === ImageType)?.contentUrl || ''})`;
                         const textElement = page.elements.find(e => e.elementType === TextType);

                         pageContent.text = textElement?.contentText || '';
                         pageContent.position = textElement?.elementMetadata.contentPosition || { x: 250, y: 200 };
                         pageContent.textAreaSize = textElement?.elementMetadata.contentSize || { width: '200px', height: '200px' };
                    } else if (page.pageLayout.templateId === 2) {
                        const topTextElement = page.elements.find(e => e.elementMetadata?.valueKey === 'topText');
                        const leftContentElement = page.elements.find(e => e.elementMetadata?.valueKey === 'leftContent');
                        const rightContentElement = page.elements.find(e => e.elementMetadata?.valueKey === 'rightContent');

                        pageContent.topText = topTextElement?.contentText || '';
                        pageContent.leftContentType = leftContentElement?.elementType;
                        pageContent.rightContentType = rightContentElement?.elementType;
                        pageContent.leftContent = {
                            text: leftContentElement?.contentText,
                            image: leftContentElement?.contentUrl,
                        };
                        pageContent.rightContent = {
                            text: rightContentElement?.contentText,
                            image: rightContentElement?.contentUrl,
                        };
                    }


                    return {
                        id: page.pageNumber,
                        template: page.pageLayout.templateId,
                        content: pageContent
                    };
                });
                
                setPages(formattedPages);

            } catch (err) {
                console.error('Failed to fetch course details:', err);
                setError('Failed to load course details. Please try again.');
            } finally {
                setIsLoading(false);
            }
        };

        if (lessonId) {
            fetchCourseDetails();
        }
    }, [lessonId, navigate]);

    const handlePageNavigation = (direction) => {
        if (direction === 'prev' && currentPageIndex > 0) {
            setCurrentPageIndex(currentPageIndex - 1);
        } else if (direction === 'next' && currentPageIndex < pages.length - 1) {
            setCurrentPageIndex(currentPageIndex + 1);
        }
    };

    if (isLoading) {
        return <div className="add-course-container">Loading course details...</div>;
    }

    if (error) {
        return <div className="add-course-container error-message">{error}</div>;
    }

    if (!courseInfo) {
        return <div className="add-course-container">Course not found.</div>;
    }

    return (
        <div className="add-course-container">
            <CourseInfoSection
                courseInfo={courseInfo}
                onInfoChange={() => {}} // Pass an empty function for read-only mode
                isReadOnly={true} // New prop to disable editing
            />

            <PageEditorSection
                pages={pages}
                currentPageIndex={currentPageIndex}
                onContentChange={() => {}}
                onPageNavigation={handlePageNavigation}
                isReadOnly={true} // New prop to disable editing
            />

            {/* No buttons for saving or editing */}
            
            <div className="save-course-section">
                <button onClick={() => navigate('/dashboard/courses')} className="back-button">
                    Back to Courses
                </button>
            </div>
        </div>
    );
};

export default ViewCoursePage;