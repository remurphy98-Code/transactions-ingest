namespace TransactionsIngest.Api.Models;

public enum TransactionStatus
{
    Active = 0,
    Revoked = 1,
    Finalized = 2
}

public class Transaction
{
    public int Id { get; set; }

    public string SourceSystem { get; set; } = string.Empty;
    public int TransactionId { get; set; }

    public string? CardNumber { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransactionTime { get; set; } // UTC

    public TransactionStatus Status { get; set; } = TransactionStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<AuditEntry> AuditEntries { get; set; } = new();
}
