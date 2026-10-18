/**
 * Google Gemini Online AI Client (Streaming REST API)
 *
 * Implements direct SSE streaming against the official Google Generative Language API
 * (v1beta/models/{model}:streamGenerateContent?alt=sse).
 *
 * Security Invariants:
 * - The user's API key is NEVER logged, written to disk in plaintext, or included in error messages.
 * - All outbound requests are explicit and trigger a visual "[Leaves your PC]" badge.
 * - Responses are streamed incrementally and can be aborted at any moment.
 */

export interface GeminiConfig {
  apiKey: string;
  modelName?: string;
  temperature?: number;
}

export interface GeminiPart {
  text: string;
}

export interface GeminiContent {
  role: "user" | "model";
  parts: GeminiPart[];
}

export interface GeminiUsageMetadata {
  promptTokenCount?: number;
  candidatesTokenCount?: number;
  totalTokenCount?: number;
}

export interface GeminiStreamResult {
  text: string;
  usage?: GeminiUsageMetadata;
}

export class GeminiModelClient {
  private apiKey: string;
  private modelName: string;
  private temperature: number;

  public static readonly DEFAULT_MODEL = "gemini-2.5-flash";
  public static readonly SUPPORTED_MODELS = [
    "gemini-2.5-flash",
    "gemini-2.5-pro",
    "gemini-1.5-flash",
    "gemini-1.5-pro"
  ];

  constructor(config: GeminiConfig) {
    this.apiKey = (config.apiKey || "").trim();
    this.modelName = config.modelName || GeminiModelClient.DEFAULT_MODEL;
    this.temperature = config.temperature ?? 0.7;
  }

  public get model(): string {
    return this.modelName;
  }

  public setModel(model: string): void {
    if (model && model.trim()) {
      this.modelName = model.trim();
    }
  }

  public setApiKey(key: string): void {
    this.apiKey = (key || "").trim();
  }

  public hasApiKey(): boolean {
    return this.apiKey.length > 0;
  }

  public clearApiKey(): void {
    this.apiKey = "";
  }

  /**
   * Sanitizes any error message to ensure the user's API key is never leaked
   */
  public redactSecret(msg: string): string {
    if (!this.apiKey || this.apiKey.length < 4) return msg;
    return msg.split(this.apiKey).join("[REDACTED_API_KEY]");
  }

  /**
   * Tests connection with the Gemini endpoint
   */
  public async testConnection(signal?: AbortSignal): Promise<{ ok: boolean; message: string; model: string }> {
    if (!this.hasApiKey()) {
      return { ok: false, message: "No Gemini API key configured. Enter your key in Settings.", model: this.modelName };
    }

    const endpoint = `https://generativelanguage.googleapis.com/v1beta/models/${this.modelName}?key=${this.apiKey}`;

    try {
      const res = await fetch(endpoint, {
        method: "GET",
        headers: { "Content-Type": "application/json" },
        signal: signal || AbortSignal.timeout(6000)
      });

      if (res.ok) {
        return { ok: true, message: `Connected successfully to Google Gemini (${this.modelName})`, model: this.modelName };
      }

      if (res.status === 400 || res.status === 403) {
        return { ok: false, message: "Invalid API key or unauthorized access. Please verify your Google AI key.", model: this.modelName };
      }

      return { ok: false, message: `Google Gemini service returned HTTP ${res.status}`, model: this.modelName };
    } catch (err: unknown) {
      const raw = err instanceof Error ? err.message : String(err);
      return { ok: false, message: this.redactSecret(`Connection error: ${raw}`), model: this.modelName };
    }
  }

  /**
   * Streams chat completions from the Gemini API using Server-Sent Events (alt=sse)
   */
  public async streamChat(
    contents: GeminiContent[],
    onToken: (token: string) => void,
    signal?: AbortSignal
  ): Promise<GeminiStreamResult> {
    if (!this.hasApiKey()) {
      throw new Error("No Gemini API key configured. Please enter your Google AI key in Settings.");
    }

    const endpoint = `https://generativelanguage.googleapis.com/v1beta/models/${this.modelName}:streamGenerateContent?alt=sse&key=${this.apiKey}`;

    const body = {
      contents,
      generationConfig: {
        temperature: this.temperature,
        maxOutputTokens: 2048
      }
    };

    let res: Response;
    try {
      res = await fetch(endpoint, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
        signal
      });
    } catch (err: unknown) {
      const raw = err instanceof Error ? err.message : String(err);
      throw new Error(this.redactSecret(`Network error while contacting Google Gemini: ${raw}`));
    }

    if (!res.ok) {
      let errDetails = `HTTP ${res.status} ${res.statusText}`;
      try {
        const errJson = await res.json();
        if (errJson?.error?.message) {
          errDetails = errJson.error.message;
        }
      } catch {
        // Fall back to status text
      }

      if (res.status === 400 || res.status === 403) {
        throw new Error("Invalid Gemini API key or unauthorized. Please verify your key in Settings.");
      }
      if (res.status === 429) {
        throw new Error("Gemini quota or rate limit exceeded. Please wait a moment and try again.");
      }

      throw new Error(this.redactSecret(`Google Gemini error: ${errDetails}`));
    }

    if (!res.body) {
      throw new Error("No response stream received from Google Gemini API.");
    }

    const reader = res.body.getReader();
    const decoder = new TextDecoder();
    let fullText = "";
    let buffer = "";
    let usage: GeminiUsageMetadata | undefined;

    try {
      while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        buffer += decoder.decode(value, { stream: true });
        const lines = buffer.split("\n");
        buffer = lines.pop() || "";

        for (const line of lines) {
          const trimmed = line.trim();
          if (!trimmed || !trimmed.startsWith("data:")) continue;
          const jsonStr = trimmed.replace(/^data:\s*/, "");

          try {
            const data = JSON.parse(jsonStr);
            if (data?.candidates?.[0]?.content?.parts) {
              for (const part of data.candidates[0].content.parts) {
                if (part.text) {
                  fullText += part.text;
                  onToken(part.text);
                }
              }
            }
            if (data?.usageMetadata) {
              usage = {
                promptTokenCount: data.usageMetadata.promptTokenCount,
                candidatesTokenCount: data.usageMetadata.candidatesTokenCount,
                totalTokenCount: data.usageMetadata.totalTokenCount
              };
            }
          } catch {
            // Partial chunk parsing
          }
        }
      }
    } catch (streamErr: unknown) {
      if (signal?.aborted) {
        return { text: fullText, usage };
      }
      const raw = streamErr instanceof Error ? streamErr.message : String(streamErr);
      throw new Error(this.redactSecret(`Stream interrupted: ${raw}`));
    }

    return { text: fullText, usage };
  }
}
