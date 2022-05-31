function registerGameComponentObject(dotNetObjRef) {
    window.dotNetObjRef = dotNetObjRef;
    updateClientSizes();
    window.mousePosition = { x: 0, y: 0 };
}

function setDocumentTitle(title) {
    document.title = title;
}