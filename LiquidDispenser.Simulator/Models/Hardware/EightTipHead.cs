using LiquidDispenser.Core.Interfaces;
using System;
using System.Linq;

namespace LiquidDispenser.Simulator.Models.Hardware;

public class EightTipHead : IPipetteHead
{
    public EightTipHead()
    {
        AspiratedVolumes = new double[TipCount];
    }

    public double[] AspiratedVolumes { get; private set; }

    public bool HasTips { get; private set; }

    public int TipCount => 8;

    public double TipPitchY => 9.0; // Standard 8-tip 96-well pitch is 9.0mm

    public async Task AspirateAsync(double volumePerTip)
    {
        if (!HasTips)
            throw new InvalidOperationException("Cannot aspirate without tips.");

        await Task.Delay(300); // Simulate plunger movement
        for (int i = 0; i < TipCount; i++)
        {
            AspiratedVolumes[i] += volumePerTip;
        }
    }

    public async Task DispenseAsync(double volumePerTip)
    {
        if (!HasTips)
            throw new InvalidOperationException("Cannot dispense without tips.");
        if (AspiratedVolumes.Any(v => v < volumePerTip))
        {
            // For simplicity, we just empty what we have if we request more, or throw. Let's throw for realism.
            throw new InvalidOperationException("Insufficient volume in one or more tips.");
        }

        await Task.Delay(300); // Simulate plunger movement
        for (int i = 0; i < TipCount; i++)
        {
            AspiratedVolumes[i] -= volumePerTip;
        }
    }

    public async Task DropTipsAsync()
    {
        await Task.Delay(200);
        HasTips = false;
        Array.Clear(AspiratedVolumes, 0, TipCount);
    }

    public async Task PickupTipsAsync()
    {
        await Task.Delay(400); // Simulating pressing into a tip rack
        HasTips = true;
    }
}
