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
using System.Windows.Input;
using Caliburn.Micro;
using GalaSoft.MvvmLight.CommandWpf;
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
        private bool _isToIncludeLogs = true;
        private List<FormElement> _formElements = new();
        private bool _isFormBeingSent;
        private bool _hasErrors = true;
        private bool _isEmailValid = true;
        private string _category;
        private bool _isLoggedIn;

        public FormViewModel(IEventAggregator eventAggregator, IUserStorage userStorage,
            IFormElementBuilder formElementBuilder)
        {
            _eventAggregator = eventAggregator;
            _userStorage = userStorage;
            _formElementBuilder = formElementBuilder;
            eventAggregator.Subscribe(this);
            SendReportCommand = new RelayCommand(SendReportAction, () => !_hasErrors && !_isFormBeingSent);
        }

        public ICommand SendReportCommand { get; }

        public bool IsToIncludeLogs
        {
            get => _isToIncludeLogs;
            set
            {
                Set(ref _isToIncludeLogs, value);
                NotifyOfPropertyChange(nameof(IsToShowLogsWarning));
            }
        }

        public bool IsEmailValid
        {
            get => _isEmailValid;
            set => Set(ref _isEmailValid, value);
        }

        public bool IsToShowLogsWarning => !IsToIncludeLogs;

        public List<FormElement> FormElements
        {
            get => _formElements;
            set => Set(ref _formElements, value);
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
            _hasErrors = true;
            _isEmailValid = true;
            _category = null;
        }

        public void OnUserLoggedOut()
        {
            _isLoggedIn = false;
            ResetForm();
        }

        public void Handle(FillTheFormAction message)
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
        }

        public void Handle(FormStateChange message)
        {
            _isFormBeingSent = message.State == FormState.Sending;
            if (message.State == FormState.Sent)
            {
                ResetForm();
                _hasErrors = true;
            }
        }

        private void OnFormElementChanged(object sender, PropertyChangedEventArgs e)
        {
            ValidateForm();
        }

        private void ValidateForm()
        {
            ValidateEmailField();
            _hasErrors = FormElements.Any(element => !element.IsValid());
        }

        private void ValidateEmailField()
        {
            FormElement emailField = FormElements.GetEmailField();
            if (emailField != null)
            {
                IsEmailValid = emailField.IsValid();
            }
        }

        public void Handle(RetryAction message)
        {
            SendReportAction();
        }

        private void SendReportAction()
        {
            _eventAggregator.PublishOnUIThread(new SendReportAction(_category, FormElements, IsToIncludeLogs));
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