using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows.Media;

namespace LiquidDispenser.App.ViewModels;

public partial class WellViewModel : ObservableObject
{
    [ObservableProperty]
    private double _capacity;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CellColor))]
    [NotifyPropertyChangedFor(nameof(ToolTipText))]
    private string _liquidType = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CellColor))]
    [NotifyPropertyChangedFor(nameof(ToolTipText))]
    [NotifyPropertyChangedFor(nameof(FillPercentage))]
    [NotifyPropertyChangedFor(nameof(FillHeight))]
    private double _volume;

    public WellViewModel(int row, int col, double capacity, double volume, string liquidType)
    {
        Row = row;
        Column = col;
        Capacity = capacity;
        Volume = volume;
        LiquidType = liquidType;
    }

    public SolidColorBrush CellColor
    {
        get
        {
            if (Volume <= 0)
                return Brushes.Transparent;
            if (string.IsNullOrEmpty(LiquidType))
                return Brushes.DarkGray;

            // Expected format: "Plate_{row}_{col}"
            var parts = LiquidType.Split('_');
            if (parts.Length == 3 && parts[0] == "Plate")
            {
                if (int.TryParse(parts[1], out int r) && int.TryParse(parts[2], out int c))
                {
                    // Create a deterministic vibrant gradient topology across the biggest plate i.e 384-well plate
                    // 16 rows, 24 cols. Total unique positions = 384.

                    // Map the Hue (0-360) across Columns (X axis) giving a left-to-right rainbow
                    double hue = (c / 24.0) * 360.0;

                    // Map the Lightness (Y axis). 0.3 (darkish) to 0.7 (lightish) based on Row.
                    double lightness = 0.3 + ((r / 16.0) * 0.4);

                    return new SolidColorBrush(HslToRgb(hue, 1.0, lightness));
                }
            }

            return Brushes.DimGray;
        }
    }

    public int Column { get; }

    public double FillHeight => FillPercentage * 16.0;

    public double FillPercentage => Capacity > 0 ? (Volume / Capacity) : 0;

    public int Row { get; }

    public string ToolTipText => $"[{Row},{Column}] Vol: {Volume:F1}/{Capacity}uL\nSource: {(!string.IsNullOrEmpty(LiquidType) ? LiquidType : "Empty")}";

    // Helper method to convert HSL to RGB in C#
    private static Color HslToRgb(double h, double s, double l)
    {
        byte r = 0, g = 0, b = 0;

        if (s == 0)
        {
            r = g = b = (byte)(l * 255);
        }
        else
        {
            double v1, v2;
            double hue = h / 360;

            v2 = (l < 0.5) ? (l * (1 + s)) : ((l + s) - (l * s));
            v1 = 2 * l - v2;

            r = (byte)(255 * HueToRgb(v1, v2, hue + (1.0 / 3)));
            g = (byte)(255 * HueToRgb(v1, v2, hue));
            b = (byte)(255 * HueToRgb(v1, v2, hue - (1.0 / 3)));
        }

        return Color.FromRgb(r, g, b);
    }

    private static double HueToRgb(double v1, double v2, double vH)
    {
        if (vH < 0)
            vH += 1;
        if (vH > 1)
            vH -= 1;
        if ((6 * vH) < 1)
            return v1 + (v2 - v1) * 6 * vH;
        if ((2 * vH) < 1)
            return v2;
        if ((3 * vH) < 2)
            return v1 + (v2 - v1) * ((2.0 / 3) - vH) * 6;
        return v1;
    }
}
