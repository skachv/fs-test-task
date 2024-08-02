using System.ComponentModel.DataAnnotations;

namespace Common.Task2;

public class Contact
{
    [Key]
    public long ContactId { get; set; }

    public long ClientId { get; set; }

    public Client Client { get; set; }

    [StringLength(255)]
    public string? ContactType { get; set; }

    [StringLength(255)]
    public string? ContactValue { get; set; }
}
