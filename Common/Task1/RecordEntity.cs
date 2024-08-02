using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.Task1;

public class RecordEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public required int Index { get; set; }

    public required int Code { get; set; }

    public string? Value { get; set; }
}
