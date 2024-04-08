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

using ProtonVPN.Client.UI.Login.Forms;

namespace ProtonVPN.Client.Models.Navigation;

public class LoginViewNavigator : ViewNavigatorBase, ILoginViewNavigator
{
    public LoginViewNavigator(IViewMapper viewMapper) : base(viewMapper)
    { }

    public Task NavigateToLoginAsync()
    {
        return NavigateToAsync<LoginFormViewModel>();
    }

    public Task NavigateToTwoFactorAsync()
    {
        return NavigateToAsync<TwoFactorFormViewModel>();
    }

    public Task NavigateToLoadingAsync()
    {
        return NavigateToAsync<LoadingFormViewModel>();
    }
}