namespace LiquidDispenser.Core.Interfaces
{
    public interface IWell
    {
        double Capacity { get; set; }
        string LiquidType { get; set; }
        string Name { get; set; }
        double Volume { get; set; }

        double CalculateX(double originX, double pitchX, int column);
        double CalculateY(double originY, double pitchY, int row);
    }
}