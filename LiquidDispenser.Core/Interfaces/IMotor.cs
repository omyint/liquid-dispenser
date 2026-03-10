namespace LiquidDispenser.Core.Interfaces
{
    public interface IMotor
    {
        double CurrentPosition { get; }

        string Name { get; }

        Task HomeAsync(CancellationToken cancellationToken = default);

        Task MoveToAsync(double position, double speed, bool isRelative, CancellationToken cancellationToken = default);
    }
}
