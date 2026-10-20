using System;

namespace Leo.Core.Companion;

public class GestureControllerConfig
{
    public double InactivityTimeoutMs { get; set; } = 120000.0;
}

public class LeoGestureController
{
    private readonly LeoCharacterEngine _engine;
    private long _lastActivityTimestamp = Environment.TickCount64;
    private readonly double _inactivityTimeoutMs;
    private bool _isVoiceActive = false;
    private bool _isProcessingPrompt = false;

    public LeoGestureController(LeoCharacterEngine engine, GestureControllerConfig? config = null)
    {
        _engine = engine;
        _inactivityTimeoutMs = config?.InactivityTimeoutMs ?? 120000.0;
    }

    public void NotifyUserActivity()
    {
        _lastActivityTimestamp = Environment.TickCount64;
        if (_engine.CurrentMood == LeoMood.Sleepy)
        {
            _engine.SetMood(LeoMood.Idle);
        }
    }

    public void SetVoiceListening(bool listening)
    {
        _isVoiceActive = listening;
        NotifyUserActivity();
        if (listening)
        {
            _engine.SetMood(LeoMood.Listening);
        }
        else if (!_isProcessingPrompt)
        {
            _engine.SetMood(LeoMood.Idle);
        }
    }

    public void SetProcessingPrompt(bool processing)
    {
        _isProcessingPrompt = processing;
        NotifyUserActivity();
        if (processing)
        {
            _engine.SetMood(LeoMood.Thinking);
        }
        else if (!_isVoiceActive)
        {
            _engine.SetMood(LeoMood.Idle);
        }
    }

    public void SetSpeaking(bool speaking)
    {
        NotifyUserActivity();
        if (speaking)
        {
            _engine.SetMood(LeoMood.Talking);
        }
        else if (_isVoiceActive)
        {
            _engine.SetMood(LeoMood.Listening);
        }
        else if (_isProcessingPrompt)
        {
            _engine.SetMood(LeoMood.Thinking);
        }
        else
        {
            _engine.SetMood(LeoMood.Idle);
        }
    }

    public void TriggerHappyReaction()
    {
        NotifyUserActivity();
        _engine.SetMood(LeoMood.Happy);
    }

    public void Update()
    {
        long now = Environment.TickCount64;
        if (!_isVoiceActive &&
            !_isProcessingPrompt &&
            _engine.CurrentMood != LeoMood.Sleepy &&
            (now - _lastActivityTimestamp) >= _inactivityTimeoutMs)
        {
            _engine.SetMood(LeoMood.Sleepy);
        }
    }

    public LeoMood CurrentMood => _engine.CurrentMood;
}
