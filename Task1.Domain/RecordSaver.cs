using System.ComponentModel.DataAnnotations;
using Task1.Domain.Models;

namespace Task1.Domain;

public class RecordSaver
{
    private readonly IRecordRepository _repository;

    public RecordSaver(IRecordRepository repository)
    {
        _repository = repository;
    }

    public Task<Record[]> SaveItems(KeyValuePair<int, string?>[] incomingData)
    {
        var hasRepeatedCodes = incomingData
            .GroupBy(x => x.Key)
            .Where(g => g.Count() > 1)
            .Count() > 0;

        if (hasRepeatedCodes)
        {
            throw new ValidationException("Sequence contains repeated code.");
        }

        var records = incomingData
            .OrderBy(x => x.Key)
            .Select((x, index) => new Record { Index = index + 1, Code = x.Key, Value = x.Value })
            .ToArray();

        return _repository.SaveRecords(records);
    }
}
