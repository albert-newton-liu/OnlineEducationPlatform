import React from 'react';
import Template1 from '../../../../../components/CourseTemplate1';
import Template2 from '../../../../../components/CourseTemplate2';

const PageEditorSection = ({
    pages,
    currentPageIndex,
    hasUnsavedChanges,
    onContentChange,
    onSavePage,
    onDeletePage,
    onPageNavigation,
    onModalOpen,
}) => {
    const currentPage = pages[currentPageIndex];

    return (
        <div className="page-editor-section">
            <header className="page-editor-header">
                <h2>Page Content Editor</h2>
                <div className="button-group">
                    <button
                        onClick={onModalOpen}
                        className="add-page-button"
                        disabled={hasUnsavedChanges}
                    >
                        Add a Page
                    </button>
                    <button
                        onClick={onSavePage}
                        className="save-page-button"
                        disabled={ pages.length === 0 || !hasUnsavedChanges  }
                    >
                        Save Page
                    </button>
                    <button
                        onClick={onDeletePage}
                        className="delete-page-button"
                        disabled={pages.length === 0}
                    >
                        Delete Page
                    </button>
                </div>
            </header>

            {currentPage && currentPage.template === 1 && (
                <Template1
                    content={currentPage.content}
                    onContentChange={onContentChange}
                />
            )}
            {currentPage && currentPage.template === 2 && (
                <Template2
                    content={currentPage.content}
                    onContentChange={onContentChange}
                />
            )}

            {!currentPage && <div className="placeholder-message">Click "Add a Page" to create your first course page.</div>}

            <div className="page-navigation-section">
                <button onClick={() => onPageNavigation('prev')} disabled={currentPageIndex === 0}>Previous Page</button>
                <span>Page {pages.length > 0 ? currentPageIndex + 1 : 0} of {pages.length}</span>
                <button onClick={() => onPageNavigation('next')} disabled={currentPageIndex === pages.length - 1 || pages.length === 0}>Next Page</button>
            </div>
        </div>
    );
};

export default PageEditorSection;