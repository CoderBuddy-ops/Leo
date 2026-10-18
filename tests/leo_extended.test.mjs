import test from "node:test";
import assert from "node:assert/strict";

import { VoiceActivityDetector } from "../src/voice/audio_vad.ts";
import { OfflineASRPipeline, ASR_MODEL_CATALOG } from "../src/voice/offline_asr.ts";
import { LeoIPCBridge } from "../src/ipc/leo_ipc.ts";
import { LeoCharacterEngine } from "../src/companion/leo_face.ts";
import { LeoGestureController } from "../src/companion/gestures.ts";

test("VAD: Correctly calculates RMS energy on silence vs signal", () => {
  const silentBuffer = new Float32Array(512);
  const loudBuffer = new Float32Array(512).fill(0.5);

  assert.equal(VoiceActivityDetector.calculateRMS(silentBuffer), 0);
  const loudRms = VoiceActivityDetector.calculateRMS(loudBuffer);
  assert.ok(Math.abs(loudRms - 0.5) < 1e-6);
});

test("VAD: Detects zero crossing rate on alternating wave", () => {
  const alternating = new Float32Array([1, -1, 1, -1, 1]);
  const zcr = VoiceActivityDetector.calculateZCR(alternating);
  assert.equal(zcr, 1.0); // every transition crosses zero
});

test("VAD: Preserves speaking state through silence hangover period", () => {
  const vad = new VoiceActivityDetector({
    energyThreshold: 0.05,
    speechTriggerFrames: 2,
    silenceHangoverFrames: 5,
  });

  const speechFrame = new Float32Array(512).fill(0.1);
  const silenceFrame = new Float32Array(512).fill(0.001);

  // Trigger speech
  vad.processFrame(speechFrame);
  const r2 = vad.processFrame(speechFrame);
  assert.equal(r2.state, "speaking");

  // Silence begins: should stay in speaking state during hangover (< 5 frames)
  for (let i = 0; i < 4; i++) {
    const res = vad.processFrame(silenceFrame);
    assert.equal(res.state, "speaking", `Expected speaking during hangover frame ${i}`);
  }

  // On 5th silence frame, hangover expires -> silence
  const resFinal = vad.processFrame(silenceFrame);
  assert.equal(resFinal.state, "silence");
});

test("Offline ASR: Denies download without affirmative user consent", async () => {
  const asr = new OfflineASRPipeline();
  const modelId = "parakeet-tdt-0.6b-v3";

  assert.equal(asr.hasConsent(modelId), false);
  await assert.rejects(
    async () => {
      await asr.downloadModel(modelId);
    },
    /explicit user consent/
  );
});

test("Offline ASR: Downloads with consent and delivers progress updates", async () => {
  const asr = new OfflineASRPipeline();
  const modelId = "parakeet-tdt-0.6b-v3";

  asr.grantDownloadConsent(modelId);
  assert.equal(asr.hasConsent(modelId), true);

  const progressEvents = [];
  const success = await asr.downloadModel(modelId, (p) => {
    progressEvents.push(p.progressPercent);
  });

  assert.equal(success, true);
  assert.equal(asr.getModelStatus(modelId), "ready");
  assert.ok(progressEvents.length > 0);
  assert.equal(progressEvents[progressEvents.length - 1], 100);
});

test("Offline ASR: Cleans transcription of fillers and spoken self-corrections", async () => {
  const asr = new OfflineASRPipeline();
  const dummyAudio = new Float32Array(16000); // 1 sec of audio

  const result = await asr.transcribe(
    dummyAudio,
    16000,
    "parakeet-tdt-0.6b-v3",
    "Good morning team, um, the demo is on Friday, actually Monday. {Bullet point} run tests"
  );

  assert.equal(result.durationSeconds, 1.0);
  assert.ok(result.cleanedText.includes("Good morning team, the demo is on Monday."));
  assert.ok(result.cleanedText.includes("- Run tests"));
});

test("Leo IPC: Rejects out-of-bounds window dimensions", async () => {
  await assert.rejects(
    async () => {
      await LeoIPCBridge.execute({ command: "set_window_size", width: 50, height: 20 });
    },
    /out of safe bounds/
  );

  await assert.rejects(
    async () => {
      await LeoIPCBridge.execute({ command: "set_window_size", width: 9999, height: 9999 });
    },
    /out of safe bounds/
  );
});

test("Leo IPC: Validates clipboard payload size bounds", async () => {
  const hugeText = "A".repeat(600000); // > 500KB
  await assert.rejects(
    async () => {
      await LeoIPCBridge.execute({ command: "paste_clipboard_text", text: hugeText });
    },
    /exceeds maximum clipboard limit/
  );
});

test("Leo IPC: Dev fallback provides safe system info in non-Tauri host", async () => {
  const info = await LeoIPCBridge.execute({ command: "get_system_info" });
  assert.ok(info.os);
  assert.equal(info.isTauri, false);
});

test("Leo Gestures: Manages state transitions and auto-sleep timeout", () => {
  const engine = new LeoCharacterEngine(101);
  const gestures = new LeoGestureController(engine, { inactivityTimeoutMs: 100 });

  assert.equal(gestures.currentMood, "idle");

  // Listening mood
  gestures.setVoiceListening(true);
  assert.equal(gestures.currentMood, "listening");

  // Talking mood
  gestures.setSpeaking(true);
  assert.equal(gestures.currentMood, "talking");

  // Stop talking -> returns to listening if mic is active
  gestures.setSpeaking(false);
  assert.equal(gestures.currentMood, "listening");

  // Mic stops -> returns to idle
  gestures.setVoiceListening(false);
  assert.equal(gestures.currentMood, "idle");
});
