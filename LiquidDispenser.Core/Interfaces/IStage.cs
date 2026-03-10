namespace LiquidDispenser.Core.Interfaces
{
    public interface IStage<TAxis> where TAxis : Enum
    {
        IReadOnlyList<IMotor> Motors { get; }

        string StageName { get; }

        IMotor GetMotor(TAxis axis);

        Task HomeAsync(CancellationToken cancellationToken = default);

        Task MoveToAsync(
            Dictionary<TAxis, double> positions,
            double speed,
            bool isRelative,
            CancellationToken cancellationToken = default);
    }
}
