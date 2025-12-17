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

using CommunityToolkit.Mvvm.ComponentModel;
using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Common.Helpers;
using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.History;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.UI.Main.Home.Details.Connection;

public partial class SpeedChartComponentViewModel : ActivatableViewModelBase,
    IEventMessageReceiver<NetworkTrafficChangedMessage>,
    IEventMessageReceiver<MainWindowVisibilityChangedMessage>
{
    private const double Y_AXIS_BUFFER = 1.1;

    private readonly INetworkTrafficManager _networkTrafficManager;
    private readonly IMainWindowActivator _mainWindowActivator;

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

    [ObservableProperty]
    private IReadOnlyList<double> _downloadSpeedHistory = [];

    [ObservableProperty]
    private IReadOnlyList<double> _uploadSpeedHistory = [];

    [ObservableProperty]
    private double _lowerBound;

    [ObservableProperty]
    private double _midpoint;

    [ObservableProperty]
    private double _upperBound;

    public SpeedChartComponentViewModel(
        INetworkTrafficManager networkTrafficManager, 
        IMainWindowActivator mainWindowActivator,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _networkTrafficManager = networkTrafficManager;
        _mainWindowActivator = mainWindowActivator;
    }

    public void Receive(NetworkTrafficChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateAll);
    }

    public void Receive(MainWindowVisibilityChangedMessage message)
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
        if (!IsActive || !_mainWindowActivator.IsWindowVisible)
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

    private void InvalidateSpeedGraph()
    {
        IReadOnlyList<NetworkTraffic> speedHistory = _networkTrafficManager.GetSpeedHistory();
        IEnumerable<long> downloadHistory = speedHistory.Select(nt => (long)nt.BytesDownloaded);
        IEnumerable<long> uploadHistory = speedHistory.Select(nt => (long)nt.BytesUploaded);

        long maxSpeed = Math.Max(downloadHistory.Max(), uploadHistory.Max());
        (double scaledMaxSpeed, ByteMetrics metric) = ByteConversionHelper.CalculateSize(maxSpeed);

        Metric = metric;
        double scaleFactor = ByteConversionHelper.GetScaleFactor(Metric);

        double roundedScaledMaxSpeed = CalculateYAxisLimit(scaledMaxSpeed);

        LowerBound = 0.0;
        Midpoint = roundedScaledMaxSpeed / 2.0;
        UpperBound = roundedScaledMaxSpeed;

        DownloadSpeedHistory = downloadHistory.Select(n => GetScaledDataPoint(n, scaleFactor)).ToList();
        UploadSpeedHistory = uploadHistory.Select(n => GetScaledDataPoint(n, scaleFactor)).ToList();
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
