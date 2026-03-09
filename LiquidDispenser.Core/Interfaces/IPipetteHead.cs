namespace LiquidDispenser.Core.Interfaces;

public interface IPipetteHead
{
    double[] AspiratedVolumes { get; }

    bool HasTips { get; }

    int TipCount { get; }

    double TipPitchY { get; } // Spacing between tips in mm

    Task AspirateAsync(double volumePerTip);

    Task DispenseAsync(double volumePerTip);

    Task DropTipsAsync();

    Task PickupTipsAsync();
}
