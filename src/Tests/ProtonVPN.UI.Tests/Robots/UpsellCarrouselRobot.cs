﻿/*
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

namespace ProtonVPN.UI.Tests.Robots;
public class UpsellCarrouselRobot
{
    protected Element UpgradeButton = Element.ByName("Upgrade");
    protected Element CloseButton = Element.ByAutomationId("Close");
    protected Element NextUpsellButton = Element.ByAutomationId("MoveToNextUpsellFeatureButton");
    protected Element BackUpsellButton = Element.ByAutomationId("MoveToPreviousUpsellFeatureButton");
    protected Element ServersUpsellDescription = Element.ByName("Open source VPN");
    protected Element SpeedUpsellTitle = Element.ByName("Browse at even higher speeds (up to 10 Gbps)");
    protected Element StreamingUpsellTitles = Element.ByName("Stream your favorite movies and TV shows");
    protected Element NetshieldUpsellTitle = Element.ByName("Enjoy ad-free browsing");
    protected Element SecureCoreUpsellTitle = Element.ByName("Add another layer of protection to your connection");
    protected Element P2pUpsellTitle = Element.ByName("Unlock peer-to-peer downloads and file sharing (P2P)");
    protected Element TenDevicesUpsellTitle = Element.ByName("Connect up to 10 devices at once");
    protected Element TorUpsellTitle = Element.ByName("Access the Tor network for extra privacy");
    protected Element SplitTunnelingUpsellTitle = Element.ByName("Get the best of both worlds");
    protected Element ProfilesUpsellTitle = Element.ByName("Get quick access to frequent connections");
    protected Element AdvancedSettingsUpsellTitle = Element.ByName("Unlock advanced VPN customization");

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
        public Verifications ServersUpsellIsDisplayed()
        {
            ServersUpsellDescription.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications ServersSpeedUpsellIsDisplayed()
        {
            SpeedUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications StreamingUpsellIsDisplayed()
        {
            StreamingUpsellTitles.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications NetshieldUpsellIsDisplayed()
        {
            NetshieldUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications SecureCoreUpsellIsDisplayed()
        {
            SecureCoreUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications P2pUpsellIsDisplayed()
        {
            P2pUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications TenDevicesUpsellIsDisplayed()
        {
            TenDevicesUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications TorUpsellIsDisplayed()
        {
            TorUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications SplitTunnelingUpsellIsDisplayed()
        {
            SplitTunnelingUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications ProfilesUpsellIsDisplayed()
        {
            ProfilesUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications AdvancedSettingsUpsellIsDisplayed()
        {
            AdvancedSettingsUpsellTitle.WaitUntilDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }

        public Verifications UpgradeButtonIsDisplayed()
        {
            UpgradeButtonIsDisplayed();
            UpgradeButton.WaitUntilDisplayed();
            return this;
        }
    }

    public Verifications Verify => new();
}
