/**
 * Leo Character Engine: 2D Canvas Procedural Face & Spring-Damper Dynamics
 *
 * Implements continuous second-order damped harmonic oscillators for organic gaze saccades,
 * eye scaling, luminescent glow flares, and expressiveness without external media assets.
 * 
 * Equations:
 *   ddot{x} + 2*zeta*omega0*dot{x} + omega0^2*(x - x_target) = 0
 */

export interface LeoMoodState {
  open_l: number;
  open_r: number;
  eye_size: number;
  squint: number;
  look_x: number;
  look_y: number;
  brow_tilt: number;
  glow: number;
  stretch: number;
  dots: number;
  zzz: number;
}

export type LeoMood =
  | "idle"
  | "hover"
  | "listening"
  | "thinking"
  | "happy"
  | "talking"
  | "concentrate"
  | "sleepy"
  | "offline";

export const LEO_MOOD_PRESETS: Record<LeoMood, Partial<LeoMoodState>> = {
  idle:        { open_l: 1.0, open_r: 1.0, eye_size: 1.00, look_x:  0.0, look_y:  0.0, glow: 0.30 },
  hover:       { open_l: 1.0, open_r: 1.0, eye_size: 1.06, look_x:  0.0, look_y:  0.0, glow: 0.55 },
  listening:   { open_l: 1.0, open_r: 1.0, eye_size: 1.08, look_x:  0.0, look_y: -0.1, glow: 0.50 },
  thinking:    { open_l: 1.0, open_r: 1.0, eye_size: 1.00, look_x:  0.65,look_y: -0.85,brow_tilt: -0.15, glow: 0.35, dots: 1.0 },
  happy:       { open_l: 0.8, open_r: 0.8, eye_size: 1.00, squint:  0.70, glow: 0.80 },
  talking:     { open_l: 1.0, open_r: 1.0, eye_size: 1.02, glow: 0.45 },
  concentrate: { open_l: 1.0, open_r: 1.0, eye_size: 1.04, look_x: -0.1, look_y: -0.5, glow: 0.40 },
  sleepy:      { open_l: 0.0, open_r: 0.0, eye_size: 1.00, zzz: 1.0, glow: 0.05 },
  offline:     { open_l: 0.5, open_r: 0.5, eye_size: 0.95, glow: 0.10 }
};

export const SPRING_PARAMETERS: Record<string, [number, number]> = {
  look_x:    [300, 0.60],
  look_y:    [300, 0.60],
  brow_tilt: [150, 0.60],
  eye_size:  [180, 0.60],
  glow:      [120, 0.85],
  stretch:   [320, 0.45],
  default:   [220, 0.75]
};

/**
 * 32-bit Mulberry32 deterministic PRNG
 * Uses the increment constant 1831565813 (0x6D2B79F5) and xor/imul mixing operations
 * exactly matching the verified HeyNixi generator.
 */
export function createMulberry32(seed: number): () => number {
  let t = seed >>> 0;
  return () => {
    t |= 0;
    t = (t + 1831565813) | 0;
    let n = Math.imul(t ^ (t >>> 15), 1 | t);
    n = n + Math.imul(n ^ (n >>> 7), 61 | n) ^ n;
    return ((n ^ (n >>> 14)) >>> 0) / 4294967296;
  };
}

export class LeoCharacterEngine {
  public currentMood: LeoMood = "idle";
  public val: LeoMoodState;
  public vel: Record<string, number> = {};
  public clock: number = 0;
  private prng: () => number;
  private blinkTimer: number = -1;
  private nextBlinkAt: number = 2.5;

  constructor(seed = 42) {
    this.prng = createMulberry32(seed);
    this.val = {
      open_l: 1.0, open_r: 1.0, eye_size: 1.0, squint: 0.0,
      look_x: 0.0, look_y: 0.0, brow_tilt: 0.0, glow: 0.3,
      stretch: 0.0, dots: 0.0, zzz: 0.0,
      ...LEO_MOOD_PRESETS["idle"]
    };

    for (const key of Object.keys(this.val)) {
      this.vel[key] = 0;
    }
  }

  public setMood(mood: LeoMood): void {
    if (LEO_MOOD_PRESETS[mood]) {
      this.currentMood = mood;
    }
  }

  public advance(dt: number, targetLook?: [number, number]): void {
    // Clamp delta-time to avoid numerical instability
    const step = Math.min(dt, 0.1);
    this.clock += step;

    // Resolve target state
    const targetState: LeoMoodState = {
      ...this.val,
      ...LEO_MOOD_PRESETS[this.currentMood]
    };

    if (targetLook) {
      targetState.look_x = Math.max(-1, Math.min(1, targetLook[0]));
      targetState.look_y = Math.max(-1, Math.min(1, targetLook[1]));
    }

    // Step spring-damper physics using stable substepping (dt <= 1/120s)
    let remaining = step;
    const substep = 1 / 120;
    while (remaining > 1e-6) {
      const h = Math.min(substep, remaining);
      remaining -= h;

      for (const key of Object.keys(this.val) as (keyof LeoMoodState)[]) {
        const [omega0, zeta] = SPRING_PARAMETERS[key] || SPRING_PARAMETERS["default"];
        const current = this.val[key];
        const target = targetState[key];
        const velocity = this.vel[key] || 0;

        // Hooke's Law + viscous damping
        const force = (target - current) * omega0 - 2 * Math.sqrt(omega0) * zeta * velocity;
        this.vel[key] = velocity + force * h;
        this.val[key] = current + this.vel[key] * h;
      }
    }

    // Periodic blinking
    if (this.blinkTimer >= 0) {
      this.blinkTimer += step;
      if (this.blinkTimer > 0.16) {
        this.blinkTimer = -1;
        this.nextBlinkAt = this.clock + 2.0 + this.prng() * 3.0;
      }
    } else if (this.clock >= this.nextBlinkAt && this.currentMood !== "sleepy") {
      this.blinkTimer = 0;
    }
  }

  public getBlinkScale(): number {
    if (this.blinkTimer < 0) return 1.0;
    return Math.max(0, 1.0 - Math.sin((Math.PI * this.blinkTimer) / 0.16));
  }

  /**
   * Subtle biological breathing float (respects reduced motion)
   */
  public getBreathingOffset(reducedMotion = false): number {
    if (reducedMotion || this.currentMood === "offline") return 0;
    const freq = this.currentMood === "sleepy" ? 1.0 : this.currentMood === "listening" ? 2.4 : 1.6;
    const amp = this.currentMood === "sleepy" ? 0.8 : 1.2;
    return Math.sin(this.clock * freq) * amp;
  }

  /**
   * Dynamic squash & stretch scales from spring physics
   */
  public getSquashStretch(): { sx: number; sy: number } {
    const stretch = this.val.stretch || 0;
    return {
      sx: 1.0 + stretch * 0.25,
      sy: 1.0 - stretch * 0.20,
    };
  }

  /**
   * Trigger physical tap / pet impulse
   */
  public triggerImpulse(intensity = 1.0): void {
    this.vel.stretch = (this.vel.stretch || 0) + 12.0 * intensity;
    this.vel.look_y = (this.vel.look_y || 0) - 2.5 * intensity;
  }
}
