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

using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Modals;
using ProtonVPN.Profiles.Form;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
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
            _serverTypes ?? (_serverTypes = ServerTypeViewModel.AllServerTypes().ToList());

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

        private bool _showErrors;
        public bool ShowErrors
        {
            get => _showErrors;
            private set => Set(ref _showErrors, value);
        }

        private List<string> _errors;
        public List<string> Errors
        {
            get => _errors;
            private set => Set(ref _errors, value);
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
            SetServerType(item.Features);
            ShowErrors = false;
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
                else
                {
                    Errors = Form.GetErrors();
                    ShowErrors = true;
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

            var result = Form.Cancel();
            if (result.HasValue && !result.Value)
            {
                TryClose();
            }
        }

        private bool CanCancel() => !Busy;

        private void CloseErrorsAction()
        {
            ShowErrors = false;
        }

        private void ClearForms()
        {
            Errors = null;
            CloseErrorsAction();

            _standardProfileFormViewModel.Clear();
            _secureCoreProfileFormViewModel.Clear();
            _p2PProfileFormViewModel.Clear();
            _torProfileFormViewModel.Clear();
        }
    }
}
