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

using System.ServiceModel;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Service.Contract.Settings;

namespace ProtonVPN.Service.Settings
{
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Single)]
    public class SettingsHandler : ISettingsContract
    {
        private readonly IServiceSettings _settings;

        public SettingsHandler(IServiceSettings settings)
        {
            _settings = settings;
        }

        public void Apply(SettingsContract settings)
        {
            Ensure.NotNull(settings, nameof(settings));

            _settings.Apply(settings);
        }
    }
}
