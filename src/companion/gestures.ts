/**
 * Leo Companion Gesture & Mood Controller
 * 
 * Manages organic state transitions for Leo's digital face:
 * - Natural saccades and blinks during 'idle'
 * - Attention and widening during 'listening'
 * - Glance and oscillation during 'thinking'
 * - Rhythmic speech cadence during 'speaking'
 * - Sleepiness after inactivity period (> 120s)
 * - Joy on positive interactions
 */

import { LeoCharacterEngine, type LeoMood } from "./leo_face.ts";

export interface GestureControllerConfig {
  inactivityTimeoutMs: number;
}

export class LeoGestureController {
  private engine: LeoCharacterEngine;
  private lastActivityTime: number = Date.now();
  private inactivityTimeoutMs: number;
  private isVoiceActive: boolean = false;
  private isProcessingPrompt: boolean = false;

  constructor(engine: LeoCharacterEngine, config: GestureControllerConfig = { inactivityTimeoutMs: 120000 }) {
    this.engine = engine;
    this.inactivityTimeoutMs = config.inactivityTimeoutMs;
  }

  public notifyUserActivity(): void {
    this.lastActivityTime = Date.now();
    if (this.engine.currentMood === "sleepy") {
      this.engine.setMood("idle");
    }
  }

  public setVoiceListening(listening: boolean): void {
    this.isVoiceActive = listening;
    this.notifyUserActivity();
    if (listening) {
      this.engine.setMood("listening");
    } else if (!this.isProcessingPrompt) {
      this.engine.setMood("idle");
    }
  }

  public setProcessingPrompt(processing: boolean): void {
    this.isProcessingPrompt = processing;
    this.notifyUserActivity();
    if (processing) {
      this.engine.setMood("thinking");
    } else if (!this.isVoiceActive) {
      this.engine.setMood("idle");
    }
  }

  public setSpeaking(speaking: boolean): void {
    this.notifyUserActivity();
    if (speaking) {
      this.engine.setMood("talking");
    } else if (this.isVoiceActive) {
      this.engine.setMood("listening");
    } else if (this.isProcessingPrompt) {
      this.engine.setMood("thinking");
    } else {
      this.engine.setMood("idle");
    }
  }

  public triggerHappyReaction(): void {
    this.notifyUserActivity();
    this.engine.setMood("happy");
    setTimeout(() => {
      if (this.engine.currentMood === "happy") {
        this.engine.setMood("idle");
      }
    }, 2500);
  }

  public update(): void {
    const now = Date.now();
    if (
      !this.isVoiceActive &&
      !this.isProcessingPrompt &&
      this.engine.currentMood !== "sleepy" &&
      now - this.lastActivityTime >= this.inactivityTimeoutMs
    ) {
      this.engine.setMood("sleepy");
    }
  }

  public get currentMood(): LeoMood {
    return this.engine.currentMood;
  }
}
