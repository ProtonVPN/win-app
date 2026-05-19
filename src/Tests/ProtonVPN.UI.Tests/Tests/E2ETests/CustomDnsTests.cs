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
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
[Category("ARM")]
public class CustomDnsTests : BaseTest
{
    private const string ENABLE_CUSTOM_DNS_TITLE = "Enable custom DNS servers?";
    private const string ENABLE_CUSTOM_DNS_DESCRIPTION = "You won't be able to use NetShield when connected with a custom DNS server.";
    private const string ENABLE_CUSTOM_DNS_BUTTON = "Enable";

    private const string UNSAVED_CHANGES_TITLE = "Discard unsaved changes?";
    private const string UNSAVED_CHANGES_DISCARD_BUTTON = "Discard changes";
    private const string UNSAVED_CHANGES_KEEP_EDITING_BUTTON = "Keep editing";

    private const string FIRST_CUSTOM_DNS_SERVER = "8.8.8.8";
    private const string SECOND_CUSTOM_DNS_SERVER = "1.1.1.1";
    private const string NEW_CUSTOM_DNS_SERVER = "22.33.0.5";

    private const string QUAD9_DNS_SERVER = "9.9.9.9";
    private const string ALTERNATE_DNS_SERVER = "76.76.19.19";
    private const string OPENDNS_DNS_SERVER = "208.67.222.222";

    private static readonly (IpSelectorAction Action, string Ip)[] _scenarios =
        [
            (Action: IpSelectorAction.Add, Ip: NEW_CUSTOM_DNS_SERVER),
            (Action: IpSelectorAction.Remove, Ip: SECOND_CUSTOM_DNS_SERVER),
            (Action: IpSelectorAction.Tick, Ip: FIRST_CUSTOM_DNS_SERVER)
        ];

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        LaunchApp();
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test, Order(0)]
    [Property("TestCaseId", "602403")]
    public void NetshieldIsDisabledWhenCustomDnsIsEnabled()
    {
        SettingRobot
            .OpenSettings()
            .Verify.IsNetshieldEnabledStateDisplayed()
            .OpenAdvancedSettings();

        AdvancedSettingsRobot
            .NavigateToCustomDns()
            .EnableCustomDnsToggle();

        ConfirmationRobot
            .Verify.IsOverlayDisplayed()
                   .OverlayTextContains(ENABLE_CUSTOM_DNS_TITLE)
                   .OverlayTextContains(ENABLE_CUSTOM_DNS_DESCRIPTION)
                   .OverlayButtonsEquals(primary: ENABLE_CUSTOM_DNS_BUTTON)
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .ApplySettings()
            .Verify.IsNetshieldDisabledStateDisplayed()
            .CloseSettings();
    }

    [Test, Order(1)]
    [Property("TestCaseId", "602404")]
    public void CustomDnsIsSet()
    {
        NavigateToCustomDnsSetting();

        AdvancedSettingsRobot
            .EditCustomDnsServers();

        IpSelectorRobot
            .AddIpAddress(FIRST_CUSTOM_DNS_SERVER)
            .AddIpAddress(SECOND_CUSTOM_DNS_SERVER)
            .Verify.WasIpAdded(FIRST_CUSTOM_DNS_SERVER)
                   .WasIpAdded(SECOND_CUSTOM_DNS_SERVER);
        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        DnsHelper.IsCustomDnsAddressSet(FIRST_CUSTOM_DNS_SERVER, order: 0);
        DnsHelper.IsCustomDnsAddressSet(SECOND_CUSTOM_DNS_SERVER, order: 1);
    }

    [Test, Order(2)]
    [Property("TestCaseId", "602405")]
    public void CustomDnsIsDisabledByTickingCheckBox()
    {
        NavigateToCustomDnsSetting();

        AdvancedSettingsRobot
            .EditCustomDnsServers();

        IpSelectorRobot
            .TickIpAddressCheckBox(FIRST_CUSTOM_DNS_SERVER);
        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .Reconnect();

        HomeRobot
            .Verify.IsConnected();

        DnsHelper.IsCustomDnsAddressNotSet(FIRST_CUSTOM_DNS_SERVER);
        DnsHelper.IsCustomDnsAddressSet(SECOND_CUSTOM_DNS_SERVER);
    }

    [Test, Order(3)]
    [Property("TestCaseId", "863611")]
    [TestCaseSource(typeof(TestConstants), nameof(AllProtocols))]
    public void CustomDnsUsingDifferentProtocols(Protocol protocol)
    {
        PerformProtocolTest(protocol);
    }

    [Test, Order(4)]
    [Property("TestCaseId", "863616")]
    [TestCaseSource(typeof(TestConstants), nameof(WireGuardProtocols))]
    public void CustomDnsUsingDifferentProTunProtocols(Protocol protocol)
    {
        PerformProtocolTest(protocol, shouldEnableProTun: true);
    }

    [Test, Order(5)]
    [Property("TestCaseId", "610989")]
    public void ReconnectionRequiredAfterUpdatingTheCustomDnsConfiguration()
    {
        // Pre-condition: FIRST_CUSTOM_DNS_SERVER is added and disabled, SECOND_CUSTOM_DNS_SERVER is added and enabled
        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        NavigateToCustomDnsSetting();

        foreach ((IpSelectorAction Action, string Ip) in _scenarios)
        {
            AdvancedSettingsRobot
                .EditCustomDnsServers();

            switch (Action)
            {
                case IpSelectorAction.Add:
                    IpSelectorRobot.AddIpAddress(Ip);
                    break;
                case IpSelectorAction.Remove:
                    IpSelectorRobot.RemoveIp(Ip);
                    break;
                case IpSelectorAction.Tick:
                    IpSelectorRobot.TickIpAddressCheckBox(Ip);
                    break;
            }

            ConfirmationRobot
                .PrimaryAction()
                .Verify.IsOverlayClosed();

            SettingRobot
                .Verify.IsReconnectBtnDisplayed()
                .GoBack();

            ConfirmationRobot
                .Verify.IsOverlayDisplayed()
                .OverlayTextContains(UNSAVED_CHANGES_TITLE)
                .OverlayButtonsEquals(
                    primary: UNSAVED_CHANGES_DISCARD_BUTTON,
                    cancel: UNSAVED_CHANGES_KEEP_EDITING_BUTTON)
                .PrimaryAction()
                .Verify.IsOverlayClosed();

            AdvancedSettingsRobot.NavigateToCustomDns();
        }

        SettingRobot
            .CloseSettings();
        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();
    }

    [Test, Order(6)]
    [Property("TestCaseId", "610990")]
    public void DiscardCustomDnsConfiguration()
    {
        NavigateToCustomDnsSetting();

        AdvancedSettingsRobot
            .EditCustomDnsServers();

        IpSelectorRobot
            .AddIpAddress(NEW_CUSTOM_DNS_SERVER)
            .TickIpAddressCheckBox(FIRST_CUSTOM_DNS_SERVER)
            .RemoveIp(SECOND_CUSTOM_DNS_SERVER);
        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        AdvancedSettingsRobot
            .Verify.CustomDnsContainsIpAddress(NEW_CUSTOM_DNS_SERVER)
                   .CustomDnsContainsIpAddress(FIRST_CUSTOM_DNS_SERVER)
                   .CustomDnsDoesNotContainIpAddress(SECOND_CUSTOM_DNS_SERVER);

        SettingRobot
            .Verify.IsApplyBtnDisplayed()
            .CloseSettings();

        ConfirmationRobot
            .Verify.IsOverlayDisplayed()
            .OverlayTextContains(UNSAVED_CHANGES_TITLE)
            .OverlayButtonsEquals(
                primary: UNSAVED_CHANGES_DISCARD_BUTTON,
                cancel: UNSAVED_CHANGES_KEEP_EDITING_BUTTON)
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .OpenSettings()
            .OpenAdvancedSettings();
        AdvancedSettingsRobot
            .NavigateToCustomDns()
            .Verify.CustomDnsContainsIpAddress(SECOND_CUSTOM_DNS_SERVER)
                   .CustomDnsDoesNotContainIpAddress(FIRST_CUSTOM_DNS_SERVER)
                   .CustomDnsDoesNotContainIpAddress(NEW_CUSTOM_DNS_SERVER);
    }

    [Test, Order(7)]
    [Property("TestCaseId", "760743")]
    public void ReorderingCustomDnsServers()
    {
        AdvancedSettingsRobot
            .EditCustomDnsServers();

        IpSelectorRobot
            .RemoveAllIps()
            .AddIpAddress(QUAD9_DNS_SERVER)
            .AddIpAddress(ALTERNATE_DNS_SERVER)
            .AddIpAddress(OPENDNS_DNS_SERVER);

        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        DnsHelper.IsCustomDnsAddressSet(QUAD9_DNS_SERVER, order: 0);
        DnsHelper.IsCustomDnsAddressSet(ALTERNATE_DNS_SERVER, order: 1);
        DnsHelper.IsCustomDnsAddressSet(OPENDNS_DNS_SERVER, order: 2);

        SettingRobot
            .OpenSettings()
            .OpenAdvancedSettings();

        AdvancedSettingsRobot
            .NavigateToCustomDns()
            .EditCustomDnsServers();

        IpSelectorRobot
            .ReorderIpAddress(QUAD9_DNS_SERVER, IpOrderDirection.Down)
            .ReorderIpAddress(OPENDNS_DNS_SERVER, IpOrderDirection.Up);

        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .Reconnect();

        DnsHelper.IsCustomDnsAddressSet(ALTERNATE_DNS_SERVER, order: 0);
        DnsHelper.IsCustomDnsAddressSet(OPENDNS_DNS_SERVER, order: 1);
        DnsHelper.IsCustomDnsAddressSet(QUAD9_DNS_SERVER, order: 2);
    }

    [Test, Order(8)]
    [Property("TestCaseId", "602406")]
    public void CustomDnsServerRemoval()
    {
        NavigateToCustomDnsSetting();

        AdvancedSettingsRobot
            .EditCustomDnsServers();

        IpSelectorRobot
            .RemoveIp(OPENDNS_DNS_SERVER);
        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .Reconnect();

        HomeRobot
            .Verify.IsConnected();

        DnsHelper.IsCustomDnsAddressNotSet(OPENDNS_DNS_SERVER);
        DnsHelper.IsCustomDnsAddressSet(ALTERNATE_DNS_SERVER, order: 0);
        DnsHelper.IsCustomDnsAddressSet(QUAD9_DNS_SERVER, order: 1);
    }

    [Test, Order(9)]
    [Property("TestCaseId", "602407")]
    public void DisablingCustomDnsRemovesDnsServers()
    {
        NavigateToCustomDnsSetting();

        AdvancedSettingsRobot
            .DisableCustomDnsToggle();

        SettingRobot
            .Reconnect();

        HomeRobot
            .Verify.IsConnected();

        DnsHelper.IsCustomDnsAddressNotSet(OPENDNS_DNS_SERVER);
        DnsHelper.IsCustomDnsAddressNotSet(ALTERNATE_DNS_SERVER);
        DnsHelper.IsCustomDnsAddressNotSet(QUAD9_DNS_SERVER);

        CommonUiFlows.EnsureUserIsDisconnected();
    }

    private static void PerformProtocolTest(Protocol protocol, bool shouldEnableProTun = false)
    {
        CommonUiFlows.EnsureUserIsDisconnected();
        CommonUiFlows.ChangeProtocol(protocol, shouldEnableProTun);

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
                   .IsProtocolDisplayed(protocol, shouldEnableProTun);

        DnsHelper.IsCustomDnsAddressSet(SECOND_CUSTOM_DNS_SERVER);

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();
    }

    private static void NavigateToCustomDnsSetting()
    {
        SettingRobot
            .OpenSettings()
            .OpenAdvancedSettings();

        AdvancedSettingsRobot
            .NavigateToCustomDns();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Cleanup();
    }
}