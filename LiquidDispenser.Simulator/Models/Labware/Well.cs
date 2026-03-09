using LiquidDispenser.Core.Interfaces;

namespace LiquidDispenser.Simulator.Models.Labware;

public class Well : IWell
{
    public Well(string name, double initialVolume = 0, double capacity = 100)
    {
        Name = name;
        Volume = initialVolume;
        Capacity = capacity;
    }

    public double Capacity { get; set; }

    public string LiquidType { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public double Volume { get; set; }

    // Physical absolute coordinates on the deck
    public double CalculateX(double originX, double pitchX, int column) => originX + (column * pitchX);

    public double CalculateY(double originY, double pitchY, int row) => originY + (row * pitchY);
}
