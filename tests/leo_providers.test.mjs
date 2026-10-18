import { test } from "node:test";
import assert from "node:assert/strict";
import { GeminiModelClient } from "../src/models/gemini_client.ts";
import { AIProviderManager } from "../src/models/ai_provider.ts";

test("GeminiModelClient initializes with default model and handles API key", () => {
  const client = new GeminiModelClient({ apiKey: "AIzaSyFakeKeyForTesting123456" });
  assert.equal(client.model, "gemini-2.5-flash");
  assert.equal(client.hasApiKey(), true);

  client.setModel("gemini-1.5-flash");
  assert.equal(client.model, "gemini-1.5-flash");

  client.clearApiKey();
  assert.equal(client.hasApiKey(), false);
});

test("GeminiModelClient strictly redacts API key from error strings", () => {
  const secretKey = "AIzaSySecretApiKeyToProtect98765";
  const client = new GeminiModelClient({ apiKey: secretKey });

  const leakAttempt = `Failed to contact endpoint: https://api.google.com?key=${secretKey}&model=gemini`;
  const sanitized = client.redactSecret(leakAttempt);

  assert.equal(sanitized.includes(secretKey), false);
  assert.equal(sanitized.includes("[REDACTED_API_KEY]"), true);
});

test("GeminiModelClient rejects without network when key is missing", async () => {
  const client = new GeminiModelClient({ apiKey: "" });
  const check = await client.testConnection();
  assert.equal(check.ok, false);
  assert.match(check.message, /No Gemini API key/i);

  await assert.rejects(
    async () => {
      await client.streamChat([{ role: "user", parts: [{ text: "hi" }] }], () => {});
    },
    /No Gemini API key/i
  );
});

test("AIProviderManager classifies egress boundaries correctly", () => {
  const manager = new AIProviderManager("ollama");
  assert.equal(manager.isLocal, true);
  assert.equal(manager.egressLabel, "Local");

  manager.setProvider("lmstudio");
  assert.equal(manager.isLocal, true);
  assert.equal(manager.egressLabel, "Local");

  manager.setProvider("offline");
  assert.equal(manager.isLocal, true);
  assert.equal(manager.egressLabel, "Local");

  manager.setProvider("gemini");
  assert.equal(manager.isLocal, false);
  assert.equal(manager.egressLabel, "Leaves your PC");
});

test("AIProviderManager blocks all cloud egress in offline mode", async () => {
  const manager = new AIProviderManager("offline");
  await assert.rejects(
    async () => {
      await manager.streamChat("Test query", [], () => {});
    },
    /Offline Mode is active/i
  );
});

test("AIProviderManager records usage statistics", async () => {
  const manager = new AIProviderManager("ollama");
  const statsBefore = manager.getUsage("ollama");
  assert.equal(statsBefore.totalRequests, 0);

  // Mock stream chat failure because loopback is not actively hosting
  try {
    await manager.streamChat("Hello", [], () => {});
  } catch {
    // Expected if no local ollama is running
  }

  const statsAfter = manager.getUsage("ollama");
  assert.equal(statsAfter.totalRequests, 1);
  assert.ok(statsAfter.lastUsedTimestamp);
});
