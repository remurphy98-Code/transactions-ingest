using Microsoft.EntityFrameworkCore;
using TransactionsIngest.Api.Data;
using TransactionsIngest.Api.Models;

namespace TransactionsIngest.Api.Services;

public class TransactionImportService
{
    private readonly AppDbContext _db;
    private readonly IEnumerable<ISourceSystemClient> _sourceClients;
    private readonly ILogger<TransactionImportService> _logger;

    public TransactionImportService(
        AppDbContext db,
        IEnumerable<ISourceSystemClient> sourceClients,
        ILogger<TransactionImportService> logger)
    {
        _db = db;
        _sourceClients = sourceClients;
        _logger = logger;
    }

    public async Task ImportAsync(CancellationToken cancellationToken = default)
    {
        var batchId = Guid.NewGuid().ToString("N")[..8];
        _logger.LogInformation("Starting import batch {BatchId}", batchId);

        foreach (var client in _sourceClients)
        {
            await ImportSourceAsync(client, batchId, cancellationToken);
        }

        // Optional: finalize transactions older than 24 hours
        await FinalizeOldTransactionsAsync(batchId, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Completed import batch {BatchId}", batchId);
    }

    private async Task ImportSourceAsync(ISourceSystemClient client, string batchId, CancellationToken ct)
    {
        var snapshot = await client.GetSnapshotAsync(ct);
        var source = client.SourceSystem;
        var now = DateTime.UtcNow;
        var cutoff = now.AddHours(-24);

        var existing = await _db.Transactions
            .Where(t => t.SourceSystem == source)
            .ToListAsync(ct);

        var snapshotKeys = snapshot.Select(s => s.TransactionId).ToHashSet();

        foreach (var incoming in snapshot)
        {
            var match = existing.FirstOrDefault(t => t.TransactionId == incoming.TransactionId);

            if (match == null)
            {
                var entity = MapToEntity(incoming);
                entity.Status = TransactionStatus.Active;
                _db.Transactions.Add(entity);
                AddAudit(entity, "Created", null, null, null, batchId);
                _logger.LogInformation("Created {Source}:{Id}", source, incoming.TransactionId);
            }
            else if (match.Status != TransactionStatus.Finalized)
            {
                var changed = CompareAndUpdate(match, incoming, batchId);
                if (changed)
                {
                    match.UpdatedAt = now;
                    _logger.LogInformation("Updated {Source}:{Id}", source, incoming.TransactionId);
                }
            }
        }

        // Revoke missing transactions that are within 24 hours
        foreach (var tx in existing.Where(t => t.Status == TransactionStatus.Active && t.TransactionTime >= cutoff))
        {
            if (!snapshotKeys.Contains(tx.TransactionId))
            {
                tx.Status = TransactionStatus.Revoked;
                tx.UpdatedAt = now;
                AddAudit(tx, "Revoked", null, "Active", "Revoked", batchId);
                _logger.LogInformation("Revoked {Source}:{Id}", source, tx.TransactionId);
            }
        }
    }

    private bool CompareAndUpdate(Transaction existing, SourceTransaction incoming, string batchId)
    {
        var changed = false;

        changed |= Diff(existing, "CardNumber", existing.CardNumber, incoming.CardNumber, batchId);
        changed |= Diff(existing, "LocationCode", existing.LocationCode, incoming.LocationCode, batchId);
        changed |= Diff(existing, "ProductName", existing.ProductName, incoming.ProductName, batchId);
        changed |= Diff(existing, "Amount", existing.Amount.ToString("F2"), incoming.Amount.ToString("F2"), batchId);
        changed |= Diff(existing, "TransactionTime", existing.TransactionTime.ToString("o"), incoming.TransactionTime.ToString("o"), batchId);

        if (changed)
        {
            existing.CardNumber = incoming.CardNumber;
            existing.LocationCode = incoming.LocationCode;
            existing.ProductName = incoming.ProductName;
            existing.Amount = incoming.Amount;
            existing.TransactionTime = incoming.TransactionTime;
        }

        return changed;
    }

    private bool Diff(Transaction tx, string field, string? oldVal, string? newVal, string batchId)
    {
        if (oldVal != newVal)
        {
            AddAudit(tx, "Updated", field, oldVal, newVal, batchId);
            return true;
        }
        return false;
    }

    private async Task FinalizeOldTransactionsAsync(string batchId, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddHours(-24);
        var oldActive = await _db.Transactions
            .Where(t => t.Status == TransactionStatus.Active && t.TransactionTime < cutoff)
            .ToListAsync(ct);

        foreach (var tx in oldActive)
        {
            tx.Status = TransactionStatus.Finalized;
            tx.UpdatedAt = DateTime.UtcNow;
            AddAudit(tx, "Finalized", null, "Active", "Finalized", batchId);
        }
    }

    private static Transaction MapToEntity(SourceTransaction s) => new()
    {
        SourceSystem = s.SourceSystem,
        TransactionId = s.TransactionId,
        CardNumber = s.CardNumber,
        LocationCode = s.LocationCode,
        ProductName = s.ProductName,
        Amount = s.Amount,
        TransactionTime = s.TransactionTime
    };

    private void AddAudit(Transaction tx, string changeType, string? field, string? oldVal, string? newVal, string batchId)
    {
        tx.AuditEntries.Add(new AuditEntry
        {
            ChangeType = changeType,
            FieldName = field,
            OldValue = oldVal,
            NewValue = newVal,
            ImportBatchId = batchId
        });
    }
}
