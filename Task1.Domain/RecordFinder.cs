using System.ComponentModel.DataAnnotations;
using Task1.Domain.Models;

namespace Task1.Domain;

public class RecordFinder
{
    private readonly IRecordRepository _repository;

    public RecordFinder(IRecordRepository repository)
    {
        _repository = repository;
    }

    public Task<Record[]> Find(RecordSearchQuery? searchQuery)
    {
        if(searchQuery != null)
        {
            List<string> errors = [];

            errors.ValidateBoundaries(searchQuery.IndexFrom, searchQuery.IndexTo, nameof(searchQuery.IndexFrom), nameof(searchQuery.IndexTo));
            errors.ValidateBoundaries(searchQuery.CodeFrom, searchQuery.CodeTo, nameof(searchQuery.CodeFrom), nameof(searchQuery.CodeTo));

            errors.ValidateArrayNotEmpty(searchQuery.Indexes, nameof(searchQuery.Indexes));
            errors.ValidateArrayNotEmpty(searchQuery.Codes, nameof(searchQuery.Codes));

            errors.ValidateGreaterThan(searchQuery.Skip, 0, nameof(searchQuery.Skip));
            errors.ValidateGreaterThan(searchQuery.Take, 1, nameof(searchQuery.Take));

            if(errors.Count > 0)
            {
                throw new ValidationException(string.Join("; ", errors));
            }
        }

        return _repository.SearchRecords(searchQuery);
    }
}
