/*
 * Copyright (c) 2023 Proton AG
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
using ProtonVPN.Client.Common.Queues;
using ProtonVPN.Client.Legacy.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using SkiaSharp;

namespace ProtonVPN.Client.Legacy.UI.Home.Details;

public partial class VpnSpeedViewModel : ViewModelBase,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    private const int MAX_DATA_POINTS = 30;
    private const int DEFAULT_VALUE = 0;

    private readonly IConnectionManager _connectionManager;
    private readonly ISettings _settings;

    private readonly FixedSizeQueue<long> _downloadDataPoints = new(MAX_DATA_POINTS, DEFAULT_VALUE);
    private readonly FixedSizeQueue<long> _uploadDataPoints = new(MAX_DATA_POINTS, DEFAULT_VALUE);

    [ObservableProperty]
    private string _speedUnit;

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

    public string FormattedDownloadSpeed => Localizer.GetFormattedSpeed(DownloadSpeed);

    public string FormattedUploadSpeed => Localizer.GetFormattedSpeed(UploadSpeed);

    public string FormattedDownloadVolume => Localizer.GetFormattedSize(DownloadVolume);

    public string FormattedUploadVolume => Localizer.GetFormattedSize(UploadVolume);

    public string FormattedTotalVolume => Localizer.GetFormattedSize(DownloadVolume + UploadVolume);

    public bool IsVpnAcceleratorTaglineVisible => _settings.IsVpnAcceleratorEnabled && _settings.VpnPlan.IsPaid;

    private ICartesianAxis _yAxis;

    public ISeries[] Series { get; set; }

    public IEnumerable<ICartesianAxis> XAxes { get; set; }

    public IEnumerable<ICartesianAxis> YAxes { get; set; }

    public Margin DrawMargin { get; } = new(Margin.Auto);

    public SmartObservableCollection<double> ScaledDownloadDataPoints = [];

    public SmartObservableCollection<double> ScaledUploadDataPoints = [];

    public SmartObservableCollection<double> Separators { get; } = [];

    public VpnSpeedViewModel(
        ILocalizationProvider localizationProvider,
        IConnectionManager connectionManager,
        ILogger logger,
        IIssueReporter issueReporter,
        ISettings settings)
        : base(localizationProvider, logger, issueReporter)
    {
        _connectionManager = connectionManager;
        _settings = settings;

        InitializeSpeedGraph();
    }

    private void InitializeSpeedGraph()
    {
        Series =
        [
            GetLineSeries(ScaledDownloadDataPoints, new SKColor(75, 185, 157, 255)),
            GetLineSeries(ScaledUploadDataPoints, new SKColor(247, 96, 123, 255)),
        ];

        _yAxis = new Axis
        {
            Position = AxisPosition.End,
            LabelsAlignment = Align.End,
            LabelsPaint = new SolidColorPaint(new SKColor(167, 164, 181, 255)),
            TextSize = 12,
            SeparatorsPaint = new SolidColorPaint(new SKColor(74, 70, 88, 255))
            {
                StrokeThickness = 1,
                PathEffect = new DashEffect([3, 3]),
                ZIndex = 0,
            },
            CustomSeparators = Separators,
        };

        XAxes = new List<ICartesianAxis> { new Axis { IsVisible = false, CustomSeparators = [], } };
        YAxes = new List<ICartesianAxis> { _yAxis };
    }

    private LineSeries<double> GetLineSeries(IEnumerable<double> values, SKColor color)
    {
        return new()
        {
            Values = values,
            Stroke = new SolidColorPaint(color)
            {
                StrokeThickness = 2,
                ZIndex = 1,
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

    public async void RefreshAsync(bool isToUpdateGraph)
    {
        NetworkTraffic speed = await _connectionManager.GetCurrentSpeedAsync();

        DownloadSpeed = (long)speed.BytesDownloaded;
        UploadSpeed = (long)speed.BytesUploaded;

        NetworkTraffic volume = await _connectionManager.GetTrafficBytesAsync();

        DownloadVolume = (long)volume.BytesDownloaded;
        UploadVolume = (long)volume.BytesUploaded;

        _downloadDataPoints.Enqueue(DownloadSpeed);
        _uploadDataPoints.Enqueue(UploadSpeed);

        if (isToUpdateGraph)
        {
            UpdateScaledDataPoints();
            UpdateSeparators();
        }
    }

    private void UpdateScaledDataPoints()
    {
        ByteMetrics metric = GetMetric();
        double scaleFactor = ByteConversionHelper.GetScaleFactor(metric);

        SpeedUnit = Localizer.GetFormat("Format_SpeedUnit", Localizer.GetSpeedUnit(metric));

        ScaledDownloadDataPoints.Reset(_downloadDataPoints.Select(n => GetScaledDataPoint(n, scaleFactor)).ToList());
        ScaledUploadDataPoints.Reset(_uploadDataPoints.Select(n => GetScaledDataPoint(n, scaleFactor)).ToList());
    }

    private double GetScaledDataPoint(long number, double scaleFactor)
    {
        return Math.Round(number / scaleFactor, 1);
    }

    private ByteMetrics GetMetric()
    {
        long maxSpeed = Math.Max(_downloadDataPoints.Max(), _uploadDataPoints.Max());
        (_, ByteMetrics metric) = ByteConversionHelper.CalculateSize(maxSpeed);
        return metric;
    }

    private void UpdateSeparators()
    {
        double maxSpeed = Math.Max(ScaledDownloadDataPoints.Max(), ScaledUploadDataPoints.Max());

        _yAxis.MinLimit = 0;
        _yAxis.MaxLimit = maxSpeed;

        Separators.Reset([0, Math.Round(maxSpeed / 2, 1), Math.Round(maxSpeed, 1)]);
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.IsVpnAcceleratorEnabled))
        {
            ExecuteOnUIThread(() => OnPropertyChanged(nameof(IsVpnAcceleratorTaglineVisible)));
        }
    }

    public void Receive(LoggedInMessage message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(IsVpnAcceleratorTaglineVisible)));
    }

    public void Receive(VpnPlanChangedMessage message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(IsVpnAcceleratorTaglineVisible)));
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (message.ConnectionStatus != ConnectionStatus.Connected && message.ConnectionStatus != ConnectionStatus.Disconnected)
        {
            return;
        }

        ExecuteOnUIThread(InvalidateDataPoints);
    }

    private void InvalidateDataPoints()
    {
        _downloadDataPoints.Reset();
        _uploadDataPoints.Reset();

        UpdateScaledDataPoints();
        UpdateSeparators();
    }

    protected override void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(FormattedDownloadSpeed));
        OnPropertyChanged(nameof(FormattedUploadSpeed));
        OnPropertyChanged(nameof(FormattedDownloadVolume));
        OnPropertyChanged(nameof(FormattedUploadVolume));
        OnPropertyChanged(nameof(FormattedTotalVolume));
    }
}