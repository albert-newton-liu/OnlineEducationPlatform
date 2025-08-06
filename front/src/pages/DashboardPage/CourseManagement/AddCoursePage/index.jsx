import React, { useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import CourseInfoSection from './CourseInfoSection';
import PageEditorSection from './PageEditorSection';
import { API_BASE_URL } from '../../../../constant/Constants'
import { TextType, ImageType } from '../../../../constant/Constants'

import './AddCoursePage.css';

const AddCoursePage = () => {
     const navigate = useNavigate();

    const [courseInfo, setCourseInfo] = useState({
        title: '',
        description: '',
        difficultyLevel: 0,
    });

    const [pages, setPages] = useState([]);
    const [currentPageIndex, setCurrentPageIndex] = useState(0);

    const [isTemplateModalOpen, setIsTemplateModalOpen] = useState(false);
    const [hasUnsavedChanges, setHasUnsavedChanges] = useState(false);

    const handleCourseInfoChange = (e) => {
        const { name, value } = e.target;
        setCourseInfo(prev => ({ ...prev, [name]: value }));
        // setHasUnsavedChanges(true);
    };

    const handleContentChange = (newContent) => {

        const updatedPages = [...pages];
        updatedPages[currentPageIndex].content = newContent;
        setPages(updatedPages);
        setHasUnsavedChanges(true);

    };

    const handleSavePage = () => {
        console.log(`Saving page ${currentPageIndex + 1}...`);
        setHasUnsavedChanges(false);
    };

    const handleAddPage = (templateType) => {
        if (hasUnsavedChanges && !window.confirm("You have unsaved changes. Are you sure you want to add a new page?")) {
            return;
        }

        let newContent = {};
        if (templateType === 1) {
            newContent = {
                text: "",
                backgroundImage: "",
                position: { x: 250, y: 200 },
                textAreaSize: { width: '200px', height: '200px' }
            };
        } else if (templateType === 2) {
            newContent = {
                topText: "",
                leftContentType: TextType,
                rightContentType: TextType,
                leftContent: { text: "", image: null },
                rightContent: { text: "", image: null },
            };
        }

        const newPage = {
            id: Date.now(),
            template: templateType,
            content: { ...newContent }
        };
        setPages(prev => [...prev, newPage]);
        setCurrentPageIndex(pages.length);
        setIsTemplateModalOpen(false);
        setHasUnsavedChanges(true);
    };

    const handleDeletePage = () => {
        if (window.confirm("Are you sure you want to delete this page?")) {
            const updatedPages = pages.filter((_, index) => index !== currentPageIndex);

            setPages(updatedPages);

            if (updatedPages.length > 0) {
                const newIndex = currentPageIndex > 0 ? currentPageIndex - 1 : 0;
                setCurrentPageIndex(newIndex);
            } else {
                setCurrentPageIndex(0);
            }
            setHasUnsavedChanges(false);
        }
    };

    const handlePageNavigation = (direction) => {
        if (hasUnsavedChanges) {
            if (!window.confirm("You have unsaved changes. Are you sure you want to navigate away?")) {
                return;
            }
        }

        if (direction === 'prev' && currentPageIndex > 0) {
            setCurrentPageIndex(currentPageIndex - 1);
        } else if (direction === 'next' && currentPageIndex < pages.length - 1) {
            setCurrentPageIndex(currentPageIndex + 1);
        }

        setHasUnsavedChanges(false);
    };

    const handleSaveCourse = async () => {
        if (!courseInfo.title.trim()) {
            alert('Course Title cannot be empty.');
            return;
        }
        if (!courseInfo.description.trim()) {
            alert('Course Description cannot be empty.');
            return;
        }
        if (hasUnsavedChanges) {
            alert('Please save the current page content before saving the course.');
            return;
        }
        if (pages.length === 0) {
            alert('Please add at least one page to the course.');
            return;
        }

        const formattedPages = pages.map((page, index) => {
            const elements = [];
            let order = 0;

            if (page.template === 1) {
                elements.push({
                    ElementType: ImageType,
                    ContentUrl: page.content.backgroundImage.replace('url(', '').replace(')', ''),
                    Order: order++,
                });
                elements.push({
                    ElementType: TextType,
                    ContentText: page.content.text,
                    ElementMetadata: {
                        ContentPosition: page.content.position,
                        ContentSize: page.content.textAreaSize,
                    },
                    Order: order++,
                });
            } else if (page.template === 2) {
                elements.push({
                    ElementType: TextType,
                    ContentText: page.content.topText,
                    Order: order++,
                    ElementMetadata: { ValueKey: 'topText' }
                });
                elements.push({
                    ElementType: page.content.leftContentType,
                    ContentText: page.content.leftContent.text,
                    ContentUrl: page.content.leftContent.image,
                    Order: order++,
                    ElementMetadata: { ValueKey: 'leftContent' }
                });
                elements.push({
                    ElementType: page.content.rightContentType,
                    ContentText: page.content.rightContent.text,
                    ContentUrl: page.content.rightContent.image,
                    Order: order++,
                    ElementMetadata: { ValueKey: 'rightContent' }
                });
            }

            return {
                PageNumber: index + 1,
                PageLayout: { TemplateId: page.template },
                Elements: elements,
            };
        });

        const lessonData = {
            Title: courseInfo.title,
            Description: courseInfo.description,
            DifficultyLevel: courseInfo.difficultyLevel,
            Pages: formattedPages,
            TeacherId: localStorage.getItem('userId')
        };

        const token = localStorage.getItem('userToken');

        console.log("Submitting the following course data to the server:", lessonData);
        try {

            const response = await axios.post(
                `${API_BASE_URL}/api/Lesson/addlesson`,
                lessonData, 
                {           
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                }
            );
            console.log(response.data);
            navigate('/dashboard/courses')

        } catch (err) {
            console.error('Failed to fetch user data:', err);
            setError('Failed to load user data. Please try logging in again.');
            if (err.response && (err.response.status === 401 || err.response.status === 403)) {
                handleLogout();
            }
        }
    };

    return (
        <div className="add-course-container">
            <CourseInfoSection
                courseInfo={courseInfo}
                onInfoChange={handleCourseInfoChange}
            />

            <PageEditorSection
                pages={pages}
                currentPageIndex={currentPageIndex}
                hasUnsavedChanges={hasUnsavedChanges}
                onContentChange={handleContentChange}
                onSavePage={handleSavePage}
                onAddPage={handleAddPage}
                onDeletePage={handleDeletePage}
                onPageNavigation={handlePageNavigation}
                onModalOpen={() => setIsTemplateModalOpen(true)}
            />

            <div className="save-course-section">
                <button onClick={handleSaveCourse} className="save-course-button">
                    Save Course
                </button>
            </div>

            {isTemplateModalOpen && (
                <div className="modal-overlay">
                    <div className="template-modal">
                        <h3>Select a Page Template</h3>
                        <button onClick={() => handleAddPage(1)}>Template 1: Full-Screen Image + Text</button>
                        <button onClick={() => handleAddPage(2)}>Template 2: Voice Text + Split Content</button>
                        <button onClick={() => setIsTemplateModalOpen(false)}>Cancel</button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default AddCoursePage;