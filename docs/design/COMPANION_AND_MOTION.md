# Companion Character & Motion Design: Leo

**Document ID:** `DESIGN-LEO-001`  
**Version:** `1.0.0` (Status: Approved)  
**Companion Identity:** **Leo** (The Personal AI Companion)

---

## 1. Original Character Identity & Concept

In strict compliance with `LICENSE-ASSETS.md` and proprietary asset boundaries, **Leo** features a 100% original identity, geometric silhouette, and behavioral model:

- **Personality:** Attentive, calm, responsive, slightly playful, never childish or chaotic.
- **Visual Representation:** An ultra-minimalist, sleek geometric companion rendered directly on an HTML5 2D Canvas.
- **Silhouette:** A precision rounded pill capsule (`rx = 27px`) with dual expressive digital eyes.
- **Palette:**
  - Resting Pill: Obsidian Black (`#101114`) or Soft Platinum (`#F4F6F8`) with auto-flipping high-contrast typography.
  - Active Eyes: Pure Phosphor White (`#FFFFFF`) with adjustable hue and luminescent aura.
  - Waveform / Audio State: Electric Sapphire (`#3882F6`) when listening, Warm Amber (`#F59E0B`) when thinking, Emerald Green (`#22C55E`) on completion.

---

## 2. Spring-Damper Harmonic Motion Physics

Leo's movements are computed dynamically every frame using second-order damped harmonic oscillators rather than pre-baked video frames or keyframed sprites:

$$\ddot{x} + 2\zeta\omega_0 \dot{x} + \omega_0^2(x - x_{\text{target}}) = 0$$

### Exact Oscillation Parameters
| Parameter | $\omega_0$ (Natural Frequency) | $\zeta$ (Damping Ratio) | Visual Effect |
| :--- | :--- | :--- | :--- |
| `look_x`, `look_y` | $300\text{ rad/s}$ | $0.60$ | Snappy, organic gaze saccades with subtle settling |
| `brow_tilt` | $150\text{ rad/s}$ | $0.60$ | Deliberate eyebrow expressiveness (thinking / questioning) |
| `eye_size` | $180\text{ rad/s}$ | $0.60$ | Eye aperture expansion on audio input / alert |
| `glow` | $120\text{ rad/s}$ | $0.85$ | Smooth luminescent phosphor flare without harsh flashing |
| `stretch` | $320\text{ rad/s}$ | $0.45$ | Dynamic squish and stretch during hop bounces |
| `default` | $220\text{ rad/s}$ | $0.75$ | Critical damping for general facial feature recovery |

---

## 3. Emotional State Machine & Mood Presets

```javascript
// Leo Character Engine Core Configuration
export const LEO_MOODS = {
  idle:        { open_l: 1.0, open_r: 1.0, eye_size: 1.00, look_x:  0.0, look_y:  0.0, glow: 0.30 },
  hover:       { open_l: 1.0, open_r: 1.0, eye_size: 1.06, look_x:  0.0, look_y:  0.0, glow: 0.55 },
  listening:   { open_l: 1.0, open_r: 1.0, eye_size: 1.08, look_x:  0.0, look_y: -0.1, glow: 0.50 },
  thinking:    { open_l: 1.0, open_r: 1.0, eye_size: 1.00, look_x:  0.65,look_y: -0.85,brow_tilt: -0.15, glow: 0.35 },
  happy:       { open_l: 0.8, open_r: 0.8, eye_size: 1.00, squint:  0.70, glow: 0.80 },
  talking:     { open_l: 1.0, open_r: 1.0, eye_size: 1.02, glow: 0.45 },
  concentrate: { open_l: 1.0, open_r: 1.0, eye_size: 1.04, look_x: -0.1, look_y: -0.5, glow: 0.40 },
  sleepy:      { open_l: 0.0, open_r: 0.0, eye_size: 1.00, zzz: 1.0, glow: 0.05 },
  offline:     { open_l: 0.5, open_r: 0.5, eye_size: 0.95, glow: 0.10 }
};
```

---

## 4. Performance, Battery, and Reduced Motion Policies

1. **Idle CPU Efficiency:**
   - When the user is inactive and no audio or agent task is executing, Leo enters `sleepy` mode after 120 seconds.
   - In sleepy mode, frame rendering drops from 60 FPS to **0 FPS (fully frozen until cursor hover or global hotkey press)**.
2. **Reduced Motion Compliance (`prefers-reduced-motion: reduce`):**
   - Disables continuous hopping, breathing oscillations, and gaze drift.
   - Smooth transitions between states use instant step updates with subtle opacity crossfades.
