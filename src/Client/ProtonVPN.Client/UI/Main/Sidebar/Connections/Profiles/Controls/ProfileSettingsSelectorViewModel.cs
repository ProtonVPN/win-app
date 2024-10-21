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
using ProtonVPN.Client.Common.Collections;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Main.Sidebar.Connections.Profiles.Contracts;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Main.Sidebar.Connections.Profiles.Controls;

public partial class ProfileSettingsSelectorViewModel : ViewModelBase, IProfileSettingsSelector
{
    private readonly ICommonItemFactory _commonItemFactory;

    [ObservableProperty]
    private ProtocolItem? _selectedProtocol;

    public SmartObservableCollection<ProtocolItem> Protocols { get; } = [];

    public ProfileSettingsSelectorViewModel(
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        ICommonItemFactory commonItemFactory)
        : base(localizationProvider,
               logger,
               issueReporter)
    {
        _commonItemFactory = commonItemFactory;
    }

    public IProfileSettings GetProfileSettings()
    {
        return new ProfileSettings()
        {
            Protocol = SelectedProtocol?.Protocol ?? DefaultSettings.VpnProtocol
        };
    }

    public void SetProfileSettings(IProfileSettings settings)
    {
        settings ??= ProfileSettings.Default;

        InvalidateProtocols();

        SelectedProtocol = Protocols.FirstOrDefault(p => p.Protocol == settings.Protocol);
    }

    private void InvalidateProtocols()
    {
        Protocols.Reset(GetProtocolsByOrder().Select(_commonItemFactory.GetProtocol));
    }

    private static IEnumerable<VpnProtocol> GetProtocolsByOrder()
    {
        yield return VpnProtocol.Smart;
        yield return VpnProtocol.WireGuardUdp;
        yield return VpnProtocol.WireGuardTcp;
        yield return VpnProtocol.WireGuardTls;
        yield return VpnProtocol.OpenVpnUdp;
        yield return VpnProtocol.OpenVpnTcp;
    }
}