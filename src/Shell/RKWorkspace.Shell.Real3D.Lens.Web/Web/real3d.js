import * as THREE from "three";
import { RoomEnvironment } from "three/addons/environments/RoomEnvironment.js";

const canvas = document.querySelector("#real3d-canvas");
const status = document.querySelector("#status");
const lookButtons = {
  glass: document.querySelector("#look-glass"),
  tunnel: document.querySelector("#look-tunnel"),
  hybrid: document.querySelector("#look-hybrid")
};

const renderer = new THREE.WebGLRenderer({
  canvas,
  antialias: true,
  alpha: false,
  powerPreference: "high-performance"
});
renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
renderer.setSize(window.innerWidth, window.innerHeight);
renderer.outputColorSpace = THREE.SRGBColorSpace;
renderer.toneMapping = THREE.ACESFilmicToneMapping;
renderer.toneMappingExposure = 1.12;
renderer.shadowMap.enabled = true;
renderer.shadowMap.type = THREE.PCFSoftShadowMap;

const scene = new THREE.Scene();
scene.background = new THREE.Color(0x111719);
const camera = new THREE.PerspectiveCamera(38, window.innerWidth / window.innerHeight, 0.1, 80);
camera.position.set(0, 1.45, 6.2);
camera.lookAt(0.25, 0.05, 0);

const pmrem = new THREE.PMREMGenerator(renderer);
scene.environment = pmrem.fromScene(new RoomEnvironment(renderer), 0.05).texture;

const keyLight = new THREE.DirectionalLight(0xffffff, 4.6);
keyLight.position.set(-3.2, 4.8, 3.8);
keyLight.castShadow = true;
keyLight.shadow.mapSize.set(2048, 2048);
keyLight.shadow.camera.near = 0.5;
keyLight.shadow.camera.far = 12;
scene.add(keyLight);
scene.add(new THREE.HemisphereLight(0xbfd7e1, 0x283238, 1.35));

const desktopTexture = createDesktopTexture();
desktopTexture.colorSpace = THREE.SRGBColorSpace;
const desktopMaterial = new THREE.MeshStandardMaterial({
  map: desktopTexture,
  roughness: 0.64,
  metalness: 0.03
});
const desktop = new THREE.Mesh(new THREE.PlaneGeometry(8.8, 4.95, 1, 1), desktopMaterial);
desktop.position.set(0, -0.42, -0.62);
desktop.receiveShadow = true;
scene.add(desktop);

const contactShadowTexture = createContactShadowTexture();
const contactShadow = new THREE.Mesh(
  new THREE.PlaneGeometry(1.42, 0.82),
  new THREE.MeshBasicMaterial({
    map: contactShadowTexture,
    transparent: true,
    opacity: 0.28,
    depthWrite: false
  })
);
contactShadow.position.set(-1.35, -0.385, 0.01);
scene.add(contactShadow);

const cardGeometry = new THREE.PlaneGeometry(1.08, 0.66, 18, 10);
const baseCardPositions = cardGeometry.attributes.position.array.slice();
const cardMaterial = new THREE.MeshPhysicalMaterial({
  color: 0xf4f1e8,
  roughness: 0.38,
  metalness: 0,
  clearcoat: 0.62,
  clearcoatRoughness: 0.18,
  side: THREE.DoubleSide
});
const card = new THREE.Mesh(cardGeometry, cardMaterial);
card.position.set(-1.35, 0.03, 0.18);
card.castShadow = true;
scene.add(card);

const cardText = createLabelSprite("Rechnung.pdf");
cardText.position.set(0, 0, 0.022);
card.add(cardText);

const portalGroup = new THREE.Group();
portalGroup.position.set(1.72, 0.14, 0.12);
portalGroup.rotation.y = -0.22;
scene.add(portalGroup);

const glassMaterial = new THREE.MeshPhysicalMaterial({
  color: 0xf6ffff,
  transmission: 1.0,
  thickness: 2.8,
  ior: 1.47,
  roughness: 0.015,
  metalness: 0.0,
  clearcoat: 1.0,
  clearcoatRoughness: 0.025,
  envMapIntensity: 2.8,
  transparent: true,
  opacity: 0.36,
  side: THREE.DoubleSide,
  depthWrite: false
});

const lensShell = new THREE.Mesh(new THREE.SphereGeometry(1.0, 128, 64), glassMaterial);
lensShell.scale.set(1.24, 0.82, 0.34);
lensShell.castShadow = true;
portalGroup.add(lensShell);

const rimMaterial = new THREE.MeshPhysicalMaterial({
  color: 0xf9ffff,
  transmission: 0.75,
  thickness: 1.8,
  ior: 1.5,
  roughness: 0.02,
  metalness: 0,
  clearcoat: 1,
  clearcoatRoughness: 0.02,
  envMapIntensity: 3.2,
  transparent: true,
  opacity: 0.62,
  side: THREE.DoubleSide
});
const outerRim = new THREE.Mesh(new THREE.TorusGeometry(0.87, 0.026, 18, 160), rimMaterial);
outerRim.scale.set(1.32, 0.86, 0.32);
portalGroup.add(outerRim);

const tunnelCurve = new THREE.CatmullRomCurve3([
  new THREE.Vector3(0.08, 0.0, 0.0),
  new THREE.Vector3(0.34, 0.0, -0.22),
  new THREE.Vector3(0.60, 0.02, -0.56),
  new THREE.Vector3(0.90, -0.02, -0.98)
]);
const tunnelMaterial = new THREE.MeshPhysicalMaterial({
  color: 0x0f1719,
  roughness: 0.18,
  metalness: 0.1,
  transmission: 0.24,
  thickness: 1.2,
  ior: 1.38,
  clearcoat: 0.8,
  envMapIntensity: 1.8,
  transparent: true,
  opacity: 0.72,
  side: THREE.DoubleSide
});
const tunnel = new THREE.Mesh(new THREE.TubeGeometry(tunnelCurve, 96, 0.44, 48, false), tunnelMaterial);
tunnel.rotation.z = Math.PI / 2;
portalGroup.add(tunnel);

const ringMaterial = new THREE.MeshBasicMaterial({
  color: 0xeaffff,
  transparent: true,
  opacity: 0.18,
  blending: THREE.AdditiveBlending,
  depthWrite: false
});
const rings = [];
for (let i = 0; i < 20; i++) {
  const ring = new THREE.Mesh(new THREE.TorusGeometry(0.82 - i * 0.026, 0.0045, 8, 128), ringMaterial.clone());
  ring.scale.set(1.18 - i * 0.018, 0.76 - i * 0.010, 0.22);
  ring.position.z = -i * 0.038;
  ring.material.opacity = 0.16 - i * 0.005;
  portalGroup.add(ring);
  rings.push(ring);
}

const throat = new THREE.Mesh(
  new THREE.CircleGeometry(0.34, 96),
  new THREE.MeshBasicMaterial({ color: 0x020304, transparent: true, opacity: 0.78, depthWrite: false })
);
throat.scale.set(1.5, 0.48, 1);
throat.position.z = 0.035;
portalGroup.add(throat);

const causticMaterial = new THREE.MeshBasicMaterial({
  color: 0xffffff,
  transparent: true,
  opacity: 0.24,
  blending: THREE.AdditiveBlending,
  depthWrite: false
});
const caustics = [];
for (let i = 0; i < 12; i++) {
  const curve = new THREE.CatmullRomCurve3([
    new THREE.Vector3(-0.62 + i * 0.10, -0.16 + Math.sin(i) * 0.08, 0.07),
    new THREE.Vector3(-0.22 + i * 0.045, 0.18 * Math.sin(i * 1.7), 0.10),
    new THREE.Vector3(0.45 - i * 0.025, 0.12 * Math.cos(i), 0.08)
  ]);
  const line = new THREE.Mesh(new THREE.TubeGeometry(curve, 20, 0.003, 6, false), causticMaterial.clone());
  line.material.opacity = 0.10 + (i % 4) * 0.03;
  portalGroup.add(line);
  caustics.push(line);
}

const state = {
  look: "hybrid",
  holding: false,
  target: new THREE.Vector3(-1.35, 0.03, 0.18),
  pointer: new THREE.Vector2(0, 0),
  velocity: new THREE.Vector3(),
  portalPull: 0,
  lastPosition: card.position.clone(),
  desktopStream: null
};

setLook("hybrid");
status.textContent = "Nimm das Ding und fuehre es langsam in die Linse.";

canvas.addEventListener("pointerdown", event => {
  const cardScreen = card.position.clone().project(camera);
  const sx = (cardScreen.x * 0.5 + 0.5) * window.innerWidth;
  const sy = (-cardScreen.y * 0.5 + 0.5) * window.innerHeight;
  const distance = Math.hypot(event.clientX - sx, event.clientY - sy);
  if (distance < 150) {
    state.holding = true;
    canvas.setPointerCapture(event.pointerId);
    canvas.classList.add("is-carrying");
    status.textContent = "In der Hand";
  }
});

canvas.addEventListener("pointermove", event => {
  state.pointer.set((event.clientX / window.innerWidth) * 2 - 1, -(event.clientY / window.innerHeight) * 2 + 1);
  if (!state.holding) {
    return;
  }

  state.target.copy(pointerToWorld(state.pointer));
});

canvas.addEventListener("pointerup", event => {
  if (!state.holding) {
    return;
  }

  state.holding = false;
  canvas.releasePointerCapture(event.pointerId);
  canvas.classList.remove("is-carrying");
  if (state.portalPull > 0.68) {
    status.textContent = "Im Durchgang";
    state.target.copy(portalGroup.position).add(new THREE.Vector3(-0.04, -0.03, 0.18));
  } else {
    status.textContent = "Abgelegt";
    state.target.z = 0.18;
  }
});

lookButtons.glass.addEventListener("click", () => setLook("glass"));
lookButtons.tunnel.addEventListener("click", () => setLook("tunnel"));
lookButtons.hybrid.addEventListener("click", () => setLook("hybrid"));
document.querySelector("#desktop-feed").addEventListener("click", enableDesktopFeed);

window.addEventListener("resize", () => {
  camera.aspect = window.innerWidth / window.innerHeight;
  camera.updateProjectionMatrix();
  renderer.setSize(window.innerWidth, window.innerHeight);
});

renderer.setAnimationLoop(render);

function render(timeMs) {
  const time = timeMs * 0.001;
  card.position.lerp(state.target, state.holding ? 0.16 : 0.075);
  state.velocity.copy(card.position).sub(state.lastPosition);
  state.lastPosition.copy(card.position);

  const toPortal = portalGroup.position.clone().sub(card.position);
  const distanceToPortal = toPortal.length();
  state.portalPull = THREE.MathUtils.clamp(1 - (distanceToPortal - 0.34) / 1.35, 0, 1);
  const pullEase = smoothstep(state.portalPull);

  if (state.holding) {
    const tiltX = THREE.MathUtils.clamp(-state.velocity.y * 5.0, -0.30, 0.30);
    const tiltY = THREE.MathUtils.clamp(state.velocity.x * 4.5, -0.34, 0.34);
    card.rotation.x = THREE.MathUtils.lerp(card.rotation.x, tiltX, 0.18);
    card.rotation.y = THREE.MathUtils.lerp(card.rotation.y, tiltY, 0.18);
    card.position.z = THREE.MathUtils.lerp(card.position.z, 0.44, 0.10);
    card.scale.setScalar(THREE.MathUtils.lerp(card.scale.x, 0.82, 0.09));
  } else {
    card.rotation.x = THREE.MathUtils.lerp(card.rotation.x, 0, 0.10);
    card.rotation.y = THREE.MathUtils.lerp(card.rotation.y, 0, 0.10);
    card.position.z = THREE.MathUtils.lerp(card.position.z, state.portalPull > 0.68 ? 0.24 : 0.18, 0.08);
    card.scale.setScalar(THREE.MathUtils.lerp(card.scale.x, state.portalPull > 0.68 ? 0.34 : 1.0, 0.06));
  }

  deformCardGeometry(pullEase);
  contactShadow.position.x = card.position.x + state.velocity.x * 2.8;
  contactShadow.position.y = -0.384;
  contactShadow.position.z = card.position.z - 0.16;
  contactShadow.scale.set(1 + card.position.z * 0.42, 1 + card.position.z * 0.26, 1);
  contactShadow.material.opacity = state.holding ? 0.20 + pullEase * 0.22 : 0.08;

  const open = state.holding ? 0.45 + pullEase * 0.55 : Math.max(0.18, pullEase);
  portalGroup.scale.setScalar(THREE.MathUtils.lerp(portalGroup.scale.x, 0.94 + open * 0.12, 0.06));
  lensShell.material.opacity = THREE.MathUtils.lerp(lensShell.material.opacity, lookOpacity(open), 0.06);
  throat.material.opacity = 0.38 + pullEase * 0.52;
  tunnel.rotation.z = Math.PI / 2 + time * 0.035;
  rings.forEach((ring, index) => {
    ring.rotation.z = time * (0.05 + index * 0.002);
    ring.material.opacity = (0.06 + open * 0.11) * (1 - index / rings.length);
  });
  caustics.forEach((line, index) => {
    line.rotation.z = Math.sin(time * 0.22 + index) * 0.08;
    line.material.opacity = (0.05 + open * 0.18) * (0.62 + Math.sin(time * 0.9 + index) * 0.28);
  });

  renderer.render(scene, camera);
}

function deformCardGeometry(portalPull) {
  const positions = cardGeometry.attributes.position.array;
  const portalLocal = card.worldToLocal(portalGroup.position.clone());
  const direction = new THREE.Vector2(portalLocal.x, portalLocal.y);
  if (direction.lengthSq() < 0.0001) {
    direction.set(1, 0);
  }
  direction.normalize();

  for (let i = 0; i < positions.length; i += 3) {
    const baseX = baseCardPositions[i];
    const baseY = baseCardPositions[i + 1];
    const edge = THREE.MathUtils.clamp(((baseX * direction.x + baseY * direction.y) + 0.54) / 1.08, 0, 1);
    const lead = smoothstep(edge) * portalPull;
    const squeeze = lead * lead;
    positions[i] = baseX - direction.x * squeeze * 0.28;
    positions[i + 1] = baseY - direction.y * squeeze * 0.18 - baseY * squeeze * 0.46;
    positions[i + 2] = baseCardPositions[i + 2] - lead * 0.18;
  }

  cardGeometry.attributes.position.needsUpdate = true;
  cardGeometry.computeVertexNormals();
}

function pointerToWorld(pointer) {
  const vector = new THREE.Vector3(pointer.x, pointer.y, 0.5).unproject(camera);
  const direction = vector.sub(camera.position).normalize();
  const distance = (0.20 - camera.position.z) / direction.z;
  const world = camera.position.clone().add(direction.multiplyScalar(distance));
  world.x = THREE.MathUtils.clamp(world.x, -3.2, 2.3);
  world.y = THREE.MathUtils.clamp(world.y, -1.45, 1.55);
  world.z = 0.28;
  return world;
}

function setLook(look) {
  state.look = look;
  Object.values(lookButtons).forEach(button => button.classList.remove("is-active"));
  lookButtons[look].classList.add("is-active");
  if (look === "glass") {
    glassMaterial.opacity = 0.46;
    glassMaterial.envMapIntensity = 3.6;
    tunnelMaterial.opacity = 0.34;
  } else if (look === "tunnel") {
    glassMaterial.opacity = 0.26;
    glassMaterial.envMapIntensity = 2.2;
    tunnelMaterial.opacity = 0.86;
  } else {
    glassMaterial.opacity = 0.36;
    glassMaterial.envMapIntensity = 2.8;
    tunnelMaterial.opacity = 0.72;
  }
}

function lookOpacity(open) {
  if (state.look === "glass") {
    return 0.28 + open * 0.24;
  }
  if (state.look === "tunnel") {
    return 0.16 + open * 0.18;
  }
  return 0.22 + open * 0.22;
}

async function enableDesktopFeed() {
  try {
    const stream = await navigator.mediaDevices.getDisplayMedia({ video: true, audio: false });
    const video = document.createElement("video");
    video.srcObject = stream;
    video.muted = true;
    video.playsInline = true;
    await video.play();
    const texture = new THREE.VideoTexture(video);
    texture.colorSpace = THREE.SRGBColorSpace;
    desktop.material.map = texture;
    desktop.material.needsUpdate = true;
    state.desktopStream = stream;
    status.textContent = "Desktop-Livebild aktiv";
  } catch {
    status.textContent = "Desktop-Livebild nicht aktiviert";
  }
}

function createDesktopTexture() {
  const c = document.createElement("canvas");
  c.width = 1920;
  c.height = 1080;
  const ctx = c.getContext("2d");
  const gradient = ctx.createLinearGradient(0, 0, c.width, c.height);
  gradient.addColorStop(0, "#d9e3e2");
  gradient.addColorStop(1, "#b9c8c7");
  ctx.fillStyle = gradient;
  ctx.fillRect(0, 0, c.width, c.height);
  ctx.strokeStyle = "rgba(58,70,72,0.16)";
  ctx.lineWidth = 2;
  for (let x = 0; x < c.width; x += 120) {
    ctx.beginPath();
    ctx.moveTo(x, 0);
    ctx.lineTo(x, c.height);
    ctx.stroke();
  }
  for (let y = 0; y < c.height; y += 120) {
    ctx.beginPath();
    ctx.moveTo(0, y);
    ctx.lineTo(c.width, y);
    ctx.stroke();
  }
  drawDesktopCard(ctx, 170, 160, "Textdokument");
  drawDesktopCard(ctx, 1420, 230, "Ablage");
  drawDesktopCard(ctx, 1180, 710, "Rechnung");
  return new THREE.CanvasTexture(c);
}

function drawDesktopCard(ctx, x, y, label) {
  ctx.fillStyle = "rgba(255,255,255,0.74)";
  ctx.strokeStyle = "rgba(30,40,42,0.16)";
  ctx.lineWidth = 2;
  ctx.roundRect(x, y, 185, 128, 12);
  ctx.fill();
  ctx.stroke();
  ctx.fillStyle = "rgba(28,37,40,0.72)";
  ctx.font = "600 24px Segoe UI";
  ctx.fillText(label, x + 20, y + 72);
}

function createContactShadowTexture() {
  const c = document.createElement("canvas");
  c.width = 512;
  c.height = 256;
  const ctx = c.getContext("2d");
  const gradient = ctx.createRadialGradient(256, 128, 12, 256, 128, 230);
  gradient.addColorStop(0, "rgba(0,0,0,0.55)");
  gradient.addColorStop(0.42, "rgba(0,0,0,0.23)");
  gradient.addColorStop(1, "rgba(0,0,0,0)");
  ctx.fillStyle = gradient;
  ctx.fillRect(0, 0, c.width, c.height);
  return new THREE.CanvasTexture(c);
}

function createLabelSprite(text) {
  const c = document.createElement("canvas");
  c.width = 512;
  c.height = 128;
  const ctx = c.getContext("2d");
  ctx.fillStyle = "#101719";
  ctx.font = "700 34px Segoe UI";
  ctx.textAlign = "center";
  ctx.textBaseline = "middle";
  ctx.fillText(text, 256, 66);
  const texture = new THREE.CanvasTexture(c);
  texture.colorSpace = THREE.SRGBColorSpace;
  const material = new THREE.SpriteMaterial({ map: texture, transparent: true });
  const sprite = new THREE.Sprite(material);
  sprite.scale.set(0.62, 0.16, 1);
  return sprite;
}

function smoothstep(value) {
  const x = THREE.MathUtils.clamp(value, 0, 1);
  return x * x * (3 - 2 * x);
}
