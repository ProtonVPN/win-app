/*
 * Copyright (c) 2020 Proton Technologies AG
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
using ProtonVPN.Core.Modals;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Modals.Dialogs;
using ProtonVPN.Modals.Upsell;
using ProtonVPN.Profiles.Servers;
using ProtonVPN.Resources;
using ServerViewModel = ProtonVPN.Profiles.Servers.ServerViewModel;

namespace ProtonVPN.Profiles.Form
{
    public abstract class AbstractForm : Screen
    {
        private readonly ColorProvider _colorProvider;
        protected readonly IUserStorage UserStorage;
        private readonly ProfileManager _profileManager;
        private readonly IDialogs _dialogs;
        private readonly Common.Configuration.Config _appConfig;
        private readonly IModals _modals;
        protected readonly ServerManager ServerManager;

        private readonly List<string> _errors = new List<string>();

        private bool _unsavedChanges;

        private string _profileId;

        protected AbstractForm(
            Common.Configuration.Config appConfig,
            ColorProvider colorProvider,
            IUserStorage userStorage,
            ProfileManager profileManager,
            IDialogs dialogs,
            IModals modals,
            ServerManager serverManager)
        {
            _appConfig = appConfig;
            _profileManager = profileManager;
            UserStorage = userStorage;
            _colorProvider = colorProvider;
            _dialogs = dialogs;
            _modals = modals;
            ServerManager = serverManager;

            SelectColorCommand = new RelayCommand<string>(SelectColorAction);
        }

        public ICommand SelectColorCommand { get; set; }

        private bool _editMode;
        public bool EditMode
        {
            get => _editMode;
            private set => Set(ref _editMode, value);
        }

        public Protocol[] Protocols => new[] {Protocol.Auto, Protocol.OpenVpnUdp, Protocol.OpenVpnTcp};

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

        private Protocol _protocol = Protocol.Auto;
        public Protocol Protocol
        {
            get => _protocol;
            set
            {
                Set(ref _protocol, value);
                _unsavedChanges = true;
            }
        }

        public virtual void Load()
        {
            SelectRandomColor();
        }

        public async Task<bool> Save()
        {
            if (!await IsFormValid())
            {
                return false;
            }

            if (EditMode)
            {
                await _profileManager.UpdateProfile(GetProfile());
            }
            else
            {
                await _profileManager.AddProfile(GetProfile());
            }

            return true;
        }

        public void EnableEditMode()
        {
            EditMode = true;
        }

        public virtual void LoadProfile(Profile profile)
        {
            ProfileName = profile.Name;
            Protocol = profile.Protocol;
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
            ProfileName = "";
            Protocol = Protocol.Auto;
            SelectedServer = null;
            ColorCode = null;

            _profileId = null;
            EditMode = false;
            _unsavedChanges = false;
            _errors.Clear();

            ClearServers();
        }

        public bool? Cancel()
        {
            return HasUnsavedChanges() ? ShowDiscardModal() : false;
        }

        public List<string> GetErrors()
        {
            var list = new List<string>();
            var lastIndex = _errors.Count - 1;
            foreach (var error in _errors)
            {
                string message;
                if (_errors.IndexOf(error).Equals(lastIndex))
                {
                    message = error;
                }
                else
                {
                    message = error + ", ";
                }

                list.Add(message);
            }

            return list;
        }

        public virtual bool HasUnsavedChanges()
        {
            if (EditMode)
            {
                return _unsavedChanges;
            }

            return !string.IsNullOrEmpty(ProfileName);
        }

        protected void MarkFormDataChanged()
        {
            _unsavedChanges = true;
        }

        protected List<IServerViewModel> GetPredefinedServerViewModels()
        {
            return new List<IServerViewModel>
            {
                new PredefinedServerViewModel
                {
                    Name = StringResources.Get("Profiles_Profile_Name_val_Fastest"),
                    Icon = "Signal",
                    Type = ProfileType.Fastest
                },
                new PredefinedServerViewModel
                {
                    Name = StringResources.Get("Profiles_Profile_Name_val_Random"),
                    Icon = "Random",
                    Type = ProfileType.Random
                }
            };
        }

        protected List<IServerViewModel> GetServerViewModels(IReadOnlyCollection<Server> serverList)
        {
            var result = serverList.Select(s => new ServerViewModel
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

        protected virtual async Task<bool> IsFormValid()
        {
            _errors.Clear();

            ProfileName = ProfileName?.Trim();

            if (string.IsNullOrEmpty(ProfileName))
            {
                AddError(StringResources.Get("Profiles_Profile_Error_msg_NameEmpty"));
                return false;
            }

            if (ProfileName.Length > _appConfig.MaxProfileNameLength)
            {
                AddError(StringResources.Format("Profiles_Profile_Error_msg_NameTooLong", _appConfig.MaxProfileNameLength));
                return false;
            }

            if (ColorCode == null)
            {
                AddError(StringResources.Get("Profiles_Profile_Error_msg_ColorEmpty"));
                return false;
            }

            if (SelectedServer == null)
            {
                AddError(StringResources.Get("Profiles_Profile_Error_msg_ServerEmpty"));
                return false;
            }

            var profile = GetProfile();
            if (EditMode && await _profileManager.OtherProfileWithNameExists(profile) ||
                !EditMode && await _profileManager.ProfileWithNameExists(profile))
            {
                AddError(StringResources.Get("Profiles_Profile_Error_msg_NameExists"));
                return false;
            }

            return true;
        }

        protected virtual Profile GetProfile()
        {
            return new Profile(_profileId)
            {
                Features = GetFeatures(),
                Name = ProfileName,
                Protocol = Protocol,
                ColorCode = ColorCode,
                ProfileType = SelectedServer.Type
            };
        }

        protected abstract Features GetFeatures();

        protected void AddError(string error)
        {
            _errors.Add(error);
        }

        private bool ShowUpgradeMessageForServer(Server server)
        {
            var user = UserStorage.User();
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
            ColorCode = _colorProvider.RandomColor();
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
            var settings = DialogSettings
                .FromMessage(StringResources.Get("Profiles_Profile_msg_DiscardChangesConfirm"))
                .WithPrimaryButtonText(StringResources.Get("Profiles_Profile_btn_KeepEditing"))
                .WithSecondaryButtonText(StringResources.Get("Profiles_Profile_btn_Discard"));

            return _dialogs.ShowQuestion(settings);
        }
    }
}
