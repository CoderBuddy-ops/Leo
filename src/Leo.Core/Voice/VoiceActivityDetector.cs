using System;

namespace Leo.Core.Voice;

public class VADConfig
{
    public int SampleRate { get; set; } = 16000;
    public int FrameSize { get; set; } = 512;
    public double EnergyThreshold { get; set; } = 0.015;
    public int SilenceHangoverFrames { get; set; } = 15; // ~480ms hangover
    public int SpeechTriggerFrames { get; set; } = 3;    // ~96ms sustained speech
}

public class VADFrameResult
{
    public double Rms { get; set; }
    public double Zcr { get; set; }
    public bool IsSpeechFrame { get; set; }
    public string State { get; set; } = "silence";
}

/// <summary>
/// Leo Voice Activity Detector (VAD).
/// Local-Only Invariant: Audio data NEVER leaves this device. All signal analysis runs in memory.
/// Processes 16kHz mono PCM frames using Root Mean Square (RMS) energy + Zero Crossing Rate (ZCR)
/// with a hangover filter to prevent clipping speech endings.
/// </summary>
public class VoiceActivityDetector
{
    private readonly VADConfig _config;
    private int _consecutiveSpeechFrames = 0;
    private int _consecutiveSilenceFrames = 0;
    private string _currentState = "silence";

    public VoiceActivityDetector(VADConfig? config = null)
    {
        _config = config ?? new VADConfig();
    }

    public static double CalculateRMS(ReadOnlySpan<float> samples)
    {
        if (samples.Length == 0) return 0.0;
        double sumSquares = 0.0;
        for (int i = 0; i < samples.Length; i++)
        {
            sumSquares += samples[i] * samples[i];
        }
        return Math.Sqrt(sumSquares / samples.Length);
    }

    public static double CalculateZCR(ReadOnlySpan<float> samples)
    {
        if (samples.Length < 2) return 0.0;
        int crossings = 0;
        for (int i = 1; i < samples.Length; i++)
        {
            if ((samples[i] >= 0 && samples[i - 1] < 0) || (samples[i] < 0 && samples[i - 1] >= 0))
            {
                crossings++;
            }
        }
        return (double)crossings / (samples.Length - 1);
    }

    public VADFrameResult ProcessFrame(ReadOnlySpan<float> samples)
    {
        double rms = CalculateRMS(samples);
        double zcr = CalculateZCR(samples);

        bool isAboveThreshold = rms >= _config.EnergyThreshold;

        if (isAboveThreshold)
        {
            _consecutiveSpeechFrames++;
            _consecutiveSilenceFrames = 0;

            if (_consecutiveSpeechFrames >= _config.SpeechTriggerFrames)
            {
                _currentState = "speaking";
            }
        }
        else
        {
            _consecutiveSilenceFrames++;
            _consecutiveSpeechFrames = 0;

            if (_currentState == "speaking")
            {
                if (_consecutiveSilenceFrames >= _config.SilenceHangoverFrames)
                {
                    _currentState = "silence";
                }
            }
        }

        return new VADFrameResult
        {
            Rms = rms,
            Zcr = zcr,
            IsSpeechFrame = isAboveThreshold,
            State = _currentState
        };
    }

    public void Reset()
    {
        _consecutiveSpeechFrames = 0;
        _consecutiveSilenceFrames = 0;
        _currentState = "silence";
    }
}
