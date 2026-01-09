(function () {
    async function downloadFileFromStream(fileName, contentStreamReference) {
        const arrayBuffer = await contentStreamReference.arrayBuffer();
        const blob = new Blob([arrayBuffer]);
        const url = URL.createObjectURL(blob);
        var link = document.createElement('a');
        link.download = fileName;
        link.href = url
        document.body.appendChild(link);
        link.click();
        URL.revokeObjectURL(url);
        document.body.removeChild(link);
    }

    async function setAuthCookie(accessToken, expiresIn) {
        const response = await fetch('/api/auth/set-cookie', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                accessToken: accessToken,
                expiresIn: expiresIn
            }),
            credentials: 'same-origin'
        });
        return response.ok;
    }

    async function clearAuthCookie() {
        const response = await fetch('/api/auth/clear-cookie', {
            method: 'POST',
            credentials: 'same-origin'
        });
        return response.ok;
    }

    window.tools = {
        interop: {
            downloadFileFromStream
        },
        auth: {
            setAuthCookie,
            clearAuthCookie
        }
    }
})();