using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TransactionsIngest.Api.Data;
using TransactionsIngest.Api.Models;
using TransactionsIngest.Api.Services;

namespace TransactionsIngest.Tests;

public class ImportServiceTests
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Import_CreatesNewTransactions()
    {
        await using var db = CreateDb();
        var clients = new List<ISourceSystemClient> { new StoreUptownClient() };
        var service = new TransactionImportService(db, clients, NullLogger<TransactionImportService>.Instance);

        await service.ImportAsync();

        var count = await db.Transactions.CountAsync();
        Assert.Equal(2, count);
        Assert.Contains(await db.AuditEntries.ToListAsync(), a => a.ChangeType == "Created");
    }

    [Fact]
    public async Task Import_IsIdempotent()
    {
        await using var db = CreateDb();
        var clients = new List<ISourceSystemClient> { new StoreUptownClient() };
        var service = new TransactionImportService(db, clients, NullLogger<TransactionImportService>.Instance);

        await service.ImportAsync();
        var firstCount = await db.Transactions.CountAsync();
        var firstAudits = await db.AuditEntries.CountAsync();

        await service.ImportAsync();
        var secondCount = await db.Transactions.CountAsync();
        var secondAudits = await db.AuditEntries.CountAsync();

        Assert.Equal(firstCount, secondCount);
        Assert.Equal(firstAudits, secondAudits);
    }
}
