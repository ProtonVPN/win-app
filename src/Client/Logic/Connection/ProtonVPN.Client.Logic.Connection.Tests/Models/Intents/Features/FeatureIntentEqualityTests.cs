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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;

namespace ProtonVPN.Client.Logic.Connection.Tests.Models.Intents.Features;

[TestClass]
public class FeatureIntentEqualityTests
{
    [TestMethod]
    public void FeatureIntent_ShouldBeEqual_GivenSameReference()
    {
        IFeatureIntent featureFastestSecureCore = new SecureCoreFeatureIntent();
        IFeatureIntent featureSecureCore = new SecureCoreFeatureIntent("SE");
        IFeatureIntent featureP2P = new P2PFeatureIntent();
        IFeatureIntent featureTor = new TorFeatureIntent();

        Assert.IsTrue(featureFastestSecureCore.IsSameAs(featureFastestSecureCore));
        Assert.IsTrue(featureSecureCore.IsSameAs(featureSecureCore));
        Assert.IsTrue(featureP2P.IsSameAs(featureP2P));
        Assert.IsTrue(featureTor.IsSameAs(featureTor));
    }

    [TestMethod]
    public void FeatureIntent_ShouldBeEqual_GivenSameIntent()
    {
        IFeatureIntent featureFastestSecureCoreA = new SecureCoreFeatureIntent();
        IFeatureIntent featureFastestSecureCoreB = new SecureCoreFeatureIntent();
        IFeatureIntent featureSecureCoreA = new SecureCoreFeatureIntent("SE");
        IFeatureIntent featureSecureCoreB = new SecureCoreFeatureIntent("SE");
        IFeatureIntent featureP2PA = new P2PFeatureIntent();
        IFeatureIntent featureP2PB = new P2PFeatureIntent();
        IFeatureIntent featureTorA = new TorFeatureIntent();
        IFeatureIntent featureTorB = new TorFeatureIntent();

        Assert.IsTrue(featureFastestSecureCoreA.IsSameAs(featureFastestSecureCoreB));
        Assert.IsTrue(featureFastestSecureCoreB.IsSameAs(featureFastestSecureCoreA));
        Assert.IsTrue(featureSecureCoreA.IsSameAs(featureSecureCoreB));
        Assert.IsTrue(featureSecureCoreB.IsSameAs(featureSecureCoreA));
        Assert.IsTrue(featureP2PA.IsSameAs(featureP2PB));
        Assert.IsTrue(featureP2PB.IsSameAs(featureP2PA));
        Assert.IsTrue(featureTorA.IsSameAs(featureTorB));
        Assert.IsTrue(featureTorB.IsSameAs(featureTorA));
    }

    [TestMethod]
    public void FeatureIntent_ShouldNotBeEqual_GivenDifferentIntent()
    {
        IFeatureIntent featureFastestSecureCore = new SecureCoreFeatureIntent();
        IFeatureIntent featureSecureCoreA = new SecureCoreFeatureIntent("SE");
        IFeatureIntent featureSecureCoreB = new SecureCoreFeatureIntent("CH");
        IFeatureIntent featureP2P = new P2PFeatureIntent();
        IFeatureIntent featureTor = new TorFeatureIntent();

        Assert.IsFalse(featureFastestSecureCore.IsSameAs(featureSecureCoreA));
        Assert.IsFalse(featureFastestSecureCore.IsSameAs(featureSecureCoreB));
        Assert.IsFalse(featureFastestSecureCore.IsSameAs(featureP2P));
        Assert.IsFalse(featureFastestSecureCore.IsSameAs(featureTor));
        Assert.IsFalse(featureSecureCoreA.IsSameAs(featureFastestSecureCore));
        Assert.IsFalse(featureSecureCoreA.IsSameAs(featureSecureCoreB));
        Assert.IsFalse(featureSecureCoreA.IsSameAs(featureP2P));
        Assert.IsFalse(featureSecureCoreA.IsSameAs(featureTor));
        Assert.IsFalse(featureSecureCoreB.IsSameAs(featureFastestSecureCore));
        Assert.IsFalse(featureSecureCoreB.IsSameAs(featureSecureCoreA));
        Assert.IsFalse(featureSecureCoreB.IsSameAs(featureP2P));
        Assert.IsFalse(featureSecureCoreB.IsSameAs(featureTor));
        Assert.IsFalse(featureP2P.IsSameAs(featureFastestSecureCore));
        Assert.IsFalse(featureP2P.IsSameAs(featureSecureCoreA));
        Assert.IsFalse(featureP2P.IsSameAs(featureSecureCoreB));
        Assert.IsFalse(featureP2P.IsSameAs(featureTor));
        Assert.IsFalse(featureTor.IsSameAs(featureFastestSecureCore));
        Assert.IsFalse(featureTor.IsSameAs(featureSecureCoreA));
        Assert.IsFalse(featureTor.IsSameAs(featureSecureCoreB));
        Assert.IsFalse(featureTor.IsSameAs(featureP2P));
    }
}
