using System.ComponentModel.DataAnnotations;

namespace Common.Task2;

public class Client
{
    [Key]
    public long ClientId { get; set; }

    [StringLength(200)]
    public string? ClientName { get; set; }

    public ICollection<Contact> Contacts { get; set; }
}
