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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.AppLogs;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Name;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Profiles.Servers;
using ProtonVPN.Translations;

namespace ProtonVPN.Profiles
{
    public class ProfileViewModelFactory
    {
        private readonly ILogger _logger;
        private readonly ServerManager _serverManager;
        private readonly ProfileManager _profileManager;

        public ProfileViewModelFactory(ILogger logger, 
            ServerManager serverManager, 
            ProfileManager profileManager)
        {
            _logger = logger;
            _serverManager = serverManager;
            _profileManager = profileManager;
        }

        public async Task<List<ProfileViewModel>> GetProfiles()
        {
            return (await _profileManager.GetProfiles())
                .Where(profile => profile.IsPredefined ||
                                  profile.Server != null ||
                                  profile.ProfileType is ProfileType.Fastest or ProfileType.Random)
                .Select(GetProfileViewModel)
                .Where(viewModel => viewModel != null)
                .ToList();
        }

        private ProfileViewModel GetProfileViewModel(Profile profile)
        {
            if (profile.IsPredefined)
            {
                return CreatePredefinedVpnProfile(profile);
            }

            ProfileViewModel viewModel = new(profile);

            if (!string.IsNullOrEmpty(profile.ServerId))
            {
                Server server = GetProfileServer(profile);
                if (!ServerExists(server))
                {
                    _logger.Warn<AppLog>($"Server \"{profile.ServerId}\" doesn't exist in the cache");
                }

                if (viewModel.SecureCore)
                {
                    viewModel.ConnectionName = new SecureCoreProfileName
                    {
                        EntryCountry = server?.EntryCountry,
                        ExitCountry = profile.CountryCode,
                        Server = null
                    };
                }
                else
                {
                    viewModel.ConnectionName = new StandardProfileName
                    {
                        CountryCode = profile.CountryCode,
                        Server = server?.Name
                    };
                }

                viewModel.Server = server;
            }
            else
            {
                if (profile.Features.IsSecureCore())
                {
                    viewModel.ConnectionName = new SecureCoreProfileName
                    {
                        EntryCountry = null,
                        ExitCountry = !string.IsNullOrEmpty(profile.CountryCode) ? profile.CountryCode : null,
                        Server = ServerNameAsProfile(profile.ProfileType)
                    };
                }
                else if (profile.Features.SupportsTor() || profile.Features.SupportsP2P())
                {
                    viewModel.ConnectionName = new CustomProfileName
                    {
                        Name = ServerTypeViewModel.TypeName(profile.Features),
                        CountryCode = profile.CountryCode,
                        Server = ServerNameAsProfile(profile.ProfileType)
                    };
                }
                else
                {
                    viewModel.ConnectionName = new StandardProfileName
                    {
                        CountryCode = profile.CountryCode,
                        Server = ServerNameAsProfile(profile.ProfileType)
                    };
                }
            }

            return viewModel;
        }

        private string ServerNameAsProfile(ProfileType type)
        {
            string profileType = Enum.GetName(typeof(ProfileType), type);
            return Translation.Get($"Profiles_Profile_Name_val_{profileType}");
        }

        private PredefinedProfileViewModel CreatePredefinedVpnProfile(Profile profile)
        {
            PredefinedProfileViewModel profileViewModel = new(profile);
            
            switch (profileViewModel.Id)
            {
                case "Fastest":
                    profileViewModel.Icon = "Bolt";
                    profileViewModel.Text = Translation.Get("Profiles_Profile_Name_val_Fastest");
                    profileViewModel.Description = Translation.Get("Profiles_Profile_Description_val_Fastest");
                    break;
                case "Random":
                    profileViewModel.Icon = "ArrowsSwapRight";
                    profileViewModel.Text = Translation.Get("Profiles_Profile_Name_val_Random");
                    profileViewModel.Description = Translation.Get("Profiles_Profile_Description_val_Random");
                    break;
            }

            return profileViewModel;
        }

        private Server GetProfileServer(Profile profile)
        {
            return _serverManager.GetServer(new ServerById(profile.ServerId));
        }

        private bool ServerExists(Server server)
        {
            return server != null;
        }
    }
}
