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
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Home.Status;

public partial class NetShieldStatsViewModel : ViewModelBase,
    IEventMessageReceiver<NetShieldStatsChanged>,
    IEventMessageReceiver<ConnectionStatusChanged>
{
    private const int AVG_AD_SIZE_IN_BYTES = 200000;
    private const int AVG_TRACKER_SIZE_IN_BYTES = 50000;
    private const int AVG_MALWARE_SIZE_IN_BYTES = 750000;

    private readonly IConnectionManager _connectionManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TrackersStoppedLabel))]
    private long _numberOfTrackersStopped;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AdsBlockedLabel))]
    private long _numberOfAdsBlocked;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DataSaved))]
    private long _dataSavedInBytes;

    public string AdsBlockedLabel => Localizer.GetPlural("Home_NetShield_AdsBlocked", NumberOfAdsBlocked);
    public string TrackersStoppedLabel => Localizer.GetPlural("Home_NetShield_TrackersStopped", NumberOfTrackersStopped);

    public NetShieldStatsViewModel(
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        IConnectionManager connectionManager)
        : base(localizationProvider, logger, issueReporter)
    {
        _connectionManager = connectionManager;
    }

    public string DataSaved => Localizer.GetFormattedSize(DataSavedInBytes);

    protected override void OnLanguageChanged()
    {
        InvalidateAllProperties();
    }

    public void Receive(NetShieldStatsChanged message)
    {
        ExecuteOnUIThread(() =>
        {
            if (_connectionManager.IsConnected)
            {
                SetNetShieldStats(message);
            }
            else
            {
                ClearNetShieldStats();
            }
        });
    }

    private void SetNetShieldStats(NetShieldStatsChanged stats)
    {
        NumberOfAdsBlocked = stats.NumOfAdvertisementUrlsBlocked;
        NumberOfTrackersStopped = stats.NumOfTrackingUrlsBlocked;

        long adDataSavedInBytes = stats.NumOfAdvertisementUrlsBlocked * AVG_AD_SIZE_IN_BYTES;
        long trackerDataSavedInBytes = stats.NumOfTrackingUrlsBlocked * AVG_TRACKER_SIZE_IN_BYTES;
        long malwareDataSavedInBytes = stats.NumOfMaliciousUrlsBlocked * AVG_MALWARE_SIZE_IN_BYTES;
        DataSavedInBytes = adDataSavedInBytes + trackerDataSavedInBytes + malwareDataSavedInBytes;
    }

    private void ClearNetShieldStats()
    {
        NumberOfAdsBlocked = 0;
        NumberOfTrackersStopped = 0;
        DataSavedInBytes = 0;
    }

    public void Receive(ConnectionStatusChanged message)
    {
        if (message.ConnectionStatus != ConnectionStatus.Connected)
        {
            ExecuteOnUIThread(ClearNetShieldStats);
        }
    }
}