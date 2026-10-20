using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Leo.Application.Interfaces;
using Leo.Application.Services;
using Leo.Core.Companion;
using Leo.Core.Models;
using Xunit;

namespace Leo.Core.Tests;

public class FakeAiProvider : IAiProvider
{
    public AiProviderType ProviderType => AiProviderType.Gemini;
    public string CurrentModel { get; set; } = "fake-model-1";

    public List<string> TokensToEmit { get; set; } = ["Hello", " ", "there!", " How", " can", " I", " help?"];
    public int DelayPerTokenMs { get; set; } = 5;
    public bool ShouldThrow { get; set; } = false;

    public async IAsyncEnumerable<string> StreamChatAsync(
        IEnumerable<ChatMessage> history,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (ShouldThrow)
        {
            throw new InvalidOperationException("Simulated provider failure");
        }

        foreach (var token in TokensToEmit)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (DelayPerTokenMs > 0)
            {
                await Task.Delay(DelayPerTokenMs, cancellationToken);
            }
            yield return token;
        }
    }

    public Task<ConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (ShouldThrow)
        {
            return Task.FromResult(new ConnectionTestResult
            {
                Ok = false,
                Message = "Simulated failure",
                IsLocal = false,
                Model = CurrentModel
            });
        }

        return Task.FromResult(new ConnectionTestResult
        {
            Ok = true,
            Message = "Connected OK",
            IsLocal = false,
            Model = CurrentModel
        });
    }
}

public class ApplicationTests
{
    [Fact]
    public async Task LeoConversationManager_StreamsResponseAndTriggersGestures()
    {
        var engine = new LeoCharacterEngine(42);
        var gestures = new LeoGestureController(engine);
        var fakeProvider = new FakeAiProvider();
        var manager = new LeoConversationManager(engine, gestures, fakeProvider);

        var receivedTokens = new List<string>();
        manager.OnTokenReceived += (token) => receivedTokens.Add(token);

        bool completed = false;
        manager.OnGenerationCompleted += () => completed = true;

        await manager.SendMessageAsync("What is 2 + 2?");

        Assert.True(completed);
        Assert.False(manager.IsGenerating);
        Assert.Equal(2, manager.Messages.Count); // 1 user + 1 assistant
        Assert.Equal("user", manager.Messages[0].Role);
        Assert.Equal("What is 2 + 2?", manager.Messages[0].Content);
        Assert.Equal("assistant", manager.Messages[1].Role);
        Assert.Equal("Hello there! How can I help?", manager.Messages[1].Content);
        Assert.NotEmpty(receivedTokens);
    }

    [Fact]
    public async Task LeoConversationManager_VoicePrompt_CleansFillersBeforeSending()
    {
        var engine = new LeoCharacterEngine(42);
        var gestures = new LeoGestureController(engine);
        var fakeProvider = new FakeAiProvider();
        var manager = new LeoConversationManager(engine, gestures, fakeProvider);

        await manager.SendMessageAsync("Um, hello, uh, what is the date?", isVoice: true);

        Assert.Equal("Hello, what is the date?", manager.Messages[0].Content);
    }

    [Fact]
    public async Task LeoConversationManager_SupportsCancellation()
    {
        var engine = new LeoCharacterEngine(42);
        var gestures = new LeoGestureController(engine);
        var fakeProvider = new FakeAiProvider { DelayPerTokenMs = 100 };
        var manager = new LeoConversationManager(engine, gestures, fakeProvider);

        var sendTask = manager.SendMessageAsync("Take a long time");

        // Give it a moment to begin generating
        await Task.Delay(20);
        Assert.True(manager.IsGenerating);

        // Cancel
        manager.CancelGeneration();
        await sendTask;

        Assert.False(manager.IsGenerating);
    }

    [Fact]
    public async Task LeoConversationManager_HandlesErrorsGracefully()
    {
        var engine = new LeoCharacterEngine(42);
        var gestures = new LeoGestureController(engine);
        var fakeProvider = new FakeAiProvider { ShouldThrow = true };
        var manager = new LeoConversationManager(engine, gestures, fakeProvider);

        string? caughtError = null;
        manager.OnError += (err) => caughtError = err;

        await manager.SendMessageAsync("This will fail");

        Assert.NotNull(caughtError);
        Assert.Contains("Simulated provider failure", caughtError);
        Assert.False(manager.IsGenerating);
        Assert.Equal(LeoMood.Idle, engine.CurrentMood);
    }

    [Fact]
    public void AppSettings_HasSafeSensibleDefaults()
    {
        var settings = new AppSettings();
        Assert.False(settings.ReducedMotion);
        Assert.False(settings.LaunchAtStartup);
        Assert.Equal(AiProviderType.Gemini, settings.AiConfig.ProviderType);
        Assert.Equal("gemini-2.5-flash", settings.AiConfig.ModelName);
        Assert.Equal("http://127.0.0.1:11434/v1", settings.AiConfig.LocalEndpointUrl);
    }
}
