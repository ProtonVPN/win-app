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

namespace ProtonVPN.Client.Models.Urls;

public class Urls : IUrls
{
    public string ProtocolsLearnMore => "https://protonvpn.com/blog/whats-the-best-vpn-protocol/";
    public string CreateAccount => "https://account.protonvpn.com/signup";
    public string ResetPassword => "https://account.protonvpn.com/reset-password";
    public string ForgotUsername => "https://account.protonvpn.com/forgot-username";
    public string TroubleSigningIn => "https://protonvpn.com/support/login-problems";
}