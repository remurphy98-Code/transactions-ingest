using TransactionsIngest.Api.Models;

namespace TransactionsIngest.Api.Services;

public class StoreDowntownClient : ISourceSystemClient
{
    public string SourceSystem => "Store Downtown";

    public Task<IReadOnlyList<SourceTransaction>> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var data = new List<SourceTransaction>
        {
            new()
            {
                SourceSystem = SourceSystem,
                TransactionId = 100001,
                CardNumber = "***0002",
                LocationCode = "DOWNTOWN",
                ProductName = "Mechanical Keyboard",
                Amount = 89.99m,
                TransactionTime = now.AddHours(-5)
            }
        };
        return Task.FromResult<IReadOnlyList<SourceTransaction>>(data);
    }
}
