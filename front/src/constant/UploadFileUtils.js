export const uploadFile = async (file) => {

    if (!file) return;
    const formData = new FormData();
    formData.append('file', file);

    try {
        const response = await fetch('http://localhost:8081/api/upload', {
            method: 'POST',
            body: formData,
        });

        if (!response.ok) {
            throw new Error('Upload failed');
        }

        const data = await response.json();
        console.log(data.filename)
        const imageUrl = `http://localhost:8081/api/files/${data.filename}`;

        return imageUrl;

    } catch (error) {
        console.error('Error uploading image:', error);
        alert('upload file fail');
    }
}