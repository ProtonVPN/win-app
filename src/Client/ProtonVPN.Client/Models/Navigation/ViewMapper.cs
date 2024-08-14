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

using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.UI.Connections.Countries;
using ProtonVPN.Client.UI.Connections.Gateways;
using ProtonVPN.Client.UI.Connections.P2P;
using ProtonVPN.Client.UI.Connections.Profiles;
using ProtonVPN.Client.UI.Connections.Profiles.Overlays;
using ProtonVPN.Client.UI.Connections.SecureCore;
using ProtonVPN.Client.UI.Connections.Tor;
using ProtonVPN.Client.UI.Dialogs.Overlays;
using ProtonVPN.Client.UI.Dialogs.Overlays.Welcome;
using ProtonVPN.Client.UI.Features.KillSwitch;
using ProtonVPN.Client.UI.Features.NetShield;
using ProtonVPN.Client.UI.Features.PortForwarding;
using ProtonVPN.Client.UI.Features.SplitTunneling;
using ProtonVPN.Client.UI.Gallery;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Client.UI.Home.ConnectionCard.Overlays;
using ProtonVPN.Client.UI.Login;
using ProtonVPN.Client.UI.Login.Forms;
using ProtonVPN.Client.UI.Login.Overlays;
using ProtonVPN.Client.UI.ReportIssue;
using ProtonVPN.Client.UI.ReportIssue.Results;
using ProtonVPN.Client.UI.ReportIssue.Steps;
using ProtonVPN.Client.UI.Settings;
using ProtonVPN.Client.UI.Settings.Pages;
using ProtonVPN.Client.UI.Settings.Pages.About;
using ProtonVPN.Client.UI.Settings.Pages.Advanced;
using ProtonVPN.Client.UI.Settings.Pages.DefaultConnections;
using ProtonVPN.Client.UI.Upsell.Carousel;
using ProtonVPN.Client.UI.Upsell.Carousel.Features;
using ProtonVPN.Client.UI.Features.NetShield;
using ProtonVPN.Client.UI.Features.KillSwitch;
using ProtonVPN.Client.UI.Features.PortForwarding;
using ProtonVPN.Client.UI.Features.SplitTunneling;
using ProtonVPN.Client.UI.Dialogs.Overlays.Welcome;
using ProtonVPN.Client.UI.Connections.Countries;
using ProtonVPN.Client.UI.Connections.P2P;
using ProtonVPN.Client.UI.Connections.SecureCore;
using ProtonVPN.Client.UI.Connections.Tor;
using ProtonVPN.Client.UI.Connections.Gateways;
using ProtonVPN.Client.UI.Connections.Profiles;
using ProtonVPN.Client.UI.Announcements.Modals;
using ProtonVPN.Client.UI.Connections.Profiles.Overlays;

namespace ProtonVPN.Client.Models.Navigation;

public class ViewMapper : IViewMapper
{
    private readonly List<ViewMappingPair> _pages = new();
    private readonly List<ViewMappingPair> _overlays = new();
    private readonly List<ViewMappingPair> _dialogs = new();

    public ViewMapper()
    {
        ConfigurePages();
        ConfigureOverlays();
        ConfigureDialogs();
    }

    public Type GetPageType<TPageViewModel>() where TPageViewModel : PageViewModelBase
    {
        return GetPageType(typeof(TPageViewModel));
    }

    public Type GetOverlayType<TOverlayViewModel>() where TOverlayViewModel : OverlayViewModelBase
    {
        return GetOverlayType(typeof(TOverlayViewModel));
    }

    public Type GetDialogType<TPageViewModel>() where TPageViewModel : PageViewModelBase
    {
        return GetDialogType(typeof(TPageViewModel));
    }

    public Type GetPageType(Type viewModelType)
    {
        lock (_pages)
        {
            return GetViewType(_pages, viewModelType);
        }
    }

    public Type GetOverlayType(Type viewModelType)
    {
        lock (_overlays)
        {
            return GetViewType(_overlays, viewModelType);
        }
    }

    public Type GetDialogType(Type viewModelType)
    {
        lock (_dialogs)
        {
            return GetViewType(_dialogs, viewModelType);
        }
    }

    public PageViewModelBase GetPageViewModel(Type pageType)
    {
        lock (_pages)
        {
            Type viewModelType = GetViewModelType(_pages, pageType);

            return App.GetService(viewModelType) as PageViewModelBase
                ?? throw new ArgumentException($"Corresponding view model not found for '{pageType}'.");
        }
    }

    public OverlayViewModelBase GetOverlayViewModel(Type overlayType)
    {
        lock (_overlays)
        {
            Type viewModelType = GetViewModelType(_overlays, overlayType);

            return App.GetService(viewModelType) as OverlayViewModelBase
                ?? throw new ArgumentException($"Corresponding view model not found for '{overlayType}'.");
        }
    }

    public PageViewModelBase GetDialogViewModel(Type dialogType)
    {
        lock (_dialogs)
        {
            Type viewModelType = GetViewModelType(_dialogs, dialogType);

            return App.GetService(viewModelType) as PageViewModelBase
                ?? throw new ArgumentException($"Corresponding view model not found for '{dialogType}'.");
        }
    }

    protected void ConfigurePages()
    {
        ConfigurePage<HomeViewModel, HomePage>();
        ConfigurePage<GatewaysPageViewModel, GatewaysPage>();
        ConfigurePage<ProfilesPageViewModel, ProfilesPage>();
        ConfigurePage<CountriesPageViewModel, CountriesPage>();
        ConfigurePage<CountryPageViewModel, CountryPage>();
        ConfigurePage<P2PCountriesPageViewModel, P2PCountriesPage>();
        ConfigurePage<P2PCountryPageViewModel, P2PCountryPage>();
        ConfigurePage<SecureCoreCountriesPageViewModel, SecureCoreCountriesPage>();
        ConfigurePage<TorCountriesPageViewModel, TorCountriesPage>();
        ConfigurePage<NetShieldViewModel, NetShieldPage>();
        ConfigurePage<KillSwitchViewModel, KillSwitchPage>();
        ConfigurePage<PortForwardingViewModel, PortForwardingPage>();
        ConfigurePage<SplitTunnelingViewModel, SplitTunnelingPage>();
        ConfigurePage<DefaultConnectionViewModel, DefaultConnectionPage>();
        ConfigurePage<ProtocolViewModel, ProtocolPage>();
        ConfigurePage<VpnAcceleratorViewModel, VpnAcceleratorPage>();
        ConfigurePage<AdvancedSettingsViewModel, AdvancedSettingsPage>();
        ConfigurePage<DebugLogsViewModel, DebugLogsPage>();
        ConfigurePage<CustomDnsServersViewModel, CustomDnsServersPage>();
        ConfigurePage<AutoStartupViewModel, AutoStartupPage>();
        ConfigurePage<CensorshipViewModel, CensorshipPage>();
        ConfigurePage<SettingsViewModel, SettingsPage>();
        ConfigurePage<LoginShellViewModel, LoginShellPage>();
        ConfigurePage<LoginFormViewModel, LoginForm>();
        ConfigurePage<TwoFactorFormViewModel, TwoFactorForm>();
        ConfigurePage<LoadingFormViewModel, LoadingForm>();
        ConfigurePage<UpsellCarouselShellViewModel, UpsellCarouselShellPage>();
        ConfigurePage<AdvancedSettingsUpsellFeatureViewModel, AdvancedSettingsUpsellFeaturePage>();
        ConfigurePage<MultipleDevicesUpsellFeatureViewModel, MultipleDevicesUpsellFeaturePage>();
        ConfigurePage<NetShieldUpsellFeatureViewModel, NetShieldUpsellFeaturePage>();
        ConfigurePage<P2PUpsellFeatureViewModel, P2PUpsellFeaturePage>();
        ConfigurePage<SecureCoreUpsellFeatureViewModel, SecureCoreUpsellFeaturePage>();
        ConfigurePage<SpeedUpsellFeatureViewModel, SpeedUpsellFeaturePage>();
        ConfigurePage<SplitTunnelingUpsellFeatureViewModel, SplitTunnelingUpsellFeaturePage>();
        ConfigurePage<StreamingUpsellFeatureViewModel, StreamingUpsellFeaturePage>();
        ConfigurePage<TorUpsellFeatureViewModel, TorUpsellFeaturePage>();
        ConfigurePage<WorldwideCoverageUpsellFeatureViewModel, WorldwideCoverageUpsellFeaturePage>();
        ConfigurePage<ReportIssueShellViewModel, ReportIssueShellPage>();
        ConfigurePage<CategorySelectionViewModel, CategorySelectionPage>();
        ConfigurePage<QuickFixesViewModel, QuickFixesPage>();
        ConfigurePage<ContactFormViewModel, ContactFormPage>();
        ConfigurePage<ReportIssueResultViewModel, ReportIssueResultPage>();
        ConfigurePage<DeveloperToolsViewModel, DeveloperToolsPage>();
        ConfigurePage<AboutViewModel, AboutPage>();
        ConfigurePage<LicensingViewModel, LicensingPage>();

        ConfigureDebugPages();
    }

    [Conditional("DEBUG")]
    protected void ConfigureDebugPages()
    {
        ConfigurePage<GalleryViewModel, GalleryPage>();
        ConfigurePage<GalleryItemViewModel, GalleryItemPage>();
    }

    protected void ConfigureOverlays()
    {
        ConfigureOverlay<LatencyOverlayViewModel, LatencyOverlayDialog>();
        ConfigureOverlay<ProtocolOverlayViewModel, ProtocolOverlayDialog>();
        ConfigureOverlay<ServerLoadOverlayViewModel, ServerLoadOverlayDialog>();
        ConfigureOverlay<HumanVerificationOverlayViewModel, HumanVerificationOverlayDialog>();
        ConfigureOverlay<SsoLoginOverlayViewModel, SsoLoginOverlayDialog>();
        ConfigureOverlay<SecureCoreOverlayViewModel, SecureCoreOverlayDialog>();
        ConfigureOverlay<SmartRoutingOverlayViewModel, SmartRoutingOverlayDialog>();
        ConfigureOverlay<P2POverlayViewModel, P2POverlayDialog>();
        ConfigureOverlay<TorOverlayViewModel, TorOverlayDialog>();
        ConfigureOverlay<FreeConnectionsOverlayViewModel, FreeConnectionsOverlayDialog>();
        ConfigureOverlay<ChangeServerOverlayViewModel, ChangeServerOverlayDialog>();
        ConfigureOverlay<TroubleshootingOverlayViewModel, TroubleshootingOverlayDialog>();
        ConfigureOverlay<WelcomeOverlayViewModel, WelcomeOverlayDialog>();
        ConfigureOverlay<WelcomeToVpnPlusOverlayViewModel, WelcomeToVpnPlusOverlayDialog>();
        ConfigureOverlay<WelcomeToVpnUnlimitedOverlayViewModel, WelcomeToVpnUnlimitedOverlayDialog>();
        ConfigureOverlay<WelcomeToVpnB2BOverlayViewModel, WelcomeToVpnB2BOverlayDialog>();
        ConfigureOverlay<EditProfileOverlayViewModel, EditProfileOverlayDialog>();
    }

    protected void ConfigureDialogs()
    {
        ConfigureDialog<ReportIssueShellViewModel, ReportIssueWindow>();
        ConfigureDialog<UpsellCarouselShellViewModel, UpsellCarouselWindow>();
        ConfigureDialog<AnnouncementModalViewModel, AnnouncementModalWindow>();
    }

    private Type GetViewType(List<ViewMappingPair> sourceList, Type viewModelType)
    {
        return sourceList.FirstOrDefault(p => p.ViewModelType == viewModelType)?.ViewType
            ?? throw new ArgumentException($"Corresponding view not found for '{viewModelType}'. Did you forget to configure the mapping in the ViewMapper?");
    }

    private Type GetViewModelType(List<ViewMappingPair> sourceList, Type viewType)
    {
        return sourceList.FirstOrDefault(p => p.ViewType == viewType)?.ViewModelType
            ?? throw new ArgumentException($"Corresponding view model not found for '{viewType}'. Did you forget to configure the mapping in the ViewMapper?");
    }

    private void ConfigureViewMappingPair<VM, V>(List<ViewMappingPair> sourceList)
    {
        Type viewType = typeof(V);
        if (sourceList.Any(p => p.ViewType == viewType))
        {
            throw new ArgumentException($"The mapping for '{viewType}' is already configured in the ViewMapper");
        }

        Type viewModelType = typeof(VM);
        if (sourceList.Any(p => p.ViewModelType == viewModelType))
        {
            throw new ArgumentException($"The mapping for '{viewModelType}' is already configured in the ViewMapper");
        }

        ViewMappingPair pair = new(viewType: viewType, viewModelType: viewModelType);
        sourceList.Add(pair);
    }

    private void ConfigurePage<VM, V>()
        where VM : PageViewModelBase
        where V : Page
    {
        lock (_pages)
        {
            ConfigureViewMappingPair<VM, V>(_pages);
        }
    }

    private void ConfigureOverlay<VM, V>()
        where VM : OverlayViewModelBase
        where V : ContentDialog
    {
        lock (_overlays)
        {
            ConfigureViewMappingPair<VM, V>(_overlays);
        }
    }

    private void ConfigureDialog<VM, V>()
        where VM : PageViewModelBase
        where V : Window
    {
        lock (_dialogs)
        {
            ConfigureViewMappingPair<VM, V>(_dialogs);
        }
    }
}