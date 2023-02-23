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

using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ProtonVPN.Config.Url;
using ProtonVPN.Core.Settings;

namespace ProtonVPN.Modals
{
    public class DiscourageSecureCoreModalViewModel : BaseModalViewModel
    {
        private readonly IActiveUrls _urls;
        private readonly IAppSettings _appSettings;
        private bool _isToNotShowThisMessageAgain;

        public ICommand ActivateSecureCoreCommand { get; }
        public ICommand OpenSecureCoreArticlePageCommand { get; }

        public bool IsToNotShowThisMessageAgain
        {
            get => _isToNotShowThisMessageAgain;
            set => Set(ref _isToNotShowThisMessageAgain, value);
        }

        public DiscourageSecureCoreModalViewModel(IActiveUrls urls, IAppSettings appSettings)
        {
            _urls = urls;
            _appSettings = appSettings;
            ActivateSecureCoreCommand = new RelayCommand(ActivateSecureCoreAction);
            OpenSecureCoreArticlePageCommand = new RelayCommand(OpenSecureCoreArticlePageAction);
        }

        public void ActivateSecureCoreAction()
        {
            if (_isToNotShowThisMessageAgain)
            {
                _appSettings.DoNotShowDiscourageSecureCoreDialog = true;
            }

            TryClose(true);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            IsToNotShowThisMessageAgain = true;
        }

        private void OpenSecureCoreArticlePageAction()
        {
            _urls.AboutSecureCoreUrl.Open();
        }
    }
}