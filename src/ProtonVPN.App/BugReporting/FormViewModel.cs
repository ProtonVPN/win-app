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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using ProtonVPN.Account;
using ProtonVPN.BugReporting.Attachments;
using ProtonVPN.Common.OS.Processes;
using ProtonVPN.Core.Auth;
using ProtonVPN.Core.MVVM;
using ProtonVPN.Core.Settings;
using ProtonVPN.Core.User;
using ProtonVPN.Servers;

namespace ProtonVPN.BugReporting
{
    public class FormViewModel : ViewModel, IUserDataAware, ILogoutAware, IUserLocationAware
    {
        private string _account;
        private string _plan;
        private string _version;
        private string _feedback;
        private string _email;
        private string _isp;
        private string _country;
        private string _planColor;

        private readonly IUserStorage _userStorage;
        private readonly Attachments.Attachments _attachments;
        private readonly IOsProcesses _processes;
        private readonly Common.Configuration.Config _appConfig;

        public FormViewModel(
            Common.Configuration.Config appConfig,
            IUserStorage userStorage,
            Attachments.Attachments attachments,
            IOsProcesses processes)
        {
            _appConfig = appConfig;
            _userStorage = userStorage;
            _attachments = attachments;
            _processes = processes;

            AddAttachmentCommand = new RelayCommand(AddAttachment);
            RemoveAttachmentCommand = new RelayCommand<Attachment>(RemoveAttachment);
            OpenAttachmentCommand = new RelayCommand<Attachment>(OpenAttachment);
        }

        public ICommand AddAttachmentCommand { get; set; }
        public ICommand RemoveAttachmentCommand { get; set; }
        public ICommand OpenAttachmentCommand { get; set; }

        public ObservableCollection<Attachment> Attachments => _attachments.Items;

        public string Email
        {
            get => _email;
            set => Set(ref _email, value);
        }

        public string Feedback
        {
            get => _feedback;
            set => Set(ref _feedback, value);
        }

        public string Account
        {
            get => _account;
            set => Set(ref _account, value);
        }

        public string Plan
        {
            get => _plan;
            set => Set(ref _plan, value);
        }

        public string Version
        {
            get => _version;
            set => Set(ref _version, value);
        }

        public string PlanColor
        {
            get => _planColor;
            set => Set(ref _planColor, value);
        }

        public string Isp
        {
            get => _isp;
            set => Set(ref _isp, value);
        }

        public string Country
        {
            get => _country;
            set => Set(ref _country, value);
        }

        public double MaxFileSize => _appConfig.ReportBugMaxFileSize;

        public void Load()
        {
            ClearForm();
            LoadUserData();
            LoadUserLocation();
            _attachments.Load();
        }

        public void RemoveAttachment(Attachment attachment)
        {
            _attachments.Remove(attachment);
        }

        public void AddAttachment()
        {
            _attachments.SelectFiles();
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Feedback);
        }

        public KeyValuePair<string, string>[] GetFields()
        {
            return new[]
            {
                new KeyValuePair<string, string>("OS", "Windows"),
                new KeyValuePair<string, string>("OSVersion", Environment.OSVersion.ToString()),
                new KeyValuePair<string, string>("Client", "Windows app"),
                new KeyValuePair<string, string>("ClientVersion", Version),
                new KeyValuePair<string, string>("Title", "Windows app form"),
                new KeyValuePair<string, string>("Description", Feedback),
                new KeyValuePair<string, string>("Username", Account),
                new KeyValuePair<string, string>("Plan", Plan),
                new KeyValuePair<string, string>("Email", Email),
                new KeyValuePair<string, string>("Country", string.IsNullOrEmpty(Country) ? "" : Country),
                new KeyValuePair<string, string>("ISP", string.IsNullOrEmpty(Isp) ? "" : Isp),
                new KeyValuePair<string, string>("ClientType", "2")
            };
        }

        public void OnUserDataChanged()
        {
            LoadUserData();
        }

        public Task OnUserLocationChanged(UserLocationEventArgs e)
        {
            if (e.State == UserLocationState.Success)
            {
                LoadUserLocation();
            }

            return Task.CompletedTask;
        }

        public void OnUserLoggedOut()
        {
            Email = string.Empty;
        }

        private void ClearForm()
        {
            Feedback = string.Empty;
        }

        private void LoadUserData()
        {
            var user = _userStorage.User();
            Account = user.Username;
            Plan = VpnPlanHelper.GetPlanName(user.VpnPlan);
            Version = _appConfig.AppVersion;
            PlanColor = VpnPlanHelper.GetPlanColor(user.VpnPlan);
            if (EmailValidator.IsValid(user.Username))
            {
                Email = user.Username;
            }
        }

        private void LoadUserLocation()
        {
            var location = _userStorage.Location();
            Country = Countries.GetName(location.Country);
            Isp = location.Isp;
        }

        private void OpenAttachment(Attachment attachment)
        {
            _processes.Open(attachment.Path);
        }
    }
}
