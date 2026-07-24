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
using ProtonVPN.UI.Tests.Enums.Locations;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("1")]
public class SecureCoreTests : FreshSessionSetUp
{
    private const string PROFILE_NAME = "Profile B";

    private string? _ipAddressNotConnected = null;
    private const Country VIA_COUNTRY_ICELAND = Country.Iceland;
    private const Country VIA_COUNTRY_SWEDEN = Country.Sweden;
    private const Country COUNTRY_NAME = Country.Australia;

    [SetUp]
    public void TestInitialize()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
        _ipAddressNotConnected = NetworkUtils.GetIpAddressWithRetry();
    }

    [Test]
    [Property("TestCaseId", "602370")]
    public void ConnectToSecureCoreServerViaCountriesList()
    {
        SidebarRobot
            .NavigateToSecureCoreCountriesTab()
            .ConnectToCountry(COUNTRY_NAME);
        HomeRobot
            .Verify.IsConnected();

        string ipAfterConnection = NetworkUtils.GetIpAddressWithRetry();
        HomeRobot
            .Verify.AssertVpnConnectionEstablished(_ipAddressNotConnected!, ipAfterConnection);

        SidebarRobot
            .ExpandCities(COUNTRY_NAME)
            .ConnectViaSecureCore(COUNTRY_NAME, VIA_COUNTRY_ICELAND);
        HomeRobot
            .Verify.IsConnected();
        NetworkUtils.VerifyUserIsConnectedToExpectedCountry(COUNTRY_NAME);

        SidebarRobot
            .ConnectViaSecureCore(COUNTRY_NAME, VIA_COUNTRY_SWEDEN);
        HomeRobot
            .Verify.IsConnected();
        NetworkUtils.VerifyUserIsConnectedToExpectedCountry(COUNTRY_NAME);
    }

    [Test]
    [Property("TestCaseId", "602374")]
    public void DisconnectFromSecureCoreServerViaCountriesList()
    {
        SidebarRobot
            .NavigateToSecureCoreCountriesTab()
            .ExpandCities(COUNTRY_NAME);
        ConnectToSecureCore(VIA_COUNTRY_SWEDEN);
        SidebarRobot
            .DisconnectViaCountry(COUNTRY_NAME);
        HomeRobot
            .Verify.IsDisconnected();
        NetworkUtils.VerifyIpAddressMatchesWithRetry(_ipAddressNotConnected);

        ConnectToSecureCore(VIA_COUNTRY_ICELAND);
        SidebarRobot
            .DisconnectViaSecureCore(COUNTRY_NAME, VIA_COUNTRY_ICELAND);
        HomeRobot
            .Verify.IsDisconnected();
        NetworkUtils.VerifyIpAddressMatchesWithRetry(_ipAddressNotConnected);
    }

    [Test]
    [Property("TestCaseId", "602372,602373")]
    [Category("ARM")]
    [Category("SMOKE_1")]
    public void QuickConnectToSecureCoreServerAndDisconnect()
    {
        AddConnectionInRecents();

        HomeRobot
            .SelectDefaultConnectionCountry(COUNTRY_NAME, TestConstants.ViaPrefix + VIA_COUNTRY_ICELAND.GetName())
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
    [Property("TestCaseId", "602430,602431")]
    [Retry(3)]
    public void ConnectToSecureCoreServerViaProfilesAndDisconnect()
    {
        CreateSecureCoreProfile();

        SidebarRobot
            .ConnectToProfile(PROFILE_NAME);
        HomeRobot
            .Verify.IsConnected()
                   .ConnectionCardTitleEquals(PROFILE_NAME)
                   .ConnectionCardDescriptionContains($"{COUNTRY_NAME.GetName()} {TestConstants.ViaPrefix}{VIA_COUNTRY_SWEDEN.GetName()}");
        NetworkUtils.VerifyUserIsConnectedToExpectedCountry(COUNTRY_NAME);

        SidebarRobot
            .Verify.IsDisconnectButtonOnHoverDisplayed(PROFILE_NAME)
                   .IsGreenDotDisplayed(PROFILE_NAME)
            .DisconnectViaProfile(PROFILE_NAME);
        HomeRobot
            .Verify.IsDisconnected();

        NetworkUtils.VerifyIpAddressMatchesWithRetry(_ipAddressNotConnected);
    }

    [Test]
    [Property("TestCaseId", "602432,602433")]
    public void ConnectToSecureCoreServerViaRecentsAndDisconnect()
    {
        AddConnectionInRecents();

        SidebarRobot
            .NavigateToRecents()
            .ConnectViaSecureCore(COUNTRY_NAME, VIA_COUNTRY_ICELAND);
        HomeRobot
            .Verify.IsConnected()
                   .ConnectionCardTitleEquals(COUNTRY_NAME.GetName())
                   .ConnectionCardDescriptionContains(VIA_COUNTRY_ICELAND.GetName());
        NetworkUtils.VerifyUserIsConnectedToExpectedCountry(COUNTRY_NAME);

        SidebarRobot
            .Verify.IsDisconnectButtonOnHoverDisplayed(COUNTRY_NAME.GetName())
                   .IsGreenDotDisplayed(COUNTRY_NAME.GetName())
            .DisconnectViaSecureCore(COUNTRY_NAME, VIA_COUNTRY_ICELAND);
        HomeRobot
            .Verify.IsDisconnected();

        NetworkUtils.VerifyIpAddressMatchesWithRetry(_ipAddressNotConnected);
    }

    private void CreateSecureCoreProfile()
    {
        SidebarRobot
            .NavigateToProfiles()
            .ClickCreateProfile();

        ProfileRobot
            .SetProfileName(PROFILE_NAME)
            .SelectConnectionType(ConnectionType.SecureCore)
            .SelectCountry(COUNTRY_NAME)
            .SelectMiddleCountry(VIA_COUNTRY_SWEDEN)
            .SaveProfile();
    }

    private void AddConnectionInRecents()
    {
        SidebarRobot
            .NavigateToSecureCoreCountriesTab()
            .ExpandCities(COUNTRY_NAME)
            .ConnectViaSecureCore(COUNTRY_NAME, VIA_COUNTRY_ICELAND);
        HomeRobot
            .Verify.IsConnected()
            .Disconnect()
            .Verify.IsDisconnected();
    }

    private void ConnectToSecureCore(Country viaCountry)
    {
        SidebarRobot
            .ConnectViaSecureCore(COUNTRY_NAME, viaCountry);
        HomeRobot
            .Verify.IsConnected();
    }
}