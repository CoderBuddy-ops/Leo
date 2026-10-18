/**
 * Speech Transcript Cleanup and Formatting Pipeline
 *
 * Implements deterministic rule-based transformations to remove spoken verbal filler words,
 * resolve mid-sentence verbal self-corrections, and format spoken lists cleanly.
 */

export interface SpeechCleanupOptions {
  removeFillers?: boolean;
  resolveCorrections?: boolean;
  formatBulletLists?: boolean;
  verbatimMode?: boolean;
}

export class SpeechCleanupFilter {
  private static FILLERS_REGEX = /\b(um|uh|er|ah|like,\s*actually|you know)\b[\s,]*/gi;

  /**
   * Cleans spoken transcripts according to configured options
   */
  public static clean(transcript: string, options: SpeechCleanupOptions = {}): string {
    if (!transcript || options.verbatimMode) {
      return transcript;
    }

    let text = transcript;

    // 1. Resolve spoken self-corrections (e.g. "[Thursday, actually] Friday" -> "Friday")
    if (options.resolveCorrections !== false) {
      // Matches patterns like: "at 5, actually 6" or "Thursday, wait no, Friday"
      text = text.replace(/([a-zA-Z0-9_\-]+),\s*(?:actually|wait no|I mean|make that)\s+([a-zA-Z0-9_\-]+)/gi, '$2');
      text = text.replace(/\[([^\]]+),\s*actually\]\s*([a-zA-Z0-9_\-]+)/gi, '$2');
    }

    // 2. Remove filler words (ums, uhs)
    if (options.removeFillers !== false) {
      text = text.replace(this.FILLERS_REGEX, '');
      text = text.replace(/\[(?:um|uh),?\]\s*/gi, '');
    }

    // 3. Convert spoken list syntax into clean markdown bullet points
    if (options.formatBulletLists !== false) {
      if (/\{?Bullet point\}?/i.test(text)) {
        const parts = text.split(/\{?Bullet point\}?/i);
        const header = parts[0].trim();
        const items = parts.slice(1).map(item => `- ${item.trim().replace(/^([a-z])/, (c) => c.toUpperCase())}`);
        text = header ? `${header}\n${items.join('\n')}` : items.join('\n');
      }
    }

    // 4. Clean up whitespace and punctuation
    text = text.replace(/\s{2,}/g, ' ').trim();
    if (text.length > 0) {
      // Ensure first character is capitalized
      text = text.charAt(0).toUpperCase() + text.slice(1);
      // Ensure trailing punctuation if missing
      if (!/[.!?]$/.test(text) && !text.includes('\n')) {
        text += '.';
      }
    }

    return text;
  }
}
