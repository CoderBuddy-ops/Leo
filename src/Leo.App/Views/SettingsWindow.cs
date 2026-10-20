using System;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;
using Leo.Application.Interfaces;
using Leo.Core.Models;
using Leo.Infrastructure.Providers;
using Leo.Infrastructure.Security;

namespace Leo.App.Views;

public class SettingsWindow : Window
{
    private readonly ISettingsService _settingsService;
    private readonly GeminiProvider _geminiProvider;
    private readonly LocalAiProvider _localProvider;
    private readonly Action<IAiProvider> _onProviderChanged;

    private readonly ComboBox _providerCombo = new();
    private readonly PasswordBox _geminiKeyBox = new();
    private readonly ComboBox _geminiModelCombo = new();
    private readonly Button _testGeminiBtn = new();
    private readonly TextBlock _geminiStatusText = new();

    private readonly TextBox _localUrlBox = new();
    private readonly ComboBox _localModelCombo = new();
    private readonly Button _detectLocalBtn = new();
    private readonly Button _testLocalBtn = new();
    private readonly TextBlock _localStatusText = new();

    private readonly CheckBox _reducedMotionBox = new();
    private readonly CheckBox _launchAtStartupBox = new();
    private readonly CheckBox _muteAudioBox = new();
    private readonly Slider _volumeSlider = new();

    private readonly StackPanel _geminiPanel = new();
    private readonly StackPanel _localPanel = new();
    private readonly TextBlock _privacyBadgeText = new();

    public SettingsWindow(
        ISettingsService settingsService,
        GeminiProvider geminiProvider,
        LocalAiProvider localProvider,
        Action<IAiProvider> onProviderChanged)
    {
        _settingsService = settingsService;
        _geminiProvider = geminiProvider;
        _localProvider = localProvider;
        _onProviderChanged = onProviderChanged;

        Title = "Leo Settings";

        IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        appWindow.Resize(new SizeInt32(520, 680));

        BuildUi();
        LoadCurrentValues();
    }

    private void BuildUi()
    {
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Padding = new Thickness(24)
        };

        var root = new StackPanel { Spacing = 20 };

        // Header
        var header = new TextBlock
        {
            Text = "Leo Settings & Preferences",
            FontSize = 20,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White)
        };
        root.Children.Add(header);

        // Section 1: AI Provider Selection
        var providerCard = CreateSectionCard("AI Provider & Models");
        var providerStack = new StackPanel { Spacing = 12 };

        _providerCombo.Items.Add("Google Gemini (Cloud AI)");
        _providerCombo.Items.Add("Local AI (Ollama / LM Studio)");
        _providerCombo.SelectedIndex = _settingsService.CurrentSettings.AiConfig.ProviderType == AiProviderType.Gemini ? 0 : 1;
        _providerCombo.SelectionChanged += (s, e) => UpdateProviderView();

        providerStack.Children.Add(new TextBlock { Text = "Active Provider:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        providerStack.Children.Add(_providerCombo);

        // Privacy indicator
        var privacyBadgeBorder = new Border
        {
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(10, 6, 10, 6),
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(30, 59, 130, 246)),
            BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(100, 59, 130, 246)),
            BorderThickness = new Thickness(1)
        };
        _privacyBadgeText.FontSize = 12;
        privacyBadgeBorder.Child = _privacyBadgeText;
        providerStack.Children.Add(privacyBadgeBorder);

        // Gemini controls
        _geminiPanel.Spacing = 8;
        _geminiPanel.Children.Add(new TextBlock { Text = "Gemini API Key (stored securely in Windows Credential Manager):", FontSize = 12 });
        _geminiKeyBox.PasswordRevealMode = PasswordRevealMode.Peek;
        _geminiKeyBox.PlaceholderText = "Paste your Google AI API key here";
        _geminiPanel.Children.Add(_geminiKeyBox);

        _geminiPanel.Children.Add(new TextBlock { Text = "Model:", FontSize = 12 });
        foreach (var m in GeminiProvider.SupportedModels)
        {
            _geminiModelCombo.Items.Add(m);
        }
        _geminiModelCombo.SelectedItem = _geminiProvider.CurrentModel;
        _geminiPanel.Children.Add(_geminiModelCombo);

        _testGeminiBtn.Content = "Test Gemini Connection";
        _testGeminiBtn.Click += async (s, e) => await TestGeminiAsync();
        _geminiPanel.Children.Add(_testGeminiBtn);
        _geminiStatusText.FontSize = 12;
        _geminiPanel.Children.Add(_geminiStatusText);

        // Local AI controls
        _localPanel.Spacing = 8;
        _localPanel.Children.Add(new TextBlock { Text = "Local Server Endpoint (OpenAI-compatible):", FontSize = 12 });
        _localUrlBox.Text = _localProvider.EndpointUrl;
        _localPanel.Children.Add(_localUrlBox);

        var localBtnRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        _detectLocalBtn.Content = "Detect Installed Models";
        _detectLocalBtn.Click += async (s, e) => await DetectLocalModelsAsync();
        _testLocalBtn.Content = "Test Connection";
        _testLocalBtn.Click += async (s, e) => await TestLocalAsync();
        localBtnRow.Children.Add(_detectLocalBtn);
        localBtnRow.Children.Add(_testLocalBtn);
        _localPanel.Children.Add(localBtnRow);

        _localPanel.Children.Add(new TextBlock { Text = "Installed Model:", FontSize = 12 });
        _localModelCombo.PlaceholderText = "Select or type model name";
        _localPanel.Children.Add(_localModelCombo);
        _localStatusText.FontSize = 12;
        _localPanel.Children.Add(_localStatusText);

        providerStack.Children.Add(_geminiPanel);
        providerStack.Children.Add(_localPanel);
        providerCard.Child = providerStack;
        root.Children.Add(providerCard);

        // Section 2: Audio & Voice
        var audioCard = CreateSectionCard("Voice & Audio Preferences");
        var audioStack = new StackPanel { Spacing = 10 };

        _muteAudioBox.Content = "Mute audio response playback";
        _muteAudioBox.IsChecked = _settingsService.CurrentSettings.AudioMuted;
        audioStack.Children.Add(_muteAudioBox);

        audioStack.Children.Add(new TextBlock { Text = "Speech Volume:", FontSize = 12 });
        _volumeSlider.Minimum = 0;
        _volumeSlider.Maximum = 100;
        _volumeSlider.Value = _settingsService.CurrentSettings.SpeechVolume * 100.0;
        audioStack.Children.Add(_volumeSlider);

        audioCard.Child = audioStack;
        root.Children.Add(audioCard);

        // Section 3: Appearance & System
        var systemCard = CreateSectionCard("System & Animation");
        var systemStack = new StackPanel { Spacing = 10 };

        _reducedMotionBox.Content = "Reduced motion (halting continuous floating oscillation)";
        _reducedMotionBox.IsChecked = _settingsService.CurrentSettings.ReducedMotion;
        systemStack.Children.Add(_reducedMotionBox);

        _launchAtStartupBox.Content = "Launch Leo at Windows sign-in";
        _launchAtStartupBox.IsChecked = _settingsService.CurrentSettings.LaunchAtStartup;
        systemStack.Children.Add(_launchAtStartupBox);

        systemCard.Child = systemStack;
        root.Children.Add(systemCard);

        // Action buttons
        var actionRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12, HorizontalAlignment = HorizontalAlignment.Right };
        var saveBtn = new Button
        {
            Content = "Save & Apply Settings",
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 37, 99, 235)),
            Foreground = new SolidColorBrush(Colors.White)
        };
        saveBtn.Click += async (s, e) => await SaveSettingsAsync();

        var closeBtn = new Button { Content = "Close" };
        closeBtn.Click += (s, e) => this.Close();

        actionRow.Children.Add(saveBtn);
        actionRow.Children.Add(closeBtn);
        root.Children.Add(actionRow);

        scrollViewer.Content = root;
        this.Content = scrollViewer;
    }

    private void LoadCurrentValues()
    {
        _geminiKeyBox.Password = _geminiProvider.GetApiKey();
        _geminiModelCombo.SelectedItem = _geminiProvider.CurrentModel;
        _localUrlBox.Text = _localProvider.EndpointUrl;
        _localModelCombo.Items.Add(_localProvider.CurrentModel);
        _localModelCombo.SelectedItem = _localProvider.CurrentModel;

        UpdateProviderView();
    }

    private void UpdateProviderView()
    {
        bool isGemini = _providerCombo.SelectedIndex == 0;
        _geminiPanel.Visibility = isGemini ? Visibility.Visible : Visibility.Collapsed;
        _localPanel.Visibility = isGemini ? Visibility.Collapsed : Visibility.Visible;

        if (isGemini)
        {
            _privacyBadgeText.Text = "Leaves your PC: Prompts sent to Google Gemini cloud via your private API key.";
            _privacyBadgeText.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 96, 165, 250));
        }
        else
        {
            _privacyBadgeText.Text = "Local Only: Prompts processed entirely on your machine. Zero network egress.";
            _privacyBadgeText.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 52, 211, 153));
        }
    }

    private async Task TestGeminiAsync()
    {
        _testGeminiBtn.IsEnabled = false;
        _geminiStatusText.Text = "Testing connection...";
        _geminiStatusText.Foreground = new SolidColorBrush(Colors.LightGray);

        _geminiProvider.SetApiKey(_geminiKeyBox.Password);
        if (_geminiModelCombo.SelectedItem is string m)
        {
            _geminiProvider.CurrentModel = m;
        }

        var result = await _geminiProvider.TestConnectionAsync();
        _geminiStatusText.Text = result.Message;
        _geminiStatusText.Foreground = result.Ok
            ? new SolidColorBrush(Colors.LightGreen)
            : new SolidColorBrush(Colors.OrangeRed);

        _testGeminiBtn.IsEnabled = true;
    }

    private async Task DetectLocalModelsAsync()
    {
        _detectLocalBtn.IsEnabled = false;
        _localStatusText.Text = "Detecting models...";
        _localProvider.EndpointUrl = _localUrlBox.Text.Trim();

        var models = await _localProvider.DiscoverModelsAsync();
        _localModelCombo.Items.Clear();
        foreach (var m in models)
        {
            _localModelCombo.Items.Add(m);
        }

        if (models.Count > 0)
        {
            _localModelCombo.SelectedIndex = 0;
            _localStatusText.Text = $"Found {models.Count} installed models.";
            _localStatusText.Foreground = new SolidColorBrush(Colors.LightGreen);
        }
        else
        {
            _localStatusText.Text = "No models detected. Ensure Ollama or LM Studio is running.";
            _localStatusText.Foreground = new SolidColorBrush(Colors.Orange);
        }

        _detectLocalBtn.IsEnabled = true;
    }

    private async Task TestLocalAsync()
    {
        _testLocalBtn.IsEnabled = false;
        _localStatusText.Text = "Testing local connection...";
        _localProvider.EndpointUrl = _localUrlBox.Text.Trim();
        if (_localModelCombo.SelectedItem is string m)
        {
            _localProvider.CurrentModel = m;
        }

        var result = await _localProvider.TestConnectionAsync();
        _localStatusText.Text = result.Message;
        _localStatusText.Foreground = result.Ok
            ? new SolidColorBrush(Colors.LightGreen)
            : new SolidColorBrush(Colors.OrangeRed);

        _testLocalBtn.IsEnabled = true;
    }

    private async Task SaveSettingsAsync()
    {
        var settings = _settingsService.CurrentSettings;
        bool isGemini = _providerCombo.SelectedIndex == 0;

        settings.AiConfig.ProviderType = isGemini ? AiProviderType.Gemini : AiProviderType.Local;
        if (isGemini)
        {
            _geminiProvider.SetApiKey(_geminiKeyBox.Password);
            if (_geminiModelCombo.SelectedItem is string gm)
            {
                _geminiProvider.CurrentModel = gm;
                settings.AiConfig.ModelName = gm;
            }
            _onProviderChanged(_geminiProvider);
        }
        else
        {
            _localProvider.EndpointUrl = _localUrlBox.Text.Trim();
            settings.AiConfig.LocalEndpointUrl = _localProvider.EndpointUrl;
            if (_localModelCombo.SelectedItem is string lm)
            {
                _localProvider.CurrentModel = lm;
                settings.AiConfig.ModelName = lm;
            }
            _onProviderChanged(_localProvider);
        }

        settings.AudioMuted = _muteAudioBox.IsChecked ?? false;
        settings.SpeechVolume = _volumeSlider.Value / 100.0;
        settings.ReducedMotion = _reducedMotionBox.IsChecked ?? false;
        settings.LaunchAtStartup = _launchAtStartupBox.IsChecked ?? false;

        await _settingsService.SaveSettingsAsync(settings);

        this.Close();
    }

    private static Border CreateSectionCard(string title)
    {
        var card = new Border
        {
            CornerRadius = new CornerRadius(8),
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(20, 255, 255, 255)),
            BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(40, 255, 255, 255)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(16)
        };
        return card;
    }
}
