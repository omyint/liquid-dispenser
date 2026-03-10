using LiquidDispenser.Core.Interfaces;
using LiquidDispenser.Simulator.Models.Hardware;
using System;

namespace LiquidDispenser.Simulator;

public class Instrument
{
    public Instrument()
    {
        Stage = new XyzStage();
        Head = new EightTipHead();
    }

    public event EventHandler? StateChanged;

    public double CurrentX => Stage.GetMotor(Axis.X).CurrentPosition;

    public double CurrentY => Stage.GetMotor(Axis.Y).CurrentPosition;

    public double CurrentZ => Stage.GetMotor(Axis.Z).CurrentPosition;

    public IPipetteHead Head { get; }

    public IStage<Axis> Stage { get; }

    public async Task ExecuteTransferAsync(TransferJob job, CancellationToken cancellationToken = default)
    {
        // Calculate the physical Cartesian coordinate of the FIRST tip (Tip 0)
        var firstSourceWell = job.SourceLabware.Wells[job.SourceRowStart, job.SourceColumnIndex];
        double sourceX = firstSourceWell.CalculateX(
            job.SourceLabware.OriginX,
            job.SourceLabware.PitchX,
            job.SourceColumnIndex);
        double sourceY = firstSourceWell.CalculateY(
            job.SourceLabware.OriginY,
            job.SourceLabware.PitchY,
            job.SourceRowStart);


        await MoveToAsync(sourceX, sourceY, 50, 200, cancellationToken);
        await MoveToAsync(sourceX, sourceY, 10, 50, cancellationToken);

        if (!Head.HasTips)
            await Head.PickupTipsAsync(cancellationToken);

        ValidateTipAlignment(sourceX, sourceY, "Aspirate");

        await Head.AspirateAsync(job.Volume, cancellationToken);

        // Calculate which generic rows align physically with the 8 tips
        for (int i = 0; i < Head.TipCount; i++)
        {
            // Tip i is located at: sourceY + (i * Head.TipPitchY)
            double tipY = sourceY + (i * Head.TipPitchY);

            // Map tipY backward into a Labware Row Index
            // Index = (Y - OriginY) / PitchY
            double geometricRowOffset = (tipY - job.SourceLabware.OriginY) / job.SourceLabware.PitchY;

            // Only act if the Tip aligns perfectly (within limits) to an integer index grid cell
            if (Math.Abs(geometricRowOffset % 1) <= (double.Epsilon * 100))
            {
                int r = (int)Math.Round(geometricRowOffset);
                if (r >= 0 && r < job.SourceLabware.Rows)
                {
                    job.SourceLabware.Wells[r, job.SourceColumnIndex].Volume -= job.Volume;
                }
            }
        }

        await MoveToAsync(sourceX, sourceY, 50, 100, cancellationToken);

        // Move to Dest
        var firstDestCell = job.DestinationLabware.Wells[job.DestRowStart, job.DestColumn];
        double destX = firstDestCell.CalculateX(
            originX: job.DestinationLabware.OriginX,
            pitchX: job.DestinationLabware.PitchX,
            column: job.DestColumn);
        double destY = firstDestCell.CalculateY(
            originY: job.DestinationLabware.OriginY,
            pitchY: job.DestinationLabware.PitchY,
            row: job.DestRowStart);

        await MoveToAsync(destX, destY, 50, 200, cancellationToken);
        await MoveToAsync(destX, destY, 5, 50, cancellationToken);

        ValidateTipAlignment(destX, destY, "Dispense");

        await Head.DispenseAsync(job.Volume, cancellationToken);

        for (int i = 0; i < Head.TipCount; i++)
        {
            // Calculate exact Source Array Index for Liquid Type matching 
            double sourceTipY = sourceY + (i * Head.TipPitchY);
            double sourceRowOffset = (sourceTipY - job.SourceLabware.OriginY) / job.SourceLabware.PitchY;
            string sourceLiquidType = string.Empty;

            if (Math.Abs(sourceRowOffset % 1) <= (double.Epsilon * 100))
            {
                int srcR = (int)Math.Round(sourceRowOffset);
                if (srcR >= 0 && srcR < job.SourceLabware.Rows)
                {
                    sourceLiquidType = job.SourceLabware.Wells[srcR, job.SourceColumnIndex].LiquidType;
                }
            }

            // Calculate Dest physical tip-to-well overlap
            double destTipY = destY + (i * Head.TipPitchY);
            double destRowOffset = (destTipY - job.DestinationLabware.OriginY) / job.DestinationLabware.PitchY;

            if (Math.Abs(destRowOffset % 1) <= (double.Epsilon * 100))
            {
                int destR = (int)Math.Round(destRowOffset);
                if (destR >= 0 && destR < job.DestinationLabware.Rows)
                {
                    var destCell = job.DestinationLabware.Wells[destR, job.DestColumn];
                    destCell.Volume += job.Volume;

                    // If we successfully mapped a source liquid and we are filling an empty cell
                    if (!string.IsNullOrEmpty(sourceLiquidType) && destCell.Volume <= job.Volume)
                    {
                        destCell.LiquidType = sourceLiquidType;
                    }
                }
            }
        }

        await MoveToAsync(destX, destY, 50, 100, cancellationToken);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await Stage.HomeAsync(cancellationToken);
        OnStateChanged();
    }


    //easy to use method to move XYZ
    private async Task MoveToAsync(
        double x,
        double y,
        double z,
        double speed,
        CancellationToken cancellationToken = default)
    {
        var movments = new Dictionary<Axis, double> { { Axis.X, x }, { Axis.Y, y }, { Axis.Z, z } };
        await Stage.MoveToAsync(movments, speed, isRelative: false, cancellationToken);
        OnStateChanged();
    }

    private void OnStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ValidateTipAlignment(double targetX, double targetY, string operationName)
    {
        if (Math.Abs(CurrentX - targetX) > 0.1 || Math.Abs(CurrentY - targetY) > 0.1)
        {
            throw new InvalidOperationException(
                $"Physical crash detected! Cannot {operationName} at X:{CurrentX}, Y:{CurrentY}. Target well is at X:{targetX}, Y:{targetY}");
        }
    }
}
