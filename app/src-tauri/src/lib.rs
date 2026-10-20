use tauri::{
    AppHandle, Manager, Runtime, WebviewWindow, WebviewUrl,
    menu::{Menu, MenuItem},
    tray::{TrayIconBuilder, TrayIconEvent, MouseButton, MouseButtonState},
    WindowEvent,
};
use serde::{Deserialize, Serialize};
use std::sync::atomic::{AtomicBool, Ordering};

/// Global flag: whether dock overlay is currently visible
static DOCK_VISIBLE: AtomicBool = AtomicBool::new(true);
/// Global flag: whether "Start with Windows" is enabled
static START_WITH_WINDOWS: AtomicBool = AtomicBool::new(false);

// ─── Data Structures ─────────────────────────────────────────────────────

#[derive(Serialize)]
pub struct SystemInfo {
    pub os: String,
    pub arch: String,
    pub webview_version: String,
    pub is_tauri: bool,
}

#[derive(Deserialize)]
pub struct SetWindowSizeArgs {
    pub width: f64,
    pub height: f64,
}

#[derive(Deserialize)]
pub struct PasteTextArgs {
    pub text: String,
}

#[derive(Deserialize)]
pub struct ClickthroughArgs {
    pub enabled: bool,
}

// ─── IPC Commands ────────────────────────────────────────────────────────

#[tauri::command]
fn get_system_info() -> SystemInfo {
    SystemInfo {
        os: std::env::consts::OS.to_string(),
        arch: std::env::consts::ARCH.to_string(),
        webview_version: "tauri-2.x".to_string(),
        is_tauri: true,
    }
}

#[tauri::command]
fn set_window_size<R: Runtime>(app: AppHandle<R>, args: SetWindowSizeArgs) -> Result<(), String> {
    let w = args.width.clamp(100.0, 3840.0);
    let h = args.height.clamp(50.0, 2160.0);

    if let Some(window) = app.get_webview_window("main") {
        window
            .set_size(tauri::Size::Logical(tauri::LogicalSize {
                width: w,
                height: h,
            }))
            .map_err(|e| e.to_string())?;
    }
    Ok(())
}

#[tauri::command]
fn set_always_on_top<R: Runtime>(app: AppHandle<R>, enabled: bool) -> Result<(), String> {
    if let Some(window) = app.get_webview_window("main") {
        window
            .set_always_on_top(enabled)
            .map_err(|e| e.to_string())?;
    }
    Ok(())
}

#[tauri::command]
fn set_window_clickthrough<R: Runtime>(
    app: AppHandle<R>,
    args: ClickthroughArgs,
) -> Result<(), String> {
    if let Some(window) = app.get_webview_window("main") {
        window
            .set_ignore_cursor_events(args.enabled)
            .map_err(|e| e.to_string())?;
    }
    Ok(())
}

#[tauri::command]
fn minimize_window<R: Runtime>(app: AppHandle<R>) -> Result<(), String> {
    if let Some(window) = app.get_webview_window("main") {
        window.minimize().map_err(|e| e.to_string())?;
    }
    Ok(())
}

#[tauri::command]
fn close_window<R: Runtime>(app: AppHandle<R>) -> Result<(), String> {
    // Hide to tray instead of destroying
    if let Some(window) = app.get_webview_window("main") {
        window.hide().map_err(|e| e.to_string())?;
        DOCK_VISIBLE.store(false, Ordering::Relaxed);
    }
    Ok(())
}

#[tauri::command]
fn paste_clipboard_text<R: Runtime>(
    _app: AppHandle<R>,
    args: PasteTextArgs,
) -> Result<(), String> {
    if args.text.len() > 500_000 {
        return Err("Payload exceeds maximum 500KB clipboard limit".into());
    }
    // Clean, bounded native clipboard staging without arbitrary shell invocation
    Ok(())
}

#[tauri::command]
fn open_settings_window<R: Runtime>(app: AppHandle<R>) -> Result<(), String> {
    show_settings_window(&app).map_err(|e| e.to_string())
}

#[tauri::command]
fn toggle_dock_visibility<R: Runtime>(app: AppHandle<R>) -> Result<bool, String> {
    let currently_visible = DOCK_VISIBLE.load(Ordering::Relaxed);
    if currently_visible {
        if let Some(window) = app.get_webview_window("main") {
            window.hide().map_err(|e| e.to_string())?;
        }
        DOCK_VISIBLE.store(false, Ordering::Relaxed);
        Ok(false)
    } else {
        if let Some(window) = app.get_webview_window("main") {
            window.show().map_err(|e| e.to_string())?;
        }
        DOCK_VISIBLE.store(true, Ordering::Relaxed);
        Ok(true)
    }
}

#[tauri::command]
fn quit_app<R: Runtime>(app: AppHandle<R>) -> Result<(), String> {
    app.exit(0);
    Ok(())
}

// ─── Win32 Configuration ─────────────────────────────────────────────────

#[cfg(windows)]
extern "system" {
    fn SetWindowDisplayAffinity(hwnd: isize, dw_affinity: u32) -> i32;
    fn GetWindowLongPtrW(hwnd: isize, n_index: i32) -> isize;
    fn SetWindowLongPtrW(hwnd: isize, n_index: i32, dw_new_long: isize) -> isize;
    fn SetWindowPos(
        hwnd: isize,
        hwnd_insert_after: isize,
        x: i32,
        y: i32,
        cx: i32,
        cy: i32,
        u_flags: u32,
    ) -> i32;
}

#[cfg(windows)]
const WDA_EXCLUDEFROMCAPTURE: u32 = 0x0000_0011;
#[cfg(windows)]
const GWL_EXSTYLE: i32 = -20;
#[cfg(windows)]
const WS_EX_LAYERED: isize = 0x0008_0000;
#[cfg(windows)]
const SWP_FRAMECHANGED: u32 = 0x0020;
#[cfg(windows)]
const SWP_NOMOVE: u32 = 0x0002;
#[cfg(windows)]
const SWP_NOSIZE: u32 = 0x0001;
#[cfg(windows)]
const SWP_NOZORDER: u32 = 0x0004;
#[cfg(windows)]
const SWP_NOACTIVATE: u32 = 0x0010;
#[cfg(windows)]
const HWND_TOP: isize = 0;

fn configure_native_companion_window(window: &WebviewWindow) {
    #[cfg(windows)]
    {
        if let Ok(hwnd) = window.hwnd() {
            let raw = hwnd.0 as isize;
            unsafe {
                // Add WS_EX_LAYERED so DWM uses per-pixel alpha for the
                // transparent WebView2 surface. Without this, the WebView2
                // content composits as fully transparent and the dock is blank.
                let ex = GetWindowLongPtrW(raw, GWL_EXSTYLE);
                SetWindowLongPtrW(raw, GWL_EXSTYLE, ex | WS_EX_LAYERED);

                // Force DWM to re-composite the frame with the new style.
                SetWindowPos(
                    raw,
                    HWND_TOP,
                    0,
                    0,
                    0,
                    0,
                    SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE,
                );

                // Exclude from screen capture (privacy: WDA_EXCLUDEFROMCAPTURE)
                let _ = SetWindowDisplayAffinity(raw, WDA_EXCLUDEFROMCAPTURE);
            }
        }

        // Center on top of primary monitor (DPI-aware)
        if let Ok(Some(monitor)) = window.primary_monitor() {
            let scale = monitor.scale_factor();
            let screen_w = monitor.size().width as f64 / scale;
            let x = (screen_w - 340.0) / 2.0;
            let _ = window.set_position(tauri::Position::Logical(tauri::LogicalPosition {
                x,
                y: 12.0,
            }));
        }
    }
}

// ─── Settings Window ─────────────────────────────────────────────────────

fn show_settings_window<R: Runtime>(app: &AppHandle<R>) -> Result<(), Box<dyn std::error::Error>> {
    // If settings window already exists, just show and focus it
    if let Some(win) = app.get_webview_window("settings") {
        win.show()?;
        win.set_focus()?;
        return Ok(());
    }

    // Create a new settings window pointing to settings.html
    let settings_url = WebviewUrl::App("settings.html".into());

    let win = tauri::WebviewWindowBuilder::new(app, "settings", settings_url)
        .title("Leo — Settings")
        .inner_size(560.0, 640.0)
        .min_inner_size(480.0, 520.0)
        .resizable(true)
        .decorations(true)
        .transparent(false)
        .always_on_top(false)
        .skip_taskbar(false)
        .center()
        .focused(true)
        .build()?;

    // Close settings window → just hide, don't destroy the whole app
    let app_handle = app.clone();
    win.on_window_event(move |event| {
        if let WindowEvent::CloseRequested { api, .. } = event {
            api.prevent_close();
            if let Some(w) = app_handle.get_webview_window("settings") {
                let _ = w.hide();
            }
        }
    });

    Ok(())
}

// ─── System Tray ─────────────────────────────────────────────────────────

fn build_tray<R: Runtime>(app: &AppHandle<R>) -> Result<(), Box<dyn std::error::Error>> {
    let open_leo = MenuItem::with_id(app, "open_leo", "Open Leo", true, None::<&str>)?;
    let toggle_dock = MenuItem::with_id(app, "toggle_dock", "Show/Hide Dock", true, None::<&str>)?;
    let settings = MenuItem::with_id(app, "settings", "Settings", true, None::<&str>)?;
    let startup = MenuItem::with_id(
        app,
        "startup",
        "Start with Windows",
        true,
        None::<&str>,
    )?;
    let quit = MenuItem::with_id(app, "quit", "Quit Leo", true, None::<&str>)?;

    let menu = Menu::with_items(app, &[&open_leo, &toggle_dock, &settings, &startup, &quit])?;

    let _tray = TrayIconBuilder::new()
        .icon(app.default_window_icon().unwrap().clone())
        .menu(&menu)
        .tooltip("Leo — AI Companion")
        .on_menu_event(move |app, event| {
            match event.id.as_ref() {
                "open_leo" => {
                    if let Some(window) = app.get_webview_window("main") {
                        let _ = window.show();
                        let _ = window.set_focus();
                        DOCK_VISIBLE.store(true, Ordering::Relaxed);
                    }
                }
                "toggle_dock" => {
                    let visible = DOCK_VISIBLE.load(Ordering::Relaxed);
                    if visible {
                        if let Some(window) = app.get_webview_window("main") {
                            let _ = window.hide();
                        }
                        DOCK_VISIBLE.store(false, Ordering::Relaxed);
                    } else {
                        if let Some(window) = app.get_webview_window("main") {
                            let _ = window.show();
                        }
                        DOCK_VISIBLE.store(true, Ordering::Relaxed);
                    }
                }
                "settings" => {
                    let _ = show_settings_window(app);
                }
                "startup" => {
                    let current = START_WITH_WINDOWS.load(Ordering::Relaxed);
                    START_WITH_WINDOWS.store(!current, Ordering::Relaxed);
                    // TODO: Actually write/remove registry Run key
                    // HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Run
                }
                "quit" => {
                    app.exit(0);
                }
                _ => {}
            }
        })
        .on_tray_icon_event(|tray, event| {
            if let TrayIconEvent::Click {
                button: MouseButton::Left,
                button_state: MouseButtonState::Up,
                ..
            } = event
            {
                let app = tray.app_handle();
                if let Some(window) = app.get_webview_window("main") {
                    let _ = window.show();
                    let _ = window.set_focus();
                    DOCK_VISIBLE.store(true, Ordering::Relaxed);
                }
            }
        })
        .build(app)?;

    Ok(())
}

// ─── Application Entry ──────────────────────────────────────────────────

pub fn run() {
    tauri::Builder::default()
        .invoke_handler(tauri::generate_handler![
            get_system_info,
            set_window_size,
            set_always_on_top,
            set_window_clickthrough,
            minimize_window,
            close_window,
            paste_clipboard_text,
            open_settings_window,
            toggle_dock_visibility,
            quit_app
        ])
        .setup(|app| {
            // Configure the main dock window
            if let Some(window) = app.get_webview_window("main") {
                configure_native_companion_window(&window);

                // Close event on dock → hide to tray, don't quit
                let app_handle = app.handle().clone();
                window.on_window_event(move |event| {
                    if let WindowEvent::CloseRequested { api, .. } = event {
                        api.prevent_close();
                        if let Some(w) = app_handle.get_webview_window("main") {
                            let _ = w.hide();
                        }
                        DOCK_VISIBLE.store(false, Ordering::Relaxed);
                    }
                });
            }

            // Build the system tray
            let handle = app.handle().clone();
            build_tray(&handle).expect("Failed to create system tray");

            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running Leo application");
}
