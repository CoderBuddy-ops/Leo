using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Leo.Application.Interfaces;
using Leo.Core.Companion;
using Leo.Core.Models;
using Leo.Core.Voice;

namespace Leo.Application.Services;

public class LeoConversationManager
{
    private readonly List<ChatMessage> _messages = new();
    private readonly LeoCharacterEngine _engine;
    private readonly LeoGestureController _gestures;
    private IAiProvider _activeProvider;
    private CancellationTokenSource? _activeCts;

    public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();
    public bool IsGenerating { get; private set; } = false;

    public event Action<string>? OnTokenReceived;
    public event Action<ChatMessage>? OnMessageAdded;
    public event Action<string>? OnError;
    public event Action? OnGenerationCompleted;

    public LeoConversationManager(LeoCharacterEngine engine, LeoGestureController gestures, IAiProvider defaultProvider)
    {
        _engine = engine;
        _gestures = gestures;
        _activeProvider = defaultProvider;
    }

    public void SetActiveProvider(IAiProvider provider)
    {
        _activeProvider = provider;
    }

    public void AddSystemMessage(string content)
    {
        var msg = new ChatMessage("system", content);
        _messages.Add(msg);
        OnMessageAdded?.Invoke(msg);
    }

    public void ClearHistory()
    {
        _messages.Clear();
    }

    public void CancelGeneration()
    {
        if (IsGenerating && _activeCts != null)
        {
            _activeCts.Cancel();
            _gestures.SetProcessingPrompt(false);
            _gestures.SetSpeaking(false);
            IsGenerating = false;
        }
    }

    public async Task SendMessageAsync(string prompt, bool isVoice = false)
    {
        if (string.IsNullOrWhiteSpace(prompt) || IsGenerating)
            return;

        string cleanedPrompt = isVoice ? SpeechCleanupFilter.Clean(prompt) : prompt.Trim();
        var userMessage = new ChatMessage("user", cleanedPrompt);
        _messages.Add(userMessage);
        OnMessageAdded?.Invoke(userMessage);

        IsGenerating = true;
        _activeCts = new CancellationTokenSource();
        var ct = _activeCts.Token;

        _gestures.SetProcessingPrompt(true);

        var assistantMessage = new ChatMessage("assistant", string.Empty);
        _messages.Add(assistantMessage);
        OnMessageAdded?.Invoke(assistantMessage);

        var responseBuilder = new StringBuilder();

        try
        {
            bool hasStartedSpeaking = false;

            await foreach (string token in _activeProvider.StreamChatAsync(_messages, ct))
            {
                if (!hasStartedSpeaking)
                {
                    _gestures.SetProcessingPrompt(false);
                    _gestures.SetSpeaking(true);
                    hasStartedSpeaking = true;
                }

                responseBuilder.Append(token);
                assistantMessage.Content = responseBuilder.ToString();
                OnTokenReceived?.Invoke(token);
            }

            _gestures.TriggerHappyReaction();
        }
        catch (OperationCanceledException)
        {
            if (responseBuilder.Length == 0)
            {
                assistantMessage.Content = "[Generation cancelled]";
            }
        }
        catch (Exception ex)
        {
            string safeError = ex.Message;
            OnError?.Invoke(safeError);
            assistantMessage.Content = $"Error: {safeError}";
            _engine.SetMood(LeoMood.Idle);
        }
        finally
        {
            _gestures.SetSpeaking(false);
            _gestures.SetProcessingPrompt(false);
            IsGenerating = false;
            _activeCts?.Dispose();
            _activeCts = null;
            OnGenerationCompleted?.Invoke();
        }
    }
}
