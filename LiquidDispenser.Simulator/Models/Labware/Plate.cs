using LiquidDispenser.Core.Interfaces;
using System;

namespace LiquidDispenser.Simulator.Models.Labware;

public enum PlateFormat
{
    Plate96,
    Plate384
}

public class Plate : ILabware
{
    public Plate(PlateFormat format = PlateFormat.Plate384)
    {
        Format = format;
        double capacity;
        switch (format)
        {
            case PlateFormat.Plate96:
                Rows = 8;
                Columns = 12;
                PitchX = 9.0;
                PitchY = 9.0;
                capacity = 300;
                break;
            case PlateFormat.Plate384:
            default:
                Rows = 16;
                Columns = 24;
                PitchX = 4.5;
                PitchY = 4.5;
                capacity = 100;
                break;
        }

        Wells = new Well[Rows, Columns];
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                string name = $"{(char)('A' + r)}{c + 1}";
                Wells[r, c] = new Well(name, initialVolume: capacity, capacity: capacity)
                {
                    LiquidType = $"Plate_{r}_{c}"
                };
            }
        }
    }

    public int Columns { get; }

    public PlateFormat Format { get; }

    public double OriginX { get; } = 20.0;

    public double OriginY { get; } = 20.0;

    public double PitchX { get; }

    public double PitchY { get; }

    public int Rows { get; }

    public IWell[,] Wells { get; }
}
