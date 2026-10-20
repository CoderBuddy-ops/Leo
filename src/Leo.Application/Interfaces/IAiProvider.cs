using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Leo.Core.Models;

namespace Leo.Application.Interfaces;

public interface IAiProvider
{
    AiProviderType ProviderType { get; }
    string CurrentModel { get; set; }
    Task<ConnectionTestResult> TestConnectionAsync(CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> StreamChatAsync(IEnumerable<ChatMessage> history, CancellationToken cancellationToken = default);
}
