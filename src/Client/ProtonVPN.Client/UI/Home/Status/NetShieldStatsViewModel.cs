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
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Home.Status;

public partial class NetShieldStatsViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _numberOfTrackersStopped;

    [ObservableProperty]
    private int _numberOfAdsBlocked;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DataSaved))]
    private long _dataSavedInBytes;

    public NetShieldStatsViewModel(ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(localizationProvider, logger, issueReporter)
    {
        _numberOfTrackersStopped = 14;
        _numberOfAdsBlocked = 21;
        _dataSavedInBytes = 1500;
    }

    public string DataSaved => Localizer.GetFormattedSize(DataSavedInBytes);

    protected override void OnLanguageChanged()
    {
        OnPropertyChanged(nameof(DataSaved));
    }
}