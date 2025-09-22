/*
 * Copyright (c) 2024 Proton AG
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

using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Common.Helpers;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.History;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Common.Core.Networking;
using SkiaSharp;

namespace ProtonVPN.Client.UI.Main.Home.Details.Connection;

public partial class VpnSpeedViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<NetworkTrafficChangedMessage>
{
    private const double Y_AXIS_BUFFER = 1.1;

    private readonly SKColor _uploadGraphColor = new(247, 96, 123, 255);
    private readonly SKColor _downloadGraphColor = new(75, 185, 157, 255);

    private readonly INetworkTrafficManager _networkTrafficManager;

    private ICartesianAxis _xAxis;
    private ICartesianAxis _yAxis;
    private ISeries _downloadSeries;
    private ISeries _uploadSeries;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedDownloadSpeed))]
    private long _downloadSpeed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedUploadSpeed))]
    private long _uploadSpeed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedDownloadVolume))]
    [NotifyPropertyChangedFor(nameof(FormattedTotalVolume))]
    private long _downloadVolume;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedUploadVolume))]
    [NotifyPropertyChangedFor(nameof(FormattedTotalVolume))]
    private long _uploadVolume;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpeedUnit))]
    private ByteMetrics _metric;

    public string FormattedDownloadSpeed => Localizer.GetFormattedSpeed(DownloadSpeed);

    public string FormattedUploadSpeed => Localizer.GetFormattedSpeed(UploadSpeed);

    public string FormattedDownloadVolume => Localizer.GetFormattedSize(DownloadVolume);

    public string FormattedUploadVolume => Localizer.GetFormattedSize(UploadVolume);

    public string FormattedTotalVolume => Localizer.GetFormattedSize(DownloadVolume + UploadVolume);

    public string SpeedUnit => Localizer.GetFormat("Format_SpeedUnit", Localizer.GetSpeedUnit(Metric));

    public ISeries[] Series { get; set; } = [];

    public IEnumerable<ICartesianAxis> XAxes { get; set; } = [];

    public IEnumerable<ICartesianAxis> YAxes { get; set; } = [];

    public Margin DrawMargin { get; } = new(Margin.Auto);

    public SmartObservableCollection<double> ScaledDownloadDataPoints { get; } = [];

    public SmartObservableCollection<double> ScaledUploadDataPoints { get; } = [];

    public SmartObservableCollection<double> Separators { get; } = [];

    public VpnSpeedViewModel(
        INetworkTrafficManager networkTrafficManager,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _networkTrafficManager = networkTrafficManager;

        _xAxis = GetXAxis();
        _yAxis = GetYAxis();
        _uploadSeries = GetLineSeries(ScaledUploadDataPoints, _uploadGraphColor, useDashEffect: true);
        _downloadSeries = GetLineSeries(ScaledDownloadDataPoints, _downloadGraphColor);

        XAxes = [_xAxis];
        YAxes = [_yAxis];
        Series = [_uploadSeries, _downloadSeries];
    }

    public void Receive(NetworkTrafficChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateAll);
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        InvalidateSpeedGraph();
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(FormattedDownloadSpeed));
        OnPropertyChanged(nameof(FormattedUploadSpeed));
        OnPropertyChanged(nameof(FormattedDownloadVolume));
        OnPropertyChanged(nameof(FormattedUploadVolume));
        OnPropertyChanged(nameof(FormattedTotalVolume));
        OnPropertyChanged(nameof(SpeedUnit));
    }

    private void InvalidateAll()
    {
        if (!IsActive)
        {
            return;
        }

        NetworkTraffic speed = _networkTrafficManager.GetSpeed();
        NetworkTraffic volume = _networkTrafficManager.GetVolume();

        DownloadSpeed = (long)speed.BytesDownloaded;
        UploadSpeed = (long)speed.BytesUploaded;

        DownloadVolume = (long)volume.BytesDownloaded;
        UploadVolume = (long)volume.BytesUploaded;

        InvalidateSpeedGraph();
    }

    private Axis GetXAxis()
    {
        return new Axis()
        {
            IsVisible = false,
            CustomSeparators = []
        };
    }

    private Axis GetYAxis()
    {
        return new Axis
        {
            Position = AxisPosition.Start,
            LabelsAlignment = Align.Start,
            LabelsPaint = new SolidColorPaint(new SKColor(167, 164, 181, 255)),
            TextSize = 10,
            SeparatorsPaint = new SolidColorPaint(new SKColor(74, 70, 88, 255))
            {
                StrokeThickness = 1,
                PathEffect = new DashEffect([3, 3]),
                ZIndex = 0,
            },
            CustomSeparators = Separators,
        };
    }

    private LineSeries<double> GetLineSeries(ICollection<double> values, SKColor color, bool useDashEffect = false)
    {
        return new(values)
        {
            Stroke = new SolidColorPaint(color)
            {
                StrokeThickness = 1,
                PathEffect = useDashEffect ? new DashEffect([3, 3]) : null,
                ZIndex = 2,
            },
            Fill = new LinearGradientPaint(
                [
                    new SKColor(color.Red, color.Green, color.Blue, (byte)(255 * 0.25)),
                    new SKColor(color.Red, color.Green, color.Blue, 0)
                ],
                new SKPoint(0.5f, 0),
                new SKPoint(0.5f, 1)),
            LineSmoothness = 1,
            GeometrySize = 0,
            IsHoverable = false,
            ZIndex = 1
        };
    }

    private void InvalidateSpeedGraph()
    {
        IReadOnlyList<NetworkTraffic> speedHistory = _networkTrafficManager.GetSpeedHistory();
        IEnumerable<long> downloadHistory = speedHistory.Select(nt => (long)nt.BytesDownloaded);
        IEnumerable<long> uploadHistory = speedHistory.Select(nt => (long)nt.BytesUploaded);

        long maxSpeed = Math.Max(downloadHistory.Max(), uploadHistory.Max());
        (double scaledMaxSpeed, ByteMetrics metric) = ByteConversionHelper.CalculateSize(maxSpeed);

        Metric = metric;
        double scaleFactor = ByteConversionHelper.GetScaleFactor(Metric);

        OnPropertyChanged(nameof(SpeedUnit));

        ScaledDownloadDataPoints.Reset(downloadHistory.Select(n => GetScaledDataPoint(n, scaleFactor)).ToList());
        ScaledUploadDataPoints.Reset(uploadHistory.Select(n => GetScaledDataPoint(n, scaleFactor)).ToList());

        double roundedScaledMaxSpeed = CalculateYAxisLimit(scaledMaxSpeed);

        _yAxis.MinLimit = 0;
        _yAxis.MaxLimit = roundedScaledMaxSpeed;

        Separators.Reset([0, roundedScaledMaxSpeed / 2.0, roundedScaledMaxSpeed]);
    }

    private double GetScaledDataPoint(long number, double scaleFactor)
    {
        return Math.Round(number / scaleFactor, 1);
    }

    private double CalculateYAxisLimit(double maxValue)
    {
        if (maxValue <= 0)
        {
            return 10;
        }

        maxValue *= Y_AXIS_BUFFER;

        double roundedLimit;

        if (maxValue < 1)
        {
            // For values less than 1, round up to the nearest 0.2
            roundedLimit = Math.Ceiling(maxValue * 5) / 5;
        }
        else if (maxValue <= 5)
        {
            // For values between 1 and 5, round up to the nearest 1
            roundedLimit = Math.Ceiling(maxValue);
        }
        else if (maxValue <= 20)
        {
            // For values between 5 and 20, round up to the nearest 2
            roundedLimit = Math.Ceiling(maxValue / 2) * 2;
        }
        else
        {
            // For values greater than 10, use a scale-based rounding factor
            double scale = Math.Pow(10, Math.Floor(Math.Log10(maxValue)));
            roundedLimit = Math.Ceiling(maxValue / scale) * scale;
        }

        return roundedLimit;
    }
}