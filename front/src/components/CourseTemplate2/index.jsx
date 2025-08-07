import React, { useEffect, useRef } from 'react';
import { useLocation } from 'react-router-dom';
import { uploadFile } from "../../constant/UploadFileUtils";

import { TextType, ImageType } from '../../constant/Constants'

const Template2 = ({ content, onContentChange, speak, isReadOnly = false }) => {

    const location = useLocation();
    const isViewMode = isReadOnly || location.pathname.includes('/view-course/');

    const contentRef = useRef(content);
    const onContentChangeRef = useRef(onContentChange);

    useEffect(() => {
        contentRef.current = content;
    }, [content]);

    useEffect(() => {
        onContentChangeRef.current = onContentChange;
    }, [onContentChange]);


    const handleTextChange = (e, field) => {
        onContentChangeRef.current({
            ...contentRef.current,
            [field]: e.target.value,
        });
    };

    const handleSplitContentChange = (e, side) => {
        onContentChangeRef.current({
            ...contentRef.current,
            [`${side}Content`]: {
                ...contentRef.current[`${side}Content`],
                text: e.target.value,
            },
        });
    };

    const handleTypeChange = (e, side) => {
        onContentChangeRef.current({
            ...contentRef.current,
            [`${side}ContentType`]: e.target.value,
        });
    };

    const handleFileChange = async (e, side) => {
        const file = e.target.files[0];
        if (file) {
            try {
                const imageUrl = await uploadFile(file);
                onContentChangeRef.current({
                    ...contentRef.current,
                    [`${side}Content`]: {
                        ...contentRef.current[`${side}Content`],
                        image: imageUrl,
                        text: null,
                    }
                });
            } catch (error) {
                console.error("Image upload failed:", error);
            }
        }
    };

    const handleRemoveImage = (side) => {
        onContentChangeRef.current({
            ...contentRef.current,
            [`${side}Content`]: {
                ...contentRef.current[`${side}Content`],
                image: null,
            }
        });
    };

    const handleTextClick = () => {
        if (!isViewMode) return;

        const selection = window.getSelection();
        const selectedText = selection.toString().trim();

        if (selectedText.length > 0) {
            // 如果有选中的文字，只发音选中的文字
            speak(selectedText);
        } else {
            // 如果没有选中文字，可以取消当前发音
            // 或者选择发音全部文本
            speak(content.text);
        }
    };




    return (
        <div className="template-2-container">
            <div className="template-2-top">
                {
                    isViewMode ? (
                        <div
                            className="template-2-top-textarea"
                            style={{ whiteSpace: 'pre-wrap', }}
                            onClick={handleTextClick} 
                        >
                            {content.topText}
                        </div>
                    ) : (
                        <textarea
                            placeholder="Enter top text..."
                            value={content.topText || ''}
                            onChange={(e) => handleTextChange(e, 'topText')}
                        />
                    )
                }
            </div>
            <div className="template-2-bottom">
                <div
                    className="content-module"
                    style={{ width: content.leftContent?.size?.width || '100%' }}
                >
                    {!isViewMode && (
                        <select
                            value={content.leftContentType}
                            onChange={(e) => handleTypeChange(e, 'left')}
                        >
                            <option value={TextType}>Text</option>
                            <option value={ImageType}>Image</option>
                        </select>
                    )}

                    {content.leftContentType === TextType ? (
                        isViewMode ? (
                            <div
                                className="content-module-textarea"
                                style={{ whiteSpace: 'pre-wrap', }}
                                onClick={handleTextClick} 
                            >
                                {content.leftContent?.text}
                            </div>
                        ) : (
                            <textarea
                                placeholder="Enter left content text..."
                                value={content.leftContent?.text || ''}
                                onChange={(e) => handleSplitContentChange(e, 'left')}
                            />
                        )
                    ) : (
                        <div className="image-display">
                            {content.leftContent?.image ? (
                                <>
                                    <img src={content.leftContent.image} alt="Left content" />
                                    {!isViewMode && (
                                        <button onClick={() => handleRemoveImage('left')}>Remove Image</button>)}
                                </>
                            ) : (
                                !isViewMode && (
                                    <input type="file" accept="image/*" onChange={(e) => handleFileChange(e, 'left')} />
                                )
                            )}
                        </div>
                    )}
                </div>
                <div
                    className="content-module"
                    style={{ width: content.rightContent?.size?.width || '100%' }}
                >
                    {!isViewMode && (
                        <select
                            value={content.rightContentType}
                            onChange={(e) => handleTypeChange(e, 'right')}
                        >
                            <option value={TextType}>Text</option>
                            <option value={ImageType}>Image</option>
                        </select>
                    )}

                    {content.rightContentType === TextType ? (
                        isViewMode ? (
                            <div
                                className="content-module-textarea"
                                style={{ whiteSpace: 'pre-wrap', }}
                                onClick={handleTextClick} 
                            >
                                {content.rightContent?.text}
                            </div>
                        ) : (
                            <textarea
                                placeholder="Enter right content text..."
                                value={content.rightContent?.text || ''}
                                onChange={(e) => handleSplitContentChange(e, 'right')}
                            />
                        )

                    ) : (
                        <div className="image-display">
                            {content.rightContent?.image ? (
                                <>
                                    <img src={content.rightContent.image} alt="Right content" />
                                    {!isViewMode && (
                                        <button onClick={() => handleRemoveImage('right')}>Remove Image</button>
                                    )}

                                </>
                            ) : (
                                !isViewMode && (
                                    <input type="file" accept="image/*" onChange={(e) => handleFileChange(e, 'right')} />
                                )
                            )}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default Template2;