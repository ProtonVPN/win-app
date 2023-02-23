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

using System.Net;
using ProtonVPN.Api;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Core.Modals;
using ProtonVPN.Translations;

namespace ProtonVPN.Modals.ApiActions
{
    public class ActionableFailureApiResultEventHandler
    {
        private readonly IModals _modals;
        private readonly ApiActionModalViewModel _apiActionModalViewModel;

        public ActionableFailureApiResultEventHandler(IModals modals,
            ApiActionModalViewModel apiActionModalViewModel,
            IApiClient apiClient,
            ITokenClient tokenClient)
        {
            _modals = modals;
            _apiActionModalViewModel = apiActionModalViewModel;

            apiClient.OnActionableFailureResult += OnActionableFailureResult;
            tokenClient.OnActionableFailureResult += OnActionableFailureResult;
        }

        private void OnActionableFailureResult(object sender, ActionableFailureApiResultEventArgs e)
        {
            string error = e.Result.Error;
            if (e.Result.ResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                error = Translation.Get("Login_Error_msg_Unauthorized");
            }

            _apiActionModalViewModel.SetView(error, e.Result.Actions);
            _modals.Show<ApiActionModalViewModel>();
        }
    }
}