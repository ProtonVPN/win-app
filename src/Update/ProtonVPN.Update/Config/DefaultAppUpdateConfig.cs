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

using System;
using ProtonVPN.Common.OS.Net.Http;
using ProtonVPN.Update.Contracts.Config;

namespace ProtonVPN.Update.Config
{
    /// <summary>
    /// The simple DTO object for providing configuration data to Update module.
    /// </summary>
    public class DefaultAppUpdateConfig : IAppUpdateConfig
    {
        public IHttpClient FeedHttpClient { get; set; }
        public IHttpClient FileHttpClient { get; set; }
        public IFeedUrlProvider FeedUriProvider { get; set; }
        public Version CurrentVersion { get; set; }
        public string UpdatesPath { get; set; }
        public string EarlyAccessCategoryName { get; set; }
        public TimeSpan MinProgressDuration { get; set; }
    }
}