/*
* Copyright (c) 2026 Proton AG
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

using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
public class SecureCoreTests : FreshSessionSetUp
{
    private const string PROFILE_NAME = "Profile B";

    private string? _ipAddressNotConnected = null;
    private readonly string _viaCountryIceland = "via Iceland";
    private readonly string _viaCountrySweden = "via Sweden";
    private static readonly string _countryName = "Australia";

    [SetUp]
    public void TestInitialize()
    {
        _ipAddressNotConnected = NetworkUtils.GetIpAddressWithRetry();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test]
    public void ConnectToSecureCoreServerViaCountriesList()
    {
        SidebarRobot
            .NavigateToSecureCoreCountriesTab()
            .ConnectToCountry(_countryName);
        HomeRobot
            .Verify.IsConnected();

        string ipAfterConnection = NetworkUtils.GetIpAddressWithRetry();
        HomeRobot
            .Verify.AssertVpnConnectionEstablished(_ipAddressNotConnected!, ipAfterConnection);

        SidebarRobot
            .ExpandCities(_countryName)
            .ConnectViaSecureCore(_countryName, _viaCountryIceland);
        HomeRobot
            .Verify.IsConnected();
        NetworkUtils.VerifyUserIsConnectedToExpectedCountry(_countryName);

        SidebarRobot
            .ConnectViaSecureCore(_countryName, _viaCountrySweden);
        HomeRobot
            .Verify.IsConnected();
        NetworkUtils.VerifyUserIsConnectedToExpectedCountry(_countryName);
    }

    [Test]
    public void DisconnectFromSecureCoreServerViaCountriesList()
    {
        SidebarRobot
            .NavigateToSecureCoreCountriesTab()
            .ExpandCities(_countryName);
        ConnectToSecureCore(_viaCountrySweden);
        SidebarRobot
            .DisconnectViaCountry(_countryName);
        HomeRobot
            .Verify.IsDisconnected();
        NetworkUtils.VerifyIpAddressMatchesWithRetry(_ipAddressNotConnected);

        ConnectToSecureCore(_viaCountryIceland);
        SidebarRobot
            .DisconnectViaSecureCore(_countryName, _viaCountryIceland);
        HomeRobot
            .Verify.IsDisconnected();
        NetworkUtils.VerifyIpAddressMatchesWithRetry(_ipAddressNotConnected);
    }

    [Test]
    public void QuickConnectToSecureCoreServerAndDisconnect()
    {
        AddConnectionInRecents();

        HomeRobot
            .SelectDefaultConnectionCountry(_countryName, _viaCountryIceland)
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        string ipAfterConnection = NetworkUtils.GetIpAddressWithRetry();
        HomeRobot
            .Verify.AssertVpnConnectionEstablished(_ipAddressNotConnected!, ipAfterConnection)
            .Disconnect()
            .Verify.IsDisconnected();
        NetworkUtils.VerifyIpAddressMatchesWithRetry(_ipAddressNotConnected);
    }

    [Test]
    public void ConnectToSecureCoreServerViaProfilesAndDisconnect()
    {
        CreateSecureCoreProfile();

        SidebarRobot
            .ConnectToProfile(PROFILE_NAME);
        HomeRobot
            .Verify.IsConnected()
                   .ConnectionCardTitleEquals(PROFILE_NAME)
                   .ConnectionCardDescriptionEquals($"{_countryName} {_viaCountrySweden}");
        NetworkUtils.VerifyUserIsConnectedToExpectedCountry(_countryName);

        SidebarRobot
            .Verify.IsDisconnectBtnOnHoverDisplayed(PROFILE_NAME)
                   .IsGreenDotDisplayed(PROFILE_NAME)
            .DisconnectViaProfile(PROFILE_NAME);
        HomeRobot
            .Verify.IsDisconnected();
        NetworkUtils.VerifyIpAddressMatchesWithRetry(_ipAddressNotConnected);
    }

    [Test]
    public void ConnectToSecureCoreServerViaRecentsAndDisconnect()
    {
        AddConnectionInRecents();

        SidebarRobot
            .NavigateToRecents()
            .ConnectViaSecureCore(_countryName, _viaCountryIceland);
        HomeRobot
            .Verify.IsConnected()
                   .ConnectionCardTitleEquals(_countryName)
                   .ConnectionCardDescriptionEquals(_viaCountryIceland);
        NetworkUtils.VerifyUserIsConnectedToExpectedCountry(_countryName);

        SidebarRobot
            .Verify.IsDisconnectBtnOnHoverDisplayed(_countryName)
                   .IsGreenDotDisplayed(_countryName)
            .DisconnectViaSecureCore(_countryName, _viaCountryIceland);
        HomeRobot
            .Verify.IsDisconnected();
        NetworkUtils.VerifyIpAddressMatchesWithRetry(_ipAddressNotConnected);
    }

    private void CreateSecureCoreProfile()
    {
        SidebarRobot
            .NavigateToProfiles()
            .CreateProfile();

        ProfileRobot
            .SetProfileName(PROFILE_NAME)
            .SelectConnectionType(ConnectionType.SecureCore)
            .SelectCountry(_countryName)
            .SelectMiddleCountry(_viaCountrySweden)
            .SaveProfile();
    }

    private void AddConnectionInRecents()
    {
        SidebarRobot
            .NavigateToSecureCoreCountriesTab()
            .ExpandCities(_countryName)
            .ConnectViaSecureCore(_countryName, _viaCountryIceland);
        HomeRobot
            .Verify.IsConnected()
            .Disconnect()
            .Verify.IsDisconnected();
    }

    private void ConnectToSecureCore(string viaCountry)
    {
        SidebarRobot
            .ConnectViaSecureCore(_countryName, viaCountry);
        HomeRobot
            .Verify.IsConnected();
    }
}