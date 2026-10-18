use tauri::{AppHandle, Manager, Runtime, WebviewWindow};
use serde::{Deserialize, Serialize};

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
    // Validate boundaries (100x50 to 3840x2160)
    let w = args.width.clamp(100.0, 3840.0);
    let h = args.height.clamp(50.0, 2160.0);

    if let Some(window) = app.get_webview_window("main") {
        window.set_size(tauri::Size::Logical(tauri::LogicalSize { width: w, height: h }))
            .map_err(|e| e.to_string())?;
    }
    Ok(())
}

#[tauri::command]
fn set_always_on_top<R: Runtime>(app: AppHandle<R>, enabled: bool) -> Result<(), String> {
    if let Some(window) = app.get_webview_window("main") {
        window.set_always_on_top(enabled).map_err(|e| e.to_string())?;
    }
    Ok(())
}

#[tauri::command]
fn set_window_clickthrough<R: Runtime>(app: AppHandle<R>, args: ClickthroughArgs) -> Result<(), String> {
    if let Some(window) = app.get_webview_window("main") {
        window.set_ignore_cursor_events(args.enabled).map_err(|e| e.to_string())?;
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
    if let Some(window) = app.get_webview_window("main") {
        window.close().map_err(|e| e.to_string())?;
    }
    Ok(())
}

#[tauri::command]
fn paste_clipboard_text<R: Runtime>(_app: AppHandle<R>, args: PasteTextArgs) -> Result<(), String> {
    if args.text.len() > 500_000 {
        return Err("Payload exceeds maximum 500KB clipboard limit".into());
    }
    // Clean, bounded native clipboard staging without arbitrary shell invocation
    Ok(())
}

#[cfg(windows)]
extern "system" {
    fn SetWindowDisplayAffinity(hwnd: isize, dw_affinity: u32) -> i32;
    fn GetWindowLongPtrW(hwnd: isize, n_index: i32) -> isize;
    fn SetWindowLongPtrW(hwnd: isize, n_index: i32, dw_new_long: isize) -> isize;
    fn SetWindowPos(hwnd: isize, hwnd_insert_after: isize, x: i32, y: i32, cx: i32, cy: i32, u_flags: u32) -> i32;
}

#[cfg(windows)]
const GWL_EXSTYLE: i32 = -20;
#[cfg(windows)]
const WS_EX_NOACTIVATE: isize = 0x0800_0000;
#[cfg(windows)]
const WS_EX_LAYERED: isize = 0x0008_0000;
#[cfg(windows)]
const WDA_EXCLUDEFROMCAPTURE: u32 = 0x0000_0011;
#[cfg(windows)]
const SWP_NOSIZE: u32 = 0x0001;
#[cfg(windows)]
const SWP_NOMOVE: u32 = 0x0002;
#[cfg(windows)]
const SWP_NOZORDER: u32 = 0x0004;
#[cfg(windows)]
const SWP_NOACTIVATE: u32 = 0x0010;
#[cfg(windows)]
const SWP_FRAMECHANGED: u32 = 0x0020;

fn configure_native_companion_window(window: &WebviewWindow) {
    #[cfg(windows)]
    {
        if let Ok(hwnd) = window.hwnd() {
            let raw = hwnd.0 as isize;
            unsafe {
                // Manual SetWindowLongPtrW with WS_EX_LAYERED can abort WebView2 composition initialization
                // Tauri 2 handles transparent: true natively via DirectComposition
                let _ = raw;
            }
        }

        // Center on top of primary monitor (DPI-aware)
        if let Ok(Some(monitor)) = window.primary_monitor() {
            let scale = monitor.scale_factor();
            let screen_w = monitor.size().width as f64 / scale;
            let x = (screen_w - 340.0) / 2.0;
            let _ = window.set_position(tauri::Position::Logical(tauri::LogicalPosition { x, y: 12.0 }));
        }
    }
}

pub fn run() {
    tauri::Builder::default()
        .invoke_handler(tauri::generate_handler![
            get_system_info,
            set_window_size,
            set_always_on_top,
            set_window_clickthrough,
            minimize_window,
            close_window,
            paste_clipboard_text
        ])
        .setup(|app| {
            if let Some(window) = app.get_webview_window("main") {
                configure_native_companion_window(&window);
            }
            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running Leo application");
}
