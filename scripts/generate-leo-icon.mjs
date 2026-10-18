#!/usr/bin/env node
/**
 * Leo Icon Generator — circular app icon captured from a real frame of the
 * Leo facial animation engine (src/companion/leo_face.ts).
 *
 * Reproduces the exact idle-frame geometry the capsule renders in the app
 * (face canvas is 120x96 logical units):
 *   - mood "idle" at t < 2.5s: eyes fully open (blink scale = 1), look (0,0)
 *   - eye ellipses rx=9.5, ry=15.5 at x=40/80, y=48  (cream #FAF5FF)
 *   - warm copper glow radius 24 behind each eye     (rgba(249,115,22,*))
 * Cute-ified for the icon only: a small smile mouth stroke at (60, 63),
 * radius 7, thickness 2.4 (cream), and a soft blush tint under the eyes.
 * The cyan specular dot from the in-app frame is intentionally omitted on
 * small icons; at 16-32px it read as a stray blue dot instead of a highlight.
 * mounted on a deep-charcoal circular disc with a copper accretion rim that
 * echoes the capsule border and the black-hole brand icon.
 *
 * Pure Node (built-in zlib only). Deterministic per-pixel rendering with
 * smoothstep edge falloffs, then area-averaged downsamples for AA quality.
 *
 * Outputs:
 *   assets/brand/leo-face-icon-1024.png   (canonical master)
 *   app/src-tauri/icons/32x32.png
 *   app/src-tauri/icons/128x128.png
 *   app/src-tauri/icons/128x128@2x.png    (256)
 *   app/src-tauri/icons/icon.png          (512)
 *   app/src-tauri/icons/icon.ico          (256/128/64/48/32/16 PNG-encoded)
 *   app/public/favicon.png                (256)
 */

import { deflateSync } from "node:zlib";
import { writeFileSync, mkdirSync } from "node:fs";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const ROOT = resolve(dirname(fileURLToPath(import.meta.url)), "..");
// Overridable output root so tests can regenerate into a scratch dir without
// touching the blessed assets (byte-identical output proves determinism).
const OUTPUT_ROOT = process.env.LEO_ICON_OUTPUT_ROOT
  ? resolve(process.env.LEO_ICON_OUTPUT_ROOT)
  : ROOT;

/* ------------------------------------------------------------------ */
/* Frame geometry & palette (face-canvas logical units)                */
/* ------------------------------------------------------------------ */

const FRAME = {
  face: { cx: 60, cy: 48 },
  eyeL: { x: 40, y: 48 },
  eyeR: { x: 80, y: 48 },
  eyeRx: 9.5,
  eyeRy: 15.5,
  look: { x: 0, y: 0 }, // idle look vector
  glowR: 24,
  glowAlpha: 0.42, // idle 0.30, modestly boosted for icon presence
  // Cute icon-only additions:
  mouth: { x: 60, y: 63, r: 7, thick: 2.4 }, // smile arc (cream)
  blush: { dl: 34, y: 59, rx: 6, ry: 4, alpha: 0.22 }, // cheeks beside/below eyes
};

const PALETTE = {
  cream: [250, 245, 255],
  blush: [249, 113, 122],
  copper: [249, 115, 22],
  charcoal: [21, 23, 31], // capsule gradient light end
  void: [10, 11, 16], // capsule gradient dark end, lifted for visibility
};

/* ------------------------------------------------------------------ */
/* Math helpers                                                        */
/* ------------------------------------------------------------------ */

function smoothstep(a, b, x) {
  const t = Math.min(1, Math.max(0, (x - a) / (b - a)));
  return t * t * (3 - 2 * t);
}
function lerp(a, b, t) {
  return a + (b - a) * t;
}

/* ------------------------------------------------------------------ */
/* Per-pixel renderer (master resolution)                              */
/* ------------------------------------------------------------------ */

function renderMaster(size = 1024) {
  const data = new Float64Array(size * size * 4); // straight-alpha float

  const discC = size / 2;
  const discY = size * (506 / 1024); // eyes at optical centre of disc
  const R = size * (500 / 1024); // 12px transparent rim padding
  const s = size / (FRAME.face.cx * 2 * 1.015); // face-wide mapping
  const eyeY = discY + FRAME.look.y * 5 * s;
  const eyeXL = discC - 20 * s + FRAME.look.x * 7 * s;
  const eyeXR = discC + 20 * s + FRAME.look.x * 7 * s;
  const eRx = FRAME.eyeRx * s;
  const eRy = FRAME.eyeRy * s;
  const glowR = FRAME.glowR * s;

  const chi = PALETTE.charcoal;
  const voi = PALETTE.void;
  const cop = PALETTE.copper;
  const crm = PALETTE.cream;
  const blu = PALETTE.blush;

  // Cute face geometry (icon-only warmth, built on the same face mapping).
  const mth = FRAME.mouth;
  const mX = discC + (mth.x - FRAME.face.cx) * s;
  const mY = discY + (mth.y - FRAME.face.cy) * s;
  const mR = mth.r * s;
  const mHalf = (mth.thick * 0.5) * s;
  const bXl = discC - FRAME.blush.dl * s;
  const bXr = discC + FRAME.blush.dl * s;
  const bY = discY + (FRAME.blush.y - FRAME.face.cy) * s;
  const bRx = FRAME.blush.rx * s;
  const bRy = FRAME.blush.ry * s;
  const blushA = FRAME.blush.alpha;

  const gi = 1 / (glowR * glowR);
  const ei = 1 / (eRy * eRy);

  for (let y = 0; y < size; y++) {
    const py = y + 0.5;
    for (let x = 0; x < size; x++) {
      const px = x + 0.5;
      const dx = px - discC;
      const dy = py - discY;
      const d2 = dx * dx + dy * dy;
      const d = Math.sqrt(d2);

      // Anti-aliased disc coverage.
      const cover = 1 - smoothstep(R - 1.6, R + 1.6, d);
      if (cover <= 0) continue;
      const o = (y * size + x) * 4;

      // Base disc: capsule-style charcoal, lit from top-left, dark centre.
      const lg = Math.min(1, (px + py) / (R * 1.9));
      let r = lerp(chi[0], voi[0], 0.15 + 0.8 * lg);
      let g = lerp(chi[1], voi[1], 0.15 + 0.8 * lg);
      let b = lerp(chi[2], voi[2], 0.15 + 0.8 * lg);

      // Black-hole core vignette: darker toward centre, bright toward rim.
      const rimGlow = smoothstep(0.18, 0.95, d / R);
      const mul = 0.5 + 0.5 * rimGlow;
      r *= mul;
      g *= mul;
      b *= mul;

      // Warm aura inside the rim (echo of the glow flares at eye level).
      const aura = smoothstep(R * 0.75, R * 0.97, d) * 0.18;
      r += cop[0] * aura;
      g += cop[1] * aura;
      b += cop[2] * aura;

      // Copper accretion rim ring + bright thin edge line.
      const ringPos = (d - R * 0.925) / (R * 0.035);
      const ring = Math.exp(-ringPos * ringPos) * 0.85;
      const edge =
        smoothstep(R * 0.955, R * 0.972, d) * (1 - smoothstep(R * 0.985, R, d)) * 1.15;
      const rim = ring + edge;
      r += cop[0] * rim;
      g += cop[1] * rim;
      b += cop[2] * rim;

      // Warm copper glow behind each eye (as rendered in app, idle=0.30).
      const dl2 = (px - eyeXL) * (px - eyeXL) + (py - eyeY) * (py - eyeY);
      const dr2 = (px - eyeXR) * (px - eyeXR) + (py - eyeY) * (py - eyeY);
      const gl = Math.max(0, 1 - dl2 * gi);
      const gr = Math.max(0, 1 - dr2 * gi);
      const glowA = FRAME.glowAlpha * (gl * gl + gr * gr);
      r += cop[0] * glowA;
      g += cop[1] * glowA;
      b += cop[2] * glowA;

      // Eyes: luminous cream ellipses with soft AA edges.
      const dqL = ((px - eyeXL) / eRx) ** 2 + ((py - eyeY) / eRy) ** 2;
      const dqR = ((px - eyeXR) / eRx) ** 2 + ((py - eyeY) / eRy) ** 2;
      const qe = Math.min(Math.sqrt(dqL), Math.sqrt(dqR));
      const eyeCov = 1 - smoothstep(0.92, 1.05, qe);
      r = r * (1 - eyeCov) + crm[0] * eyeCov;
      g = g * (1 - eyeCov) + crm[1] * eyeCov;
      b = b * (1 - eyeCov) + crm[2] * eyeCov;

      // Soft blush ellipses under the eyes (cute cheek tint).
      const dbl = ((px - bXl) / bRx) ** 2 + ((py - bY) / bRy) ** 2;
      const dbr = ((px - bXr) / bRx) ** 2 + ((py - bY) / bRy) ** 2;
      const qc = Math.min(Math.sqrt(dbl), Math.sqrt(dbr));
      const blushCov = (1 - smoothstep(0.7, 1.05, qc)) * blushA;
      r = r * (1 - blushCov) + blu[0] * blushCov;
      g = g * (1 - blushCov) + blu[1] * blushCov;
      b = b * (1 - blushCov) + blu[2] * blushCov;

      // Cute little smile: a stroked lower arc centred above the mouth line.
      const mdx = px - mX;
      const mdyRaw = py - mY;
      const mdy = mdyRaw - mR; // distance to the arc's centre point
      const md = Math.sqrt(mdx * mdx + mdy * mdy);
      // Only the lower semicircle (mdyRaw >= mY) forms the smile; ends taper.
      const mAngle = Math.atan2(mdyRaw, mdx); // 0..PI on the lower arc
      const tEnd = 1 - smoothstep(0.7, 0.95, Math.abs(mAngle - Math.PI / 2) / (Math.PI / 2));
      const mouthBand = 1 - smoothstep(mHalf * 0.75, mHalf * 1.1, Math.abs(md - mR));
      // mdyRaw is the offset BELOW the mouth line; only the lower arc smiles.
      const mouthCov = mdyRaw >= 0 ? mouthBand * tEnd : 0;
      r = r * (1 - mouthCov) + crm[0] * mouthCov;
      g = g * (1 - mouthCov) + crm[1] * mouthCov;
      b = b * (1 - mouthCov) + crm[2] * mouthCov;

      data[o] = r;
      data[o + 1] = g;
      data[o + 2] = b;
      data[o + 3] = cover * 255;
    }
  }
  return data;
}

/* ------------------------------------------------------------------ */
/* Area-averaged downsample (premultiplied accumulation)               */
/* ------------------------------------------------------------------ */

function resize(master, fromSize, toSize) {
  const data = new Uint8ClampedArray(toSize * toSize * 4);
  const scale = fromSize / toSize;
  for (let ty = 0; ty < toSize; ty++) {
    const y0 = Math.floor(ty * scale);
    const y1 = Math.max(Math.ceil((ty + 1) * scale), y0 + 1);
    for (let tx = 0; tx < toSize; tx++) {
      const x0 = Math.floor(tx * scale);
      const x1 = Math.max(Math.ceil((tx + 1) * scale), x0 + 1);
      let ar = 0;
      let ag = 0;
      let ab = 0;
      let aa = 0;
      let n = 0;
      for (let y = y0; y < y1; y++) {
        for (let x = x0; x < x1; x++) {
          const o = (y * fromSize + x) * 4;
          const a = master[o + 3] / 255;
          ar += master[o] * a;
          ag += master[o + 1] * a;
          ab += master[o + 2] * a;
          aa += a;
          n++;
        }
      }
      const t = (ty * toSize + tx) * 4;
      if (aa > 0.0001) {
        data[t] = Math.round(ar / aa);
        data[t + 1] = Math.round(ag / aa);
        data[t + 2] = Math.round(ab / aa);
      }
      data[t + 3] = Math.round((aa / n) * 255);
    }
  }
  return data;
}

/* ------------------------------------------------------------------ */
/* PNG encoder (8-bit RGBA, filter 0, built-in zlib)                   */
/* ------------------------------------------------------------------ */

const CRC_TABLE = new Uint32Array(256);
for (let n = 0; n < 256; n++) {
  let c = n;
  for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
  CRC_TABLE[n] = c >>> 0;
}
function crc32(buf) {
  let c = 0xffffffff;
  for (let i = 0; i < buf.length; i++) c = CRC_TABLE[(c ^ buf[i]) & 0xff] ^ (c >>> 8);
  return (c ^ 0xffffffff) >>> 0;
}

function pngChunk(type, data) {
  const out = Buffer.alloc(12 + data.length);
  out.writeUInt32BE(data.length, 0);
  out.write(type, 4, "ascii");
  data.copy(out, 8);
  out.writeUInt32BE(crc32(out.subarray(4, 8 + data.length)), 8 + data.length);
  return out;
}

function encodePNG(rgba, w, h) {
  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(w, 0);
  ihdr.writeUInt32BE(h, 4);
  ihdr[8] = 8; // bit depth
  ihdr[9] = 6; // colour type RGBA
  const raw = Buffer.alloc(h * (w * 4 + 1));
  for (let y = 0; y < h; y++) {
    raw[y * (w * 4 + 1)] = 0; // filter: none
    Buffer.from(rgba.buffer, rgba.byteOffset + y * w * 4, w * 4).copy(
      raw,
      y * (w * 4 + 1) + 1
    );
  }
  return Buffer.concat([
    Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]),
    pngChunk("IHDR", ihdr),
    pngChunk("IDAT", deflateSync(raw, { level: 9 })),
    pngChunk("IEND", Buffer.alloc(0)),
  ]);
}

/* ------------------------------------------------------------------ */
/* ICO writer (PNG-compressed entries, all sizes <= 256)               */
/* ------------------------------------------------------------------ */

function writeICO(sizes, pngs) {
  const count = sizes.length;
  const header = Buffer.alloc(6);
  header.writeUInt16LE(0, 0);
  header.writeUInt16LE(1, 2);
  header.writeUInt16LE(count, 4);
  const dir = Buffer.alloc(16 * count);
  let offset = 6 + 16 * count;
  sizes.forEach((s, i) => {
    const e = i * 16;
    dir[e] = s >= 256 ? 0 : s;
    dir[e + 1] = s >= 256 ? 0 : s;
    dir[e + 2] = 0;
    dir[e + 3] = 0;
    dir.writeUInt16LE(1, e + 4);
    dir.writeUInt16LE(32, e + 6);
    dir.writeUInt32LE(pngs[i].length, e + 8);
    dir.writeUInt32LE(offset, e + 12);
    offset += pngs[i].length;
  });
  return Buffer.concat([header, dir, ...pngs]);
}

/* ------------------------------------------------------------------ */
/* Pipeline                                                            */
/* ------------------------------------------------------------------ */

console.log("Rendering Leo idle-frame icon at 1024x1024...");
const t0 = performance.now();
const master = renderMaster(1024);
console.log(`  rendered in ${(performance.now() - t0).toFixed(0)} ms`);

function masterToPNG(size) {
  return encodePNG(resize(master, 1024, size), size, size);
}

const outputs = [
  ["assets/brand/leo-face-icon-1024.png", [masterToPNG(1024)]],
  ["app/src-tauri/icons/icon.png", [masterToPNG(512)]],
  ["app/src-tauri/icons/128x128@2x.png", [masterToPNG(256)]],
  ["app/src-tauri/icons/128x128.png", [masterToPNG(128)]],
  ["app/src-tauri/icons/32x32.png", [masterToPNG(32)]],
  ["app/public/favicon.png", [masterToPNG(256)]],
];

const icoSizes = [256, 128, 64, 48, 32, 16];
outputs.push([
  "app/src-tauri/icons/icon.ico",
  [writeICO(icoSizes, icoSizes.map(masterToPNG))],
]);

for (const [rel, buffers] of outputs) {
  const abs = resolve(OUTPUT_ROOT, rel);
  mkdirSync(dirname(abs), { recursive: true });
  writeFileSync(abs, buffers[0]);
  console.log(`  wrote ${rel} (${(buffers[0].length / 1024).toFixed(1)} KB)`);
}
console.log("Done.");
