const thing = document.querySelector("#thing");
const statusText = document.querySelector("#status");
const placeButton = document.querySelector("#place");
const cancelButton = document.querySelector("#cancel");
const bubbles = [...document.querySelectorAll("[data-ablage]")];

let activeAblage = null;
let activeBubbleId = null;
let isPicked = false;
let carryAnnounced = false;
let startPoint = { x: 0, y: 0 };
let targetOffset = { x: 0, y: 0 };
let softOffset = { x: 0, y: 0 };

async function post(path, body = {}) {
    const response = await fetch(path, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body)
    });
    return response.json();
}

function setStatus(text) {
    statusText.textContent = text;
}

function softHaptic(pattern) {
    if ("vibrate" in navigator) {
        navigator.vibrate(pattern);
    }
}

function applyThingTransform() {
    const tilt = Math.max(-2.2, Math.min(2.2, softOffset.x / 34));
    thing.style.setProperty("--carry-x", `${softOffset.x}px`);
    thing.style.setProperty("--carry-y", `${softOffset.y}px`);
    thing.style.setProperty("--soft-tilt", `${tilt}deg`);
}

function settleTowardTarget() {
    softOffset.x += (targetOffset.x - softOffset.x) * 0.64;
    softOffset.y += (targetOffset.y - softOffset.y) * 0.64;
    applyThingTransform();
}

function clearActiveBubble() {
    activeAblage = null;
    activeBubbleId = null;
    bubbles.forEach((bubble) => bubble.classList.remove("is-active"));
}

function activateBubble(bubble) {
    activeAblage = bubble.dataset.ablage;
    activeBubbleId = bubble.dataset.id;
    bubbles.forEach((candidate) => {
        candidate.classList.toggle("is-active", candidate === bubble);
    });
}

function findNearBubble(clientX, clientY) {
    let nearest = null;
    let nearestDistance = Number.POSITIVE_INFINITY;
    for (const bubble of bubbles) {
        const rect = bubble.getBoundingClientRect();
        const centerX = rect.left + rect.width / 2;
        const centerY = rect.top + rect.height / 2;
        const distance = Math.hypot(clientX - centerX, clientY - centerY);
        if (distance < nearestDistance) {
            nearest = bubble;
            nearestDistance = distance;
        }
    }

    return nearestDistance < 92 ? nearest : null;
}

async function markNearBubble(bubble) {
    if (!bubble) {
        clearActiveBubble();
        setStatus("Ding liegt in deiner Hand");
        return;
    }

    const changed = activeBubbleId !== bubble.dataset.id;
    activateBubble(bubble);
    thing.classList.add("is-near");
    setStatus("Hier ablegen");
    if (changed) {
        softHaptic(12);
        await post("/api/near", { ablage: bubble.dataset.id });
    }
}

function resetThingToTray() {
    targetOffset = { x: 0, y: 0 };
    softOffset = { x: 0, y: 0 };
    thing.style.setProperty("--carry-x", "0px");
    thing.style.setProperty("--carry-y", "0px");
    thing.style.setProperty("--soft-tilt", "0deg");
    thing.classList.remove("is-picked", "is-carried", "is-near", "is-placed");
    clearActiveBubble();
}

thing.addEventListener("pointerdown", async (event) => {
    thing.setPointerCapture(event.pointerId);
    isPicked = true;
    carryAnnounced = false;
    startPoint = { x: event.clientX, y: event.clientY };
    thing.classList.remove("is-placed");
    thing.classList.add("is-picked");
    setStatus("Ding genommen");
    softHaptic(8);
    await post("/api/pick");
});

thing.addEventListener("pointermove", async (event) => {
    if (!isPicked) {
        return;
    }

    targetOffset = {
        x: event.clientX - startPoint.x,
        y: event.clientY - startPoint.y
    };
    settleTowardTarget();

    if (!carryAnnounced) {
        carryAnnounced = true;
        thing.classList.add("is-carried");
        await post("/api/carry");
    }

    await markNearBubble(findNearBubble(event.clientX, event.clientY));
});

thing.addEventListener("pointerup", async (event) => {
    if (!isPicked) {
        return;
    }

    if (thing.hasPointerCapture(event.pointerId)) {
        thing.releasePointerCapture(event.pointerId);
    }

    isPicked = false;
    thing.classList.remove("is-picked", "is-carried");
    thing.classList.add("is-placed");
    softHaptic(16);

    const placeOnAblage = Boolean(activeAblage);
    const state = await post("/api/release", {
        ablage: activeAblage,
        x: Math.round(softOffset.x),
        y: Math.round(softOffset.y),
        placeOnAblage
    });
    setStatus(state.status);
});

bubbles.forEach((bubble) => {
    bubble.addEventListener("pointerdown", async () => {
        activateBubble(bubble);
        thing.classList.add("is-near");
        setStatus("Hier ablegen");
        softHaptic(10);
        await post("/api/near", { ablage: bubble.dataset.id });
    });
});

placeButton.addEventListener("click", async () => {
    const state = activeAblage
        ? await post("/api/place", { ablage: activeAblage })
        : await post("/api/release", { x: Math.round(softOffset.x), y: Math.round(softOffset.y), placeOnAblage: false });
    thing.classList.remove("is-picked", "is-carried", "is-near");
    thing.classList.add("is-placed");
    softHaptic(18);
    setStatus(state.status);
});

cancelButton.addEventListener("click", async () => {
    const state = await post("/api/cancel");
    resetThingToTray();
    softHaptic(6);
    setStatus(state.status);
});

async function refresh() {
    const response = await fetch("/api/state");
    const state = await response.json();
    setStatus(state.status);
    bubbles.forEach((bubble) => {
        const model = state.bubbles.find((candidate) => candidate.id === bubble.dataset.id);
        if (!model) {
            return;
        }

        bubble.style.setProperty("--bubble-scale", model.scale);
        bubble.classList.toggle("is-active", model.state === "Active");
        bubble.classList.toggle("is-placed", model.state === "Placed");
    });
}

refresh();
