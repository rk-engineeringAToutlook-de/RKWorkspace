const thing = document.querySelector("#thing");
const statusText = document.querySelector("#status");
const placeButton = document.querySelector("#place");
const ablageButtons = [...document.querySelectorAll("[data-ablage]")];

let activeAblage = "Ablage Monitor";

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

function selectAblage(name) {
    activeAblage = name;
    ablageButtons.forEach((button) => {
        button.classList.toggle("is-active", button.dataset.ablage === name);
    });
}

thing.addEventListener("pointerdown", async (event) => {
    thing.setPointerCapture(event.pointerId);
    thing.classList.add("is-picked");
    setStatus("Ding genommen");
    await post("/api/pick");
});

thing.addEventListener("pointermove", async (event) => {
    if (!thing.classList.contains("is-picked")) {
        return;
    }

    const rect = thing.getBoundingClientRect();
    const drift = Math.max(-18, Math.min(18, event.movementX * 1.8));
    thing.style.transform = `translate(${drift}px, -10px) rotate(${drift / 8}deg) scale(1.02)`;

    if (event.clientX > window.innerWidth * 0.58) {
        selectAblage("Ablage Monitor");
        thing.classList.add("is-near");
        setStatus("Ablage Monitor rechts erkannt");
        await post("/api/near", { ablage: activeAblage });
    } else if (event.clientY < rect.top + 12) {
        selectAblage("Ablage Schreibtisch");
        thing.classList.add("is-near");
        setStatus("Ablage Schreibtisch vorne erkannt");
        await post("/api/near", { ablage: activeAblage });
    }
});

thing.addEventListener("pointerup", (event) => {
    if (thing.hasPointerCapture(event.pointerId)) {
        thing.releasePointerCapture(event.pointerId);
    }

    thing.style.transform = "";
});

ablageButtons.forEach((button) => {
    button.addEventListener("click", async () => {
        selectAblage(button.dataset.ablage);
        thing.classList.add("is-near");
        setStatus("Hier ablegen");
        await post("/api/near", { ablage: activeAblage });
    });
});

placeButton.addEventListener("click", async () => {
    await post("/api/place", { ablage: activeAblage });
    thing.classList.remove("is-picked", "is-near");
    thing.classList.add("is-placed");
    setStatus("Abgelegt");
});

async function refresh() {
    const response = await fetch("/api/state");
    const state = await response.json();
    setStatus(state.status);
    selectAblage(state.activeAblage);
}

selectAblage(activeAblage);
refresh();
