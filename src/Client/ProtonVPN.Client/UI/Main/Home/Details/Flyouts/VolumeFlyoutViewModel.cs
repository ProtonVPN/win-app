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
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Connection.Contracts.Models;
using ProtonVPN.Client.UI.Main.Home.Details.Contracts;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Home.Details.Flyouts;

public partial class VolumeFlyoutViewModel : ActivatableViewModelBase, IConnectionDetailsAware
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedDownloadVolume))]
    private long? _downloadVolume;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedUploadVolume))]
    private long? _uploadVolume;

    public string FormattedDownloadVolume => Localizer.GetFormattedSize(DownloadVolume);

    public string FormattedUploadVolume => Localizer.GetFormattedSize(UploadVolume);

    public VolumeFlyoutViewModel(
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter) :
        base(localizer, logger, issueReporter)
    {
    }

    public void Refresh(ConnectionDetails? connectionDetails, TrafficBytes volume, TrafficBytes speed)
    {
        if (IsActive)
        {
            DownloadVolume = (long)volume.BytesIn;
            UploadVolume = (long)volume.BytesOut;
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        DownloadVolume = null;
        UploadVolume = null;
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(FormattedDownloadVolume));
        OnPropertyChanged(nameof(FormattedUploadVolume));
    }
}