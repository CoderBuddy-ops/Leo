import { LeoCharacterEngine } from "../../src/companion/leo_face.ts";
import { LeoGestureController } from "../../src/companion/gestures.ts";
import { VoiceActivityDetector } from "../../src/voice/audio_vad.ts";
import { OfflineASRPipeline } from "../../src/voice/offline_asr.ts";
import { SpeechCleanupFilter } from "../../src/voice/speech_cleanup.ts";
import { AIProviderManager, type ProviderType } from "../../src/models/ai_provider.ts";
import { LeoIPCBridge } from "../../src/ipc/leo_ipc.ts";

// DOM Element Handles
const capsule = document.getElementById("capsule") as HTMLDivElement;
const canvas = document.getElementById("leo-canvas") as HTMLCanvasElement;
const context = canvas?.getContext("2d");
const faceContainer = document.getElementById("face-container");
const egressBadge = document.getElementById("egress-badge") as HTMLSpanElement;
const egressText = document.getElementById("egress-text") as HTMLSpanElement;
const statusSub = document.getElementById("status-sub") as HTMLElement;
const tabButtons = document.querySelectorAll<HTMLButtonElement>(".tab-btn");
const drawerBodies = document.querySelectorAll<HTMLDivElement>(".drawer-body");

const micBtn = document.getElementById("mic-btn") as HTMLButtonElement;
const levelBar = document.getElementById("level-bar") as HTMLDivElement;
const transcriptBox = document.getElementById("transcript-box") as HTMLDivElement;
const cleanSampleBtn = document.getElementById("clean-sample-btn") as HTMLButtonElement;
const copyBtn = document.getElementById("copy-btn") as HTMLButtonElement;
const pasteBtn = document.getElementById("paste-btn") as HTMLButtonElement;

const chatOutput = document.getElementById("chat-output") as HTMLDivElement;
const chatPrompt = document.getElementById("chat-prompt") as HTMLInputElement;
const sendChatBtn = document.getElementById("send-chat-btn") as HTMLButtonElement;
const cancelChatBtn = document.getElementById("cancel-chat-btn") as HTMLButtonElement;

const modelSelect = document.getElementById("model-provider-select") as HTMLSelectElement;
const geminiSettingsCard = document.getElementById("gemini-settings-card") as HTMLDivElement;
const geminiApiKeyInput = document.getElementById("gemini-api-key") as HTMLInputElement;
const toggleKeyVisibilityBtn = document.getElementById("toggle-key-visibility") as HTMLButtonElement;
const geminiModelSelect = document.getElementById("gemini-model-select") as HTMLSelectElement;
const saveGeminiKeyBtn = document.getElementById("save-gemini-key-btn") as HTMLButtonElement;
const testGeminiBtn = document.getElementById("test-gemini-btn") as HTMLButtonElement;
const clearGeminiKeyBtn = document.getElementById("clear-gemini-key-btn") as HTMLButtonElement;
const geminiStatusText = document.getElementById("gemini-status-text") as HTMLDivElement;

const usageActiveModel = document.getElementById("usage-active-model") as HTMLSpanElement;
const usageQueriesCount = document.getElementById("usage-queries-count") as HTMLSpanElement;
const usageTokensCount = document.getElementById("usage-tokens-count") as HTMLSpanElement;

const downloadAsrBtn = document.getElementById("download-asr-btn") as HTMLButtonElement;
const cancelAsrBtn = document.getElementById("cancel-asr-btn") as HTMLButtonElement;
const asrStatusText = document.getElementById("asr-status-text") as HTMLSpanElement;
const asrProgressFill = document.getElementById("asr-progress-fill") as HTMLDivElement;

// Initialize Stored Credentials & Provider Manager
const KEY_STORAGE_NAME = "leo_gemini_api_key_v1";
const PROVIDER_STORAGE_NAME = "leo_provider_type_v1";
const storedKey = localStorage.getItem(KEY_STORAGE_NAME) || "";
const storedProvider = (localStorage.getItem(PROVIDER_STORAGE_NAME) as ProviderType) || "ollama";

const providerManager = new AIProviderManager(storedProvider, storedKey);
if (modelSelect) modelSelect.value = storedProvider;
if (geminiApiKeyInput && storedKey) geminiApiKeyInput.value = storedKey;

// Initialize Core Engines
const engine = new LeoCharacterEngine(42);
const gestures = new LeoGestureController(engine, { inactivityTimeoutMs: 60000 });
const vad = new VoiceActivityDetector();
const asr = new OfflineASRPipeline();

let isRecording = false;
let audioLevelInterval: number | null = null;
let lastRenderTime = performance.now();
const reducedMotionQuery = window.matchMedia("(prefers-reduced-motion: reduce)");

// 1. Digital Character Canvas Loop
if (canvas && context) {
  const ctx = context;

  const render = (time: number) => {
    const dt = (time - lastRenderTime) / 1000;
    lastRenderTime = time;

    gestures.update();
    engine.advance(dt);

    ctx.clearRect(0, 0, canvas.width, canvas.height);

    const val = engine.val;
    const blink = engine.getBlinkScale();
    const breathing = engine.getBreathingOffset(reducedMotionQuery.matches);
    const { sx, sy } = engine.getSquashStretch();
    const mood = engine.currentMood;

    ctx.save();
    // Center at (60, 48) with breathing and squash/stretch
    ctx.translate(60, 48 + breathing);
    ctx.scale(sx, sy);
    ctx.translate(-60, -48);

    // Warm copper / amber aura glow (matching black hole accretion disk)
    if (val.glow > 0.05) {
      const glowColor = mood === "listening"
        ? "rgba(56, 189, 248, " // Controlled cyan for listening
        : "rgba(249, 115, 22, "; // Warm copper for idle/happy/talking

      const gL = ctx.createRadialGradient(40 + val.look_x * 6, 48 + val.look_y * 4, 2, 40, 48, 24);
      gL.addColorStop(0, `${glowColor}${val.glow * 0.45})`);
      gL.addColorStop(1, `${glowColor}0)`);
      ctx.fillStyle = gL;
      ctx.beginPath();
      ctx.arc(40, 48, 24, 0, Math.PI * 2);
      ctx.fill();

      const gR = ctx.createRadialGradient(80 + val.look_x * 6, 48 + val.look_y * 4, 2, 80, 48, 24);
      gR.addColorStop(0, `${glowColor}${val.glow * 0.45})`);
      gR.addColorStop(1, `${glowColor}0)`);
      ctx.fillStyle = gR;
      ctx.beginPath();
      ctx.arc(80, 48, 24, 0, Math.PI * 2);
      ctx.fill();
    }

    if (mood === "happy") {
      // Crescent happy squint eyes (⌒ ⌒)
      ctx.strokeStyle = "#FAF5FF";
      ctx.lineWidth = 4;
      ctx.lineCap = "round";

      // Left happy arch
      ctx.beginPath();
      ctx.arc(40, 52, 10, Math.PI * 1.15, Math.PI * 1.85);
      ctx.stroke();

      // Right happy arch
      ctx.beginPath();
      ctx.arc(80, 52, 10, Math.PI * 1.15, Math.PI * 1.85);
      ctx.stroke();
    } else if (mood === "sleepy") {
      // Gentle sleeping closed eyelids (- -)
      ctx.strokeStyle = "#A3A8B8";
      ctx.lineWidth = 3.5;
      ctx.lineCap = "round";

      ctx.beginPath();
      ctx.moveTo(31, 48);
      ctx.lineTo(49, 48);
      ctx.stroke();

      ctx.beginPath();
      ctx.moveTo(71, 48);
      ctx.lineTo(89, 48);
      ctx.stroke();
    } else {
      // Luminous warm cream eyes with gaze dynamics
      ctx.fillStyle = "#FAF5FF";
      const talkPulse = mood === "talking" ? (1.0 + Math.sin(time * 0.015) * 0.15) : 1.0;
      const eyeH = 15.5 * val.eye_size * blink * talkPulse;
      const eyeW = 9.5 * val.eye_size;

      // Left eye
      const leftX = 40 + val.look_x * 7;
      const leftY = 48 + val.look_y * 5;
      ctx.beginPath();
      ctx.ellipse(leftX, leftY, eyeW, Math.max(1, eyeH), 0, 0, Math.PI * 2);
      ctx.fill();

      // Right eye
      const rightX = 80 + val.look_x * 7;
      const rightY = 48 + val.look_y * 5;
      ctx.beginPath();
      ctx.ellipse(rightX, rightY, eyeW, Math.max(1, eyeH), 0, 0, Math.PI * 2);
      ctx.fill();

      // Subtle cyan specular reflection dot
      if (blink > 0.5) {
        ctx.fillStyle = "#38BDF8";
        ctx.beginPath();
        ctx.arc(leftX + 2.5, leftY - 3, 1.8, 0, Math.PI * 2);
        ctx.fill();

        ctx.beginPath();
        ctx.arc(rightX + 2.5, rightY - 3, 1.8, 0, Math.PI * 2);
        ctx.fill();
      }
    }

    ctx.restore();
    requestAnimationFrame(render);
  };

  requestAnimationFrame(render);
}

// 2. Interactive Face Click & Keyboard Interaction
const triggerFaceInteraction = () => {
  engine.triggerImpulse(1.0);
  gestures.triggerHappyReaction();
  statusSub.textContent = "Joy! • Leo is happy";
  setTimeout(() => {
    statusSub.textContent = "Ready • Space to talk";
  }, 2500);
};

faceContainer?.addEventListener("click", triggerFaceInteraction);
faceContainer?.addEventListener("keydown", (e) => {
  if (e.key === "Enter" || e.key === " ") {
    e.preventDefault();
    triggerFaceInteraction();
  }
});

// 3. Tab Switching & Window Resizing
tabButtons.forEach((btn) => {
  btn.addEventListener("click", () => {
    const tabName = btn.dataset.tab;
    tabButtons.forEach((b) => b.classList.remove("active"));
    btn.classList.add("active");

    drawerBodies.forEach((drawer) => drawer.classList.remove("active"));
    gestures.notifyUserActivity();

    if (tabName === "compact") {
      capsule.style.width = "340px";
      LeoIPCBridge.execute({ command: "set_window_size", width: 340, height: 74 }).catch(() => {});
    } else {
      capsule.style.width = "400px";
      const targetDrawer = document.getElementById(`drawer-${tabName}`);
      if (targetDrawer) {
        targetDrawer.classList.add("active");
        const height = tabName === "chat" ? 320 : tabName === "settings" ? 430 : 250;
        LeoIPCBridge.execute({ command: "set_window_size", width: 400, height }).catch(() => {});
      }
    }
  });
});

// 4. Voice Dictation & Audio Simulation
const toggleDictation = () => {
  isRecording = !isRecording;
  micBtn.classList.toggle("active", isRecording);
  gestures.setVoiceListening(isRecording);

  if (isRecording) {
    statusSub.textContent = "Listening • Local Audio Only";
    transcriptBox.textContent = "Listening... Speak naturally (fillers will be removed).";

    // Simulate real-time audio energy meter
    let mockEnergy = 0.2;
    audioLevelInterval = window.setInterval(() => {
      mockEnergy = 0.1 + Math.random() * 0.75;
      levelBar.style.width = `${Math.round(mockEnergy * 100)}%`;

      // Feed simulated frame to VAD
      const mockSamples = new Float32Array(512);
      for (let i = 0; i < 512; i++) {
        mockSamples[i] = (Math.random() - 0.5) * mockEnergy;
      }
      vad.processFrame(mockSamples);
    }, 60);
  } else {
    statusSub.textContent = "Processing speech...";
    if (audioLevelInterval) clearInterval(audioLevelInterval);
    levelBar.style.width = "0%";

    // Finalize speech & clean transcript
    const sampleRaw = "Hello team, um, the release is confirmed for Wednesday, actually Thursday.";
    const cleaned = SpeechCleanupFilter.clean(sampleRaw);
    transcriptBox.innerHTML = `<strong>Cleaned:</strong> ${cleaned}<br><span style="color: var(--text-muted); font-size: 10px;">Raw was: "${sampleRaw}"</span>`;
    statusSub.textContent = "Ready • Ctrl+Alt+Space";
  }
};

micBtn?.addEventListener("click", toggleDictation);

cleanSampleBtn?.addEventListener("click", () => {
  const sample = "Today we need to, uh, accomplish three things. {Bullet point} review pull requests {Bullet point} run integration tests {Bullet point} deploy release candidate";
  const cleaned = SpeechCleanupFilter.clean(sample);
  transcriptBox.innerHTML = `<strong>Cleaned:</strong><pre style="margin-top: 4px; font-family: inherit;">${cleaned}</pre>`;
  gestures.triggerHappyReaction();
});

copyBtn?.addEventListener("click", () => {
  const text = transcriptBox.innerText;
  navigator.clipboard?.writeText(text).then(() => {
    statusSub.textContent = "Copied to clipboard!";
    setTimeout(() => (statusSub.textContent = "Ready • Ctrl+Alt+Space"), 2000);
  });
});

pasteBtn?.addEventListener("click", async () => {
  const text = transcriptBox.innerText;
  try {
    await LeoIPCBridge.execute({ command: "paste_clipboard_text", text });
    statusSub.textContent = "Pasted to active window!";
    setTimeout(() => (statusSub.textContent = "Ready • Ctrl+Alt+Space"), 2000);
  } catch (err) {
    console.error("Paste error:", err);
  }
});

// 5. Chat Query & Streaming Output
let activeChatAbort: AbortController | null = null;
const conversationHistory: Array<{ role: "system" | "user" | "assistant"; content: string }> = [];

const updateUsageDashboard = () => {
  const usage = providerManager.getUsage();
  if (usageActiveModel) {
    usageActiveModel.textContent = `${providerManager.providerType.toUpperCase()} / ${providerManager.activeModelName}`;
  }
  if (usageQueriesCount) {
    usageQueriesCount.textContent = String(usage.totalRequests);
  }
  if (usageTokensCount) {
    usageTokensCount.textContent = providerManager.isLocal
      ? "Local (Zero Egress)"
      : `${usage.totalTokens} tokens`;
  }
};

const sendChat = async () => {
  const prompt = chatPrompt.value.trim();
  if (!prompt) return;

  chatPrompt.value = "";
  chatOutput.innerHTML = `<span style="color: var(--text-muted);">Prompt: ${prompt}</span><br><br>`;
  sendChatBtn.style.display = "none";
  cancelChatBtn.style.display = "inline-block";

  gestures.setProcessingPrompt(true);
  statusSub.textContent = "Thinking...";

  activeChatAbort = new AbortController();

  try {
    // Check if cloud egress applies
    if (!providerManager.isLocal) {
      chatOutput.innerHTML += `<div style="color: var(--accent-amber); font-size: 10px; margin-bottom: 6px;">[Leaves your PC: External Google Gemini Request]</div>`;
    }

    gestures.setSpeaking(true);
    let streamedContent = "";

    await providerManager.streamChat(
      prompt,
      conversationHistory,
      (token) => {
        if (activeChatAbort?.signal.aborted) return;
        streamedContent += token;
        const egressNotice = providerManager.isLocal
          ? ""
          : `<div style="color: var(--accent-amber); font-size: 10px; margin-bottom: 6px;">[Leaves your PC: External Google Gemini Request]</div>`;
        chatOutput.innerHTML = egressNotice +
          `<span style="color: var(--text-muted);">Prompt: ${prompt}</span><br><br>` +
          streamedContent.replace(/\n/g, "<br>");
        chatOutput.scrollTop = chatOutput.scrollHeight;
      },
      activeChatAbort.signal
    );

    conversationHistory.push({ role: "user", content: prompt });
    conversationHistory.push({ role: "assistant", content: streamedContent });
    if (conversationHistory.length > 10) conversationHistory.splice(0, 2);

    updateUsageDashboard();
    statusSub.textContent = "Ready • Ctrl+Alt+Space";
  } catch (err: unknown) {
    const raw = err instanceof Error ? err.message : String(err);
    chatOutput.innerHTML += `<br><span style="color: var(--accent-red);">Error: ${providerManager.getGeminiClient().redactSecret(raw)}</span>`;
    statusSub.textContent = "Query failed";
  } finally {
    gestures.setProcessingPrompt(false);
    gestures.setSpeaking(false);
    sendChatBtn.style.display = "inline-block";
    cancelChatBtn.style.display = "none";
    activeChatAbort = null;
  }
};

sendChatBtn?.addEventListener("click", sendChat);
cancelChatBtn?.addEventListener("click", () => activeChatAbort?.abort());
chatPrompt?.addEventListener("keydown", (e) => {
  if (e.key === "Enter") sendChat();
});

// 6. Settings & Egress Badge Handling
const updateEgressIndicator = () => {
  const selectedType = modelSelect.value as ProviderType;
  providerManager.setProvider(selectedType);
  localStorage.setItem(PROVIDER_STORAGE_NAME, selectedType);

  if (geminiSettingsCard) {
    geminiSettingsCard.style.display = selectedType === "gemini" ? "flex" : "none";
  }

  if (providerManager.isLocal) {
    egressBadge.classList.remove("cloud");
    egressText.textContent = "Local";
    egressBadge.title = "Local-first: Audio & data remain strictly on your machine.";
  } else {
    egressBadge.classList.add("cloud");
    egressText.textContent = "Leaves PC";
    egressBadge.title = "Warning: Prompt data leaves your computer to Google Gemini.";
  }

  updateUsageDashboard();
};

modelSelect?.addEventListener("change", updateEgressIndicator);

// Gemini Credentials & Model Configuration
toggleKeyVisibilityBtn?.addEventListener("click", () => {
  if (geminiApiKeyInput.type === "password") {
    geminiApiKeyInput.type = "text";
  } else {
    geminiApiKeyInput.type = "password";
  }
});

geminiModelSelect?.addEventListener("change", () => {
  providerManager.getGeminiClient().setModel(geminiModelSelect.value);
  updateUsageDashboard();
});

saveGeminiKeyBtn?.addEventListener("click", () => {
  const key = geminiApiKeyInput.value.trim();
  providerManager.getGeminiClient().setApiKey(key);
  localStorage.setItem(KEY_STORAGE_NAME, key);
  geminiStatusText.textContent = key ? "Key saved locally." : "Key cleared.";
  geminiStatusText.style.color = key ? "var(--accent-green)" : "var(--text-muted)";
  updateUsageDashboard();
});

testGeminiBtn?.addEventListener("click", async () => {
  const key = geminiApiKeyInput.value.trim();
  providerManager.getGeminiClient().setApiKey(key);
  geminiStatusText.textContent = "Testing connection...";
  geminiStatusText.style.color = "var(--accent-cyan)";

  const result = await providerManager.getGeminiClient().testConnection();
  if (result.ok) {
    geminiStatusText.textContent = result.message;
    geminiStatusText.style.color = "var(--accent-green)";
    localStorage.setItem(KEY_STORAGE_NAME, key);
  } else {
    geminiStatusText.textContent = result.message;
    geminiStatusText.style.color = "var(--accent-red)";
  }
});

clearGeminiKeyBtn?.addEventListener("click", () => {
  providerManager.getGeminiClient().clearApiKey();
  localStorage.removeItem(KEY_STORAGE_NAME);
  geminiApiKeyInput.value = "";
  geminiStatusText.textContent = "Key removed from local storage.";
  geminiStatusText.style.color = "var(--text-muted)";
  updateUsageDashboard();
});

// Initial Status Update
updateEgressIndicator();
if (storedKey) {
  geminiStatusText.textContent = "Key configured.";
  geminiStatusText.style.color = "var(--accent-green)";
}

// 7. Offline Model Download Handler
downloadAsrBtn?.addEventListener("click", async () => {
  asr.grantDownloadConsent("parakeet-tdt-0.6b-v3");
  downloadAsrBtn.style.display = "none";
  cancelAsrBtn.style.display = "inline-block";
  asrStatusText.textContent = "Downloading 0%...";

  try {
    await asr.downloadModel("parakeet-tdt-0.6b-v3", (progress) => {
      asrProgressFill.style.width = `${progress.progressPercent}%`;
      asrStatusText.textContent = `Downloading ${progress.progressPercent}%...`;
    });

    asrStatusText.textContent = "Ready (680 MB on disk)";
    asrStatusText.style.color = "var(--accent-green)";
    cancelAsrBtn.style.display = "none";
    downloadAsrBtn.style.display = "inline-block";
    downloadAsrBtn.textContent = "Re-verify Weights";
    gestures.triggerHappyReaction();
  } catch (err: unknown) {
    asrStatusText.textContent = `Cancelled: ${(err as Error).message}`;
    asrProgressFill.style.width = "0%";
    downloadAsrBtn.style.display = "inline-block";
    cancelAsrBtn.style.display = "none";
  }
});

cancelAsrBtn?.addEventListener("click", () => {
  asr.cancelDownload("parakeet-tdt-0.6b-v3");
});

// 8. Global Keyboard Accessibility
window.addEventListener("keydown", (e) => {
  if (e.key === "Escape") {
    // Collapse to mini view
    (document.querySelector('.tab-btn[data-tab="compact"]') as HTMLButtonElement)?.click();
  } else if (e.code === "Space" && e.target === document.body) {
    // Toggle dictation with Space
    toggleDictation();
  }
});
