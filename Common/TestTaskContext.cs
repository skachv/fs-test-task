using Common.Task1;
using Common.Task2;
using Common.Task3;
using Microsoft.EntityFrameworkCore;

namespace Common;

public class TestTaskContext : DbContext
{
    public TestTaskContext(DbContextOptions<TestTaskContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }

    // Task 1 tables
    public DbSet<RecordEntity> Records { get; set; }

    // Task 2 tables
    public DbSet<Client> Clients { get; set; }
    public DbSet<Contact> Contacts { get; set; }

    // Task 3 tables
    public DbSet<ClientsDate> Dates { get; set; }
}
