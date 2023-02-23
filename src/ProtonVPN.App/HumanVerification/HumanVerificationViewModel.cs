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

using ProtonVPN.HumanVerification.Contracts;
using ProtonVPN.HumanVerification.Gui;
using ProtonVPN.Modals;

namespace ProtonVPN.HumanVerification
{
    public class HumanVerificationViewModel : BaseModalViewModel
    {
        public IWebViewViewModel WebViewViewModel { get; }
        public WebView WebView { get; }

        public HumanVerificationViewModel(IWebViewViewModel webViewViewModel, WebView webView)
        {
            WebViewViewModel = webViewViewModel;
            WebView = webView;
            WebView.DataContext = webViewViewModel;
            webViewViewModel.OnHumanVerificationTokenReceived += OnHumanVerificationTokenReceived;
        }

        public string ResponseToken { get; set; }

        public override void BeforeOpenModal(dynamic token)
        {
            WebViewViewModel.SetRequestToken(token);
        }

        private void OnHumanVerificationTokenReceived(object sender, string token)
        {
            ResponseToken = token;
            TryClose(true);
        }
    }
}