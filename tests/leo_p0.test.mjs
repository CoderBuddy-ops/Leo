import test from "node:test";
import assert from "node:assert/strict";
import { LeoCharacterEngine, createMulberry32 } from "../src/companion/leo_face.ts";
import { SpeechCleanupFilter } from "../src/voice/speech_cleanup.ts";
import { LocalModelClient } from "../src/models/local_client.ts";

test("Leo Character Engine initializes with valid resting physics", () => {
  const engine = new LeoCharacterEngine(12345);
  assert.equal(engine.currentMood, "idle");
  assert.equal(engine.val.open_l, 1.0);
  assert.equal(engine.val.look_x, 0.0);
  assert.equal(engine.val.glow, 0.3);
});

test("Leo Character Engine smoothly transitions state under spring dynamics", () => {
  const engine = new LeoCharacterEngine(42);
  engine.setMood("thinking");
  assert.equal(engine.currentMood, "thinking");

  // Advance several physics frames
  for (let i = 0; i < 20; i++) {
    engine.advance(0.016);
  }

  // Value should move organically towards target (look_x = 0.65)
  assert.ok(engine.val.look_x > 0.1, `Expected look_x to advance towards 0.65, got ${engine.val.look_x}`);
});

test("Mulberry32 PRNG produces deterministic golden vectors in [0, 1)", () => {
  const rng1 = createMulberry32(42);
  const rng2 = createMulberry32(42);

  // Test pair-wise equality
  assert.equal(rng1(), rng2());

  // Test exact deterministic golden vector values for seed 42
  const golden = [0.44829055899754167, 0.8524657934904099, 0.6697340414393693];
  for (const expected of golden) {
    const val = rng1();
    assert.ok(Math.abs(val - expected) < 1e-12, `Expected ${expected}, got ${val}`);
    assert.ok(val >= 0 && val < 1.0);
  }
});

test("Speech cleanup filter removes verbal filler words (ums, uhs)", () => {
  const raw = "Hi Priya, um, the launch is on track for Thursday. Uh, I will send notes.";
  const cleaned = SpeechCleanupFilter.clean(raw);
  assert.equal(cleaned, "Hi Priya, the launch is on track for Thursday. I will send notes.");
});

test("Speech cleanup filter resolves verbal self-corrections", () => {
  const raw = "The meeting is on Thursday, actually Friday.";
  const cleaned = SpeechCleanupFilter.clean(raw);
  assert.equal(cleaned, "The meeting is on Friday.");
});

test("Speech cleanup filter formats spoken bullet points into markdown lists", () => {
  const raw = "Three priorities for Monday. {Bullet point} ship beta {Bullet point} fix bugs";
  const cleaned = SpeechCleanupFilter.clean(raw);
  assert.ok(cleaned.includes("- Ship beta"));
  assert.ok(cleaned.includes("- Fix bugs"));
});

test("LocalModelClient detects loopback addresses correctly", () => {
  assert.equal(LocalModelClient.isLoopback("http://127.0.0.1:11434/v1"), true);
  assert.equal(LocalModelClient.isLoopback("http://localhost:1234/v1"), true);
  assert.equal(LocalModelClient.isLoopback("https://api.anthropic.com/v1"), false);
  assert.equal(LocalModelClient.isLoopback("https://api.openai.com/v1"), false);
});
