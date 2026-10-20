using System.Threading.Tasks;
using Leo.Core.Models;

namespace Leo.Application.Interfaces;

public class AppSettings
{
    public AiProviderConfig AiConfig { get; set; } = new();
    public bool LaunchAtStartup { get; set; } = false;
    public bool ReducedMotion { get; set; } = false;
    public bool AudioMuted { get; set; } = false;
    public double SpeechVolume { get; set; } = 1.0;
    public string PreferredVoice { get; set; } = string.Empty;
}

public interface ISettingsService
{
    AppSettings CurrentSettings { get; }
    Task LoadSettingsAsync();
    Task SaveSettingsAsync(AppSettings settings);
}
