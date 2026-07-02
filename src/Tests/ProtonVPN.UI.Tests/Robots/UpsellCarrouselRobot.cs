/*
 * Copyright (c) 2024 Proton AG
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

using ProtonVPN.UI.Tests.UiTools;
using ProtonVPN.UI.Tests.TestsHelper;

namespace ProtonVPN.UI.Tests.Robots;
public class UpsellCarrouselRobot
{
    private static readonly string _upgradeButtonTranslated = LanguageHelper.GetTranslatedString("Common_Actions_Upgrade");
    private static readonly string _serversUpsellDescriptionTranslated = LanguageHelper.GetTranslatedString("Upsell_Carousel_WorldwideCoverage_Selection");
    private static readonly string _speedUpsellTitleTranslated = LanguageHelper.GetTranslatedString("Upsell_Carousel_Speed");
    private static readonly string _streamingUpsellTitleTranslated = LanguageHelper.GetTranslatedString("Upsell_Carousel_Streaming");
    private static readonly string _netshieldUpsellTitleTranslated = LanguageHelper.GetTranslatedString("Upsell_Carousel_NetShield");
    private static readonly string _secureCoreUpsellTitleTranslated = LanguageHelper.GetTranslatedString("Upsell_Carousel_SecureCore");
    private static readonly string _p2pUpsellTitleTranslated = LanguageHelper.GetTranslatedString("Upsell_Carousel_P2P");
    private static readonly string _p2pTorrentInProgressUpsellTitleTranslated = LanguageHelper.GetTranslatedString("Dialogs_P2PDetection_Title");
    private static readonly string _streamingInProgressUpsellTitleTranslated = LanguageHelper.GetTranslatedString("Dialogs_StreamingWarning_Title");
    private static readonly string _tenDevicesUpsellTitleTranslated = LanguageHelper.GetTranslatedString("Upsell_Carousel_MultipleDevices");
    private static readonly string _torUpsellTitleTranslated = LanguageHelper.GetTranslatedString("Upsell_Carousel_Tor");
    private static readonly string _splitTunnelingUpsellTitleTranslated = LanguageHelper.GetTranslatedString("Upsell_Carousel_SplitTunneling");
    private static readonly string _profilesUpsellTitleTranslated = LanguageHelper.GetTranslatedString("Upsell_Carousel_Profiles");
    private static readonly string _advancedSettingsUpsellTitleTranslated = LanguageHelper.GetTranslatedString("Upsell_Carousel_AdvancedSettings");

    protected Element UpgradeButton = Element.ByName(_upgradeButtonTranslated);
    protected Element ServersUpsellDescription = Element.ByName(_serversUpsellDescriptionTranslated);
    protected Element SpeedUpsellTitle = Element.ByName(_speedUpsellTitleTranslated);
    protected Element StreamingUpsellTitle = Element.ByName(_streamingUpsellTitleTranslated);
    protected Element NetshieldUpsellTitle = Element.ByName(_netshieldUpsellTitleTranslated);
    protected Element SecureCoreUpsellTitle = Element.ByName(_secureCoreUpsellTitleTranslated);
    protected Element P2PUpsellTitle = Element.ByName(_p2pUpsellTitleTranslated);
    protected Element P2PTorrentInProgressUpsellTitle = Element.ByName(_p2pTorrentInProgressUpsellTitleTranslated);
    protected Element StreamingInProgressUpsellTitle = Element.ByName(_streamingInProgressUpsellTitleTranslated);
    protected Element TenDevicesUpsellTitle = Element.ByName(_tenDevicesUpsellTitleTranslated);
    protected Element TorUpsellTitle = Element.ByName(_torUpsellTitleTranslated);
    protected Element SplitTunnelingUpsellTitle = Element.ByName(_splitTunnelingUpsellTitleTranslated);
    protected Element ProfilesUpsellTitle = Element.ByName(_profilesUpsellTitleTranslated);
    protected Element AdvancedSettingsUpsellTitle = Element.ByName(_advancedSettingsUpsellTitleTranslated);

    protected Element CloseButton = Element.ByAutomationId("Close");
    protected Element NextUpsellButton = Element.ByAutomationId("MoveToNextUpsellFeatureButton");
    protected Element BackUpsellButton = Element.ByAutomationId("MoveToPreviousUpsellFeatureButton");

    public UpsellCarrouselRobot NextUpsell()
    {
        NextUpsellButton.Click();
        return this;
    }

    public UpsellCarrouselRobot GoBackUpsell()
    {
        BackUpsellButton.Click();
        return this;
    }

    public UpsellCarrouselRobot CloseModal()
    {
        CloseButton.Click(); 
        return this;
    }

    public class Verifications : UpsellCarrouselRobot
    {
        public Verifications IsServersUpsellDisplayed()
        {
            ServersUpsellDescription.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsServersSpeedUpsellDisplayed()
        {
            SpeedUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsStreamingUpsellDisplayed()
        {
            StreamingUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsNetshieldUpsellDisplayed()
        {
            NetshieldUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsSecureCoreUpsellDisplayed()
        {
            SecureCoreUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsP2PUpsellDisplayed()
        {
            P2PUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsP2PTorrentInProgressUpsellDisplayed()
        {
            P2PTorrentInProgressUpsellTitle.WaitUntilDisplayed(TestConstants.ThirtySecondsTimeout);
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsStreamingInProgressUpsellDisplayed()
        {
            StreamingInProgressUpsellTitle.WaitUntilDisplayed(TestConstants.ThirtySecondsTimeout);
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsTenDevicesUpsellDisplayed()
        {
            TenDevicesUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsTorUpsellDisplayed()
        {
            TorUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsSplitTunnelingUpsellDisplayed()
        {
            SplitTunnelingUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsProfilesUpsellDisplayed()
        {
            ProfilesUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsAdvancedSettingsUpsellDisplayed()
        {
            AdvancedSettingsUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications IsUpgradeButtonDisplayed()
        {
            IsUpgradeButtonDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }
    }

    public Verifications Verify => new();
}
