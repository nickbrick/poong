function registerGameComponentObject(dotNetObjRef) {
    window.dotNetObjRef = dotNetObjRef;
    updateClientSizes();
    window.mousePosition = { x: 0, y: 0 };
}

function setDocumentTitle(title) {
    document.title = title;
}

backgroundIsColor = true;
hue = 300;
function rollColor() {
    hue = Math.floor(Math.random() * (540 - 300 + 1) + 300);
    document.documentElement.style.setProperty('--body-color', `hsl(${hue},100%,50%)`);
    document.documentElement.style.setProperty('--background-color', `hsl(${hue},60%,${3 * backgroundIsColor}%)`);
    setContextStyles();
}
function clearBackground() {
    backgroundIsColor ^= true;
    document.documentElement.style.setProperty('--background-color', `hsl(${hue},60%,${3 * backgroundIsColor}%)`);
}