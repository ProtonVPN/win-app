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

using System;
using ProtonVPN.Common.Legacy.OS.Net.Http;
using ProtonVPN.Update.Contracts.Config;

namespace ProtonVPN.Update.Config;

/// <summary>
/// Interface of configuration data for Update module. It should be registered
/// in an IoC Container.
/// </summary>
public interface IAppUpdateConfig
{
    IHttpClient FeedHttpClient { get; }
    IHttpClient FileHttpClient { get; }
    IFeedUrlProvider FeedUriProvider { get; }
    Version CurrentVersion { get; }
    string UpdatesPath { get; }
    string EarlyAccessCategoryName { get; }
    TimeSpan MinProgressDuration { get; }
}