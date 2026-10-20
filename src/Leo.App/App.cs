using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Leo.App.Services;
using Leo.App.Views;
using Leo.Application.Interfaces;
using Leo.Application.Services;
using Leo.Core.Companion;
using Leo.Core.Models;
using Leo.Infrastructure.Persistence;
using Leo.Infrastructure.Providers;

namespace Leo.App;

public class App : Microsoft.UI.Xaml.Application
{
    private CapsuleWindow? _capsuleWindow;
    private SystemTrayService? _trayService;
    private JsonSettingsService? _settingsService;
    private GeminiProvider? _geminiProvider;
    private LocalAiProvider? _localProvider;
    private LeoConversationManager? _conversationManager;
    private LeoCharacterEngine? _engine;
    private LeoGestureController? _gestures;

    public App()
    {
        this.UnhandledException += (s, e) =>
        {
            string log = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "unhandled.log");
            System.IO.File.AppendAllText(log, $"UnhandledException: {e.Message}\n{e.Exception}\n");
            e.Handled = true;
        };
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);
        string logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup.log");
        System.IO.File.AppendAllText(logPath, "App.OnLaunched entered\n");

        try
        {
            this.Resources.MergedDictionaries.Add(new Microsoft.UI.Xaml.Controls.XamlControlsResources());
            System.IO.File.AppendAllText(logPath, "XamlControlsResources added successfully\n");
        }
        catch (Exception ex)
        {
            System.IO.File.AppendAllText(logPath, $"XamlControlsResources failed: {ex}\n");
        }

        try
        {
            // 1. Initialize Core Engines
            _engine = new LeoCharacterEngine(42);
            _gestures = new LeoGestureController(_engine);

            // 2. Initialize Infrastructure & Settings
            _settingsService = new JsonSettingsService();
            _settingsService.LoadSettings();

            _geminiProvider = new GeminiProvider();
            if (!string.IsNullOrEmpty(_settingsService.CurrentSettings.AiConfig.ModelName))
            {
                _geminiProvider.CurrentModel = _settingsService.CurrentSettings.AiConfig.ModelName;
            }

            _localProvider = new LocalAiProvider
            {
                EndpointUrl = _settingsService.CurrentSettings.AiConfig.LocalEndpointUrl,
                CurrentModel = _settingsService.CurrentSettings.AiConfig.ModelName
            };

            // Select initial provider based on user settings
            IAiProvider initialProvider = _settingsService.CurrentSettings.AiConfig.ProviderType == AiProviderType.Local
                ? _localProvider
                : _geminiProvider;

            // 3. Initialize Conversation Orchestrator
            _conversationManager = new LeoConversationManager(_engine, _gestures, initialProvider);

            // 4. Create and Show Capsule Window
            System.IO.File.AppendAllText(logPath, "Creating CapsuleWindow...\n");
            _capsuleWindow = new CapsuleWindow(
                _engine,
                _gestures,
                _conversationManager,
                _settingsService,
                _geminiProvider,
                _localProvider);

            System.IO.File.AppendAllText(logPath, "Activating CapsuleWindow...\n");
            _capsuleWindow.Activate();
            System.IO.File.AppendAllText(logPath, "CapsuleWindow activated\n");

            // 5. Initialize System Tray Icon
            System.IO.File.AppendAllText(logPath, "Initializing TrayService...\n");
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(_capsuleWindow);
            _trayService = new SystemTrayService(hWnd);
            _trayService.OnRestoreRequested += () =>
            {
                _capsuleWindow.Activate();
            };
            _trayService.OnHideRequested += () =>
            {
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(
                    Win32Interop.GetWindowIdFromWindow(hWnd));
                appWindow.Hide();
            };
            _trayService.OnSettingsRequested += () =>
            {
                var settings = new SettingsWindow(
                    _settingsService,
                    _geminiProvider,
                    _localProvider,
                    (p) => _conversationManager.SetActiveProvider(p));
                settings.Activate();
            };
            _trayService.OnQuitRequested += () =>
            {
                _trayService?.Dispose();
                this.Exit();
            };
            _trayService.Initialize();
            System.IO.File.AppendAllText(logPath, "TrayService initialized\n");
        }
        catch (Exception ex)
        {
            System.IO.File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "launch_crash.log"), ex.ToString());
        }
    }
}
