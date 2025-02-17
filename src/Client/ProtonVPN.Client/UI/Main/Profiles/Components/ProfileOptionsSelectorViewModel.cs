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
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.UI.Main.Profiles.Contracts;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Profiles.Components;

public partial class ProfileOptionsSelectorViewModel : ViewModelBase, IProfileOptionsSelector
{
    private IProfileOptions _originalProfileOptions = ProfileOptions.Default;

    [ObservableProperty]
    private bool _isConnectAndGoEnabled;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConnectAndGoUrlValid))]
    [NotifyPropertyChangedFor(nameof(ConnectAndGoErrorMessage))]
    private string _connectAndGoUrl = string.Empty;

    public bool IsConnectAndGoUrlValid => string.IsNullOrEmpty(ConnectAndGoUrl) || ConnectAndGoUrl.IsValidUrl();

    public string ConnectAndGoErrorMessage => !IsConnectAndGoUrlValid
        ? Localizer.Get("Profile_Options_Url_Error")
        : string.Empty;

    public ProfileOptionsSelectorViewModel(
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter)
        : base(localizationProvider,
               logger,
               issueReporter)
    { }

    public IProfileOptions GetProfileOptions()
    {
        return new ProfileOptions()
        {
            IsConnectAndGoEnabled = !string.IsNullOrWhiteSpace(ConnectAndGoUrl) 
                                 && IsConnectAndGoEnabled,
            ConnectAndGoUrl = ConnectAndGoUrl.Trim()
        };
    }

    public void SetProfileOptions(IProfileOptions options)
    {
        _originalProfileOptions = options ?? ProfileOptions.Default;

        IsConnectAndGoEnabled = _originalProfileOptions.IsConnectAndGoEnabled;
        ConnectAndGoUrl = _originalProfileOptions.ConnectAndGoUrl;
    }

    public bool HasChanged()
    {
        return _originalProfileOptions.IsConnectAndGoEnabled != IsConnectAndGoEnabled
            || _originalProfileOptions.ConnectAndGoUrl != ConnectAndGoUrl;
    }

    public bool IsReconnectionRequired()
    {
        return false;
    }
}