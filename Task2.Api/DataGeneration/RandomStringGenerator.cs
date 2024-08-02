namespace Task2.Api.DataGeneration;

public class RandomStringGenerator
{
    private readonly Random _rnd;

    private string[] _dummies;

    private Dictionary<string, int> _counters;

    public RandomStringGenerator(Random random, string[] dummies)
    {
        _rnd = random;
        _dummies = dummies;
        _counters = dummies.ToDictionary(x => x , _ => 0);
    }

    public string GetNext()
    {
        var index = _rnd.Next(_dummies.Length);
        var key = _dummies[index];
        var postfix = _counters[key]++;
        return key + (postfix == 0 ? string.Empty : postfix);
    }

    public IEnumerable<string> GetNext(int number)
    {
        for(int i = 0; i < number; i++)
        {
            yield return GetNext();
        }
    }
}
