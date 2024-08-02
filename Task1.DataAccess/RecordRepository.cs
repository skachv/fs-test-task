using Common;
using Common.Task1;
using Microsoft.EntityFrameworkCore;
using Task1.Domain;
using Task1.Domain.Models;

namespace Task1.DataAccess;

public class RecordRepository : IRecordRepository
{
    private readonly TestTaskContext _context;

    public RecordRepository(TestTaskContext context)
    {
        _context = context;
    }

    public async Task<Record[]> SaveRecords(IEnumerable<Record> incomingRecords)
    {
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                await _context.Database.ExecuteSqlAsync($"TRUNCATE TABLE public.\"Records\"");

                await _context.Records.AddRangeAsync(incomingRecords
                    .Select(x => new RecordEntity { Index = x.Index, Code = x.Code, Value = x.Value }));

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        return await _context.Records.Select(x => new Record { Index = x.Index, Code = x.Code, Value = x.Value }).ToArrayAsync();
    }

    public async Task<Record[]> SearchRecords(RecordSearchQuery? query)
    {
        IQueryable<RecordEntity> dbQuery = _context.Records.OrderBy(x => x.Code);

        if(query != null)
        {
            if (query.IndexFrom.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.Index >= query.IndexFrom);
            }

            if (query.IndexTo.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.Index <= query.IndexTo);
            }

            if(query.Indexes != null)
            {
                dbQuery = dbQuery.Where(x => query.Indexes.Contains(x.Index));
            }

            if (query.CodeFrom.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.Code >= query.CodeFrom);
            }

            if (query.CodeTo.HasValue)
            {
                dbQuery = dbQuery.Where(x => x.Code <= query.CodeTo);
            }

            if (query.Codes != null)
            {
                dbQuery = dbQuery.Where(x => query.Codes.Contains(x.Code));
            }

            if (query.ValueSubstring != null)
            {
                dbQuery = dbQuery.Where(x => x.Value != null && EF.Functions.ILike(x.Value, $"%{query.ValueSubstring}%"));
            }

            if (query.Skip.HasValue)
            {
                dbQuery = dbQuery.Skip(query.Skip.Value);
            }

            if (query.Take.HasValue)
            {
                dbQuery = dbQuery.Take(query.Take.Value);
            }
        }

        return await dbQuery.Select(x => new Record { Index = x.Index, Code = x.Code, Value = x.Value }).ToArrayAsync();
    }
}
