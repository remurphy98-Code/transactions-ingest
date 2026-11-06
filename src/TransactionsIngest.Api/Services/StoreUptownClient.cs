using TransactionsIngest.Api.Models;

namespace TransactionsIngest.Api.Services;

public class StoreUptownClient : ISourceSystemClient
{
    public string SourceSystem => "Store Uptown";

    public Task<IReadOnlyList<SourceTransaction>> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        // Sample data with recent times so revoke logic can be demonstrated
        var now = DateTime.UtcNow;
        var data = new List<SourceTransaction>
        {
            new()
            {
                SourceSystem = SourceSystem,
                TransactionId = 100001,
                CardNumber = "***1111",
                LocationCode = "UPTOWN",
                ProductName = "Wireless Mouse",
                Amount = 24.99m,
                TransactionTime = now.AddHours(-2)
            },
            new()
            {
                SourceSystem = SourceSystem,
                TransactionId = 100002,
                CardNumber = "***2222",
                LocationCode = "UPTOWN",
                ProductName = "Keyboard",
                Amount = 79.99m,
                TransactionTime = now.AddHours(-10)
            }
        };
        return Task.FromResult<IReadOnlyList<SourceTransaction>>(data);
    }
}
