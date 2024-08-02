using Common;
using Common.Task3;
using Microsoft.EntityFrameworkCore;

namespace Task3.Api;

public class DatesGenerator
{
    private readonly TestTaskContext _context;

    public DatesGenerator(TestTaskContext context)
    {
        _context = context;
    }

    public void FillData(int numberOfClients, int numberOfDates, int? randomSeed)
    {
        _context.Database.ExecuteSql($"TRUNCATE TABLE public.\"Dates\"");

        var rnd = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();

        var startDay = DateTime.Now.AddYears(-5).Date;

        var range = (DateTime.Now.Date - startDay).Days;

        for (int i = 0; i < numberOfDates; i++)
        {
            var randomDate = startDay.AddDays(rnd.Next(range)).Date;

            _context.Dates.Add(new ClientsDate { ClientId = rnd.Next(numberOfClients), Date = DateOnly.FromDateTime(randomDate) });

            if (i % 10000 == 0)
            {
                _context.SaveChanges();
                _context.ChangeTracker.Clear();
            }
        }
        _context.SaveChanges();
    }
}
