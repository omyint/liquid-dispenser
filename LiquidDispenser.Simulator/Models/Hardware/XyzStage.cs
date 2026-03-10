using LiquidDispenser.Core.Interfaces;
using System;

namespace LiquidDispenser.Simulator.Models.Hardware;

public class XyzStage : IStage<Axis>
{
    public XyzStage()
    {
        Motors = new List<IMotor> { new XMotor(), new YMotor(), new ZMotor() };
    }

    public IReadOnlyList<IMotor> Motors { get; }

    public string StageName => "XYZ Stage";

    public IMotor GetMotor(Axis axis) => axis switch
    {
        Axis.X => Motors.OfType<XMotor>().FirstOrDefault()!,
        Axis.Y => Motors.OfType<YMotor>().FirstOrDefault()!,
        Axis.Z => Motors.OfType<ZMotor>().FirstOrDefault()!,
        _ => throw new ArgumentException($"Invalid axis: {axis}", nameof(axis))
    };

    public async Task HomeAsync(CancellationToken cancellationToken = default)
    {
        foreach (IMotor motor in Motors)
        {
            await motor.HomeAsync(cancellationToken);
        }
    }

    public async Task MoveToAsync(
        Dictionary<Axis, double> positions,
        double speed,
        bool isRelative,
        CancellationToken cancellationToken = default)
    {
        foreach (var (axis, position) in positions)
        {
            var motor = GetMotor(axis);
            await motor.MoveToAsync(position, speed, isRelative, cancellationToken);
        }
    }
}
