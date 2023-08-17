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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Modals;
using ProtonVPN.Profiles.Form;
using ProtonVPN.Profiles.Servers;

namespace ProtonVPN.Profiles
{
    public class ProfileFormModalViewModel : BaseModalViewModel
    {
        private readonly B2BProfileFormViewModel _b2bProfileFormViewModel;
        private readonly StandardProfileFormViewModel _standardProfileFormViewModel;
        private readonly SecureCoreProfileFormViewModel _secureCoreProfileFormViewModel;
        private readonly TorProfileFormViewModel _torProfileFormViewModel;
        private readonly P2PProfileFormViewModel _p2PProfileFormViewModel;
        private readonly ServerManager _serverManager;

        public ProfileFormModalViewModel(
            B2BProfileFormViewModel b2bProfileFormViewModel,
            StandardProfileFormViewModel standardProfileFormViewModel,
            SecureCoreProfileFormViewModel secureCoreProfileFormViewModel,
            TorProfileFormViewModel torProfileFormViewModel,
            P2PProfileFormViewModel p2ProfileFormViewModel,
            ServerManager serverManager)
        {
            _b2bProfileFormViewModel = b2bProfileFormViewModel;
            _standardProfileFormViewModel = standardProfileFormViewModel;
            _secureCoreProfileFormViewModel = secureCoreProfileFormViewModel;
            _torProfileFormViewModel = torProfileFormViewModel;
            _p2PProfileFormViewModel = p2ProfileFormViewModel;
            _serverManager = serverManager;
            _form = _standardProfileFormViewModel;

            SelectServerTypeCommand = new RelayCommand<ServerTypeViewModel>(SelectServerTypeAction);
            SaveCommand = new RelayCommand(SaveAction, CanSave);
            CancelCommand = new RelayCommand(CancelAction, CanCancel);
            CloseErrorsCommand = new RelayCommand(CloseErrorsAction);
        }

        public ICommand SelectServerTypeCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }
        public ICommand CloseErrorsCommand { get; }

        private bool _busy;
        public bool Busy
        {
            get => _busy;
            private set
            {
                if (Set(ref _busy, value))
                {
                    SaveCommand.RaiseCanExecuteChanged();
                    CancelCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string PopupPlacement { get; set; } = "Bottom";

        private ServerTypeViewModel _serverType;
        public ServerTypeViewModel ServerType
        {
            get => _serverType;
            private set => Set(ref _serverType, value);
        }

        private AbstractForm _form;

        public AbstractForm Form
        {
            get => _form;
            private set => Set(ref _form, value);
        }

        private IReadOnlyList<ServerTypeViewModel> _serverTypesCache;
        public IReadOnlyList<ServerTypeViewModel> ServerTypesCache =>
            _serverTypesCache ??= ServerTypeViewModel.AllServerTypes().ToList();

        public IReadOnlyList<ServerTypeViewModel> ServerTypes => ServerTypesCache
            .Where(s => !s.Features.IsB2B() || _serverManager.HasB2BServers()).ToList();

        public override void BeforeOpenModal(dynamic options)
        {
            if (options?.Profile == null)
            {
                return;
            }

            Profile profile = options.Profile;
            SetServerType(profile.Features);
            Form.EnableEditMode();
            Form.LoadProfile(profile);
        }

        public override async Task<bool> CanCloseAsync(CancellationToken cancellationToken = default)
        {
            if (Form.HasUnsavedChanges())
            {
                bool? result = await Form.CancelAsync();
                return result != true;
            }
            else
            {
                return true;
            }
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            if (Form.EditMode)
            {
                return;
            }

            SetServerType(_serverManager.HasB2BServers() ? Features.B2B : Features.None);
            Form.Load();
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                ClearForms();
            }
        }

        protected void SetServerType(Features features)
        {
            if (features.IsB2B())
            {
                ServerTypeViewModel b2bServerTypeViewModel = ServerTypes.FirstOrDefault(t => t.Features.IsB2B());
                if (b2bServerTypeViewModel is null)
                {
                    SetServerType(GetStandardServerType());
                }
                else
                {
                    SetServerType(b2bServerTypeViewModel);
                }
            }
            else if (features.IsSecureCore())
            {
                SetServerType(ServerTypes.First(t => t.Features.IsSecureCore()));
            }
            else if (features.SupportsTor())
            {
                SetServerType(ServerTypes.First(t => t.Features.SupportsTor()));
            }
            else if (features.SupportsP2P())
            {
                SetServerType(ServerTypes.First(t => t.Features.SupportsP2P()));
            }
            else
            {
                SetServerType(GetStandardServerType());
            }
        }

        private ServerTypeViewModel GetStandardServerType()
        {
            return ServerTypes.First(t => !t.Features.IsSecureCore()
                                       && !t.Features.SupportsTor()
                                       && !t.Features.SupportsP2P()
                                       && !t.Features.IsB2B());
        }

        private void SetServerType(ServerTypeViewModel value)
        {
            ServerType = value;
            if (value == null)
            {
                return;
            }

            Features features = value.Features;
            if (features.IsB2B())
            {
                Form = _b2bProfileFormViewModel;
            }
            else if (features.IsSecureCore())
            {
                Form = _secureCoreProfileFormViewModel;
            }
            else if (features.SupportsTor())
            {
                Form = _torProfileFormViewModel;
            }
            else if (features.SupportsP2P())
            {
                Form = _p2PProfileFormViewModel;
            }
            else
            {
                Form = _standardProfileFormViewModel;
            }
        }

        private void SelectServerTypeAction(ServerTypeViewModel item)
        {
            string previousName = Form.ProfileName;
            Common.Networking.VpnProtocol previousProtocol = Form.VpnProtocol;
            string previousColor = Form.ColorCode;

            SetServerType(item.Features);
            Form.Error = Error.None;
            Form.ColorCode = previousColor;
            Form.ProfileName = previousName;
            Form.VpnProtocol = previousProtocol;

            Form.Load();
        }

        private async void SaveAction()
        {
            if (!CanSave())
            {
                return;
            }

            Busy = true;
            try
            {
                bool saved = await Form.Save();
                if (saved)
                {
                    TryClose();
                    CloseErrorsAction();
                }
            }
            finally
            {
                Busy = false;
            }
        }

        private bool CanSave() => !Busy;

        private void CancelAction()
        {
            if (!CanCancel())
            {
                return;
            }

            TryClose();
        }

        private bool CanCancel() => !Busy;

        private void CloseErrorsAction()
        {
            Form.Error = Error.None;
        }

        private void ClearForms()
        {
            _b2bProfileFormViewModel.Clear();
            _standardProfileFormViewModel.Clear();
            _secureCoreProfileFormViewModel.Clear();
            _p2PProfileFormViewModel.Clear();
            _torProfileFormViewModel.Clear();
        }
    }
}
