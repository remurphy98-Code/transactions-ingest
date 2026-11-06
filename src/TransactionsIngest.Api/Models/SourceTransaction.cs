namespace TransactionsIngest.Api.Models;

public class SourceTransaction
{
    public string SourceSystem { get; set; } = string.Empty;
    public int TransactionId { get; set; }
    public string? CardNumber { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransactionTime { get; set; }
}
