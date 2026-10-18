/**
 * Unified AI Provider Architecture for Leo
 *
 * Encapsulates Local/Offline (Ollama, LM Studio) and Online (Google Gemini)
 * providers with clear data-egress classification, privacy badges,
 * cancellation, and usage statistics.
 */

import { LocalModelClient, type ChatMessage } from "./local_client.ts";
import { GeminiModelClient, type GeminiContent } from "./gemini_client.ts";

export type ProviderType = "offline" | "ollama" | "lmstudio" | "gemini";

export interface ProviderUsageStats {
  providerType: ProviderType;
  modelName: string;
  totalRequests: number;
  totalTokens: number;
  lastUsedTimestamp?: string;
}

export interface StreamResponse {
  fullText: string;
  tokenCount?: number;
}

export class AIProviderManager {
  private currentProviderType: ProviderType = "ollama";
  private localClient: LocalModelClient;
  private geminiClient: GeminiModelClient;
  private usageStats: Map<ProviderType, ProviderUsageStats> = new Map();

  constructor(initialProvider: ProviderType = "ollama", geminiApiKey: string = "") {
    this.currentProviderType = initialProvider;
    this.localClient = new LocalModelClient({
      endpointUrl: "http://127.0.0.1:11434/v1",
      modelName: "qwen-3.5-2b"
    });
    this.geminiClient = new GeminiModelClient({
      apiKey: geminiApiKey,
      modelName: "gemini-2.5-flash"
    });

    // Initialize usage stats for each provider
    for (const p of ["offline", "ollama", "lmstudio", "gemini"] as ProviderType[]) {
      this.usageStats.set(p, {
        providerType: p,
        modelName: p === "gemini" ? "gemini-2.5-flash" : "local",
        totalRequests: 0,
        totalTokens: 0
      });
    }
  }

  public get providerType(): ProviderType {
    return this.currentProviderType;
  }

  public setProvider(type: ProviderType): void {
    this.currentProviderType = type;
    if (type === "ollama") {
      this.localClient.updateEndpoint("http://127.0.0.1:11434/v1");
    } else if (type === "lmstudio") {
      this.localClient.updateEndpoint("http://127.0.0.1:1234/v1");
    }
  }

  public get isLocal(): boolean {
    return this.currentProviderType !== "gemini";
  }

  public get egressLabel(): string {
    return this.isLocal ? "Local" : "Leaves your PC";
  }

  public get activeModelName(): string {
    if (this.currentProviderType === "gemini") {
      return this.geminiClient.model;
    }
    if (this.currentProviderType === "offline") {
      return "None (Offline)";
    }
    return this.localClient.modelName;
  }

  public getGeminiClient(): GeminiModelClient {
    return this.geminiClient;
  }

  public getLocalClient(): LocalModelClient {
    return this.localClient;
  }

  public getUsage(provider?: ProviderType): ProviderUsageStats {
    const target = provider || this.currentProviderType;
    return this.usageStats.get(target) || {
      providerType: target,
      modelName: "unknown",
      totalRequests: 0,
      totalTokens: 0
    };
  }

  /**
   * Dispatches chat completions to the active provider with streaming token output
   */
  public async streamChat(
    prompt: string,
    history: ChatMessage[],
    onToken: (token: string) => void,
    signal?: AbortSignal
  ): Promise<StreamResponse> {
    if (this.currentProviderType === "offline") {
      throw new Error(
        "Offline Mode is active. Cloud calls are strictly disabled. Switch to a local model (Ollama / LM Studio) or enable Google Gemini."
      );
    }

    const stat = this.usageStats.get(this.currentProviderType)!;
    stat.totalRequests += 1;
    stat.lastUsedTimestamp = new Date().toISOString();

    if (this.currentProviderType === "gemini") {
      // Map history to Gemini content format
      const contents: GeminiContent[] = history.map((msg) => ({
        role: msg.role === "assistant" ? "model" : "user",
        parts: [{ text: msg.content }]
      }));
      contents.push({ role: "user", parts: [{ text: prompt }] });

      const result = await this.geminiClient.streamChat(contents, onToken, signal);
      const tokenCount = result.usage?.totalTokenCount || Math.ceil(result.text.length / 4);
      stat.totalTokens += tokenCount;
      stat.modelName = this.geminiClient.model;
      return { fullText: result.text, tokenCount };
    }

    // Local model (Ollama / LM Studio loopback)
    const messages: ChatMessage[] = [...history, { role: "user", content: prompt }];
    const fullText = await this.localClient.streamChat(messages, onToken, signal);
    const estimatedTokens = Math.ceil(fullText.length / 4);
    stat.totalTokens += estimatedTokens;
    stat.modelName = this.localClient.modelName;
    return { fullText, tokenCount: estimatedTokens };
  }
}
