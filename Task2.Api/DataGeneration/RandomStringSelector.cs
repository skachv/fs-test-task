namespace Task2.Api.DataGeneration;

public class RandomStringSelector
{
    private readonly Random _rnd;
    private readonly string[] _options;

    public RandomStringSelector(Random random, string[] options)
    {
        _rnd = random;
        _options = options;
    }

    public string ChooseNext()
    {
        return _options[_rnd.Next(_options.Length)];
    }
}
