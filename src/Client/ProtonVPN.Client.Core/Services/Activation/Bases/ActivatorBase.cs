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

using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Core.Services.Activation.Bases;

public abstract class ActivatorBase<THost> : IActivator<THost>
    where THost : class
{
    protected readonly ILogger Logger;

    public THost? Host { get; private set; }

    protected string HostTypeName { get; } = typeof(THost).Name;

    protected ActivatorBase(
        ILogger logger)
    {
        Logger = logger;
    }

    public void Initialize(THost host)
    {
        Reset();

        Host = host;

        if (Host != null)
        {
            RegisterToHostEvents();

            OnInitialized();
        }
    }

    public void Reset()
    {
        if (Host != null)
        {
            OnReset();

            UnregisterFromHostEvents();

            Host = null;
        }
    }

    protected virtual void OnInitialized()
    { }

    protected virtual void OnReset()
    { }

    protected virtual void RegisterToHostEvents()
    { }

    protected virtual void UnregisterFromHostEvents()
    { }
}