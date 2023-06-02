﻿/*
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

using ProtonVPN.Common.Configuration.Environments;
using ProtonVPN.Common.Configuration.Source;
using ProtonVPN.Common.Configuration.Storage;

namespace ProtonVPN.Common.Configuration
{
    public class ConfigFactory
    {
        private static IConfiguration _config;

        public IConfiguration Config(bool savingAllowed = false)
        {
#if DEBUG
            ConfigMode mode = savingAllowed ? ConfigMode.CustomOrSavedDefault : ConfigMode.CustomOrDefault;
#else
            var mode = ConfigMode.Default;
#endif

            return _config ??= InitializedConfig(mode);
        }

        private static IConfiguration InitializedConfig(ConfigMode mode)
        {
            CustomConfig config =
                new CustomConfig(
                    mode,
                    new DefaultConfig(),
                    new ValidatedConfigStorage(
                        new EnvironmentVariableConfigStorage(
                            new DefaultConfig(),
                            new SafeConfigStorage(
                                new FileConfigStorage(
                                    new ConfigFile()
                                )
                            )
                        )
                    )
                );


            return config.Value();
        }
    }
}
