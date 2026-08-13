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

using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using ProtonVPN.UI.Tests.Enums;
using ProtonVPN.UI.Tests.Robots;
using ProtonVPN.UI.Tests.TestBase;
using ProtonVPN.UI.Tests.TestsHelper;
using ProtonVPN.UI.Tests.TestsHelper.TestData;
using ProtonVPN.UI.Tests.TestsHelper.UiFlows;
using ProtonVPN.UI.Tests.UiTools;
using static ProtonVPN.UI.Tests.TestsHelper.TestConstants;

namespace ProtonVPN.UI.Tests.Tests.E2ETests;

[TestFixture]
[Category("3")]
[Category("ARM")]
public class CustomDnsTests : FreshSessionSetUp
{
    private const string FIRST_CUSTOM_DNS_SERVER = "8.8.8.8";
    private const string SECOND_CUSTOM_DNS_SERVER = "1.1.1.1";
    private const string NEW_CUSTOM_DNS_SERVER = "22.33.0.5";

    private const string QUAD9_DNS_SERVER = "9.9.9.9";
    private const string ALTERNATE_DNS_SERVER = "76.76.19.19";
    private const string OPENDNS_DNS_SERVER = "208.67.222.222";

    private static readonly string _enableCustomDnsTitle = LanguageHelper.GetTranslatedString("Settings_Connection_Advanced_CustomDnsServers_Conflict_Title");
    private static readonly string _enableCustomDnsDescription = LanguageHelper.GetTranslatedString("Settings_Connection_Advanced_CustomDnsServers_Conflict_Description");
    private static readonly string _enableCustomDnsButton = LanguageHelper.GetTranslatedString("Common_Actions_Enable");

    private static readonly string _unsavedChangesTitle = LanguageHelper.GetTranslatedString("Settings_DiscardChanges_Confirmation_Title");
    private static readonly string _unsavedChangesDiscardButton = LanguageHelper.GetTranslatedString("Settings_DiscardChanges_Confirmation_Action");
    private static readonly string _unsavedChangesKeepEditingButton = LanguageHelper.GetTranslatedString("Settings_DiscardChanges_Confirmation_Cancel");

    private static readonly string _ipAlreadyExistsError = LanguageHelper.GetTranslatedString("Settings_Common_IpAddresses_AlreadyExists");
    private static readonly string _invalidIpError = LanguageHelper.GetTranslatedString("Settings_Common_IpAddresses_Invalid");

    private static readonly (IpSelectorAction Action, string Ip)[] _scenarios =
        [
            (Action: IpSelectorAction.Add, Ip: NEW_CUSTOM_DNS_SERVER),
            (Action: IpSelectorAction.Remove, Ip: SECOND_CUSTOM_DNS_SERVER),
            (Action: IpSelectorAction.Tick, Ip: FIRST_CUSTOM_DNS_SERVER)
        ];

    [SetUp]
    public void SetUp()
    {
        CommonUiFlows.FullLogin(TestUserData.PlusUser);
    }

    [Test, Order(0)]
    [Property("TestCaseId", "602403")]
    [Category("SMOKE_3")]
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
                   .OverlayTextContains(_enableCustomDnsTitle)
                   .OverlayTextContains(_enableCustomDnsDescription)
                   .OverlayButtonsEquals(primary: _enableCustomDnsButton)
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .ApplySettings()
            .Verify.IsNetshieldDisabledStateDisplayed()
            .CloseSettings();
    }

    [Test, Order(1)]
    [Property("TestCaseId", "602404")]
    [Category("SMOKE_3")]
    public void CustomDnsIsSet()
    {
        TurnOnDns();

        AddCustomDnsServers([FIRST_CUSTOM_DNS_SERVER, SECOND_CUSTOM_DNS_SERVER]);

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
        TurnOnDns();

        AdvancedSettingsRobot
            .EditCustomDnsServers();

        IpSelectorRobot
            .AddIpAddress(FIRST_CUSTOM_DNS_SERVER)
            .AddIpAddress(SECOND_CUSTOM_DNS_SERVER)
            .TickIpAddressCheckBox(FIRST_CUSTOM_DNS_SERVER);
        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        SettingRobot
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        DnsHelper.IsCustomDnsAddressNotSet(FIRST_CUSTOM_DNS_SERVER);
        DnsHelper.IsCustomDnsAddressSet(SECOND_CUSTOM_DNS_SERVER);
    }

    [Test, Order(3)]
    [Property("TestCaseId", "863611")]
    [TestCaseSource(typeof(TestConstants), nameof(AllNonProTunProtocols))]
    public void CustomDnsUsingDifferentProtocols(Protocol protocol)
    {
        TurnOnDns();
        AddCustomDnsServers([SECOND_CUSTOM_DNS_SERVER]);
        PerformProtocolTest(protocol);
    }

    [Test, Order(4)]
    [Property("TestCaseId", "863616")]
    [TestCaseSource(typeof(TestConstants), nameof(ProTunProtocols))]
    public void CustomDnsUsingDifferentProTunProtocols(Protocol protocol)
    {
        TurnOnDns();
        AddCustomDnsServers([SECOND_CUSTOM_DNS_SERVER]);
        PerformProtocolTest(protocol, shouldEnableProTun: true);
    }

    [Test, Order(5)]
    [Property("TestCaseId", "610989")]
    public void ReconnectionRequiredAfterUpdatingTheCustomDnsConfiguration()
    {
        TurnOnDns();
        AddCustomDnsServers([FIRST_CUSTOM_DNS_SERVER, SECOND_CUSTOM_DNS_SERVER]);

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
                .OverlayTextContains(_unsavedChangesTitle)
                .OverlayButtonsEquals(
                    primary: _unsavedChangesDiscardButton,
                    cancel: _unsavedChangesKeepEditingButton)
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
        TurnOnDns();
        AddCustomDnsServers([FIRST_CUSTOM_DNS_SERVER, SECOND_CUSTOM_DNS_SERVER]);
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
                   .CustomDnsDoesNotContainIpAddress(FIRST_CUSTOM_DNS_SERVER)
                   .CustomDnsDoesNotContainIpAddress(SECOND_CUSTOM_DNS_SERVER);

        SettingRobot
            .Verify.IsApplyBtnDisplayed()
            .CloseSettings();

        ConfirmationRobot
            .Verify.IsOverlayDisplayed()
            .OverlayTextContains(_unsavedChangesTitle)
            .OverlayButtonsEquals(
                primary: _unsavedChangesDiscardButton,
                cancel: _unsavedChangesKeepEditingButton)
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        NavigateToCustomDnsSetting();

        AdvancedSettingsRobot
            .Verify.CustomDnsContainsIpAddress(SECOND_CUSTOM_DNS_SERVER)
                   .CustomDnsContainsIpAddress(FIRST_CUSTOM_DNS_SERVER)
                   .CustomDnsDoesNotContainIpAddress(NEW_CUSTOM_DNS_SERVER);
    }

    [Test, Order(7)]
    [Property("TestCaseId", "760743")]
    public void ReorderingCustomDnsServers()
    {
        TurnOnDns();
        AddCustomDnsServers([QUAD9_DNS_SERVER, ALTERNATE_DNS_SERVER, OPENDNS_DNS_SERVER]);

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        DnsHelper.IsCustomDnsAddressSet(QUAD9_DNS_SERVER, order: 0);
        DnsHelper.IsCustomDnsAddressSet(ALTERNATE_DNS_SERVER, order: 1);
        DnsHelper.IsCustomDnsAddressSet(OPENDNS_DNS_SERVER, order: 2);

        NavigateToCustomDnsSetting();

        AdvancedSettingsRobot
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
        TurnOnDns();
        AddCustomDnsServers([ALTERNATE_DNS_SERVER, QUAD9_DNS_SERVER, OPENDNS_DNS_SERVER]);

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

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
        TurnOnDns();

        AdvancedSettingsRobot
            .EditCustomDnsServers();
        IpSelectorRobot
            .AddIpAddress(QUAD9_DNS_SERVER);
        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();

        AdvancedSettingsRobot
            .DisableCustomDnsToggle();

        SettingRobot
            .ApplySettings()
            .CloseSettings();

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected();

        DnsHelper.IsCustomDnsAddressNotSet(QUAD9_DNS_SERVER);

        CommonUiFlows.EnsureUserIsDisconnected();
    }

    [Test]
    [Property("TestCaseId", "890302")]
    public void CustomDnsShowsErrorOnSameIp()
    {
        TurnOnDns();

        AdvancedSettingsRobot
            .EditCustomDnsServers();
        IpSelectorRobot
            .AddIpAddress(FIRST_CUSTOM_DNS_SERVER)
            .AddIpAddress(FIRST_CUSTOM_DNS_SERVER)
            .Verify.IsErrorMessageDisplayed(_ipAlreadyExistsError);
        ConfirmationRobot
            .CancelAction()
            .Verify.IsOverlayClosed();
    }

    [Test]
    [Property("TestCaseId", "890834")]
    [Ignore("JIRA - VPNWIN-3343")]
    public void CustomDnsWithMaxChars()
    {
        TurnOnDns();

        AdvancedSettingsRobot
            .EditCustomDnsServers();

        try
        {

            (Point Position, Size Size) elementBeforeTyping = UiActions.GetElementSizeAndPosition(IpSelectorRobot.IpAddressTextBox);

            UiActions.VerifyElementSizeAndPosition(IpSelectorRobot.IpAddressTextBox, elementBeforeTyping, () =>
                IpSelectorRobot
                    .AddIpAddress(InputTestData.LongInput)
            );
            IpSelectorRobot.Verify.IsErrorMessageDisplayed(_invalidIpError);
        }
        finally
        {
            ConfirmationRobot
                .CancelAction()
                .Verify.IsOverlayClosed();
        }
    }

    private static void PerformProtocolTest(Protocol protocol, bool shouldEnableProTun = false)
    {
        CommonUiFlows.EnsureUserIsDisconnected();
        CommonUiFlows.ChangeProtocol(protocol, shouldEnableProTun);

        HomeRobot
            .ConnectViaConnectionCard()
            .Verify.IsConnected()
                   .IsProtocolDisplayed(protocol);

        DnsHelper.IsCustomDnsAddressSet(SECOND_CUSTOM_DNS_SERVER);

        HomeRobot
            .Disconnect()
            .Verify.IsDisconnected();
    }

    private static void TurnOnDns()
    {
        NavigateToCustomDnsSetting();
        AdvancedSettingsRobot
            .EnableCustomDnsToggle();
        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();
    }

    private static void AddCustomDnsServers(List<string> dnsAddresses)
    {
        AdvancedSettingsRobot
            .EditCustomDnsServers();

        foreach (string Ip in dnsAddresses)
        {
            IpSelectorRobot
                .AddIpAddress(Ip)
                .Verify.WasIpAdded(Ip);
        }

        ConfirmationRobot
            .PrimaryAction()
            .Verify.IsOverlayClosed();
        SettingRobot
            .ApplySettings()
            .CloseSettings();
    }

    private static void NavigateToCustomDnsSetting()
    {
        SettingRobot
            .OpenSettings()
            .OpenAdvancedSettings();
        AdvancedSettingsRobot
            .NavigateToCustomDns();
    }
}