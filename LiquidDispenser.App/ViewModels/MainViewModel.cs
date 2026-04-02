using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiquidDispenser.Core.Data;
using LiquidDispenser.Simulator;
using LiquidDispenser.Simulator.Models.Labware;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Threading;

namespace LiquidDispenser.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly Chip _chip;
    private readonly HttpClient _http;
    private readonly Instrument _instrument;
    private readonly Plate _plate;

    [ObservableProperty]
    private double _averageDestFillHeight;

    [ObservableProperty]
    private double _averageSourceFillHeight;

    [ObservableProperty]
    private string _chipTitle;
    private CancellationTokenSource _cts;

    [ObservableProperty]
    private string _currentJobDetails = "Idle";

    [ObservableProperty]
    private double _headX;

    [ObservableProperty]
    private double _headY;

    [ObservableProperty]
    private double _headZ;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartSimulationCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopSimulationCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResetSimulationCommand))]
    private bool _isDemoRunning = false;

    [ObservableProperty]
    private bool _isRunningCloudMode;

    [ObservableProperty]
    private string _plateTitle;

    [ObservableProperty]
    private double _remainingSourceVolume;

    [ObservableProperty]
    private string _status = "Simulator Ready";

    [ObservableProperty]
    private double _totalDispensedVolume;

    [ObservableProperty]
    private double _totalSourceVolume;

    public MainViewModel(Instrument instrument, HttpClient http)
    {
        _instrument = instrument;
        _http = http;
        _instrument.StateChanged += OnInstrumentStateChanged;

        _plate = new Plate(PlateFormat.Plate96);
        _chip = new Chip(ChipFormat.Chip64x64);

        PlateTitle = $"Source Plate ({PlateRows}x{PlateColumns})";
        ChipTitle = $"Destination Chip ({ChipRows}x{ChipColumns})";

        _cts = new CancellationTokenSource();

        // Initialize Plate
        for (int r = 0; r < PlateRows; r++)
        {
            for (int c = 0; c < PlateColumns; c++)
            {
                var well = _plate.Wells[r, c];
                PlateWells.Add(new WellViewModel(r, c, well.Capacity, well.Volume, well.LiquidType));
                TotalSourceVolume += well.Volume;
            }
        }
        RemainingSourceVolume = TotalSourceVolume;

        // Initialize Chip
        for (int r = 0; r < ChipRows; r++)
        {
            for (int c = 0; c < ChipColumns; c++)
            {
                var cell = _chip.Wells[r, c];
                ChipWells.Add(new WellViewModel(r, c, cell.Capacity, cell.Volume, cell.LiquidType));
            }
        }
    }

    public int ChipColumns => _chip.Columns;

    public int ChipRows => _chip.Rows;

    public ObservableCollection<WellViewModel> ChipWells { get; } = [];

    public int PlateColumns => _plate.Columns;

    public int PlateRows => _plate.Rows;

    public ObservableCollection<WellViewModel> PlateWells { get; } = [];

    private bool CanStartDemo() => !IsDemoRunning;

    private bool CanStop() => IsDemoRunning || IsRunningCloudMode;

    private void ComputeAverageFills()
    {
        // Compute representative visual average fill (Max Canvas Height = 100 for Source, 60 for Dest)
        double sourcePercentage = RemainingSourceVolume / (TotalSourceVolume == 0 ? 1 : TotalSourceVolume); // Initially maxed
        AverageSourceFillHeight = sourcePercentage * 100.0;

        double totalDestCapacity = ChipRows * ChipColumns * _chip.Wells[0, 0].Capacity;
        double destPercentage = TotalDispensedVolume / (totalDestCapacity == 0 ? 1 : totalDestCapacity);
        AverageDestFillHeight = destPercentage * 60.0;
    }

    private async Task ExecuteAndSyncAsync(
        TransferJob job,
        string jobName,
        CancellationToken cancellationToken = default)
    {
        await Application.Current.Dispatcher
            .InvokeAsync(() => CurrentJobDetails = jobName, DispatcherPriority.Normal, cancellationToken);

        await _instrument.ExecuteTransferAsync(job, cancellationToken);

        // Because the mapping is strictly physical now, we simply re-sync the entire array 
        // to catch whatever indices where we aspirated or dispensed
        Application.Current.Dispatcher
            .Invoke(
                () =>
                {
                    for (int r = 0; r < PlateRows; r++)
                    {
                        for (int c = 0; c < PlateColumns; c++)
                        {
                            var data = _plate.Wells[r, c];
                            PlateWells[r * PlateColumns + c].Volume = data.Volume;
                        }
                    }

                    for (int r = 0; r < ChipRows; r++)
                    {
                        for (int c = 0; c < ChipColumns; c++)
                        {
                            var data = _chip.Wells[r, c];
                            var model = ChipWells[r * ChipColumns + c];
                            model.Volume = data.Volume;
                            model.LiquidType = data.LiquidType;
                        }
                    }
                },
                DispatcherPriority.Normal,
                cancellationToken);

        UpdateMetrics();
    }

    private void OnInstrumentStateChanged(object? sender, EventArgs eventArgs)
    {
        Application.Current.Dispatcher
            .Invoke(
                () =>
                {
                    HeadX = _instrument.CurrentX;
                    HeadY = _instrument.CurrentY;
                    HeadZ = _instrument.CurrentZ;
                    Status = $"Moving... X:{HeadX:F1} Y:{HeadY:F1} Z:{HeadZ:F1}";
                });

        if (IsRunningCloudMode)
        {
            _ = _http.PostAsJsonAsync(
                "/api/telemetry",
                new TelemetryPayload { X = HeadX, Y = HeadY, Z = HeadZ, Status = "Running" });
        }
    }


    [RelayCommand(CanExecute = nameof(CanStartDemo))]
    private async Task ResetSimulationAsync()
    {
        if (IsDemoRunning)
            return;

        IsRunningCloudMode = false;
        // ensure fresh token source
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        Status = "Resetting Simulator...";
        CurrentJobDetails = "Resetting Arrays";

        await Application.Current.Dispatcher
            .InvokeAsync(
                () =>
                {
                    TotalSourceVolume = 0;

                    // Reset Plate array to full capacity
                    for (int r = 0; r < PlateRows; r++)
                    {
                        for (int c = 0; c < PlateColumns; c++)
                        {
                            var data = _plate.Wells[r, c];
                            data.Volume = data.Capacity;

                            var model = PlateWells[r * PlateColumns + c];
                            model.Volume = data.Volume;
                            TotalSourceVolume += data.Volume;
                        }
                    }
                    RemainingSourceVolume = TotalSourceVolume;

                    // Clear Destination Chip array
                    for (int r = 0; r < ChipRows; r++)
                    {
                        for (int c = 0; c < ChipColumns; c++)
                        {
                            var data = _chip.Wells[r, c];
                            data.Volume = 0;
                            data.LiquidType = string.Empty;

                            var model = ChipWells[r * ChipColumns + c];
                            model.Volume = 0;
                            model.LiquidType = string.Empty;
                        }
                    }
                    TotalDispensedVolume = 0;

                    ComputeAverageFills();
                });

        try
        {
            await _instrument.InitializeAsync(_cts.Token);
            Status = "Simulator Ready";
            CurrentJobDetails = "Idle";
        }
        catch (OperationCanceledException)
        {
            // Reset was requested/cancelled — treat as stopped reset
            Status = "Stopped";
            CurrentJobDetails = "Reset Cancelled";
        }
    }

    [RelayCommand]
    private async Task StartCloudModeAsync()
    {
        if (IsRunningCloudMode)
            return;

        // ensure fresh token source
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        IsRunningCloudMode = true;
        Status = "Connecting to cloud...";
        CurrentJobDetails = "Polling API...";

        try
        {
            await Task.Run(() => _instrument.InitializeAsync(_cts.Token), _cts.Token);

            while (IsRunningCloudMode)
            {
                try
                {
                    var pendingJobs = await _http.GetFromJsonAsync<List<JobRequestDto>>("/api/jobs/pending", _cts.Token);
                    if (pendingJobs is not null && pendingJobs.Count > 0)
                    {
                        foreach (var dto in pendingJobs)
                        {
                            var job = new TransferJob(
                                _plate,
                                dto.SourceRowStart,
                                dto.SourceColumnIndex,
                                _chip,
                                dto.DestRowStart,
                                dto.DestColumn,
                                dto.Volume);
                            await Task.Run(
                                () => ExecuteAndSyncAsync(job, $"Executing Cloud Job {dto.JobId}", _cts.Token),
                                _cts.Token);
                        }
                    }
                    else
                    {
                        Status = "No pending jobs. Polling...";
                        await Task.Delay(5_000, _cts.Token);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    Status = "Cloud mode error: " + ex.Message;
                    await Task.Delay(5_000);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Status = "Cloud mode stopped.";
        }
        finally
        {
            IsRunningCloudMode = false;
            if (CurrentJobDetails.Contains("Executing Cloud Job"))
            {
                CurrentJobDetails = "Idle";
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanStartDemo))]
    private async Task StartSimulationAsync()
    {
        if (IsDemoRunning)
            return;

        IsDemoRunning = true;

        // ensure fresh token source
        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        Status = $"Executing physical alignment demo...";
        try
        {
            await Task.Run(() => _instrument.InitializeAsync(_cts.Token));

            int chipTargetRow = 0;
            int chipTargetCol = 0;

            for (int srcCol = 0; srcCol < PlateColumns; srcCol++)
            {
                //repeat 30 times to deplete the source wells
                for (int i = 0; i < 30; i++)
                {
                    if (!IsDemoRunning || _cts.IsCancellationRequested)
                        break;
                    var job1 = new TransferJob(_plate, 0, srcCol, _chip, chipTargetRow, chipTargetCol, volume: 10.0);
                    await Task.Run(
                        () => ExecuteAndSyncAsync(
                            job1,
                            $"Aspirating from plate column {srcCol + 1} -> dispensing to Chip",
                            _cts.Token));

                    // Advance chip target
                    chipTargetRow += 8;
                    if (chipTargetRow + 8 > ChipRows)
                    {
                        chipTargetRow = 0;
                        chipTargetCol++;
                    }
                    if (chipTargetCol >= ChipColumns)
                        break;
                }
            }

            await Task.Run(() => _instrument.Head.DropTipsAsync(_cts.Token));

            if (!_cts.IsCancellationRequested)
            {
                Status = "Simulation Complete";
                CurrentJobDetails = "Idle";
            }
            else
            {
                Status = "Stopped";
            }
        }
        catch (OperationCanceledException)
        {
            Status = "Stopped";
        }
        finally
        {
            IsDemoRunning = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanStop))]
    private async Task StopSimulationAsync()
    {
        IsDemoRunning = false;
        IsRunningCloudMode = false;
        Status = "Stopping...";
        CurrentJobDetails = "Stop Requested";
        _cts?.Cancel();
        await Task.Delay(1_000).ContinueWith(_ => CurrentJobDetails = "Idle");
    }

    private void UpdateMetrics()
    {
        Application.Current.Dispatcher
            .Invoke(
                () =>
                {
                    double remaining = 0;
                    for (int r = 0; r < PlateRows; r++)
                    {
                        for (int c = 0; c < PlateColumns; c++)
                        {
                            remaining += _plate.Wells[r, c].Volume;
                        }
                    }
                    RemainingSourceVolume = remaining;

                    double dispensed = 0;
                    for (int r = 0; r < ChipRows; r++)
                    {
                        for (int c = 0; c < ChipColumns; c++)
                        {
                            dispensed += _chip.Wells[r, c].Volume;
                        }
                    }
                    TotalDispensedVolume = dispensed;

                    ComputeAverageFills();
                });
    }
}
