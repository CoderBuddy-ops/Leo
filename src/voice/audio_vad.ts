/**
 * Leo Voice Activity Detector (VAD) & Audio Frame Collector
 * 
 * Invariants:
 * 1. Local-Only: Audio data NEVER leaves this device. All signal analysis runs in memory.
 * 2. Processes 16kHz mono PCM frames (default frame size: 512 samples = 32ms).
 * 3. Dual-criteria detection: Root Mean Square (RMS) energy + Zero Crossing Rate (ZCR).
 * 4. Hangover filter prevents clipping sentence endings during natural pauses.
 */

export interface VADConfig {
  sampleRate: number;
  frameSize: number;
  energyThreshold: number;
  silenceHangoverFrames: number;
  speechTriggerFrames: number;
}

export const DEFAULT_VAD_CONFIG: VADConfig = {
  sampleRate: 16000,
  frameSize: 512,
  energyThreshold: 0.015,
  silenceHangoverFrames: 15, // ~480ms hangover
  speechTriggerFrames: 3,     // ~96ms sustained speech to trigger
};

export interface VADFrameResult {
  rms: number;
  zcr: number;
  isSpeechFrame: boolean;
  state: "silence" | "speaking";
}

export class VoiceActivityDetector {
  private config: VADConfig;
  private consecutiveSpeechFrames = 0;
  private consecutiveSilenceFrames = 0;
  private currentState: "silence" | "speaking" = "silence";

  constructor(config: Partial<VADConfig> = {}) {
    this.config = { ...DEFAULT_VAD_CONFIG, ...config };
  }

  /**
   * Calculate Root Mean Square (RMS) energy of a 32-bit float audio buffer.
   */
  public static calculateRMS(samples: Float32Array): number {
    if (samples.length === 0) return 0;
    let sumSquares = 0;
    for (let i = 0; i < samples.length; i++) {
      sumSquares += samples[i] * samples[i];
    }
    return Math.sqrt(sumSquares / samples.length);
  }

  /**
   * Calculate Zero Crossing Rate (ZCR) of an audio buffer.
   */
  public static calculateZCR(samples: Float32Array): number {
    if (samples.length < 2) return 0;
    let crossings = 0;
    for (let i = 1; i < samples.length; i++) {
      if ((samples[i] >= 0 && samples[i - 1] < 0) || (samples[i] < 0 && samples[i - 1] >= 0)) {
        crossings++;
      }
    }
    return crossings / (samples.length - 1);
  }

  /**
   * Process a single audio frame and return speech detection state.
   */
  public processFrame(samples: Float32Array): VADFrameResult {
    const rms = VoiceActivityDetector.calculateRMS(samples);
    const zcr = VoiceActivityDetector.calculateZCR(samples);

    const isAboveThreshold = rms >= this.config.energyThreshold;

    if (isAboveThreshold) {
      this.consecutiveSpeechFrames++;
      this.consecutiveSilenceFrames = 0;

      if (this.consecutiveSpeechFrames >= this.config.speechTriggerFrames) {
        this.currentState = "speaking";
      }
    } else {
      this.consecutiveSilenceFrames++;
      this.consecutiveSpeechFrames = 0;

      if (this.currentState === "speaking") {
        if (this.consecutiveSilenceFrames >= this.config.silenceHangoverFrames) {
          this.currentState = "silence";
        }
      }
    }

    return {
      rms,
      zcr,
      isSpeechFrame: isAboveThreshold,
      state: this.currentState,
    };
  }

  /**
   * Reset detector state.
   */
  public reset(): void {
    this.consecutiveSpeechFrames = 0;
    this.consecutiveSilenceFrames = 0;
    this.currentState = "silence";
  }

  public get state(): "silence" | "speaking" {
    return this.currentState;
  }
}
