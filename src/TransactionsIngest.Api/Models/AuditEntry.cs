namespace TransactionsIngest.Api.Models;

public class AuditEntry
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public Transaction Transaction { get; set; } = null!;

    public string ChangeType { get; set; } = string.Empty; // Created, Updated, Revoked, Finalized
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? ImportBatchId { get; set; }
}
