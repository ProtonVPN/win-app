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

using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Settings;
using System.Collections.Generic;
using System.Linq;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Profiles.Servers;

namespace ProtonVPN.Profiles.Form
{
    public abstract class ServerProfileFormViewModel : AbstractForm
    {
        protected ServerProfileFormViewModel(
            IConfiguration appConfig,
            ColorProvider colorProvider,
            IUserStorage userStorage,
            ProfileManager profileManager,
            IDialogs dialogs,
            IModals modals,
            ServerManager serverManager,
            IProfileFactory profileFactory) 
            : base(appConfig, colorProvider, userStorage, profileManager, dialogs, modals, serverManager, profileFactory)
        {
        }

        public override void Load()
        {
            base.Load();
            if (!EditMode)
            {
                LoadServers();
                SelectedServer = Servers.FirstOrDefault();
            }
        }

        public override void LoadProfile(Profile profile)
        {
            LoadServers();
            base.LoadProfile(profile);
        }

        public override void Clear()
        {
            base.Clear();
            SelectedServer = null;
        }

        protected override Profile CreateProfile()
        {
            Profile profile = base.CreateProfile();
            profile.ServerId = SelectedServer?.Id;
            return profile;
        }

        protected virtual List<Server> GetServerList()
        {
            return ServerManager.GetServers(new ServerByFeatures(GetFeatures())).ToList();
        }

        private void LoadServers()
        {
            List<IServerViewModel> servers = GetPredefinedServerViewModels();
            servers.AddRange(GetServerViewModels(GetServerList()));
            Servers = servers;
        }
    }
}
