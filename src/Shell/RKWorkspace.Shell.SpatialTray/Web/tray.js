const surfaceName = document.querySelector("#surface-name");
const surfaceHint = document.querySelector("#surface-hint");
const thing = document.querySelector("#thing");
const thingName = document.querySelector("#thing-name");
const thingHint = document.querySelector("#thing-hint");
const emptyTrace = document.querySelector("#empty-trace");
const statusText = document.querySelector("#status");
const placeButton = document.querySelector("#place");
const cancelButton = document.querySelector("#cancel");
const compass = document.querySelector("#compass");

const surfaceId = window.location.pathname.split("/").filter(Boolean).pop() || "handy";
let activeAblage = null;
let activeBubbleId = null;
let canPickHere = false;
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

async function getState() {
    const response = await fetch(`/api/state?ablage=${encodeURIComponent(surfaceId)}`);
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
    document.querySelectorAll(".ablage-bubble").forEach((bubble) => bubble.classList.remove("is-active"));
}

function activateBubble(bubble) {
    activeAblage = bubble.dataset.id;
    activeBubbleId = bubble.dataset.id;
    document.querySelectorAll(".ablage-bubble").forEach((candidate) => {
        candidate.classList.toggle("is-active", candidate === bubble);
    });
}

function findNearBubble(clientX, clientY) {
    let nearest = null;
    let nearestDistance = Number.POSITIVE_INFINITY;
    for (const bubble of document.querySelectorAll(".ablage-bubble")) {
        const rect = bubble.getBoundingClientRect();
        const centerX = rect.left + rect.width / 2;
        const centerY = rect.top + rect.height / 2;
        const distance = Math.hypot(clientX - centerX, clientY - centerY);
        if (distance < nearestDistance) {
            nearest = bubble;
            nearestDistance = distance;
        }
    }

    return nearestDistance < 96 ? nearest : null;
}

async function markNearBubble(bubble) {
    if (!bubble) {
        clearActiveBubble();
        setStatus("Ding liegt in deiner Hand");
        return;
    }

    const changed = activeBubbleId !== bubble.dataset.id;
    activateBubble(bubble);
    thing.classList.add("is-carried");
    setStatus("Hier ablegen");
    if (changed) {
        softHaptic(12);
        await post("/api/approach", {
            carrierAblageId: surfaceId,
            targetAblageId: bubble.dataset.id
        });
    }
}

function resetThingMotion() {
    targetOffset = { x: 0, y: 0 };
    softOffset = { x: 0, y: 0 };
    thing.style.setProperty("--carry-x", "0px");
    thing.style.setProperty("--carry-y", "0px");
    thing.style.setProperty("--soft-tilt", "0deg");
}

function updateThingClasses(surfaceThing) {
    thing.classList.toggle("is-hidden", !surfaceThing.visible || surfaceThing.sourceWasHere);
    emptyTrace.classList.toggle("is-hidden", !surfaceThing.sourceWasHere);
    thing.classList.toggle("is-preview", surfaceThing.isPreviewHere);
    thing.classList.toggle("is-carried", surfaceThing.isCarriedHere || isPicked);
    thing.classList.toggle("is-placed", surfaceThing.isHere && !isPicked);
    thing.classList.toggle("is-picked", isPicked);
}

function distanceClass(distance) {
    return {
        VeryFar: "is-very-far",
        Far: "is-far",
        Medium: "is-medium",
        Near: "is-near",
        VeryNear: "is-very-near"
    }[distance] || "is-medium";
}

function renderBubbles(bubbles) {
    compass.innerHTML = "";
    for (const bubble of bubbles) {
        const node = document.createElement("div");
        node.className = `ablage-bubble ${distanceClass(bubble.distance)}`;
        node.dataset.id = bubble.ablageId;
        node.style.left = `${Math.round(bubble.x * 100)}%`;
        node.style.top = `${Math.round(bubble.y * 100)}%`;
        node.style.setProperty("--bubble-scale", bubble.scale);
        node.classList.toggle("is-active", bubble.state === "Active");
        node.classList.toggle("is-placed", bubble.state === "Placed");

        const dot = document.createElement("span");
        dot.className = "bubble-dot";
        const label = document.createElement("span");
        label.className = "bubble-label";
        label.textContent = bubble.displayName;
        const action = document.createElement("span");
        action.className = "bubble-action";
        action.textContent = bubble.actionText || "Hier ablegen";
        node.append(dot, label, action);

        node.addEventListener("pointerdown", async (event) => {
            event.preventDefault();
            activateBubble(node);
            setStatus("Hier ablegen");
            softHaptic(10);
            await post("/api/approach", {
                carrierAblageId: surfaceId,
                targetAblageId: node.dataset.id
            });
        });

        compass.append(node);
    }
}

function renderState(state) {
    surfaceName.textContent = state.surface.displayName;
    surfaceHint.textContent = state.surfaceThing.isPreviewHere ? "kommt an" : "bereit";
    thingName.textContent = state.surfaceThing.displayName;
    thingHint.textContent = state.surfaceThing.text;
    canPickHere = state.surfaceThing.isHere;
    updateThingClasses(state.surfaceThing);
    renderBubbles(state.bubbles);
    setStatus(state.status);
}

thing.addEventListener("pointerdown", async (event) => {
    event.preventDefault();
    if (!canPickHere && !thing.classList.contains("is-carried")) {
        return;
    }

    thing.setPointerCapture(event.pointerId);
    isPicked = true;
    carryAnnounced = false;
    startPoint = { x: event.clientX, y: event.clientY };
    thing.classList.remove("is-placed", "is-preview");
    thing.classList.add("is-picked");
    emptyTrace.classList.add("is-hidden");
    setStatus("Ding genommen");
    softHaptic(8);
    await post("/api/pick", { carrierAblageId: surfaceId });
});

thing.addEventListener("pointermove", async (event) => {
    if (!isPicked) {
        return;
    }

    event.preventDefault();
    targetOffset = {
        x: event.clientX - startPoint.x,
        y: event.clientY - startPoint.y
    };
    settleTowardTarget();

    if (!carryAnnounced) {
        carryAnnounced = true;
        await post("/api/move", {
            carrierAblageId: surfaceId,
            x: Math.round(softOffset.x),
            y: Math.round(softOffset.y)
        });
    }

    await markNearBubble(findNearBubble(event.clientX, event.clientY));
});

thing.addEventListener("pointerup", async (event) => {
    if (!isPicked) {
        return;
    }

    event.preventDefault();
    if (thing.hasPointerCapture(event.pointerId)) {
        thing.releasePointerCapture(event.pointerId);
    }

    isPicked = false;
    thing.classList.remove("is-picked", "is-carried");
    thing.classList.add("is-placed");
    softHaptic(16);

    const state = activeAblage
        ? await post("/api/place", {
            carrierAblageId: surfaceId,
            targetAblageId: activeAblage,
            x: Math.round(softOffset.x),
            y: Math.round(softOffset.y)
        })
        : await post("/api/release", {
            carrierAblageId: surfaceId,
            x: Math.round(softOffset.x),
            y: Math.round(softOffset.y),
            placeOnAblage: false
        });
    clearActiveBubble();
    renderState(state);
});

placeButton.addEventListener("click", async () => {
    const state = activeAblage
        ? await post("/api/place", {
            carrierAblageId: surfaceId,
            targetAblageId: activeAblage,
            x: Math.round(softOffset.x),
            y: Math.round(softOffset.y)
        })
        : await post("/api/release", {
            carrierAblageId: surfaceId,
            x: Math.round(softOffset.x),
            y: Math.round(softOffset.y),
            placeOnAblage: false
        });
    isPicked = false;
    resetThingMotion();
    clearActiveBubble();
    softHaptic(18);
    renderState(state);
});

cancelButton.addEventListener("click", async () => {
    const state = await post("/api/cancel", { carrierAblageId: surfaceId });
    isPicked = false;
    resetThingMotion();
    clearActiveBubble();
    softHaptic(6);
    renderState(state);
});

async function refresh() {
    if (isPicked) {
        return;
    }

    const state = await getState();
    renderState(state);
}

refresh();
setInterval(refresh, 250);
