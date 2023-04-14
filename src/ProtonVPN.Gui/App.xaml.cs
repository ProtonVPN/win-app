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

using ProtonVPN.Gui.Activation;
using ProtonVPN.Gui.Contracts.Services;
using ProtonVPN.Gui.Models;
using ProtonVPN.Gui.Services;
using ProtonVPN.Gui.ViewModels;
using ProtonVPN.Gui.ViewModels.Pages;
using ProtonVPN.Gui.ViewModels.Pages.Countries;
using ProtonVPN.Gui.ViewModels.Pages.Settings;
using ProtonVPN.Gui.ViewModels.Pages.Settings.Advanced;
using ProtonVPN.Gui.Views;
using ProtonVPN.Gui.Views.Pages;
using ProtonVPN.Gui.Views.Pages.Countries;
using ProtonVPN.Gui.Views.Pages.Settings;
using ProtonVPN.Gui.Views.Pages.Settings.Advanced;

namespace ProtonVPN.Gui;

public partial class App : Application
{
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

            // Views and ViewModels
            services.AddTransient<SettingsViewModel>();
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
            services.AddTransient<CountriesViewModel>();
            services.AddTransient<CountriesPage>();
            services.AddTransient<HomeViewModel>();
            services.AddTransient<HomePage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        UnhandledException += App_UnhandledException;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    public IHost Host
    {
        get;
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

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        await App.GetService<IActivationService>().ActivateAsync(args);
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }
}