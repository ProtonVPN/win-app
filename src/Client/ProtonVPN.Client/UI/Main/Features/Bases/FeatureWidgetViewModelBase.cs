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

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Bases.ViewModels;
using ProtonVPN.Client.Contracts.Enums;
using ProtonVPN.Client.Contracts.Services.Navigation;
using ProtonVPN.Client.UI.Main.Features.KillSwitch;
using ProtonVPN.Client.UI.Main.Features.NetShield;
using ProtonVPN.Client.UI.Main.Features.PortForwarding;
using ProtonVPN.Client.UI.Main.Features.SplitTunneling;
using ProtonVPN.Client.UI.Main.Widgets.Bases;
using ProtonVPN.Client.UI.Main.Widgets.Contracts;

namespace ProtonVPN.Client.UI.Main.Features.Bases;

public abstract class FeatureWidgetViewModelBase : SideWidgetViewModelBase, ISideHeaderWidget, IEventMessageReceiver<SettingChangedMessage>
{
    protected readonly ISettings Settings;

    private readonly ImageIcon _featureIcon = new();

    public override int SortIndex => (int)ConnectionFeature;

    public override IconElement Icon => GetFeatureIcon();

    public string Status => GetFeatureStatus();

    public ConnectionFeature ConnectionFeature { get; }

    protected FeatureWidgetViewModelBase(
        ILocalizationProvider localizer,
        ILogger logger,
        IIssueReporter issueReporter,
        IMainViewNavigator mainViewNavigator,
        ISettings settings,
        ConnectionFeature connectionFeature)
        : base(localizer, logger, issueReporter, mainViewNavigator)
    {
        Settings = settings;

        ConnectionFeature = connectionFeature;
    }

    public override Task<bool> InvokeAsync()
    {
        return MainViewNavigator.NavigateToFeatureViewAsync(ConnectionFeature);
    }

    protected abstract string GetFeatureStatus();

    protected abstract string GetFeatureToggleSettingName();

    protected abstract ImageSource GetFeatureIconSource();

    private ImageIcon GetFeatureIcon()
    {
        _featureIcon.Source = GetFeatureIconSource();
        return _featureIcon;
    }

    protected override void InvalidateIsSelected()
    {
        PageViewModelBase? currentPageContext = MainViewNavigator.GetCurrentPageContext();

        IsSelected = ConnectionFeature switch
        {
            ConnectionFeature.KillSwitch => currentPageContext is KillSwitchPageViewModel,
            ConnectionFeature.NetShield => currentPageContext is NetShieldPageViewModel,
            ConnectionFeature.PortForwarding => currentPageContext is PortForwardingPageViewModel,
            ConnectionFeature.SplitTunneling => currentPageContext is SplitTunnelingPageViewModel,
            _ => false
        };
    }


    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == GetFeatureToggleSettingName())
        {
            OnPropertyChanged(nameof(Icon));
        }
    }
}