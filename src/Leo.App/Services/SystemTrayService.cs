using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Leo.App.Services;

public class SystemTrayService : IDisposable
{
    private readonly IntPtr _hWnd;
    private bool _isCreated = false;
    private IntPtr _hIcon = IntPtr.Zero;

    private const int WM_USER = 0x0400;
    public const int WM_TRAYICON = WM_USER + 101;

    private const int NIM_ADD = 0x00000000;
    private const int NIM_MODIFY = 0x00000001;
    private const int NIM_DELETE = 0x00000002;

    private const int NIF_MESSAGE = 0x00000001;
    private const int NIF_ICON = 0x00000002;
    private const int NIF_TIP = 0x00000004;

    private const uint MF_STRING = 0x00000000;
    private const uint MF_SEPARATOR = 0x00000800;
    private const uint TPM_RIGHTBUTTON = 0x0002;
    private const uint TPM_RETURNCMD = 0x0100;

    public const int IDM_RESTORE = 1001;
    public const int IDM_HIDE = 1002;
    public const int IDM_SETTINGS = 1003;
    public const int IDM_QUIT = 1004;

    private delegate IntPtr SubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, nuint uIdSubclass, nuint dwRefData);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern bool SetWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, nuint uIdSubclass, nuint dwRefData);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern IntPtr DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("comctl32.dll", SetLastError = true)]
    private static extern bool RemoveWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, nuint uIdSubclass);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);

    private readonly SubclassProc _subclassDelegate;
    private bool _isSubclassed = false;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uID;
        public int uFlags;
        public int uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public int dwState;
        public int dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public int uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public int dwInfoFlags;
        public Guid guidItem;
        public IntPtr hBalloonIcon;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool Shell_NotifyIconW(int dwMessage, ref NOTIFYICONDATA lpData);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("user32.dll")]
    private static extern IntPtr CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool AppendMenuW(IntPtr hMenu, uint uFlags, nuint uIDNewItem, string? lpNewItem);

    [DllImport("user32.dll")]
    private static extern bool DestroyMenu(IntPtr hMenu);

    [DllImport("user32.dll")]
    private static extern int TrackPopupMenuEx(IntPtr hmenu, uint fuFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    public event Action? OnRestoreRequested;
    public event Action? OnHideRequested;
    public event Action? OnSettingsRequested;
    public event Action? OnQuitRequested;

    public SystemTrayService(IntPtr hWnd)
    {
        _hWnd = hWnd;
        _subclassDelegate = WindowSubclassCallback;
    }

    public void Initialize()
    {
        string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
        if (File.Exists(iconPath))
        {
            _hIcon = LoadImage(IntPtr.Zero, iconPath, 1 /* IMAGE_ICON */, 16, 16, 0x00000010 /* LR_LOADFROMFILE */);
        }

        if (_hIcon == IntPtr.Zero)
        {
            _hIcon = LoadIcon(IntPtr.Zero, (IntPtr)32512 /* IDI_APPLICATION */);
        }

        var nid = new NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = _hWnd,
            uID = 1,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = WM_TRAYICON,
            hIcon = _hIcon,
            szTip = "Leo — Windows AI Assistant"
        };

        _isCreated = Shell_NotifyIconW(NIM_ADD, ref nid);
        _isSubclassed = SetWindowSubclass(_hWnd, _subclassDelegate, 101, 0);
    }

    private IntPtr WindowSubclassCallback(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, nuint uIdSubclass, nuint dwRefData)
    {
        if (uMsg == WM_TRAYICON)
        {
            int mouseMsg = (int)lParam & 0xFFFF;
            if (mouseMsg == 0x0205) // WM_RBUTTONUP
            {
                ShowContextMenu();
                return IntPtr.Zero;
            }
            else if (mouseMsg == 0x0202 || mouseMsg == 0x0203) // WM_LBUTTONUP or WM_LBUTTONDBLCLK
            {
                OnRestoreRequested?.Invoke();
                return IntPtr.Zero;
            }
        }

        return DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }

    public void ShowContextMenu()
    {
        IntPtr hMenu = CreatePopupMenu();
        if (hMenu == IntPtr.Zero) return;

        try
        {
            AppendMenuW(hMenu, MF_STRING, IDM_RESTORE, "Restore / Open Capsule");
            AppendMenuW(hMenu, MF_STRING, IDM_HIDE, "Hide Capsule");
            AppendMenuW(hMenu, MF_SEPARATOR, 0, null);
            AppendMenuW(hMenu, MF_STRING, IDM_SETTINGS, "Settings...");
            AppendMenuW(hMenu, MF_SEPARATOR, 0, null);
            AppendMenuW(hMenu, MF_STRING, IDM_QUIT, "Quit Leo");

            GetCursorPos(out var pt);
            SetForegroundWindow(_hWnd);

            int cmd = TrackPopupMenuEx(hMenu, TPM_RIGHTBUTTON | TPM_RETURNCMD, pt.X, pt.Y, _hWnd, IntPtr.Zero);
            switch (cmd)
            {
                case IDM_RESTORE:
                    OnRestoreRequested?.Invoke();
                    break;
                case IDM_HIDE:
                    OnHideRequested?.Invoke();
                    break;
                case IDM_SETTINGS:
                    OnSettingsRequested?.Invoke();
                    break;
                case IDM_QUIT:
                    OnQuitRequested?.Invoke();
                    break;
            }
        }
        finally
        {
            DestroyMenu(hMenu);
        }
    }

    public void Dispose()
    {
        if (_isSubclassed)
        {
            RemoveWindowSubclass(_hWnd, _subclassDelegate, 101);
            _isSubclassed = false;
        }

        if (_isCreated)
        {
            var nid = new NOTIFYICONDATA
            {
                cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
                hWnd = _hWnd,
                uID = 1
            };
            Shell_NotifyIconW(NIM_DELETE, ref nid);
            _isCreated = false;
        }

        if (_hIcon != IntPtr.Zero)
        {
            DestroyIcon(_hIcon);
            _hIcon = IntPtr.Zero;
        }
    }
}
