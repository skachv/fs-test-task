namespace Task1.Domain.Models;

public class RecordSearchQuery
{
    public int? Skip { get; set; }
    public int? Take { get; set; }
    public int? IndexFrom { get; set; }
    public int? IndexTo { get; set; }
    public int[]? Indexes { get; set; }
    public int? CodeFrom { get; set; }
    public int? CodeTo { get; set; }
    public int[]? Codes { get; set; }
    public string? ValueSubstring { get; set; }
}
