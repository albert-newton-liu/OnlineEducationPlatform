import React, { useEffect, useRef } from 'react';
import { uploadFile } from "../../constant/UploadFileUtils";

import {TextType, ImageType} from '../../constant/Constants'

const Template2 = ({ content, onContentChange }) => {



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




    return (
        <div className="template-2-container">
            <div className="template-2-top">
                <textarea
                    placeholder="Enter top text..."
                    value={content.topText || ''}
                    onChange={(e) => handleTextChange(e, 'topText')}
                />
            </div>
            <div className="template-2-bottom">
                <div
                    className="content-module"

                    style={{ width: content.leftContent?.size?.width || '100%' }}
                >
                    <select
                        value={content.leftContentType}
                        onChange={(e) => handleTypeChange(e, 'left')}
                    >
                        <option value={TextType}>Text</option>
                        <option value={ImageType}>Image</option>
                    </select>
                    {content.leftContentType === TextType ? (
                        <textarea
                            placeholder="Enter left content text..."
                            value={content.leftContent?.text || ''}
                            onChange={(e) => handleSplitContentChange(e, 'left')}
                        />
                    ) : (
                        <div className="image-display">
                            {content.leftContent?.image ? (
                                <>
                                    <img src={content.leftContent.image} alt="Left content" />
                                    <button onClick={() => handleRemoveImage('left')}>Remove Image</button>
                                </>
                            ) : (
                                <input type="file" accept="image/*" onChange={(e) => handleFileChange(e, 'left')} />
                            )}
                        </div>
                    )}
                </div>
                <div
                    className="content-module"

                    style={{ width: content.rightContent?.size?.width || '100%' }}
                >
                    <select
                        value={content.rightContentType}
                        onChange={(e) => handleTypeChange(e, 'right')}
                    >
                        <option value={TextType}>Text</option>
                        <option value={ImageType}>Image</option>
                    </select>
                    {content.rightContentType === TextType ? (
                        <textarea
                            placeholder="Enter right content text..."
                            value={content.rightContent?.text || ''}
                            onChange={(e) => handleSplitContentChange(e, 'right')}
                        />
                    ) : (
                        <div className="image-display">
                            {content.rightContent?.image ? (
                                <>
                                    <img src={content.rightContent.image} alt="Right content" />
                                    <button onClick={() => handleRemoveImage('right')}>Remove Image</button>
                                </>
                            ) : (
                                <input type="file" accept="image/*" onChange={(e) => handleFileChange(e, 'right')} />
                            )}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default Template2;