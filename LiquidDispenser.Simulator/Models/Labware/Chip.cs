using LiquidDispenser.Core.Interfaces;

namespace LiquidDispenser.Simulator.Models.Labware;

public enum ChipFormat
{
    Chip48x48,
    Chip64x64,
    Chip96x96
}

public class Chip : ILabware
{
    public Chip(ChipFormat format = ChipFormat.Chip48x48)
    {
        Format = format;

        switch (format)
        {
            case ChipFormat.Chip96x96:
                Rows = 96;
                Columns = 96;
                PitchX = 0.75;
                PitchY = 0.75;
                break;
            case ChipFormat.Chip64x64:
                Rows = 64;
                Columns = 64;
                PitchX = 1.0;
                PitchY = 1.0;
                break;
            case ChipFormat.Chip48x48:
            default:
                Rows = 48;
                Columns = 48;
                PitchX = 1.5;
                PitchY = 1.5;
                break;
        }

        Wells = new Well[Rows, Columns];
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                string name = $"R{r}C{c}";
                Wells[r, c] = new Well(name, initialVolume: 0, capacity: 10.0);
            }
        }
    }

    public int Columns { get; }

    public ChipFormat Format { get; }

    public double OriginX { get; } = 160.0;

    public double OriginY { get; } = 20.0;

    public double PitchX { get; }

    public double PitchY { get; }

    public int Rows { get; }

    public IWell[,] Wells { get; }
}
