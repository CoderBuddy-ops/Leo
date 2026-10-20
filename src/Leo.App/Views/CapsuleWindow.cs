using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Graphics;
using Leo.Application.Interfaces;
using Leo.Application.Services;
using Leo.Core.Companion;
using Leo.Core.Models;
using Leo.Infrastructure.Providers;

namespace Leo.App.Views;

public class CapsuleWindow : Window
{
    private readonly LeoCharacterEngine _engine;
    private readonly LeoGestureController _gestures;
    private readonly LeoConversationManager _conversation;
    private readonly ISettingsService _settingsService;
    private readonly GeminiProvider _geminiProvider;
    private readonly LocalAiProvider _localProvider;

    private readonly AppWindow _appWindow;
    private readonly IntPtr _hWnd;
    private readonly DispatcherQueueTimer _renderTimer;

    // Window dimensions
    private const int CapsuleWidth = 440;
    private const int CollapsedHeight = 84;
    private const int ExpandedHeight = 480;

    // Win32 Interop for DWM transparency and smooth dragging
    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    private const int WM_NCLBUTTONDOWN = 0x00A1;
    private const int HTCAPTION = 0x0002;

    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    // UI Elements
    private readonly Border _capsuleBorder = new();
    private readonly Grid _mainContainer = new();
    private readonly StackPanel _chatDrawer = new();
    private readonly Border _eyeLeft = new();
    private readonly Border _eyeRight = new();
    private readonly ScaleTransform _eyeLeftScale = new();
    private readonly ScaleTransform _eyeRightScale = new();
    private readonly TranslateTransform _faceTranslate = new();
    private readonly TextBlock _statusPill = new();
    private readonly Border _statusBadge = new();
    private readonly Button _micBtn = new();
    private readonly Button _chatToggleBtn = new();
    private readonly Button _settingsBtn = new();

    // Chat Drawer UI
    private readonly ScrollViewer _chatScrollViewer = new();
    private readonly StackPanel _messagesPanel = new();
    private readonly TextBox _inputBox = new();
    private readonly Button _sendBtn = new();
    private readonly Button _cancelBtn = new();
    private bool _isChatExpanded = false;

    private SettingsWindow? _settingsWindow;

    public CapsuleWindow(
        LeoCharacterEngine engine,
        LeoGestureController gestures,
        LeoConversationManager conversation,
        ISettingsService settingsService,
        GeminiProvider geminiProvider,
        LocalAiProvider localProvider)
    {
        _engine = engine;
        _gestures = gestures;
        _conversation = conversation;
        _settingsService = settingsService;
        _geminiProvider = geminiProvider;
        _localProvider = localProvider;

        _hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(_hWnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);

        // Native borderless floating window setup
        if (_appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.SetBorderAndTitleBar(false, false);
            presenter.IsAlwaysOnTop = true;
            presenter.IsMinimizable = false;
            presenter.IsMaximizable = false;
        }

        _appWindow.Resize(new SizeInt32(CapsuleWidth + 40, CollapsedHeight + 40));

        // Position horizontally centered near bottom of screen
        var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
        if (displayArea != null)
        {
            int x = (displayArea.WorkArea.Width - (CapsuleWidth + 40)) / 2;
            int y = displayArea.WorkArea.Height - (CollapsedHeight + 70);
            _appWindow.Move(new PointInt32(x, y));
        }

        LogStep("CapsuleWindow ctor start");
        // Apply DWM glass margins for genuine transparency outside the capsule
        var margins = new MARGINS { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
        DwmExtendFrameIntoClientArea(_hWnd, ref margins);
        LogStep("DWM margins applied");

        BuildCapsuleUi();
        LogStep("BuildCapsuleUi done");
        HookEvents();
        LogStep("HookEvents done");

        // 60 FPS animation timer for physics, blinking, breathing
        _renderTimer = DispatcherQueue.CreateTimer();
        _renderTimer.Interval = TimeSpan.FromMilliseconds(16.6); // ~60 fps
        _renderTimer.Tick += OnRenderTick;
        _renderTimer.Start();
        LogStep("RenderTimer started");
    }

    private static void LogStep(string msg)
    {
        string log = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "capsule_steps.log");
        System.IO.File.AppendAllText(log, msg + "\n");
    }

    private void BuildCapsuleUi()
    {
        LogStep("BuildCapsuleUi start");
        var rootGrid = new Grid
        {
            Background = null,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Padding = new Thickness(10)
        };

        _mainContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _mainContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Capsule Dock Bar
        _capsuleBorder.Width = CapsuleWidth;
        _capsuleBorder.Height = 64;
        _capsuleBorder.CornerRadius = new CornerRadius(32);
        _capsuleBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(238, 14, 16, 23)); // Deep dark glass
        _capsuleBorder.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(160, 59, 130, 246)); // Vibrant blue accent
        _capsuleBorder.BorderThickness = new Thickness(1.5);
        _capsuleBorder.Padding = new Thickness(16, 8, 16, 8);

        // Allow dragging window from capsule background
        _capsuleBorder.PointerPressed += (s, e) =>
        {
            if (e.GetCurrentPoint(null).Properties.IsLeftButtonPressed)
            {
                ReleaseCapture();
                SendMessage(_hWnd, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
            }
        };

        var barGrid = new Grid();
        barGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Face
        barGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Status / Title
        barGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Actions

        // 1. Procedural Face with Blush Marks and NO Blue Dot
        var faceContainer = CreateProceduralFace();
        Grid.SetColumn(faceContainer, 0);
        barGrid.Children.Add(faceContainer);

        // 2. Status Badge / Mode
        var statusStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(14, 0, 0, 0)
        };

        var nameText = new TextBlock
        {
            Text = "Leo",
            FontSize = 14,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.White)
        };

        _statusBadge.CornerRadius = new CornerRadius(8);
        _statusBadge.Padding = new Thickness(6, 2, 6, 2);
        _statusBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(40, 16, 185, 129));
        _statusBadge.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(100, 16, 185, 129));
        _statusBadge.BorderThickness = new Thickness(1);
        _statusBadge.HorizontalAlignment = HorizontalAlignment.Left;
        _statusBadge.Margin = new Thickness(0, 2, 0, 0);

        _statusPill.Text = "Ready";
        _statusPill.FontSize = 10;
        _statusPill.FontWeight = Microsoft.UI.Text.FontWeights.Medium;
        _statusPill.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 52, 211, 153));
        _statusBadge.Child = _statusPill;

        statusStack.Children.Add(nameText);
        statusStack.Children.Add(_statusBadge);
        Grid.SetColumn(statusStack, 1);
        barGrid.Children.Add(statusStack);

        // 3. Action Buttons (Mic, Chat, Settings)
        var btnStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            VerticalAlignment = VerticalAlignment.Center
        };

        _micBtn.Content = "🎤";
        _micBtn.Width = 36;
        _micBtn.Height = 36;
        _micBtn.CornerRadius = new CornerRadius(18);
        // ToolTipService.SetToolTip(_micBtn, "Toggle Voice Listening");
        _micBtn.Click += (s, e) => ToggleVoiceListening();

        _chatToggleBtn.Content = "💬";
        _chatToggleBtn.Width = 36;
        _chatToggleBtn.Height = 36;
        _chatToggleBtn.CornerRadius = new CornerRadius(18);
        // ToolTipService.SetToolTip(_chatToggleBtn, "Open/Close Chat");
        _chatToggleBtn.Click += (s, e) => ToggleChatDrawer();

        _settingsBtn.Content = "⚙";
        _settingsBtn.Width = 36;
        _settingsBtn.Height = 36;
        _settingsBtn.CornerRadius = new CornerRadius(18);
        // ToolTipService.SetToolTip(_settingsBtn, "Open Settings");
        _settingsBtn.Click += (s, e) => OpenSettings();

        btnStack.Children.Add(_micBtn);
        btnStack.Children.Add(_chatToggleBtn);
        btnStack.Children.Add(_settingsBtn);
        Grid.SetColumn(btnStack, 2);
        barGrid.Children.Add(btnStack);
        LogStep("barGrid columns populated");

        _capsuleBorder.Child = barGrid;
        Grid.SetRow(_capsuleBorder, 0);
        _mainContainer.Children.Add(_capsuleBorder);
        LogStep("_capsuleBorder added to _mainContainer");

        // 4. Expandable Chat Drawer
        BuildChatDrawer();
        LogStep("BuildChatDrawer done");
        Grid.SetRow(_chatDrawer, 1);
        _mainContainer.Children.Add(_chatDrawer);

        rootGrid.Children.Add(_mainContainer);
        LogStep("Setting this.Content = rootGrid");
        this.Content = rootGrid;
        LogStep("this.Content set");
    }

    private FrameworkElement CreateProceduralFace()
    {
        var faceGrid = new Grid
        {
            Width = 54,
            Height = 44,
            RenderTransform = _faceTranslate,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Clicking the face triggers a playful impulse
        faceGrid.PointerPressed += (s, e) =>
        {
            _engine.TriggerImpulse(1.0);
            _gestures.TriggerHappyReaction();
        };

        // Left Eye (Vertical oval)
        _eyeLeft.Width = 8;
        _eyeLeft.Height = 20;
        _eyeLeft.CornerRadius = new CornerRadius(4);
        _eyeLeft.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 96, 165, 250)); // Bright cyan-blue
        _eyeLeft.HorizontalAlignment = HorizontalAlignment.Left;
        _eyeLeft.VerticalAlignment = VerticalAlignment.Center;
        _eyeLeft.Margin = new Thickness(10, 0, 0, 0);
        _eyeLeft.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
        _eyeLeft.RenderTransform = _eyeLeftScale;

        // Right Eye (Vertical oval)
        _eyeRight.Width = 8;
        _eyeRight.Height = 20;
        _eyeRight.CornerRadius = new CornerRadius(4);
        _eyeRight.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 96, 165, 250));
        _eyeRight.HorizontalAlignment = HorizontalAlignment.Right;
        _eyeRight.VerticalAlignment = VerticalAlignment.Center;
        _eyeRight.Margin = new Thickness(0, 0, 10, 0);
        _eyeRight.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
        _eyeRight.RenderTransform = _eyeRightScale;

        // Subtle Symmetrical Pink Blush Marks
        var blushLeft = new Border
        {
            Width = 8,
            Height = 4,
            CornerRadius = new CornerRadius(2),
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(170, 255, 160, 175)), // Soft pink
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(8, 0, 0, 6)
        };

        var blushRight = new Border
        {
            Width = 8,
            Height = 4,
            CornerRadius = new CornerRadius(2),
            Background = new SolidColorBrush(Windows.UI.Color.FromArgb(170, 255, 160, 175)),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 8, 6)
        };

        faceGrid.Children.Add(_eyeLeft);
        faceGrid.Children.Add(_eyeRight);
        faceGrid.Children.Add(blushLeft);
        faceGrid.Children.Add(blushRight);

        return faceGrid;
    }

    private void BuildChatDrawer()
    {
        _chatDrawer.Width = CapsuleWidth;
        _chatDrawer.Height = 360;
        _chatDrawer.Margin = new Thickness(0, 10, 0, 0);
        _chatDrawer.Padding = new Thickness(16);
        _chatDrawer.CornerRadius = new CornerRadius(20);
        _chatDrawer.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(245, 16, 18, 26));
        _chatDrawer.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(120, 59, 130, 246));
        _chatDrawer.BorderThickness = new Thickness(1);
        _chatDrawer.Visibility = Visibility.Collapsed;

        // Messages List inside ScrollViewer
        _chatScrollViewer.Height = 270;
        _chatScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        _chatScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;

        _messagesPanel.Spacing = 6;
        _messagesPanel.VerticalAlignment = VerticalAlignment.Top;
        _chatScrollViewer.Content = _messagesPanel;
        _chatDrawer.Children.Add(_chatScrollViewer);

        // Input row
        var inputRow = new Grid { Margin = new Thickness(0, 8, 0, 0) };
        inputRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        inputRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        inputRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        _inputBox.PlaceholderText = "Ask Leo anything... (Enter to send)";
        _inputBox.KeyDown += async (s, e) =>
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                await SubmitPromptAsync();
            }
        };
        Grid.SetColumn(_inputBox, 0);
        inputRow.Children.Add(_inputBox);

        _sendBtn.Content = "Send";
        _sendBtn.Margin = new Thickness(6, 0, 0, 0);
        _sendBtn.Click += async (s, e) => await SubmitPromptAsync();
        Grid.SetColumn(_sendBtn, 1);
        inputRow.Children.Add(_sendBtn);

        _cancelBtn.Content = "Stop";
        _cancelBtn.Margin = new Thickness(6, 0, 0, 0);
        _cancelBtn.Visibility = Visibility.Collapsed;
        _cancelBtn.Click += (s, e) => _conversation.CancelGeneration();
        Grid.SetColumn(_cancelBtn, 2);
        inputRow.Children.Add(_cancelBtn);

        _chatDrawer.Children.Add(inputRow);
    }

    private static Border CreateMessageBubble(ChatMessage msg)
    {
        bool isUser = msg.Role == "user";
        var border = new Border
        {
            Margin = new Thickness(0, 4, 0, 4),
            Padding = new Thickness(12, 8, 12, 8),
            CornerRadius = new CornerRadius(12),
            MaxWidth = 360,
            HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            Background = isUser
                ? new SolidColorBrush(Windows.UI.Color.FromArgb(200, 37, 99, 235))
                : new SolidColorBrush(Windows.UI.Color.FromArgb(220, 30, 34, 48)),
            BorderBrush = isUser ? null : new SolidColorBrush(Windows.UI.Color.FromArgb(80, 59, 130, 246)),
            BorderThickness = isUser ? new Thickness(0) : new Thickness(1)
        };

        var sp = new StackPanel { Spacing = 2 };
        var roleText = new TextBlock
        {
            Text = isUser ? "You" : "Leo",
            FontSize = 10,
            FontWeight = Microsoft.UI.Text.FontWeights.Bold,
            Foreground = isUser
                ? new SolidColorBrush(Windows.UI.Color.FromArgb(220, 255, 255, 255))
                : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 96, 165, 250))
        };
        var contentText = new TextBlock
        {
            Text = msg.Content,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 12,
            Foreground = new SolidColorBrush(Colors.White)
        };

        sp.Children.Add(roleText);
        sp.Children.Add(contentText);
        border.Child = sp;
        return border;
    }

    private void HookEvents()
    {
        _conversation.OnMessageAdded += (msg) =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                var bubble = CreateMessageBubble(msg);
                _messagesPanel.Children.Add(bubble);
                _chatScrollViewer.ChangeView(null, _chatScrollViewer.ScrollableHeight, null, false);
            });
        };

        _conversation.OnTokenReceived += (token) =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (_messagesPanel.Children.Count > 0 &&
                    _messagesPanel.Children[^1] is Border border &&
                    border.Child is StackPanel sp &&
                    sp.Children.Count > 1 &&
                    sp.Children[1] is TextBlock tb)
                {
                    tb.Text = _conversation.Messages[^1].Content;
                    _chatScrollViewer.ChangeView(null, _chatScrollViewer.ScrollableHeight, null, false);
                }
            });
        };

        _conversation.OnError += (err) =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                UpdateStatus("Error", isError: true);
            });
        };

        _conversation.OnGenerationCompleted += () =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                _sendBtn.Visibility = Visibility.Visible;
                _cancelBtn.Visibility = Visibility.Collapsed;
                _inputBox.IsEnabled = true;
                UpdateStatus("Ready");
            });
        };
    }

    private void OnRenderTick(object? sender, object e)
    {
        try
        {
            double dt = 0.0166;
            _engine.Advance(dt);
            _gestures.Update();

            // 1. Natural Blinking
            double blink = _engine.GetBlinkScale();
            _eyeLeftScale.ScaleY = blink;
            _eyeRightScale.ScaleY = blink;

            // 2. Breathing Float
            bool reducedMotion = _settingsService.CurrentSettings.ReducedMotion;
            double breathY = _engine.GetBreathingOffset(reducedMotion);
            _faceTranslate.Y = breathY;

            // 3. Eye Gaze Offset
            double lookX = _engine.Val.LookX * 4.0;
            double lookY = _engine.Val.LookY * 4.0;
            _faceTranslate.X = lookX;
        }
        catch (Exception ex)
        {
            LogStep("OnRenderTick exception: " + ex);
        }
    }

    private async Task SubmitPromptAsync()
    {
        string prompt = _inputBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(prompt) || _conversation.IsGenerating)
            return;

        _inputBox.Text = string.Empty;
        _inputBox.IsEnabled = false;
        _sendBtn.Visibility = Visibility.Collapsed;
        _cancelBtn.Visibility = Visibility.Visible;

        UpdateStatus("Thinking...", isWorking: true);
        await _conversation.SendMessageAsync(prompt);
    }

    private void ToggleVoiceListening()
    {
        bool listening = _engine.CurrentMood != LeoMood.Listening;
        _gestures.SetVoiceListening(listening);

        if (listening)
        {
            _micBtn.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(200, 239, 68, 68)); // Active Red
            UpdateStatus("Listening...", isWorking: true);
        }
        else
        {
            _micBtn.Background = null;
            UpdateStatus("Ready");
        }
    }

    private void ToggleChatDrawer()
    {
        _isChatExpanded = !_isChatExpanded;
        _chatDrawer.Visibility = _isChatExpanded ? Visibility.Visible : Visibility.Collapsed;

        int targetHeight = _isChatExpanded ? ExpandedHeight : CollapsedHeight;
        _appWindow.Resize(new SizeInt32(CapsuleWidth + 40, targetHeight + 40));

        var windowId = Win32Interop.GetWindowIdFromWindow(_hWnd);
        var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
        if (displayArea != null)
        {
            int x = (displayArea.WorkArea.Width - (CapsuleWidth + 40)) / 2;
            int y = displayArea.WorkArea.Height - (targetHeight + 70);
            _appWindow.Move(new PointInt32(x, y));
        }
    }

    private void OpenSettings()
    {
        if (_settingsWindow == null)
        {
            _settingsWindow = new SettingsWindow(
                _settingsService,
                _geminiProvider,
                _localProvider,
                (newProvider) => _conversation.SetActiveProvider(newProvider));

            _settingsWindow.Closed += (s, e) => _settingsWindow = null;
        }

        _settingsWindow.Activate();
    }

    private void UpdateStatus(string status, bool isWorking = false, bool isError = false)
    {
        _statusPill.Text = status;
        if (isError)
        {
            _statusPill.Foreground = new SolidColorBrush(Colors.OrangeRed);
            _statusBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(40, 239, 68, 68));
            _statusBadge.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(100, 239, 68, 68));
        }
        else if (isWorking)
        {
            _statusPill.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 96, 165, 250));
            _statusBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(40, 59, 130, 246));
            _statusBadge.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(100, 59, 130, 246));
        }
        else
        {
            _statusPill.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 52, 211, 153));
            _statusBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(40, 16, 185, 129));
            _statusBadge.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(100, 16, 185, 129));
        }
    }
}
