initPlayersOverlayCanvas = function () {
    c = document.getElementById("players-overlay");
    setContextStyles();
    window.addEventListener('resize', function (e) {
        setContextStyles();
    });
    window.playerTimer = window.setInterval(function () {
        drawPlayers();
        stepPlayerPositions();
    }, 20); //TODO parametrize interval if possible
}
setContextStyles = function () {
    window.ctx = c.getContext("2d");
    window.ctx.strokeStyle = 'red';
    window.ctx.fillStyle = 'red';
    window.ctx.font = '16px monospace';
}

function setPlayersKeyframe(players) {
    delete window.players;
    window.players = players;
}
function printPlayers() {
    console.log(window.players);
}
function drawPlayers() {
    ctx = window.ctx;
    ctx.clearRect(0, 0, window.clientSizes.ClientWidth, window.clientSizes.ClientHeight);
    for (var i = 0, len = players.length; i < len; i++) {
        ctx.beginPath();
        x = window.players[i].lastPosition.x * window.clientSizes.BoardWidth / 100 + (window.clientSizes.ClientWidth - window.clientSizes.BoardWidth) / 2;
        y = window.players[i].lastPosition.y * window.clientSizes.BoardHeight / 100 + (window.clientSizes.ClientHeight - window.clientSizes.BoardHeight) / 2;
        ctx.rect(x, y, 2, 2);
        ctx.fillText(players[i].displayName, x+8, y+8);
        ctx.stroke();
    }
}
function stepPlayerPositions() {
    for (var i = 0, len = window.players.length; i < len; i++) {
        window.players[i].lastPosition.x += window.players[i].speed.x;
        window.players[i].lastPosition.y += window.players[i].speed.y;
    }
}