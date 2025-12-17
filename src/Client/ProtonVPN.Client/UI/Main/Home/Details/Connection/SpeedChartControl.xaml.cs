/*
 * Copyright (c) 2025 Proton AG
 *
 * This file is part of ProtonVPN.
 *

 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

namespace ProtonVPN.Client.UI.Main.Home.Details.Connection;

public sealed partial class SpeedChartControl
{
    public static readonly DependencyProperty LowerBoundProperty =
        DependencyProperty.Register(nameof(LowerBound), typeof(double), typeof(SpeedChartControl), new PropertyMetadata(0.0));

    public static readonly DependencyProperty MidpointProperty =
        DependencyProperty.Register(nameof(Midpoint), typeof(double), typeof(SpeedChartControl), new PropertyMetadata(50.0));

    public static readonly DependencyProperty UpperBoundProperty =
        DependencyProperty.Register(nameof(UpperBound), typeof(double), typeof(SpeedChartControl), new PropertyMetadata(100.0));

    public static readonly DependencyProperty DownloadSpeedHistoryProperty =
        DependencyProperty.Register(nameof(DownloadSpeedHistory), typeof(IReadOnlyList<double>), typeof(SpeedChartControl), new PropertyMetadata(null, OnDownloadSpeedHistoryChanged));

    public static readonly DependencyProperty UploadSpeedHistoryProperty =
        DependencyProperty.Register(nameof(UploadSpeedHistory), typeof(IReadOnlyList<double>), typeof(SpeedChartControl), new PropertyMetadata(null, OnUploadSpeedHistoryChanged));

    public double LowerBound
    {
        get => (double)GetValue(LowerBoundProperty);
        set => SetValue(LowerBoundProperty, value);
    }

    public double Midpoint
    {
        get => (double)GetValue(MidpointProperty);
        set => SetValue(MidpointProperty, value);
    }

    public double UpperBound
    {
        get => (double)GetValue(UpperBoundProperty);
        set => SetValue(UpperBoundProperty, value);
    }

    public IReadOnlyList<double> DownloadSpeedHistory
    {
        get => (IReadOnlyList<double>)GetValue(DownloadSpeedHistoryProperty);
        set => SetValue(DownloadSpeedHistoryProperty, value);
    }

    public IReadOnlyList<double> UploadSpeedHistory
    {
        get => (IReadOnlyList<double>)GetValue(UploadSpeedHistoryProperty);
        set => SetValue(UploadSpeedHistoryProperty, value);
    }

    public SpeedChartControl()
    {
        InitializeComponent();
    }

    private static void OnDownloadSpeedHistoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SpeedChartControl chart)
        {
            chart.UpdateDownloadChart();
        }
    }

    private static void OnUploadSpeedHistoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SpeedChartControl chart)
        {
            chart.UpdateUploadChart();
        }
    }

    private void OnChartCanvasSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateDownloadChart();
        UpdateUploadChart();
    }

    private void UpdateDownloadChart()
    {
        UpdateChart(DownloadPolyline, DownloadPolygon, DownloadSpeedHistory);
    }

    private void UpdateUploadChart()
    {
        UpdateChart(UploadPolyline, UploadPolygon, UploadSpeedHistory);
    }

    private void UpdateChart(Polyline polyline, Polygon polygon, IReadOnlyList<double>? speedHistory)
    {
        polyline.Points.Clear();
        polygon.Points.Clear();

        if (speedHistory is null || speedHistory.Count == 0)
        {
            return;
        }

        double canvasWidth = ChartCanvas.ActualWidth;
        double canvasHeight = ChartCanvas.ActualHeight;

        if (canvasWidth <= 0 || canvasHeight <= 0)
        {
            return;
        }

        double xStep = canvasWidth / (speedHistory.Count - 1);

        // Start polygon at bottom-left corner
        polygon.Points.Add(new Point(0, canvasHeight));

        // Add data points
        for (int i = 0; i < speedHistory.Count; i++)
        {
            double x = i * xStep;
            double y = ScaleValueToY(speedHistory[i], canvasHeight);
            Point point = new(x, y);
            polygon.Points.Add(point);
            polyline.Points.Add(point);
        }

        // Close polygon at bottom-right corner
        polygon.Points.Add(new Point(canvasWidth, canvasHeight));
    }

    private double ScaleValueToY(double value, double canvasHeight)
    {
        if (UpperBound <= LowerBound)
        {
            return canvasHeight;
        }

        // Clamp value to bounds
        double clampedValue = Math.Clamp(value, LowerBound, UpperBound);

        // Calculate ratio (0 = LowerBound, 1 = UpperBound)
        double ratio = (clampedValue - LowerBound) / (UpperBound - LowerBound);

        // Invert because Y=0 is at the top in UI coordinates
        return canvasHeight * (1 - ratio);
    }
}