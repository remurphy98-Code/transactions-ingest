using TransactionsIngest.Api.Models;

namespace TransactionsIngest.Api.Services;

public interface ISourceSystemClient
{
    string SourceSystem { get; }
    Task<IReadOnlyList<SourceTransaction>> GetSnapshotAsync(CancellationToken cancellationToken = default);
}
