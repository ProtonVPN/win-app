/*
 * Copyright (c) 2025 Proton AG
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

using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Services.Upselling;
using ProtonVPN.Common.Core.Extensions;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.Client.Models.Announcements;

public class AnnouncementActivator : IAnnouncementActivator
{
    private readonly ILogger _logger;
    private readonly IWebAuthenticator _webAuthenticator;
    private readonly IAccountUpgradeUrlLauncher _accountUpgradeUrlLauncher;

    public AnnouncementActivator(
        ILogger logger,
        IWebAuthenticator webAuthenticator,
        IAccountUpgradeUrlLauncher accountUpgradeUrlLauncher)
    {
        _logger = logger;
        _webAuthenticator = webAuthenticator;
        _accountUpgradeUrlLauncher = accountUpgradeUrlLauncher;
    }

    public async Task ActivateAsync(Announcement? announcement)
    {
        string action = announcement?.Panel?.Button?.Action ?? string.Empty;

        if (action.EqualsIgnoringCase("OpenURL"))
        {
            string baseUrl = announcement?.Panel?.Button?.Url ?? string.Empty;
            List<string> behaviors = announcement?.Panel?.Button?.Behaviors ?? new();
            ModalSource modalSource = ModalSource.PromoOffer;
            string reference = announcement?.Reference ?? string.Empty;

            string url = behaviors.Contains("AutoLogin")
                ? await _webAuthenticator.GetAuthUrlAsync(baseUrl, modalSource, reference)
                : baseUrl;

            _accountUpgradeUrlLauncher.Open(url, modalSource, reference);
        }
        else
        {
            _logger.Error<AppLog>($"The Announcement Button is null or the action '{action}' is unsupported.");
        }
    }
}