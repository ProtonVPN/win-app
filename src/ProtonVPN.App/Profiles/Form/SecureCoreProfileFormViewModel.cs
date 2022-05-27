/*
 * Copyright (c) 2022 Proton Technologies AG
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

using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Settings;
using ProtonVPN.Profiles.Servers;
using System.Collections.Generic;

namespace ProtonVPN.Profiles.Form
{
    public class SecureCoreProfileFormViewModel : BaseCountryServerProfileFormViewModel
    {
        public SecureCoreProfileFormViewModel(
            Common.Configuration.Config appConfig,
            ColorProvider colorProvider,
            IUserStorage userStorage,
            ServerManager serverManager,
            ProfileManager profileManager,
            IModals modals,
            IDialogs dialogs,
            IProfileFactory profileFactory) 
            : base(appConfig, colorProvider, userStorage, profileManager, dialogs, modals, serverManager, profileFactory)
        {
        }

        protected override Features GetFeatures()
        {
            return Features.SecureCore;
        }

        protected override List<IServerViewModel> GetServersByCountry(string countryCode)
        {
            return string.IsNullOrEmpty(countryCode) && !EditMode
                ? new List<IServerViewModel>()
                : base.GetServersByCountry(countryCode);
        }
    }
}
