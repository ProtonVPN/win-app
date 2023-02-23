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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.Networking;
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Modals.Dialogs;
using ProtonVPN.Modals.Upsell;
using ProtonVPN.Profiles.Servers;
using ProtonVPN.Translations;
using ServerViewModel = ProtonVPN.Profiles.Servers.ServerViewModel;

namespace ProtonVPN.Profiles.Form
{
    public abstract class AbstractForm : Screen
    {
        private readonly ColorProvider _colorProvider;
        protected readonly IUserStorage UserStorage;
        private readonly ProfileManager _profileManager;
        private readonly IDialogs _dialogs;
        private readonly IConfiguration _appConfig;
        private readonly IModals _modals;
        protected readonly ServerManager ServerManager;
        private readonly IProfileFactory _profileFactory;

        private bool _unsavedChanges;

        private string _profileId;

        protected AbstractForm(
            IConfiguration appConfig,
            ColorProvider colorProvider,
            IUserStorage userStorage,
            ProfileManager profileManager,
            IDialogs dialogs,
            IModals modals,
            ServerManager serverManager,
            IProfileFactory profileFactory)
        {
            _appConfig = appConfig;
            _profileManager = profileManager;
            UserStorage = userStorage;
            _colorProvider = colorProvider;
            _dialogs = dialogs;
            _modals = modals;
            ServerManager = serverManager;
            _profileFactory = profileFactory;

            SelectColorCommand = new RelayCommand<string>(SelectColorAction);
        }

        public ICommand SelectColorCommand { get; set; }

        private bool _editMode;
        public bool EditMode
        {
            get => _editMode;
            private set => Set(ref _editMode, value);
        }

        public VpnProtocol[] Protocols => new[] { VpnProtocol.Smart, VpnProtocol.OpenVpnUdp, VpnProtocol.OpenVpnTcp, VpnProtocol.WireGuard };

        private string[] _colors;
        public string[] Colors => _colors ??= _colorProvider.GetColors();

        private IReadOnlyList<IServerViewModel> _servers;
        public IReadOnlyList<IServerViewModel> Servers
        {
            get => _servers;
            protected set => Set(ref _servers, value);
        }

        private IServerViewModel _selectedServer;
        public IServerViewModel SelectedServer
        {
            get => _selectedServer;
            set
            {
                if (ShowUpgradeModal(value))
                {
                    _modals.Show<UpsellModalViewModel>();
                }

                Set(ref _selectedServer, value);
                _unsavedChanges = true;
            }
        }

        private string _colorCode;
        public string ColorCode
        {
            get => _colorCode;
            set
            {
                Set(ref _colorCode, value);
                _unsavedChanges = true;
            }
        }

        private string _profileName;
        public string ProfileName
        {
            get => _profileName;
            set
            {
                Set(ref _profileName, value);
                _unsavedChanges = true;
            }
        }

        private VpnProtocol _openVpnProtocol = VpnProtocol.Smart;
        public VpnProtocol VpnProtocol
        {
            get => _openVpnProtocol;
            set
            {
                Set(ref _openVpnProtocol, value);
                _unsavedChanges = true;
            }
        }

        private Error _error;
        public Error Error
        {
            get => _error;
            set => Set(ref _error, value);
        }

        public string ProfileErrorTooLong => Translation.GetPluralFormat("Profiles_Profile_Error_msg_NameTooLong",
            _appConfig.MaxProfileNameLength);

        public virtual void Load()
        {
            SelectRandomColor();
        }

        public async Task<bool> Save()
        {
            Error error = await GetFormError();
            if (error != Error.None)
            {
                Error = error;
                return false;
            }

            if (EditMode)
            {
                await _profileManager.UpdateProfile(CreateProfile());
            }
            else
            {
                await _profileManager.AddProfile(CreateProfile());
            }

            Clear();

            return true;
        }

        public void EnableEditMode()
        {
            EditMode = true;
        }

        public virtual void LoadProfile(Profile profile)
        {
            ProfileName = profile.Name;
            VpnProtocol = profile.VpnProtocol;
            ColorCode = profile.ColorCode;

            if (profile.Server == null)
            {
                switch (profile.ProfileType)
                {
                    case ProfileType.Fastest:
                        SelectedServer = Servers?.FirstOrDefault(s => s.Type == ProfileType.Fastest);
                        break;
                    case ProfileType.Random:
                        SelectedServer = Servers?.FirstOrDefault(s => s.Type == ProfileType.Random);
                        break;
                }
            }
            else
            {
                SelectedServer = Servers?.FirstOrDefault(s => s.Id == profile.ServerId);
            }

            _unsavedChanges = false;
            _profileId = profile.Id;
        }

        public virtual void Clear()
        {
            ProfileName = string.Empty;
            VpnProtocol = VpnProtocol.Smart;
            SelectedServer = null;
            ColorCode = null;

            _profileId = null;
            EditMode = false;
            _unsavedChanges = false;
            Error = Error.None;

            ClearServers();
        }

        public bool? Cancel()
        {
            if (HasUnsavedChanges())
            {
                bool? result = ShowDiscardModal();
                if (result == false)
                {
                    _unsavedChanges = false;
                    ProfileName = string.Empty;
                }

                return result;
            }

            return false;
        }

        public virtual bool HasUnsavedChanges()
        {
            if (EditMode)
            {
                return _unsavedChanges;
            }

            return !string.IsNullOrEmpty(ProfileName);
        }

        protected List<IServerViewModel> GetPredefinedServerViewModels()
        {
            return new()
            {
                new PredefinedServerViewModel
                {
                    Name = Translation.Get("Profiles_Profile_Name_val_Fastest"),
                    Icon = "Bolt",
                    Type = ProfileType.Fastest
                },
                new PredefinedServerViewModel
                {
                    Name = Translation.Get("Profiles_Profile_Name_val_Random"),
                    Icon = "ArrowsSwapRight",
                    Type = ProfileType.Random
                }
            };
        }

        protected List<IServerViewModel> GetServerViewModels(IReadOnlyCollection<Server> serverList)
        {
            List<IServerViewModel> result = serverList.Select(s => new ServerViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    CountryCode = s.EntryCountry,
                    UpgradeRequired = ShowUpgradeMessageForServer(s),
                    Type = ProfileType.Custom,
                })
                .Cast<IServerViewModel>()
                .ToList();

            return result;
        }

        protected virtual async Task<Error> GetFormError()
        {
            ProfileName = ProfileName?.Trim();

            if (string.IsNullOrEmpty(ProfileName))
            {
                return Error.EmptyProfileName;
            }

            if (ProfileName.Length > _appConfig.MaxProfileNameLength)
            {
                return Error.ProfileNameTooLong;
            }

            if (ColorCode == null)
            {
                return Error.EmptyColor;
            }

            if (SelectedServer == null)
            {
                return Error.EmptyServer;
            }

            Profile profile = CreateProfile();
            if (EditMode && await _profileManager.OtherProfileWithNameExists(profile) ||
                !EditMode && await _profileManager.ProfileWithNameExists(profile))
            {
                return Error.ProfileNameExists;
            }

            return Error.None;
        }

        protected virtual Profile CreateProfile()
        {
            Profile profile = _profileFactory.Create(_profileId);
            profile.Features = GetFeatures();
            profile.Name = ProfileName;
            profile.VpnProtocol = VpnProtocol;
            profile.ColorCode = _colorProvider.GetRandomColorIfInvalid(ColorCode);
            profile.ProfileType = SelectedServer.Type;
            return profile;
        }

        protected abstract Features GetFeatures();

        private bool ShowUpgradeMessageForServer(Server server)
        {
            User user = UserStorage.GetUser();
            return user.MaxTier < server.Tier;
        }

        private void ClearServers()
        {
            Servers = new List<IServerViewModel>();
        }

        private void SelectColorAction(string colorCode)
        {
            ColorCode = colorCode;
        }

        private void SelectRandomColor()
        {
            if (string.IsNullOrEmpty(ColorCode))
            {
                ColorCode = _colorProvider.GetRandomColor();
            }
        }

        private bool ShowUpgradeModal(IServerViewModel value)
        {
            if (value != null && value.UpgradeRequired)
            {
                if (EditMode)
                {
                    if (SelectedServer != null && !SelectedServer.UpgradeRequired)
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        private bool? ShowDiscardModal()
        {
            DialogSettings settings = DialogSettings
                .FromMessage(Translation.Get("Profiles_Profile_msg_DiscardChangesConfirm"))
                .WithPrimaryButtonText(Translation.Get("Profiles_Profile_btn_KeepEditing"))
                .WithSecondaryButtonText(Translation.Get("Profiles_Profile_btn_Discard"));

            return _dialogs.ShowQuestion(settings);
        }
    }
}
