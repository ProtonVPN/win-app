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
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Modals;
using ProtonVPN.Profiles.Form;
using ProtonVPN.Profiles.Servers;

namespace ProtonVPN.Profiles
{
    public class ProfileFormModalViewModel : BaseModalViewModel
    {
        private readonly StandardProfileFormViewModel _standardProfileFormViewModel;
        private readonly SecureCoreProfileFormViewModel _secureCoreProfileFormViewModel;
        private readonly TorProfileFormViewModel _torProfileFormViewModel;
        private readonly P2PProfileFormViewModel _p2PProfileFormViewModel;

        public ProfileFormModalViewModel(
            StandardProfileFormViewModel standardProfileFormViewModel,
            SecureCoreProfileFormViewModel secureCoreProfileFormViewModel,
            TorProfileFormViewModel torProfileFormViewModel,
            P2PProfileFormViewModel p2ProfileFormViewModel)
        {
            _p2PProfileFormViewModel = p2ProfileFormViewModel;
            _standardProfileFormViewModel = standardProfileFormViewModel;
            _secureCoreProfileFormViewModel = secureCoreProfileFormViewModel;
            _torProfileFormViewModel = torProfileFormViewModel;
            _p2PProfileFormViewModel = p2ProfileFormViewModel;
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
                if (!Set(ref _busy, value))
                    return;

                SaveCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            }
        }

        public string PopupPlacement { get; set; } = "Bottom";

        private IReadOnlyList<ServerTypeViewModel> _serverTypes;
        public IReadOnlyList<ServerTypeViewModel> ServerTypes =>
            _serverTypes ??= ServerTypeViewModel.AllServerTypes().ToList();

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

        public override void BeforeOpenModal(dynamic options)
        {
            if (options?.Profile == null)
                return;

            Profile profile = options.Profile;
            SetServerType(profile.Features);
            Form.EnableEditMode();
            Form.LoadProfile(profile);
        }

        public override void CanClose(Action<bool> callback)
        {
            if (Form.HasUnsavedChanges())
            {
                var result = Form.Cancel();
                callback(result != true);
            }
            else
            {
                callback(true);
            }
        }

        protected override void OnActivate()
        {
            if (Form.EditMode)
                return;

            SetServerType(Features.None);
            Form.Load();
        }

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                ClearForms();
            }
        }

        protected void SetServerType(Features features)
        {
            if (features.IsSecureCore())
                SetServerType(ServerTypes.First(t => t.Features.IsSecureCore()));
            else if (features.SupportsTor())
                SetServerType(ServerTypes.First(t => t.Features.SupportsTor()));
            else if (features.SupportsP2P())
                SetServerType(ServerTypes.First(t => t.Features.SupportsP2P()));
            else
                SetServerType(ServerTypes.First(t => !t.Features.IsSecureCore() && !t.Features.SupportsTor() && !t.Features.SupportsP2P()));
        }

        private void SetServerType(ServerTypeViewModel value)
        {
            ServerType = value;
            if (value == null)
                return;

            var features = value.Features;
            if (features.IsSecureCore())
                Form = _secureCoreProfileFormViewModel;
            else if (features.SupportsTor())
                Form = _torProfileFormViewModel;
            else if (features.SupportsP2P())
                Form = _p2PProfileFormViewModel;
            else
                Form = _standardProfileFormViewModel;
        }

        private void SelectServerTypeAction(ServerTypeViewModel item)
        {
            var previousName = Form.ProfileName;
            var previousProtocol = Form.VpnProtocol;
            var previousColor = Form.ColorCode;

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
                return;

            Busy = true;
            try
            {
                var saved = await Form.Save();
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
                return;

            TryClose();
        }

        private bool CanCancel() => !Busy;

        private void CloseErrorsAction()
        {
            Form.Error = Error.None;
        }

        private void ClearForms()
        {
            _standardProfileFormViewModel.Clear();
            _secureCoreProfileFormViewModel.Clear();
            _p2PProfileFormViewModel.Clear();
            _torProfileFormViewModel.Clear();
        }
    }
}
