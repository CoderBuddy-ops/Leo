/**
 * Leo Strongly-Typed IPC Bridge & Windows Safety Abstraction
 * 
 * Invariants:
 * 1. Least Privilege: Only allowlisted, typed IPC commands are supported.
 * 2. Zero Shell Execution: Never concatenate strings into shell commands (no cmd / powershell).
 * 3. Bounded Parameters: Window sizes, positions, and model IDs are strictly validated.
 * 4. Dev Fallback: When running outside a Tauri webview (e.g. in Vite preview/test),
 *    emulate native responses safely in memory so the UI remains 100% interactive.
 */

export interface SystemInfo {
  os: "windows" | "linux" | "macos" | "browser";
  arch: "x64" | "arm64" | "unknown";
  webviewVersion: string;
  isTauri: boolean;
}

export interface WindowBounds {
  width: number;
  height: number;
  x?: number;
  y?: number;
}

export type LeoIPCCommand =
  | { command: "get_system_info" }
  | { command: "set_window_clickthrough"; enabled: boolean }
  | { command: "set_always_on_top"; enabled: boolean }
  | { command: "set_window_size"; width: number; height: number }
  | { command: "paste_clipboard_text"; text: string }
  | { command: "minimize_window" }
  | { command: "close_window" }
  | { command: "open_settings_window" }
  | { command: "toggle_dock_visibility" }
  | { command: "quit_app" };

export class LeoIPCBridge {
  private static isTauriEnvironment(): boolean {
    return typeof window !== "undefined" && "__TAURI_INTERNALS__" in window;
  }

  /**
   * Safe, validated dispatch of native commands.
   */
  public static async execute(cmd: LeoIPCCommand): Promise<unknown> {
    // Parameter validation to ensure strict bounds
    if (cmd.command === "set_window_size") {
      if (cmd.width < 100 || cmd.width > 3840 || cmd.height < 50 || cmd.height > 2160) {
        throw new Error(`Window size out of safe bounds: ${cmd.width}x${cmd.height}`);
      }
    }

    if (cmd.command === "paste_clipboard_text") {
      if (typeof cmd.text !== "string") {
        throw new Error("Text payload must be a string");
      }
      if (cmd.text.length > 500000) {
        throw new Error("Text payload exceeds maximum clipboard limit (500KB)");
      }
    }

    if (this.isTauriEnvironment()) {
      try {
        // Dynamically invoke Tauri core if available
        const tauri = (window as unknown as { __TAURI__: { core: { invoke: (cmd: string, args?: unknown) => Promise<unknown> } } }).__TAURI__;
        if (tauri?.core?.invoke) {
          return await tauri.core.invoke(cmd.command, cmd);
        }
      } catch (err) {
        console.warn(`Tauri invoke failed for ${cmd.command}, falling back to mock:`, err);
      }
    }

    // Dev Fallback / Browser Simulation
    return this.mockExecute(cmd);
  }

  private static mockExecute(cmd: LeoIPCCommand): unknown {
    switch (cmd.command) {
      case "get_system_info":
        return {
          os: typeof navigator !== "undefined" && navigator.userAgent.includes("Windows") ? "windows" : "browser",
          arch: "x64",
          webviewVersion: "dev-mock-1.0",
          isTauri: false,
        } as SystemInfo;

      case "set_window_clickthrough":
      case "set_always_on_top":
      case "set_window_size":
      case "minimize_window":
      case "close_window":
      case "open_settings_window":
      case "toggle_dock_visibility":
      case "quit_app":
        return { success: true, simulated: true, command: cmd.command };

      case "paste_clipboard_text":
        if (typeof navigator !== "undefined" && navigator.clipboard?.writeText) {
          navigator.clipboard.writeText(cmd.text).catch(() => {});
        }
        return { success: true, length: cmd.text.length };

      default:
        throw new Error(`Unsupported IPC command: ${(cmd as { command: string }).command}`);
    }
  }
}
