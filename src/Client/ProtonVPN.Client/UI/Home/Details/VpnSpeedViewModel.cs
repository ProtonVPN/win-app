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
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Home.Details;

public partial class VpnSpeedViewModel : ViewModelBase,
    IEventMessageReceiver<SettingChangedMessage>,
    IEventMessageReceiver<LoggedInMessage>,
    IEventMessageReceiver<VpnPlanChangedMessage>
{
    private readonly IConnectionManager _connectionManager;
    private readonly ISettings _settings;

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
    }

    public async void RefreshAsync()
    {
        TrafficBytes speed = await _connectionManager.GetCurrentSpeedAsync();

        DownloadSpeed = (long)speed.BytesIn;
        UploadSpeed = (long)speed.BytesOut;

        TrafficBytes volume = await _connectionManager.GetTrafficBytesAsync();

        DownloadVolume = (long)volume.BytesIn;
        UploadVolume = (long)volume.BytesOut;
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

    protected override void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(FormattedDownloadSpeed));
        OnPropertyChanged(nameof(FormattedUploadSpeed));
        OnPropertyChanged(nameof(FormattedDownloadVolume));
        OnPropertyChanged(nameof(FormattedUploadVolume));
        OnPropertyChanged(nameof(FormattedTotalVolume));
    }
}