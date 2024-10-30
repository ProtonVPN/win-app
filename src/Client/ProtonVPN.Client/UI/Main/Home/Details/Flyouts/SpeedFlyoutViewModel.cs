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
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.Services.Browsing;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.UI.Main.Home.Details.Contracts;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Home.Details.Flyouts;

public partial class SpeedFlyoutViewModel : ActivatableViewModelBase, IConnectionDetailsAware,
    IEventMessageReceiver<SettingChangedMessage>
{
    private readonly IUrls _urls;
    private readonly ISettings _settings;
    private readonly IConnectionManager _connectionManager;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedDownloadSpeed))]
    private long? _downloadSpeed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedUploadSpeed))]
    private long? _uploadSpeed;

    public string FormattedDownloadSpeed => Localizer.GetFormattedSpeed(DownloadSpeed);

    public string FormattedUploadSpeed => Localizer.GetFormattedSpeed(UploadSpeed);

    public bool IsVpnAcceleratorTaglineVisible => _settings.IsVpnAcceleratorEnabled && _settings.VpnPlan.IsPaid;

    public string IncreaseVpnSpeedsUri => _urls.IncreaseVpnSpeeds;

    public SpeedFlyoutViewModel(
        IUrls urls,
        ISettings settings,
        IConnectionManager connectionManager,
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter) :
        base(localizer, logger, issueReporter)
    {
        _urls = urls;
        _settings = settings;
        _connectionManager = connectionManager;
    }

    public void Refresh(ConnectionDetails? connectionDetails, TrafficBytes volume, TrafficBytes speed)
    {
        if (IsActive)
        {
            DownloadSpeed = (long)speed.BytesIn;
            UploadSpeed = (long)speed.BytesOut;
        }
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName is (nameof(ISettings.IsVpnAcceleratorEnabled)) or
            (nameof(ISettings.VpnPlan)))
        {
            ExecuteOnUIThread(InvalidateTagline);
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        DownloadSpeed = null;
        UploadSpeed = null;
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(FormattedDownloadSpeed));
        OnPropertyChanged(nameof(FormattedUploadSpeed));
    }

    private void InvalidateTagline()
    {
        OnPropertyChanged(nameof(IsVpnAcceleratorTaglineVisible));
    }
}