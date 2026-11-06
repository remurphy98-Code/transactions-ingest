using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TransactionsIngest.Api.Data;
using TransactionsIngest.Api.Services;

namespace TransactionsIngest.Api.Controllers;

[ApiController]
[Route("api")]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TransactionImportService _importService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        AppDbContext db,
        TransactionImportService importService,
        ILogger<TransactionsController> logger)
    {
        _db = db;
        _importService = importService;
        _logger = logger;
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(CancellationToken cancellationToken)
    {
        try
        {
            await _importService.ImportAsync(cancellationToken);
            return Ok(new { message = "Import completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import failed");
            return StatusCode(500, new { error = "Import failed", detail = ex.Message });
        }
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions(CancellationToken cancellationToken)
    {
        var list = await _db.Transactions
            .OrderByDescending(t => t.TransactionTime)
            .Select(t => new
            {
                t.Id,
                t.SourceSystem,
                t.TransactionId,
                t.CardNumber,
                t.LocationCode,
                t.ProductName,
                t.Amount,
                t.TransactionTime,
                Status = t.Status.ToString(),
                t.CreatedAt,
                t.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [HttpGet("transactions/{id:int}/audit")]
    public async Task<IActionResult> GetAudit(int id, CancellationToken cancellationToken)
    {
        var entries = await _db.AuditEntries
            .Where(a => a.TransactionId == id)
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new
            {
                a.ChangeType,
                a.FieldName,
                a.OldValue,
                a.NewValue,
                a.Timestamp,
                a.ImportBatchId
            })
            .ToListAsync(cancellationToken);

        return Ok(entries);
    }
}
