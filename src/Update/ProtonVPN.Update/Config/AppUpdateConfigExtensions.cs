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

using ProtonVPN.Common.Helpers;

namespace ProtonVPN.Update.Config
{
    internal static class AppUpdateConfigExtensions
    {
        public static void Validate(this IAppUpdateConfig config)
        {
            Ensure.NotNull(config.FeedHttpClient, nameof(config.FeedHttpClient));
            Ensure.NotNull(config.FileHttpClient, nameof(config.FileHttpClient));
            Ensure.NotNull(config.FeedUriProvider, nameof(config.FeedUriProvider));
            Ensure.NotEmpty(config.UpdatesPath, nameof(config.UpdatesPath));
            Ensure.NotNull(config.CurrentVersion, nameof(config.CurrentVersion));
        }
    }
}