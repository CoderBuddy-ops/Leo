/**
 * Icon asset integrity tests for the generated Leo face-frame icon set.
 *
 * Validates that `scripts/generate-leo-icon.mjs` output is structurally
 * correct so the Tauri native bundle and favicon never receive malformed
 * assets. Pure structural checks (no PNG decode dependency needed).
 */

import { test } from "node:test";
import assert from "node:assert/strict";
import { readFileSync, existsSync } from "node:fs";
import { resolve, dirname } from "node:path";
import { fileURLToPath } from "node:url";

const ROOT = resolve(dirname(fileURLToPath(import.meta.url)), "..");

const PNG_SIZES = {
  "assets/brand/leo-face-icon-1024.png": 1024,
  "app/src-tauri/icons/icon.png": 512,
  "app/src-tauri/icons/128x128@2x.png": 256,
  "app/src-tauri/icons/128x128.png": 128,
  "app/src-tauri/icons/32x32.png": 32,
  "app/public/favicon.png": 256,
};

const ICO_SIZES = [256, 128, 64, 48, 32, 16];

const PNG_SIGNATURE = Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]);

/** Read IHDR width/height out of a PNG blob. */
function pngDimensions(buf) {
  assert.ok(buf.length > 24, "PNG too short to contain IHDR");
  const ihdrType = buf.readUInt32BE(12);
  assert.equal(ihdrType, 0x49484452, "first chunk is not IHDR");
  return { width: buf.readUInt32BE(16), height: buf.readUInt32BE(20), colorType: buf[25] };
}

for (const [rel, expected] of Object.entries(PNG_SIZES)) {
  test(`icon asset ${rel} is a valid ${expected}x${expected} RGBA PNG`, () => {
    const abs = resolve(ROOT, rel);
    assert.ok(existsSync(abs), "missing generated asset");
    const buf = readFileSync(abs);
    assert.deepEqual(buf.subarray(0, 8), PNG_SIGNATURE, "bad PNG signature");
    const dim = pngDimensions(buf);
    assert.equal(dim.width, expected, "IHDR width");
    assert.equal(dim.height, expected, "IHDR height");
    assert.equal(dim.colorType, 6, "expected RGBA colour type");
  });
}

test("icon.ico has valid ICO header with all bundled sizes", () => {
  const buf = readFileSync(resolve(ROOT, "app/src-tauri/icons/icon.ico"));
  assert.equal(buf.readUInt16LE(0), 0, "reserved field must be 0");
  assert.equal(buf.readUInt16LE(2), 1, "type field must be 1 (icon)");
  const count = buf.readUInt16LE(4);
  assert.equal(count, ICO_SIZES.length, "unexpected entry count");

  let offset = 6 + 16 * count;
  const seen = new Set();
  for (let i = 0; i < count; i++) {
    const e = 6 + i * 16;
    const w = buf[e] === 0 ? 256 : buf[e];
    const h = buf[e + 1] === 0 ? 256 : buf[e + 1];
    assert.equal(w, h, "entry is square");
    assert.ok(ICO_SIZES.includes(w), `unexpected ICO size ${w}`);
    assert.ok(!seen.has(w), "duplicate ICO size");
    seen.add(w);
    const dataSize = buf.readUInt32LE(e + 8);
    const dataOffset = buf.readUInt32LE(e + 12);
    assert.equal(dataOffset, offset, `entry ${w} offset implausible`);
    assert.ok(dataSize > 66, `entry ${w} payload too small for a PNG`);
    // PNG-compressed entries (Vista+) must start with the PNG signature.
    assert.deepEqual(buf.subarray(dataOffset, dataOffset + 8), PNG_SIGNATURE);
    offset += dataSize;
  }
  assert.equal(offset, buf.length, "ICO bytes fully covered by entries");
  for (const s of ICO_SIZES) assert.ok(seen.has(s), `missing ICO size ${s}`);
});

test("regenerated icon set is byte-identical to committed assets (deterministic)", async () => {
  const { mkdtempSync, readdirSync, statSync } = await import("node:fs");
  const { execFile } = await import("node:child_process");
  const tmp = mkdtempSync(resolve(ROOT, "tests", ".icon-sink-"));
  try {
    const started = Date.now();
    await new Promise((res, rej) =>
      execFile(
        process.execPath,
        [resolve(ROOT, "scripts/generate-leo-icon.mjs")],
        { env: { ...process.env, LEO_ICON_OUTPUT_ROOT: tmp } },
        (err) => (err ? rej(err) : res())
      )
    );
    assert.ok(Date.now() - started < 20000, "regeneration must finish quickly");
    for (const rel of Object.keys(PNG_SIZES).concat("app/src-tauri/icons/icon.ico")) {
      const committed = readFileSync(resolve(ROOT, rel));
      const regen = readFileSync(resolve(tmp, rel));
      assert.ok(
        committed.equals(regen),
        `${rel} has drifted from the deterministic renderer output`
      );
    }
    assert.equal(readdirSync(resolve(tmp, "app/src-tauri/icons")).length, 5, "tauri icon set complete");
    assert.ok(statSync(resolve(tmp, "assets/brand/leo-face-icon-1024.png")).size > 1000);
  } finally {
    // Best-effort cleanup of the scratch sink.
    import("node:fs").then(({ rmSync }) => rmSync(tmp, { recursive: true, force: true }));
  }
});
