function updateClientSizes() {
    window.clientSizes =
    {
        BoardWidth: document.getElementById('logic').offsetWidth,
        BoardHeight: document.getElementById('logic').offsetHeight,
        ClientWidth: window.innerWidth,
        ClientHeight: window.innerHeight
    };
    document.getElementById("players-overlay").width = window.clientSizes.ClientWidth;
    document.getElementById("players-overlay").height = window.clientSizes.ClientHeight;

}
function getLastMousePosition() {
    p = {
        X: (window.mousePosition.x - (window.clientSizes.ClientWidth - window.clientSizes.BoardWidth) / 2) / window.clientSizes.BoardWidth * 100,
        Y: (window.mousePosition.y - (window.clientSizes.ClientHeight - window.clientSizes.BoardHeight) / 2) / window.clientSizes.BoardHeight * 100
    };
    return p;
}

document.addEventListener('readystatechange', function (evt) {
    switch (evt.target.readyState) {
        case "loading":
            // The document is still loading.
            break;
        case "interactive":
            // The document has finished loading. We can now access the DOM elements.
            break;
        case "complete":
            // The document and all sub-resources have finished loading.
            window.addEventListener('resize', function (e) {
                updateClientSizes();
            });
            window.addEventListener('mousemove', function (e) {
                window.mousePosition = { x: e.clientX, y: e.clientY };
            });
            break;
    }
}, false);
