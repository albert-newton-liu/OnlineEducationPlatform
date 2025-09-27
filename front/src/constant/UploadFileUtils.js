import { API_BASE_URL } from "./Constants";

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
    
        return data.accessUrl;

    } catch (error) {
        console.error('Error uploading image:', error);
        alert('upload file fail');
    }
}