import React, { useState, useRef, useEffect, useCallback } from 'react';
import { useLocation } from 'react-router-dom';

import { uploadFile } from "../../constant/UploadFileUtils"





const Template1 = ({ content, onContentChange, speak, isReadOnly = false }) => {
    const location = useLocation();
    const isViewMode = isReadOnly || location.pathname.includes('/view-course/');

    const textOverlayRef = useRef(null);
    const [fileInputKey, setFileInputKey] = useState(Date.now());

    const {
        textAreaSize = { width: '400px', height: '200px' },
        text = "",
        backgroundImage = "",
        position = { x: 2500, y: 250 }
    } = content;

    const [isDragging, setIsDragging] = useState(false);
    const [startMousePosition, setStartMousePosition] = useState({ x: 0, y: 0 });

    const contentRef = useRef(content);

    const onContentChangeRef = useRef(onContentChange);

    const isResizingByUserRef = useRef(false);


    useEffect(() => {
        contentRef.current = content;
    }, [content]);


    useEffect(() => {
        onContentChangeRef.current = onContentChange;
    }, [onContentChange]);

    const handleImageUpload = async (e) => {
        const file = e.target.files[0];
        if (!file) return;

        try {
            const imageUrl = await uploadFile(file);
            const newBackgroundImage = `url(${imageUrl})`;

            onContentChangeRef.current({
                ...contentRef.current,
                backgroundImage: newBackgroundImage,
            });

        } catch (error) {
            console.error("Image upload failed:", error);
        } finally {
            setFileInputKey(Date.now());
        }
    };

    const handleTextChange = (e) => {
        const newText = e.target.value;
        onContentChangeRef.current({
            ...contentRef.current,
            text: newText,
        });
    };

    const handleMouseMove = useCallback((e) => {
        if (isDragging) {
            const dx = e.clientX - startMousePosition.x;
            const dy = e.clientY - startMousePosition.y;

            onContentChangeRef.current({
                ...contentRef.current,
                position: {
                    x: contentRef.current.position.x + dx,
                    y: contentRef.current.position.y + dy,
                }
            });

            setStartMousePosition({ x: e.clientX, y: e.clientY });
        }
    }, [isDragging, startMousePosition, contentRef, onContentChangeRef]);

    const handleMouseUp = useCallback(() => {
        setIsDragging(false);
    }, []);

    const handleMouseDown = useCallback((e) => {
        if (e.target.tagName !== 'TEXTAREA') {
            setIsDragging(true);
            setStartMousePosition({ x: e.clientX, y: e.clientY });
            e.preventDefault();
        }
    }, []);

    useEffect(() => {
        if (isDragging) {
            window.addEventListener('mousemove', handleMouseMove);
            window.addEventListener('mouseup', handleMouseUp);
        }
        return () => {
            window.removeEventListener('mousemove', handleMouseMove);
            window.removeEventListener('mouseup', handleMouseUp);
        };
    }, [isDragging, handleMouseMove, handleMouseUp]);

    useEffect(() => {
        const textareaElement = textOverlayRef.current.querySelector('textarea');
        if (!textareaElement) return;

        const handleResizeStart = () => {
            isResizingByUserRef.current = true;
        };
        const handleResizeEnd = () => {
            isResizingByUserRef.current = false;
        };

        textareaElement.addEventListener('mousedown', handleResizeStart);
        window.addEventListener('mouseup', handleResizeEnd);

        const resizeObserver = new ResizeObserver(entries => {
            if (!isResizingByUserRef.current) return;

            for (let entry of entries) {
                const { clientWidth, clientHeight } = entry.target;
                const newSize = {
                    width: `${clientWidth}px`,
                    height: `${clientHeight}px`,
                };

                const currentContent = contentRef.current;
                const currentWidth = currentContent.textAreaSize?.width;
                const currentHeight = currentContent.textAreaSize?.height;

                if (currentWidth !== newSize.width || currentHeight !== newSize.height) {
                    onContentChangeRef.current({
                        ...currentContent,
                        textAreaSize: newSize,
                    });
                }
            }
        });

        resizeObserver.observe(textareaElement);

        return () => {
            textareaElement.removeEventListener('mousedown', handleResizeStart);
            window.removeEventListener('mouseup', handleResizeEnd);
            resizeObserver.disconnect();
        };
    }, []);

     const handleTextClick = () => {
        if (!isViewMode) return;

        const selection = window.getSelection();
        const selectedText = selection.toString().trim();

        if (selectedText.length > 0) {
            
            speak(selectedText);
        } else {
            
            speak(content.text);
        }
    };



    return (

        <div
            className="template-1-container"
            style={{ backgroundImage: content.backgroundImage }}
        >
            {!isViewMode && (
                <input
                    key={fileInputKey}
                    type="file"
                    accept="image/*"
                    onChange={handleImageUpload}
                    className="template-image-input"
                />
            )}

            <div
                ref={textOverlayRef}
                className="template-1-text-overlay"
                style={{
                    transform: `translate(calc(-50% + ${position.x}px), calc(-50% + ${position.y}px))`

                }}
                onMouseDown={isViewMode ? null : handleMouseDown}

            >
                {isViewMode ? (
                    <div
                        className="template-1-textarea"
                        style={{
                            width: content.textAreaSize?.width,
                            height: content.textAreaSize?.height,
                            whiteSpace: 'pre-wrap',
                        }}
                         onClick={handleTextClick} 
                    >
                        {content.text}
                    </div>
                ) : (
                    <textarea
                        value={text}
                        onChange={handleTextChange}
                        placeholder="Enter your text here..."
                        style={{
                            width: `${textAreaSize.width} `,
                            height: `${textAreaSize.height} `,
                            boxSizing: 'border-box'
                        }}
                    />
                )}

            </div>
        </div>
    );
};

export default Template1;