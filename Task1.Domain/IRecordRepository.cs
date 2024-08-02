using Task1.Domain.Models;

namespace Task1.Domain;

public interface IRecordRepository
{
    Task<Record[]> SaveRecords(IEnumerable<Record> items);
    Task<Record[]> SearchRecords(RecordSearchQuery query);
}
