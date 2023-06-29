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

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.Models.Urls;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Common.Core.Enums;

namespace ProtonVPN.Client.UI.Settings.Pages;

public partial class ProtocolViewModel : PageViewModelBase, IEventMessageReceiver<SettingChangedMessage>
{
    private readonly ISettings _settings;
    private readonly IUrls _urls;

    public override string? Title => Localizer.Get("Settings_Connection_Protocol");
    public string Recommended => Localizer.Get("Settings_Protocols_Recommended").ToUpperInvariant();

    private Lazy<VpnProtocol> _selectedProtocol;

    public bool IsSmart => IsChecked(VpnProtocol.Smart);
    public bool IsWireGuardUdp => IsChecked(VpnProtocol.WireGuardUdp);
    public bool IsOpenVpnUdp => IsChecked(VpnProtocol.OpenVpnUdp);
    public bool IsOpenVpnTcp => IsChecked(VpnProtocol.OpenVpnTcp);
    public string LearnMoreUrl => _urls.ProtocolsLearnMore;

    public ProtocolViewModel(IPageNavigator pageNavigator,
        ILocalizationProvider localizationProvider,
        ISettings settings,
        IUrls urls)
        : base(pageNavigator, localizationProvider)
    {
        _settings = settings;
        _urls = urls;
        _selectedProtocol = new Lazy<VpnProtocol>(() => _settings.VpnProtocol);
    }

    public bool IsChecked(VpnProtocol protocol)
    {
        return protocol == _selectedProtocol.Value;
    }

    public void OnCheckedEvent(object sender, RoutedEventArgs e)
    {
        if (sender is not null && sender is RadioButton radioButton)
        {
            string? newValue = radioButton.Tag?.ToString();
            if (!string.IsNullOrWhiteSpace(newValue) &&
                Enum.TryParse(newValue, out VpnProtocol vpnProtocol))
            {
                _settings.VpnProtocol = vpnProtocol;
            }
        }
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.VpnProtocol) && message.NewValue is not null)
        {
            _selectedProtocol = new Lazy<VpnProtocol>((VpnProtocol)message.NewValue);
            OnPropertyChanged(nameof(IsSmart));
            OnPropertyChanged(nameof(IsWireGuardUdp));
            OnPropertyChanged(nameof(IsOpenVpnUdp));
            OnPropertyChanged(nameof(IsOpenVpnTcp));
        }
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();
        OnPropertyChanged(nameof(Recommended));
    }
}