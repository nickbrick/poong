function startTweenAnimation(id, fromX, fromY, toX, toY, time) {
    var styleTag = document.getElementById(`tweens-${id}`);
    if (!styleTag) {
        newStyleTag = document.createElement('style'); newStyleTag.id = `tweens-${id}`;
        document.body.appendChild(newStyleTag);
    }
    document.getElementById(`tweens-${id}`).innerHTML = `@keyframes anim-${id} {from {left:${fromX}%; top:${fromY}%;} to {left:${toX}%; top:${toY}%;}} .tweens-${id}{animation: anim-${id} ${time}ms linear 0s;}`
    var bodyElm = document.getElementById(id);
    if (bodyElm) {
        bodyElm.classList.remove(`tweens-${id}`);
        void bodyElm.offsetWidth; // MAGIC to restart animation
        bodyElm.classList.add(`tweens-${id}`);
    }
}