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
using ProtonVPN.Client.Logic.Services.Contracts;
using ProtonVPN.Client.Models.Activation;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Extensions;

namespace ProtonVPN.Client.Bootstrapping;

public class Bootstrapper : IBootstrapper
{
    private readonly IProcessCommunicationStarter _processCommunicationStarter;
    private readonly IMainWindowActivator _mainWindowActivator;
    private readonly ISettingsRestorer _settingsRestorer;
    private readonly IServiceManager _serviceManager;

    public Bootstrapper(IProcessCommunicationStarter processCommunicationStarter, 
        IMainWindowActivator mainWindowActivator,
        ISettingsRestorer settingsRestorer,
        IServiceManager serviceManager)
    {
        _processCommunicationStarter = processCommunicationStarter;
        _mainWindowActivator = mainWindowActivator;
        _settingsRestorer = settingsRestorer;
        _serviceManager = serviceManager;
    }

    public async Task StartAsync(LaunchActivatedEventArgs args)
    {
        ParseAndRunCommandLineArguments();
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        _serviceManager.Start();
        _processCommunicationStarter.StartAsync(cancellationToken);
        await _mainWindowActivator.ActivateAsync(args);
    }

    private void ParseAndRunCommandLineArguments()
    {
        string[] args = Environment.GetCommandLineArgs();
        foreach (string arg in args)
        {
            if (arg.EqualsIgnoringCase("-RestoreDefaultSettings"))
            {
                _settingsRestorer.Restore();
            }
        }
    }
}