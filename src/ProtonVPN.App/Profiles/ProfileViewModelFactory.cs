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
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Name;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Profiles.Servers;
using ProtonVPN.Translations;

namespace ProtonVPN.Profiles
{
    public class ProfileViewModelFactory
    {
        private readonly ILogger _logger;
        private readonly IAppSettings _appSettings;
        private readonly ServerManager _serverManager;
        private readonly ProfileManager _profileManager;
        private readonly IUserStorage _userStorage;

        public ProfileViewModelFactory(ILogger logger,
            IAppSettings appSettings,
            ServerManager serverManager, 
            ProfileManager profileManager,
            IUserStorage userStorage)
        {
            _logger = logger;
            _appSettings = appSettings;
            _serverManager = serverManager;
            _profileManager = profileManager;
            _userStorage = userStorage;
        }

        public async Task<List<ProfileViewModel>> GetProfiles()
        {
            return (await _profileManager.GetProfiles())
                .Where(IsToIncludeProfile)
                .Select(GetProfileViewModel)
                .Where(viewModel => viewModel != null)
                .ToList();
        }

        private bool IsToIncludeProfile(Profile profile)
        {
            return profile.IsPredefined ||
                   profile.Server != null ||
                   profile.ProfileType is ProfileType.Fastest or ProfileType.Random;
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
                else if (profile.Features.IsB2B())
                {
                    viewModel.ConnectionName = new B2BProfileName
                    {
                        GatewayName = profile.GatewayName,
                        Server = server?.Name
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
                else if (profile.Features.IsB2B())
                {
                    viewModel.ConnectionName = new B2BProfileName
                    {
                        GatewayName = profile.GatewayName,
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

            SetUpgradeRequired(viewModel);

            return viewModel;
        }

        private void SetUpgradeRequired(ProfileViewModel profile)
        {
            if (_appSettings.FeatureFreeRescopeEnabled && !_userStorage.GetUser().Paid())
            {
                profile.UpgradeRequired = true;
            }
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

            SetUpgradeRequired(profileViewModel);

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
