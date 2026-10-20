using System.Collections.Generic;

namespace Leo.Core.Companion;

public enum LeoMood
{
    Idle,
    Hover,
    Listening,
    Thinking,
    Happy,
    Talking,
    Concentrate,
    Sleepy,
    Offline
}

public class LeoMoodState
{
    public double OpenL { get; set; } = 1.0;
    public double OpenR { get; set; } = 1.0;
    public double EyeSize { get; set; } = 1.0;
    public double Squint { get; set; } = 0.0;
    public double LookX { get; set; } = 0.0;
    public double LookY { get; set; } = 0.0;
    public double BrowTilt { get; set; } = 0.0;
    public double Glow { get; set; } = 0.3;
    public double Stretch { get; set; } = 0.0;
    public double Dots { get; set; } = 0.0;
    public double Zzz { get; set; } = 0.0;

    public LeoMoodState Clone()
    {
        return new LeoMoodState
        {
            OpenL = this.OpenL,
            OpenR = this.OpenR,
            EyeSize = this.EyeSize,
            Squint = this.Squint,
            LookX = this.LookX,
            LookY = this.LookY,
            BrowTilt = this.BrowTilt,
            Glow = this.Glow,
            Stretch = this.Stretch,
            Dots = this.Dots,
            Zzz = this.Zzz
        };
    }
}

public static class LeoMoodPresets
{
    public static readonly Dictionary<LeoMood, (double openL, double openR, double eyeSize, double squint, double lookX, double lookY, double browTilt, double glow, double dots, double zzz)> Presets =
        new()
        {
            [LeoMood.Idle]        = (1.0, 1.0, 1.00, 0.0,  0.0,   0.0,   0.0,   0.30, 0.0, 0.0),
            [LeoMood.Hover]       = (1.0, 1.0, 1.06, 0.0,  0.0,   0.0,   0.0,   0.55, 0.0, 0.0),
            [LeoMood.Listening]   = (1.0, 1.0, 1.08, 0.0,  0.0,  -0.1,   0.0,   0.50, 0.0, 0.0),
            [LeoMood.Thinking]    = (1.0, 1.0, 1.00, 0.0,  0.65, -0.85, -0.15,  0.35, 1.0, 0.0),
            [LeoMood.Happy]       = (0.8, 0.8, 1.00, 0.70, 0.0,   0.0,   0.0,   0.80, 0.0, 0.0),
            [LeoMood.Talking]     = (1.0, 1.0, 1.02, 0.0,  0.0,   0.0,   0.0,   0.45, 0.0, 0.0),
            [LeoMood.Concentrate] = (1.0, 1.0, 1.04, 0.0, -0.1,  -0.5,   0.0,   0.40, 0.0, 0.0),
            [LeoMood.Sleepy]      = (0.0, 0.0, 1.00, 0.0,  0.0,   0.0,   0.0,   0.05, 0.0, 1.0),
            [LeoMood.Offline]     = (0.5, 0.5, 0.95, 0.0,  0.0,   0.0,   0.0,   0.10, 0.0, 0.0)
        };

    public static readonly Dictionary<string, (double omega0, double zeta)> SpringParams =
        new()
        {
            ["LookX"]     = (300.0, 0.60),
            ["LookY"]     = (300.0, 0.60),
            ["BrowTilt"]  = (150.0, 0.60),
            ["EyeSize"]   = (180.0, 0.60),
            ["Glow"]      = (120.0, 0.85),
            ["Stretch"]   = (320.0, 0.45),
            ["Default"]   = (220.0, 0.75)
        };
}
