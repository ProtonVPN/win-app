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
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.BugReporting.Actions;
using ProtonVPN.BugReporting.FormElements;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.Models;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.Users;

namespace ProtonVPN.BugReporting.Steps
{
    public class FormViewModel : Screen,
        IUserDataAware,
        ILoggedInAware,
        ILogoutAware,
        IHandle<FillTheFormAction>,
        IHandle<FormStateChange>,
        IHandle<RetryAction>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IUserStorage _userStorage;
        private readonly IFormElementBuilder _formElementBuilder;
        private bool _isFormBeingSent;
        private string _category;
        private bool _isLoggedIn;

        public ICommand SendReportCommand { get; }

        private bool _hasErrors = true;
        public bool HasErrors
        {
            get => _hasErrors;
            private set
            {
                Set(ref _hasErrors, value);
                NotifyOfPropertyChange(nameof(IsSendAllowed));
            }
        }

        public bool IsSendAllowed => !HasErrors;

        private bool _isToIncludeLogs = true;
        public bool IsToIncludeLogs
        {
            get => _isToIncludeLogs;
            set
            {
                Set(ref _isToIncludeLogs, value);
                NotifyOfPropertyChange(nameof(IsToShowLogsWarning));
            }
        }

        public bool IsToShowLogsWarning => !IsToIncludeLogs;

        private bool _isEmailValid;
        public bool IsEmailValid
        {
            get => _isEmailValid;
            set => Set(ref _isEmailValid, value);
        }

        private List<FormElement> _formElements = new();
        public List<FormElement> FormElements
        {
            get => _formElements;
            set => Set(ref _formElements, value);
        }

        public FormViewModel(IEventAggregator eventAggregator, IUserStorage userStorage,
            IFormElementBuilder formElementBuilder)
        {
            _eventAggregator = eventAggregator;
            _userStorage = userStorage;
            _formElementBuilder = formElementBuilder;
            eventAggregator.Subscribe(this);
            SendReportCommand = new RelayCommand(SendReportActionIfAllowedAndNotAlreadySending);
        }

        private void SendReportActionIfAllowedAndNotAlreadySending()
        {
            if (IsSendAllowed && !_isFormBeingSent)
            {
                SendReportAction();
            }
        }

        private void SendReportAction()
        {
            _eventAggregator.PublishOnUIThreadAsync(new SendReportAction(_category, FormElements, IsToIncludeLogs));
        }

        public void OnUserLoggedIn()
        {
            _isLoggedIn = true;
            ResetForm();
        }

        private void ResetForm()
        {
            foreach (FormElement element in FormElements)
            {
                element.PropertyChanged -= OnFormElementChanged;
            }

            FormElements.Clear();
            IsToIncludeLogs = true;
            HasErrors = true;
            _isEmailValid = false;
            _category = null;
        }

        public void OnUserLoggedOut()
        {
            _isLoggedIn = false;
            ResetForm();
        }

        public async Task HandleAsync(FillTheFormAction message, CancellationToken cancellationToken)
        {
            if (!_category.IsNullOrEmpty() && _category != message.Category)
            {
                ResetForm();
            }

            if (FormElements.Count == 0)
            {
                _category = message.Category;
                AddFormElements();
            }
        }

        private void AddFormElements()
        {
            FormElements = _formElementBuilder.GetFormElements(_category);
            foreach (FormElement element in FormElements)
            {
                element.PropertyChanged += OnFormElementChanged;
            }

            UpdateEmailInput();
            UpdateUsernameInput();
            ValidateForm();
        }

        public async Task HandleAsync(FormStateChange message, CancellationToken cancellationToken)
        {
            _isFormBeingSent = message.State == FormState.Sending;
            if (message.State == FormState.Sent)
            {
                ResetForm();
                HasErrors = true;
            }
        }

        private void OnFormElementChanged(object sender, PropertyChangedEventArgs e)
        {
            ValidateForm();
        }

        private void ValidateForm()
        {
            ValidateEmailField();
            HasErrors = FormElements.Any(element => !element.IsValid());
        }

        private void ValidateEmailField()
        {
            FormElement emailField = FormElements.GetEmailField();
            if (emailField != null)
            {
                IsEmailValid = emailField.IsValid();
            }
        }

        public async Task HandleAsync(RetryAction message, CancellationToken cancellationToken)
        {
            SendReportAction();
        }

        public void OnUserDataChanged()
        {
            UpdateEmailInput();
            UpdateUsernameInput();
        }

        private void UpdateEmailInput()
        {
            User user = _userStorage.GetUser();
            if (EmailValidator.IsValid(user.Username))
            {
                FormElement emailField = FormElements.GetEmailField();
                if (emailField != null)
                {
                    emailField.Value = user.Username;
                }
            }
        }

        private void UpdateUsernameInput()
        {
            if (_isLoggedIn)
            {
                FormElement usernameField = FormElements.GetUsernameField();
                if (usernameField != null)
                {
                    FormElements.Remove(usernameField);
                }
            }
        }
    }
}