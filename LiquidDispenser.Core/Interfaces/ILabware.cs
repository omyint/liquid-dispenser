
namespace LiquidDispenser.Core.Interfaces;

public interface ILabware
{
    int Columns { get; }

    double OriginX { get; }

    double OriginY { get; }

    double PitchX { get; }

    double PitchY { get; }

    int Rows { get; }

    IWell[,] Wells { get; }
}
