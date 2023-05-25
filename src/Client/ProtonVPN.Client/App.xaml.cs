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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using ProtonVPN.Client.Activation;
using ProtonVPN.Client.Contracts.Services;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Installers;
using ProtonVPN.Client.Logic.Connection.Installers;
using ProtonVPN.Client.Logic.Recents.Installers;
using ProtonVPN.Client.Models;
using ProtonVPN.Client.Services;
using ProtonVPN.Client.UI;
using ProtonVPN.Client.UI.Countries;
using ProtonVPN.Client.UI.Countries.Pages;
using ProtonVPN.Client.UI.Gallery;
using ProtonVPN.Client.UI.Home;
using ProtonVPN.Client.UI.Home.ConnectionCard;
using ProtonVPN.Client.UI.Home.Help;
using ProtonVPN.Client.UI.Home.Map;
using ProtonVPN.Client.UI.Home.NetShieldStats;
using ProtonVPN.Client.UI.Home.Recents;
using ProtonVPN.Client.UI.Home.VpnStatusComponent;
using ProtonVPN.Client.UI.Settings;
using ProtonVPN.Client.UI.Settings.Pages;
using ProtonVPN.Client.UI.Settings.Pages.Advanced;

namespace ProtonVPN.Client;

public partial class App
{
    public const string APPLICATION_NAME = "Proton VPN";

    public static WindowEx MainWindow { get; } = new MainWindow();

    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host { get; }

    public App()
    {
        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers

            // Services
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<IFileService, FileService>();

            // Localization
            services.AddLocalizer();

            // Views and ViewModels
            services.AddSingleton<RecentsViewModel>();
            services.AddSingleton<VpnStatusViewModel>();
            services.AddSingleton<NetShieldStatsViewModel>();
            services.AddSingleton<ConnectionCardViewModel>();
            services.AddSingleton<MapViewModel>();
            services.AddSingleton<HelpViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<CensorshipViewModel>();
            services.AddTransient<CensorshipPage>();
            services.AddTransient<AutoConnectViewModel>();
            services.AddTransient<AutoConnectPage>();
            services.AddTransient<CustomDnsServersViewModel>();
            services.AddTransient<CustomDnsServersPage>();
            services.AddTransient<VpnLogsViewModel>();
            services.AddTransient<VpnLogsPage>();
            services.AddTransient<AdvancedSettingsViewModel>();
            services.AddTransient<AdvancedSettingsPage>();
            services.AddTransient<VpnAcceleratorViewModel>();
            services.AddTransient<VpnAcceleratorPage>();
            services.AddTransient<ProtocolViewModel>();
            services.AddTransient<ProtocolPage>();
            services.AddTransient<SplitTunnelingViewModel>();
            services.AddTransient<SplitTunnelingPage>();
            services.AddTransient<PortForwardingViewModel>();
            services.AddTransient<PortForwardingPage>();
            services.AddTransient<KillSwitchViewModel>();
            services.AddTransient<KillSwitchPage>();
            services.AddTransient<NetShieldViewModel>();
            services.AddTransient<NetShieldPage>();
            services.AddTransient<CountryViewModel>();
            services.AddTransient<CountryPage>();
            services.AddSingleton<CountriesViewModel>();
            services.AddTransient<CountriesPage>();
            services.AddSingleton<HomeViewModel>();
            services.AddTransient<HomePage>();
            services.AddSingleton<GalleryViewModel>();
            services.AddTransient<GalleryPage>();
            services.AddTransient<ShellPage>();
            services.AddSingleton<ShellViewModel>();

            services.AddRecents();
            services.AddConnection();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await App.GetService<ILocalizationBuilder>().BuildAsync();
        await App.GetService<IActivationService>().ActivateAsync(args);
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }
}