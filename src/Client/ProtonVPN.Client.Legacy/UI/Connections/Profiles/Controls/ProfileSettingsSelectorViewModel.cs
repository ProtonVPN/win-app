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
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Factories;
using ProtonVPN.Client.Legacy.Contracts.ViewModels;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Factories;
using ProtonVPN.Client.Legacy.UI.Connections.Common.Items;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Legacy.UI.Connections.Profiles.Controls;

public partial class ProfileSettingsSelectorViewModel : ViewModelBase, IProfileSettingsSelector
{
    private readonly CommonItemFactory _commonItemFactory;

    [ObservableProperty]
    private ProtocolItem? _selectedProtocol;

    public SmartObservableCollection<ProtocolItem> Protocols { get; } = [];

    public ProfileSettingsSelectorViewModel(
        ICommonItemFactory commonItemFactory,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _commonItemFactory = commonItemFactory;
    }

    public IProfileSettings GetProfileSettings()
    {
        return new ProfileSettings()
        {
            VpnProtocol = SelectedProtocol?.Protocol ?? DefaultSettings.VpnProtocol
        };
    }

    public void SetProfileSettings(IProfileSettings settings)
    {
        settings ??= ProfileSettings.Default;

        InvalidateProtocols();

        SelectedProtocol = Protocols.FirstOrDefault(p => p.Protocol == settings.VpnProtocol);
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