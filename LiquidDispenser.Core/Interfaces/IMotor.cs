namespace LiquidDispenser.Core.Interfaces
{
    public interface IMotor
    {
        double CurrentPosition { get; }

        string Name { get; }

        Task HomeAsync();

        Task MoveToAsync(double position, double speed, bool isRelative);
    }
}
