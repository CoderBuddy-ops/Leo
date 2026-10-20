using System;
using System.Collections.Generic;

namespace Leo.Core.Companion;

/// <summary>
/// Leo Character Engine: Procedural Face & Spring-Damper Dynamics.
/// Implements second-order damped harmonic oscillators for organic gaze saccades,
/// eye scaling, luminescent glow flares, and natural non-uniform blinking.
/// Equations: ddot{x} + 2*zeta*omega0*dot{x} + omega0^2*(x - x_target) = 0
/// </summary>
public class LeoCharacterEngine
{
    public LeoMood CurrentMood { get; private set; } = LeoMood.Idle;
    public LeoMoodState Val { get; } = new();
    public Dictionary<string, double> Vel { get; } = new();
    public double Clock { get; private set; } = 0.0;

    private readonly Mulberry32 _prng;
    private double _blinkTimer = -1.0;
    private double _nextBlinkAt = 2.5;

    public LeoCharacterEngine(uint seed = 42)
    {
        _prng = new Mulberry32(seed);
        ResetVelocities();
        ApplyPresetImmediate(LeoMood.Idle);
    }

    private void ResetVelocities()
    {
        Vel["OpenL"] = 0.0;
        Vel["OpenR"] = 0.0;
        Vel["EyeSize"] = 0.0;
        Vel["Squint"] = 0.0;
        Vel["LookX"] = 0.0;
        Vel["LookY"] = 0.0;
        Vel["BrowTilt"] = 0.0;
        Vel["Glow"] = 0.0;
        Vel["Stretch"] = 0.0;
        Vel["Dots"] = 0.0;
        Vel["Zzz"] = 0.0;
    }

    private void ApplyPresetImmediate(LeoMood mood)
    {
        if (LeoMoodPresets.Presets.TryGetValue(mood, out var p))
        {
            Val.OpenL = p.openL;
            Val.OpenR = p.openR;
            Val.EyeSize = p.eyeSize;
            Val.Squint = p.squint;
            Val.LookX = p.lookX;
            Val.LookY = p.lookY;
            Val.BrowTilt = p.browTilt;
            Val.Glow = p.glow;
            Val.Dots = p.dots;
            Val.Zzz = p.zzz;
        }
    }

    public void SetMood(LeoMood mood)
    {
        CurrentMood = mood;
    }

    public void Advance(double dt, double? targetLookX = null, double? targetLookY = null)
    {
        double step = Math.Min(dt, 0.1);
        Clock += step;

        // Determine targets based on current mood preset
        var p = LeoMoodPresets.Presets[CurrentMood];
        double targetOpenL = p.openL;
        double targetOpenR = p.openR;
        double targetEyeSize = p.eyeSize;
        double targetSquint = p.squint;
        double targetLookXVal = targetLookX.HasValue ? Math.Clamp(targetLookX.Value, -1.0, 1.0) : p.lookX;
        double targetLookYVal = targetLookY.HasValue ? Math.Clamp(targetLookY.Value, -1.0, 1.0) : p.lookY;
        double targetBrowTilt = p.browTilt;
        double targetGlow = p.glow;
        double targetDots = p.dots;
        double targetZzz = p.zzz;
        double targetStretch = 0.0;

        // Substep physics (dt <= 1/120s)
        double remaining = step;
        const double substep = 1.0 / 120.0;

        while (remaining > 1e-6)
        {
            double h = Math.Min(substep, remaining);
            remaining -= h;

            Val.OpenL = StepSpring("OpenL", targetOpenL, h, Val.OpenL, LeoMoodPresets.SpringParams["Default"]);
            Val.OpenR = StepSpring("OpenR", targetOpenR, h, Val.OpenR, LeoMoodPresets.SpringParams["Default"]);
            Val.EyeSize = StepSpring("EyeSize", targetEyeSize, h, Val.EyeSize, LeoMoodPresets.SpringParams["EyeSize"]);
            Val.Squint = StepSpring("Squint", targetSquint, h, Val.Squint, LeoMoodPresets.SpringParams["Default"]);
            Val.LookX = StepSpring("LookX", targetLookXVal, h, Val.LookX, LeoMoodPresets.SpringParams["LookX"]);
            Val.LookY = StepSpring("LookY", targetLookYVal, h, Val.LookY, LeoMoodPresets.SpringParams["LookY"]);
            Val.BrowTilt = StepSpring("BrowTilt", targetBrowTilt, h, Val.BrowTilt, LeoMoodPresets.SpringParams["BrowTilt"]);
            Val.Glow = StepSpring("Glow", targetGlow, h, Val.Glow, LeoMoodPresets.SpringParams["Glow"]);
            Val.Stretch = StepSpring("Stretch", targetStretch, h, Val.Stretch, LeoMoodPresets.SpringParams["Stretch"]);
            Val.Dots = StepSpring("Dots", targetDots, h, Val.Dots, LeoMoodPresets.SpringParams["Default"]);
            Val.Zzz = StepSpring("Zzz", targetZzz, h, Val.Zzz, LeoMoodPresets.SpringParams["Default"]);
        }

        // Periodic natural blinking
        if (_blinkTimer >= 0)
        {
            _blinkTimer += step;
            if (_blinkTimer > 0.16)
            {
                _blinkTimer = -1.0;
                _nextBlinkAt = Clock + 2.0 + _prng.NextDouble() * 3.0;
            }
        }
        else if (Clock >= _nextBlinkAt && CurrentMood != LeoMood.Sleepy)
        {
            _blinkTimer = 0.0;
        }
    }

    private double StepSpring(string key, double target, double h, double current, (double omega0, double zeta) sp)
    {
        double v = Vel.TryGetValue(key, out var velVal) ? velVal : 0.0;
        double force = (target - current) * sp.omega0 - 2.0 * Math.Sqrt(sp.omega0) * sp.zeta * v;
        v += force * h;
        Vel[key] = v;
        return current + v * h;
    }

    public double GetBlinkScale()
    {
        if (_blinkTimer < 0) return 1.0;
        return Math.Max(0.0, 1.0 - Math.Sin((Math.PI * _blinkTimer) / 0.16));
    }

    public double GetBreathingOffset(bool reducedMotion = false)
    {
        if (reducedMotion || CurrentMood == LeoMood.Offline) return 0.0;
        double freq = CurrentMood == LeoMood.Sleepy ? 1.0 : CurrentMood == LeoMood.Listening ? 2.4 : 1.6;
        double amp = CurrentMood == LeoMood.Sleepy ? 0.8 : 1.2;
        return Math.Sin(Clock * freq) * amp;
    }

    public (double sx, double sy) GetSquashStretch()
    {
        double s = Val.Stretch;
        return (1.0 + s * 0.25, 1.0 - s * 0.20);
    }

    public void TriggerImpulse(double intensity = 1.0)
    {
        Vel["Stretch"] = (Vel.TryGetValue("Stretch", out var s) ? s : 0.0) + 12.0 * intensity;
        Vel["LookY"] = (Vel.TryGetValue("LookY", out var ly) ? ly : 0.0) - 2.5 * intensity;
    }
}
