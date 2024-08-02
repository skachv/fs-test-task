namespace Task1.Domain.Models;

public class Record
{
    public int Index { get; set; }
    public required int Code { get; set; }
    public string? Value { get; set; }
}
