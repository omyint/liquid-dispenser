using LiquidDispenser.Core.Interfaces;

namespace LiquidDispenser.Simulator;

public class TransferJob
{
    public TransferJob(
        ILabware source,
        int sourceRowStart,
        int sourceColumnIndex,
        ILabware dest,
        int destRowStart,
        int destColumn,
        double volume)
    {
        SourceLabware = source;
        SourceRowStart = sourceRowStart;
        SourceColumnIndex = sourceColumnIndex;
        DestinationLabware = dest;
        DestRowStart = destRowStart;
        DestColumn = destColumn;
        Volume = volume;
    }

    public int DestColumn { get; set; }

    public ILabware DestinationLabware { get; set; }

    public int DestRowStart { get; set; }

    public int SourceColumnIndex { get; set; }

    public ILabware SourceLabware { get; set; }

    public int SourceRowStart { get; set; }

    public double Volume { get; set; }
}
