function saveAsFile(filename, bytesBase64) {

    // Download document in Edge browser
    if (navigator.msSaveBlob) { 
        const data = window.atob(bytesBase64);
        const bytes = new Uint8Array(data.length);

        for (let i = 0; i < data.length; i++) {
            bytes[i] = data.charCodeAt(i);
        }
        const blob = new Blob([bytes.buffer], { type: "application/octet-stream" });
        navigator.msSaveBlob(blob, filename);
        return;
    }

    // Other browsers
    const link = document.createElement('a');
    link.download = filename;
    link.href = "data:application/octet-stream;base64," + bytesBase64;
    document.body.appendChild(link); // Needed for Firefox
    link.click();
    document.body.removeChild(link);
}