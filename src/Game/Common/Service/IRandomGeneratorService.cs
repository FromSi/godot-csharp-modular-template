namespace Game.Game.Common.Service;

public interface IRandomGeneratorService
{
    int RandiRange(int from, int to);
    void SetSeed(ulong seed);
}
