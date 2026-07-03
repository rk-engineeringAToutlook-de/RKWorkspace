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
let hasLifted = false;
let startPoint = { x: 0, y: 0 };
let lastPoint = { x: 0, y: 0 };
let targetOffset = { x: 0, y: 0 };
let softOffset = { x: 0, y: 0 };
let targetTilt = { x: 0, y: 0 };
let softTilt = { x: 0, y: 0 };
let tactileConfig = {
    resistancePx: 10,
    maxTilt: 4.2,
    heldScale: 0.94,
    glideMs: 180
};

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

function clamp(value, min, max) {
    return Math.max(min, Math.min(max, value));
}

function pause(ms) {
    return new Promise((resolve) => window.setTimeout(resolve, ms));
}

function applyThingTransform() {
    thing.style.setProperty("--carry-x", `${softOffset.x}px`);
    thing.style.setProperty("--carry-y", `${softOffset.y}px`);
    thing.style.setProperty("--tilt-x", `${softTilt.x}deg`);
    thing.style.setProperty("--tilt-y", `${softTilt.y}deg`);
    thing.style.setProperty("--shadow-x", `${clamp(-softTilt.x * 1.4, -8, 8)}px`);
    thing.style.setProperty("--shadow-y", `${clamp(18 + Math.abs(softTilt.y) * 1.5, 15, 28)}px`);
    thing.style.setProperty("--held-scale", hasLifted ? tactileConfig.heldScale : 1);
}

function settleTowardTarget() {
    softOffset.x += (targetOffset.x - softOffset.x) * 0.56;
    softOffset.y += (targetOffset.y - softOffset.y) * 0.56;
    softTilt.x += (targetTilt.x - softTilt.x) * 0.38;
    softTilt.y += (targetTilt.y - softTilt.y) * 0.38;
    applyThingTransform();
}

function clearActiveBubble() {
    activeAblage = null;
    activeBubbleId = null;
    document.querySelectorAll(".ablage-bubble").forEach((bubble) => {
        bubble.classList.remove("is-active", "is-opening");
    });
}

function activateBubble(bubble) {
    activeAblage = bubble.dataset.id;
    activeBubbleId = bubble.dataset.id;
    document.querySelectorAll(".ablage-bubble").forEach((candidate) => {
        candidate.classList.toggle("is-active", candidate === bubble);
        candidate.classList.toggle("is-opening", candidate === bubble);
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

    return nearestDistance < 104 ? nearest : null;
}

async function markNearBubble(bubble) {
    if (!bubble) {
        clearActiveBubble();
        thing.classList.remove("is-near-ablage", "is-gliding-into-bubble");
        setStatus("Ding liegt in deiner Hand");
        return;
    }

    const changed = activeBubbleId !== bubble.dataset.id;
    activateBubble(bubble);
    thing.classList.add("is-carried", "is-near-ablage");
    setStatus("Ablage oeffnet sich");
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
    targetTilt = { x: 0, y: 0 };
    softTilt = { x: 0, y: 0 };
    hasLifted = false;
    thing.style.setProperty("--carry-x", "0px");
    thing.style.setProperty("--carry-y", "0px");
    thing.style.setProperty("--tilt-x", "0deg");
    thing.style.setProperty("--tilt-y", "0deg");
    thing.style.setProperty("--shadow-x", "0px");
    thing.style.setProperty("--shadow-y", "18px");
    thing.style.setProperty("--held-scale", "1");
}

function updateThingClasses(surfaceThing) {
    thing.classList.toggle("is-hidden", !surfaceThing.visible || surfaceThing.sourceWasHere);
    emptyTrace.classList.toggle("is-hidden", !surfaceThing.sourceWasHere);
    thing.classList.toggle("is-preview", surfaceThing.isPreviewHere);
    thing.classList.toggle("is-arriving", surfaceThing.ghostVisible);
    thing.classList.toggle("is-carried", surfaceThing.isCarriedHere || isPicked);
    thing.classList.toggle("is-placed", surfaceThing.isHere && !isPicked);
    thing.classList.toggle("is-picked", isPicked);
    thing.classList.toggle("is-held-compact", surfaceThing.heldCompact || isPicked);
    thing.classList.toggle("is-occluded", surfaceThing.partialOcclusion || isPicked);
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
        node.classList.toggle("is-opening", bubble.opens || bubble.state === "Opening");
        node.classList.toggle("is-placed", bubble.state === "Placed");

        const dot = document.createElement("span");
        dot.className = "bubble-dot";
        const lens = document.createElement("span");
        lens.className = "bubble-lens";
        const label = document.createElement("span");
        label.className = "bubble-label";
        label.textContent = bubble.shortName || bubble.displayName;
        const opening = document.createElement("span");
        opening.className = "bubble-opening";
        opening.textContent = bubble.openingText || "";
        const action = document.createElement("span");
        action.className = "bubble-action";
        action.textContent = bubble.actionText || "Hier ablegen";
        node.append(lens, dot, label, opening, action);

        node.addEventListener("pointerdown", async (event) => {
            event.preventDefault();
            activateBubble(node);
            thing.classList.add("is-near-ablage");
            setStatus("Ablage oeffnet sich");
            softHaptic(12);
            await post("/api/approach", {
                carrierAblageId: surfaceId,
                targetAblageId: node.dataset.id
            });
        });

        compass.append(node);
    }
}

function applySurfaceThingPosition(surfaceThing) {
    if (surfaceThing.position) {
        thing.style.setProperty("--place-shift-x", `${Math.round((surfaceThing.position.x - 0.5) * 150)}px`);
        thing.style.setProperty("--place-shift-y", `${Math.round((surfaceThing.position.y - 0.5) * 110)}px`);
    } else {
        thing.style.setProperty("--place-shift-x", "0px");
        thing.style.setProperty("--place-shift-y", "0px");
    }

    thing.style.setProperty("--surface-scale", surfaceThing.displayScale || 1);
}

function renderState(state) {
    tactileConfig = {
        resistancePx: state.motion?.initialResistanceDistancePx ?? tactileConfig.resistancePx,
        maxTilt: state.motion?.vectorTiltMaxDegrees ?? tactileConfig.maxTilt,
        heldScale: state.motion?.heldCompactScale ?? tactileConfig.heldScale,
        glideMs: state.motion?.glideIntoBubbleMs ?? tactileConfig.glideMs
    };
    surfaceName.textContent = state.surface.shortName || state.surface.displayName;
    surfaceHint.textContent = state.surfaceThing.isPreviewHere ? "kommt an" : "bereit";
    thingName.textContent = state.surfaceThing.displayName;
    thingHint.textContent = state.surfaceThing.text;
    canPickHere = state.surfaceThing.isHere;
    applySurfaceThingPosition(state.surfaceThing);
    document.body.classList.toggle("is-carrying", state.surfaceThing.isCarriedHere || isPicked);
    document.body.classList.toggle("is-arriving", state.surfaceThing.isPreviewHere);
    updateThingClasses(state.surfaceThing);
    renderBubbles(state.bubbles);
    setStatus(state.status);
}

function placementPointFromMotion() {
    return {
        x: clamp(0.5 + (softOffset.x / Math.max(window.innerWidth, 1)) * 0.48, 0.12, 0.88),
        y: clamp(0.5 + (softOffset.y / Math.max(window.innerHeight, 1)) * 0.48, 0.12, 0.88)
    };
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
    lastPoint = { ...startPoint };
    resetThingMotion();
    thing.classList.remove("is-placed", "is-preview", "is-arriving", "is-gliding-into-bubble");
    thing.classList.add("is-picked", "is-touching", "is-resisting", "is-held-compact", "is-occluded");
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
    const currentPoint = { x: event.clientX, y: event.clientY };
    const movement = {
        x: currentPoint.x - lastPoint.x,
        y: currentPoint.y - lastPoint.y
    };
    lastPoint = currentPoint;

    const fromStart = {
        x: currentPoint.x - startPoint.x,
        y: currentPoint.y - startPoint.y
    };
    const distance = Math.hypot(fromStart.x, fromStart.y);

    if (!hasLifted && distance < tactileConfig.resistancePx) {
        targetOffset = {
            x: fromStart.x * 0.28,
            y: fromStart.y * 0.28
        };
        targetTilt = { x: 0, y: 0 };
        settleTowardTarget();
        return;
    }

    if (!hasLifted) {
        hasLifted = true;
        thing.classList.remove("is-resisting");
        thing.classList.add("is-lifted");
        setStatus("Ding liegt in deiner Hand");
        softHaptic(9);
    }

    targetOffset = fromStart;
    targetTilt = {
        x: clamp(movement.x * 0.34, -tactileConfig.maxTilt, tactileConfig.maxTilt),
        y: clamp(-movement.y * 0.28, -tactileConfig.maxTilt, tactileConfig.maxTilt)
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
    thing.classList.remove("is-picked", "is-carried", "is-touching", "is-resisting", "is-lifted", "is-near-ablage");
    softHaptic(16);
    const placementPoint = placementPointFromMotion();

    let state;
    if (activeAblage) {
        thing.classList.add("is-gliding-into-bubble");
        setStatus("Ding gleitet hinein");
        await pause(tactileConfig.glideMs);
        state = await post("/api/place", {
            carrierAblageId: surfaceId,
            targetAblageId: activeAblage,
            x: placementPoint.x,
            y: placementPoint.y
        });
    } else {
        state = await post("/api/release", {
            carrierAblageId: surfaceId,
            x: placementPoint.x,
            y: placementPoint.y,
            placeOnAblage: false
        });
    }

    thing.classList.remove("is-gliding-into-bubble", "is-held-compact", "is-occluded");
    thing.classList.add("is-placed");
    clearActiveBubble();
    resetThingMotion();
    renderState(state);
});

placeButton.addEventListener("click", async () => {
    const placementPoint = placementPointFromMotion();
    const state = activeAblage
        ? await post("/api/place", {
            carrierAblageId: surfaceId,
            targetAblageId: activeAblage,
            x: placementPoint.x,
            y: placementPoint.y
        })
        : await post("/api/release", {
            carrierAblageId: surfaceId,
            x: placementPoint.x,
            y: placementPoint.y,
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
