function registerGameComponentObject(dotNetObjRef) {
    window.dotNetObjRef = dotNetObjRef;
    updateClientSizes();
    window.mousePosition = { x: 0, y: 0 };
}

function setDocumentTitle(title) {
    document.title = title;
}

function rollColor() {
    document.documentElement.style.setProperty('--body-color', `hsl(${Math.floor(Math.random() * (540 - 300 + 1) + 300)},100%,50%`);
    setContextStyles();
}
