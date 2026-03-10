using LiquidDispenser.Core.Interfaces;

namespace LiquidDispenser.Simulator.Models.Hardware
{
    public abstract class MotorBase : IMotor
    {
        private const int MaxDelayMs = 1; // forced to 1ms in order to speed up.

        public double CurrentPosition { get; protected set; }

        public abstract string Name { get; }

        public virtual async Task HomeAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Delay(100, cancellationToken); // Simulate homing time
            CurrentPosition = 0;
        }

        public virtual async Task MoveToAsync(
            double position,
            double speed,
            bool isRelative,
            CancellationToken cancellationToken = default)
        {
            if (speed <= 0)
                throw new ArgumentException("Speed must be greater than zero", nameof(speed));

            double targetPosition = isRelative ? CurrentPosition + position : position;
            double distance = Math.Abs(targetPosition - CurrentPosition);

            cancellationToken.ThrowIfCancellationRequested();
            if (distance == 0)
                return;

            int totalDelayMs = (int)((distance / speed) * 1000);
            totalDelayMs = Math.Min(totalDelayMs, MaxDelayMs); // Cap the delay

            int stepDelayMs = 50; // Update position every 50ms
            int steps = totalDelayMs / stepDelayMs;
            double positionIncrement = distance / steps;

            int direction = targetPosition > CurrentPosition ? 1 : -1;

            for (int i = 0; i < steps; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(stepDelayMs, cancellationToken);
                CurrentPosition += positionIncrement * direction;
            }

            CurrentPosition = targetPosition; // Ensure exact final position
        }
    }
}
