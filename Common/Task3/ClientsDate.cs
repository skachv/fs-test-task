using System.ComponentModel.DataAnnotations;

namespace Common.Task3;

public class ClientsDate
{
    [Key]
    public int Id { get; set; }
    public int ClientId { get; set; }
    public DateOnly Date { get; set; }
}
