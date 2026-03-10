namespace LiquidDispenser.Core.Interfaces;

public interface IPipetteHead
{
    double[] AspiratedVolumes { get; }

    bool HasTips { get; }

    int TipCount { get; }

    double TipPitchY { get; } // Spacing between tips in mm

    Task AspirateAsync(double volumePerTip, CancellationToken cancellationToken = default);

    Task DispenseAsync(double volumePerTip, CancellationToken cancellationToken = default);

    Task DropTipsAsync(CancellationToken cancellationToken = default);

    Task PickupTipsAsync(CancellationToken cancellationToken = default);
}
