/**
 * Local AI Model Client (OpenAI-Compatible Streaming Loopback)
 *
 * Connects to local model servers such as Ollama (http://127.0.0.1:11434/v1)
 * or LM Studio (http://127.0.0.1:1234/v1) with streaming token support and cancellation.
 */

export interface ChatMessage {
  role: "system" | "user" | "assistant";
  content: string;
}

export interface ModelClientConfig {
  endpointUrl: string; // e.g. "http://127.0.0.1:11434/v1"
  modelName: string;   // e.g. "qwen2.5:0.5b" or "llama3.2"
  temperature?: number;
}

export class LocalModelClient {
  private config: ModelClientConfig;

  constructor(config: ModelClientConfig) {
    this.config = config;
  }

  public get endpointUrl(): string {
    return this.config.endpointUrl;
  }

  public get modelName(): string {
    return this.config.modelName;
  }

  public updateEndpoint(endpointUrl: string, modelName?: string): void {
    this.config.endpointUrl = endpointUrl;
    if (modelName) {
      this.config.modelName = modelName;
    }
  }

  /**
   * Evaluates whether an endpoint is local loopback or external
   */
  public static isLoopback(url: string): boolean {
    try {
      const parsed = new URL(url);
      return (
        parsed.hostname === "localhost" ||
        parsed.hostname === "127.0.0.1" ||
        parsed.hostname === "::1" ||
        parsed.hostname.startsWith("192.168.") ||
        parsed.hostname.startsWith("10.")
      );
    } catch {
      return false;
    }
  }

  /**
   * Checks health and availability of the model endpoint
   */
  public async checkHealth(): Promise<{ ok: boolean; message: string }> {
    try {
      const res = await fetch(`${this.config.endpointUrl}/models`, {
        method: "GET",
        signal: AbortSignal.timeout(2000)
      });
      if (res.ok) {
        return { ok: true, message: "Endpoint active and healthy" };
      }
      return { ok: false, message: `Server returned HTTP ${res.status}` };
    } catch (err: any) {
      return { ok: false, message: `Cannot connect to ${this.config.endpointUrl}: ${err.message}` };
    }
  }

  /**
   * Dispatches a streaming completion request
   */
  public async streamChat(
    messages: ChatMessage[],
    onToken: (token: string) => void,
    signal?: AbortSignal
  ): Promise<string> {
    const res = await fetch(`${this.config.endpointUrl}/chat/completions`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        model: this.config.modelName,
        messages,
        temperature: this.config.temperature ?? 0.7,
        stream: true
      }),
      signal
    });

    if (!res.ok) {
      throw new Error(`Model server error: HTTP ${res.status} ${res.statusText}`);
    }

    if (!res.body) {
      throw new Error("No response body received from model server");
    }

    const reader = res.body.getReader();
    const decoder = new TextDecoder();
    let fullResponse = "";
    let buffer = "";

    while (true) {
      const { done, value } = await reader.read();
      if (done) break;

      buffer += decoder.decode(value, { stream: true });
      const lines = buffer.split("\n");
      buffer = lines.pop() || "";

      for (const line of lines) {
        const trimmed = line.trim();
        if (!trimmed || !trimmed.startsWith("data:")) continue;
        const payload = trimmed.replace(/^data:\s*/, "");
        if (payload === "[DONE]") return fullResponse;

        try {
          const parsed = JSON.parse(payload);
          const delta = parsed.choices?.[0]?.delta?.content || "";
          if (delta) {
            fullResponse += delta;
            onToken(delta);
          }
        } catch {
          // Ignore JSON parse errors on incomplete chunks
        }
      }
    }

    return fullResponse;
  }
}
