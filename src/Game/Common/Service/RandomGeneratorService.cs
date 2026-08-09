using System;

namespace Game.Game.Common.Service;

public class RandomGeneratorService : IRandomGeneratorService
{
    private Random _random;

    public RandomGeneratorService()
    {
        _random = new Random();
    }

    public int RandiRange(int from, int to)
    {
        return _random.Next(from, to + 1);
    }

    public void SetSeed(ulong seed)
    {
        _random = new Random((int)seed);
    }
}
