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
using ProtonVPN.Client.UI.Countries;
using ProtonVPN.Client.UI.Countries.CountriesFeatureTabs;
using ProtonVPN.Client.UI.Countries.CountryFeatureTabs;
using ProtonVPN.Client.UI.Dialogs.Overlays;
using ProtonVPN.Client.UI.ReportIssue;
using ProtonVPN.Client.UI.Gallery;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Client.UI.Login;
using ProtonVPN.Client.UI.Settings;
using ProtonVPN.Client.UI.Settings.Pages;
using ProtonVPN.Client.UI.Settings.Pages.Advanced;
using ProtonVPN.Client.UI.ReportIssue.Steps;
using ProtonVPN.Client.UI.ReportIssue.Results;
using ProtonVPN.Client.UI.Login.Forms;
using ProtonVPN.Client.UI.Login.Overlays;
using ProtonVPN.Client.UI.Gateways;
using ProtonVPN.Client.UI.Home.ConnectionCard.Overlays;
using ProtonVPN.Client.UI.Settings.Pages.About;
using ProtonVPN.Client.UI.Upsell.Carousel;
using ProtonVPN.Client.UI.Upsell.Carousel.Features;
using ProtonVPN.Client.UI.Features.NetShield;
using ProtonVPN.Client.UI.Features.KillSwitch;
using ProtonVPN.Client.UI.Features.PortForwarding;
using ProtonVPN.Client.UI.Features.SplitTunneling;

namespace ProtonVPN.Client.Models.Navigation;

public class ViewMapper : IViewMapper
{
    private readonly Dictionary<string, Type> _pages = new();
    private readonly Dictionary<string, Type> _overlays = new();
    private readonly Dictionary<string, Type> _dialogs = new();

    public ViewMapper()
    {
        ConfigurePages();
        ConfigureOverlays();
        ConfigureDialogs();
    }

    public Type GetPageType<TPageViewModel>() where TPageViewModel : PageViewModelBase
    {
        return GetPageType(typeof(TPageViewModel).FullName!);
    }

    public Type GetOverlayType<TOverlayViewModel>() where TOverlayViewModel : OverlayViewModelBase
    {
        return GetOverlayType(typeof(TOverlayViewModel).FullName!);
    }

    public Type GetDialogType<TPageViewModel>() where TPageViewModel : PageViewModelBase
    {
        return GetDialogType(typeof(TPageViewModel).FullName!);
    }

    public Type GetPageType(string key)
    {
        Type? pageType;
        lock (_pages)
        {
            if (!_pages.TryGetValue(key, out pageType))
            {
                throw new ArgumentException($"Page not found: {key}. Did you forget to call ViewMapper.Configure?");
            }
        }

        return pageType;
    }

    public Type GetOverlayType(string key)
    {
        Type? overlayType;
        lock (_overlays)
        {
            if (!_overlays.TryGetValue(key, out overlayType))
            {
                throw new ArgumentException($"Overlay not found: {key}. Did you forget to call DialogActivator.ConfigureOverlay?");
            }
        }

        return overlayType;
    }

    public Type GetDialogType(string key)
    {
        Type? dialogType;
        lock (_dialogs)
        {
            if (!_dialogs.TryGetValue(key, out dialogType))
            {
                throw new ArgumentException($"Dialog not found: {key}. Did you forget to call DialogActivator.ConfigureDialog?");
            }
        }

        return dialogType;
    }

    protected void ConfigurePages()
    {
        ConfigurePage<HomeViewModel, HomePage>();
        ConfigurePage<CountriesViewModel, CountriesPage>();
        ConfigurePage<GatewaysViewModel, GatewaysPage>();
        ConfigurePage<CountryTabViewModel, CountryPage>();
        ConfigurePage<NetShieldViewModel, NetShieldPage>();
        ConfigurePage<KillSwitchViewModel, KillSwitchPage>();
        ConfigurePage<PortForwardingViewModel, PortForwardingPage>();
        ConfigurePage<SplitTunnelingViewModel, SplitTunnelingPage>();
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
        ConfigurePage<AllCountriesPageViewModel, AllCountriesPage>();
        ConfigurePage<CitiesPageViewModel, CitiesPage>();
        ConfigurePage<P2PCitiesPageViewModel, P2PCitiesPage>();
        ConfigurePage<SecureCoreCountriesPageViewModel, SecureCoreCountriesPage>();
        ConfigurePage<SecureCoreCountryPageViewModel, SecureCoreCountryPage>();
        ConfigurePage<TorServersPageViewModel, TorServersPage>();
        ConfigurePage<P2PCountriesPageViewModel, P2PCountriesPage>();
        ConfigurePage<TorCountriesPageViewModel, TorCountriesPage>();
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
    }

    protected void ConfigureDialogs()
    {
        ConfigureDialog<ReportIssueShellViewModel, ReportIssueWindow>();
        ConfigureDialog<UpsellCarouselShellViewModel, UpsellCarouselWindow>();
    }

    private void ConfigurePage<VM, V>()
        where VM : PageViewModelBase
        where V : Page
    {
        lock (_pages)
        {
            string key = typeof(VM).FullName!;
            if (_pages.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in ViewMapper");
            }

            Type type = typeof(V);
            if (_pages.Any(p => p.Value == type))
            {
                throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
            }

            _pages.Add(key, type);
        }
    }

    private void ConfigureOverlay<VM, V>()
        where VM : OverlayViewModelBase
        where V : ContentDialog
    {
        lock (_overlays)
        {
            string key = typeof(VM).FullName!;
            if (_overlays.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in DialogActivator");
            }

            Type type = typeof(V);
            if (_overlays.Any(p => p.Value == type))
            {
                throw new ArgumentException($"This type is already configured with key {_overlays.First(p => p.Value == type).Key}");
            }

            _overlays.Add(key, type);
        }
    }

    private void ConfigureDialog<VM, V>()
        where VM : PageViewModelBase
        where V : Window
    {
        lock (_dialogs)
        {
            string key = typeof(VM).FullName!;
            if (_dialogs.ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} is already configured in DialogActivator");
            }

            Type type = typeof(V);
            if (_dialogs.Any(p => p.Value == type))
            {
                throw new ArgumentException($"This type is already configured with key {_dialogs.First(p => p.Value == type).Key}");
            }

            _dialogs.Add(key, type);
        }
    }
}