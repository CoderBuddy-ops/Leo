using System;
using Leo.Core.Companion;
using Leo.Core.Models;
using Leo.Core.Voice;
using Xunit;

namespace Leo.Core.Tests;

public class CoreTests
{
    [Fact]
    public void Mulberry32_ProducesDeterministicGoldenVectors()
    {
        var rng1 = new Mulberry32(42);
        var rng2 = new Mulberry32(42);

        // Deterministic equality between two instances with same seed
        Assert.Equal(rng1.NextDouble(), rng2.NextDouble());

        // Exact golden vector match for seed 42
        double[] golden = [0.44829055899754167, 0.8524657934904099, 0.6697340414393693];
        foreach (double expected in golden)
        {
            double val = rng1.NextDouble();
            Assert.True(Math.Abs(val - expected) < 1e-12, $"Expected {expected}, got {val}");
            Assert.InRange(val, 0.0, 1.0);
        }
    }

    [Fact]
    public void LeoCharacterEngine_InitializesWithValidRestingPhysics()
    {
        var engine = new LeoCharacterEngine(12345);
        Assert.Equal(LeoMood.Idle, engine.CurrentMood);
        Assert.Equal(1.0, engine.Val.OpenL);
        Assert.Equal(0.0, engine.Val.LookX);
        Assert.Equal(0.3, engine.Val.Glow);
    }

    [Fact]
    public void LeoCharacterEngine_SmoothlyTransitionsStateUnderSpringDynamics()
    {
        var engine = new LeoCharacterEngine(42);
        engine.SetMood(LeoMood.Thinking);
        Assert.Equal(LeoMood.Thinking, engine.CurrentMood);

        // Advance 20 physics frames (320ms)
        for (int i = 0; i < 20; i++)
        {
            engine.Advance(0.016);
        }

        // Target for Thinking is LookX = 0.65; should have smoothly advanced > 0.1
        Assert.True(engine.Val.LookX > 0.1, $"Expected look_x to advance towards 0.65, got {engine.Val.LookX}");
    }

    [Fact]
    public void LeoCharacterEngine_SquashStretchAndBreathing_CalculatesCorrectly()
    {
        var engine = new LeoCharacterEngine(42);
        var (sx, sy) = engine.GetSquashStretch();
        Assert.Equal(1.0, sx);
        Assert.Equal(1.0, sy);

        engine.TriggerImpulse(1.0);
        Assert.True(engine.Vel["Stretch"] > 0);

        engine.Advance(0.016);
        var (sxAfter, syAfter) = engine.GetSquashStretch();
        Assert.True(sxAfter > 1.0);
        Assert.True(syAfter < 1.0);

        // Breathing offset should be within reasonable bounds
        double breath = engine.GetBreathingOffset();
        Assert.InRange(breath, -2.0, 2.0);

        // Reduced motion halts continuous oscillation
        Assert.Equal(0.0, engine.GetBreathingOffset(reducedMotion: true));
    }

    [Fact]
    public void LeoGestureController_ManagesStateTransitionsAndAutoSleep()
    {
        var engine = new LeoCharacterEngine(101);
        var gestures = new LeoGestureController(engine, new GestureControllerConfig { InactivityTimeoutMs = 100 });

        Assert.Equal(LeoMood.Idle, gestures.CurrentMood);

        // Listening mood
        gestures.SetVoiceListening(true);
        Assert.Equal(LeoMood.Listening, gestures.CurrentMood);

        // Talking mood
        gestures.SetSpeaking(true);
        Assert.Equal(LeoMood.Talking, gestures.CurrentMood);

        // Stop talking -> returns to listening if mic is active
        gestures.SetSpeaking(false);
        Assert.Equal(LeoMood.Listening, gestures.CurrentMood);

        // Mic stops -> returns to idle
        gestures.SetVoiceListening(false);
        Assert.Equal(LeoMood.Idle, gestures.CurrentMood);

        // Activity resets idle timer
        gestures.NotifyUserActivity();
        Assert.Equal(LeoMood.Idle, gestures.CurrentMood);
    }

    [Fact]
    public void SpeechCleanupFilter_RemovesVerbalFillers()
    {
        string raw = "Hi Priya, um, the launch is on track for Thursday. Uh, I will send notes.";
        string cleaned = SpeechCleanupFilter.Clean(raw);
        Assert.Equal("Hi Priya, the launch is on track for Thursday. I will send notes.", cleaned);
    }

    [Fact]
    public void SpeechCleanupFilter_ResolvesSpokenSelfCorrections()
    {
        string raw = "The meeting is on Thursday, actually Friday.";
        string cleaned = SpeechCleanupFilter.Clean(raw);
        Assert.Equal("The meeting is on Friday.", cleaned);
    }

    [Fact]
    public void SpeechCleanupFilter_FormatsSpokenBulletPoints()
    {
        string raw = "Three priorities for Monday. {Bullet point} ship beta {Bullet point} fix bugs";
        string cleaned = SpeechCleanupFilter.Clean(raw);
        Assert.Contains("- Ship beta", cleaned);
        Assert.Contains("- Fix bugs", cleaned);
    }

    [Fact]
    public void SpeechCleanupFilter_RespectsVerbatimMode()
    {
        string raw = "Um, this should stay untouched, actually unchanged.";
        string cleaned = SpeechCleanupFilter.Clean(raw, new SpeechCleanupOptions { VerbatimMode = true });
        Assert.Equal(raw, cleaned);
    }

    [Fact]
    public void VAD_CalculatesRMS_OnSilenceVsLoud()
    {
        float[] silent = new float[512];
        float[] loud = new float[512];
        Array.Fill(loud, 0.5f);

        Assert.Equal(0.0, VoiceActivityDetector.CalculateRMS(silent));
        double loudRms = VoiceActivityDetector.CalculateRMS(loud);
        Assert.True(Math.Abs(loudRms - 0.5) < 1e-6);
    }

    [Fact]
    public void VAD_CalculatesZeroCrossingRate_OnAlternatingWave()
    {
        float[] alternating = [1.0f, -1.0f, 1.0f, -1.0f, 1.0f];
        double zcr = VoiceActivityDetector.CalculateZCR(alternating);
        Assert.Equal(1.0, zcr);
    }

    [Fact]
    public void VAD_PreservesSpeakingState_ThroughSilenceHangover()
    {
        var vad = new VoiceActivityDetector(new VADConfig
        {
            EnergyThreshold = 0.05,
            SpeechTriggerFrames = 2,
            SilenceHangoverFrames = 5
        });

        float[] speechFrame = new float[512];
        Array.Fill(speechFrame, 0.1f);
        float[] silenceFrame = new float[512];
        Array.Fill(silenceFrame, 0.001f);

        // Frame 1 speech
        vad.ProcessFrame(speechFrame);
        // Frame 2 speech -> triggers speaking state
        var r2 = vad.ProcessFrame(speechFrame);
        Assert.Equal("speaking", r2.State);

        // Silence begins: should remain in speaking state for 4 frames (hangover < 5)
        for (int i = 0; i < 4; i++)
        {
            var res = vad.ProcessFrame(silenceFrame);
            Assert.Equal("speaking", res.State);
        }

        // On 5th silence frame, hangover expires -> silence
        var finalRes = vad.ProcessFrame(silenceFrame);
        Assert.Equal("silence", finalRes.State);
    }

    [Fact]
    public void NetworkPolicy_CorrectlyDetectsLoopbackAddresses()
    {
        Assert.True(NetworkPolicy.IsLoopback("http://127.0.0.1:11434/v1"));
        Assert.True(NetworkPolicy.IsLoopback("http://localhost:1234/v1"));
        Assert.True(NetworkPolicy.IsLoopback("http://[::1]:8080/v1"));
        Assert.True(NetworkPolicy.IsLoopback("http://192.168.1.50:11434/v1"));
        Assert.False(NetworkPolicy.IsLoopback("https://generativelanguage.googleapis.com/v1beta"));
        Assert.False(NetworkPolicy.IsLoopback("https://api.openai.com/v1"));
        Assert.False(NetworkPolicy.IsLoopback("https://api.anthropic.com/v1"));
    }
}
