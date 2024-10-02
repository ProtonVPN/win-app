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

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using ProtonVPN.IssueReporting.Installers;
using ProtonVPN.Client.Modules;
using ProtonVPN.Client.Services.Bootstrapping;

namespace ProtonVPN.Client;

public partial class App : Application
{
    public const string APPLICATION_NAME = "Proton VPN";

    public MainWindow? MainWindow { get; private set; }

    public IHost Host { get; }

    public App()
    {
        IssueReportingInitializer.Run();

        InitializeComponent();

        Host = Microsoft.Extensions.Hosting.Host
            .CreateDefaultBuilder()
            .UseContentRoot(AppContext.BaseDirectory)
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureContainer<ContainerBuilder>(builder => builder.RegisterModule<AppModule>())
            .Build();
    }

    public static T GetService<T>()
        where T : class
    {
        return (T)GetService(typeof(T));
    }

    public static object GetService(Type type)
    {
        return (App.Current as App)!.Host?.Services.GetService(type)
            ?? throw new ArgumentException($"{type} needs to be registered within Autofac.");
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        MainWindow = new();

        await GetService<IBootstrapper>().StartAsync(args);
    }
}