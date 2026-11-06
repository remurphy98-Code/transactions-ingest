using TransactionsIngest.Api.Models;

namespace TransactionsIngest.Api.Services;

public class OnlineClient : ISourceSystemClient
{
    public string SourceSystem => "Online";

    public Task<IReadOnlyList<SourceTransaction>> GetSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var data = new List<SourceTransaction>
        {
            new()
            {
                SourceSystem = SourceSystem,
                TransactionId = 100001,
                CardNumber = "***4444",
                LocationCode = "ONLINE",
                ProductName = "USB-C Docking Station",
                Amount = 129.95m,
                TransactionTime = now.AddHours(-1)
            },
            new()
            {
                SourceSystem = SourceSystem,
                TransactionId = 100003,
                CardNumber = null,
                LocationCode = "ONLINE",
                ProductName = "Gift Card",
                Amount = 50.00m,
                TransactionTime = now.AddDays(-2) // older than 24h
            }
        };
        return Task.FromResult<IReadOnlyList<SourceTransaction>>(data);
    }
}
