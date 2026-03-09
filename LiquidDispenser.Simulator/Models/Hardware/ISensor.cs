namespace LiquidDispenser.Simulator.Models.Hardware;

public interface ISensor
{
    bool DetectLiquid(double currentZ);
}
