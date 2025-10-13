import { API_BASE_URL } from "./Constants";

// Utility function to upload a file and return its accessible URL
export const uploadFile = async (file) => {

    if (!file) return;
    const formData = new FormData();
    formData.append('file', file);

    try {
        const response = await fetch(`${API_BASE_URL}/api/Upload/upload`, {
            method: 'POST',
            body: formData,
        });

        if (!response.ok) {
            throw new Error('Upload failed');
        }

        const data = await response.json();

        return `${API_BASE_URL + data.accessUrl}`;

    } catch (error) {
        console.error('Error uploading image:', error);
        alert('upload file fail');
    }
}