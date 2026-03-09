using LiquidDispenser.Core.Interfaces;

namespace LiquidDispenser.Simulator.Models.Hardware
{
    public abstract class MotorBase : IMotor
    {
        private const int MaxDelayMs = 5000; // 5 seconds max

        public double CurrentPosition { get; protected set; }

        public abstract string Name { get; }

        public virtual async Task HomeAsync()
        {
            await Task.Delay(100); // Simulate homing time
            CurrentPosition = 0;
        }

        public virtual async Task MoveToAsync(double position, double speed, bool isRelative)
        {
            if (speed <= 0)
                throw new ArgumentException("Speed must be greater than zero", nameof(speed));

            double targetPosition = isRelative ? CurrentPosition + position : position;
            double distance = Math.Abs(targetPosition - CurrentPosition);

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
                await Task.Delay(stepDelayMs);
                CurrentPosition += positionIncrement * direction;
            }

            CurrentPosition = targetPosition; // Ensure exact final position
        }
    }
}
