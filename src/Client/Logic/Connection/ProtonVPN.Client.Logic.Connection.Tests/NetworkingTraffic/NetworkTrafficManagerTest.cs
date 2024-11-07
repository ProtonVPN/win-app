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

using NSubstitute;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Client.Logic.Connection.NetworkingTraffic;
using ProtonVPN.Common.Core.Networking;

namespace ProtonVPN.Client.Logic.Connection.Tests.NetworkingTraffic;

[TestClass]
public class NetworkTrafficManagerTest
{
    private IEventMessageSender? _eventMessageSender;
    private MockOfNetworkTrafficScheduler? _mockOfNetworkTrafficScheduler;
    private NetworkTrafficManager? _networkTrafficManager;

    [TestInitialize]
    public virtual void Initialize()
    {
        _eventMessageSender = Substitute.For<IEventMessageSender>();
        _mockOfNetworkTrafficScheduler = new MockOfNetworkTrafficScheduler();
        _networkTrafficManager = new(_eventMessageSender, _mockOfNetworkTrafficScheduler);
    }

    [TestCleanup]
    public virtual void Cleanup()
    {
        _eventMessageSender = null;
        _mockOfNetworkTrafficScheduler = null;
        _networkTrafficManager = null;
    }

    [TestMethod]
    public void Test_WhenFirstNetworkTrafficResponseIsZero()
    {
        NetworkTraffic networkTraffic = NetworkTraffic.Zero;

        _mockOfNetworkTrafficScheduler!.InvokeEvent(networkTraffic);
        _eventMessageSender!.Received(1).Send(Arg.Any<NetworkTrafficChangedMessage>());

        AssertEqualNetworkTrafficWithoutDate(NetworkTraffic.Zero, _networkTrafficManager!.GetSpeed());
        AssertEqualNetworkTrafficWithoutDate(NetworkTraffic.Zero, _networkTrafficManager.GetVolume());
        AssertSpeedHistoryIsEmpty();
    }

    private void AssertEqualNetworkTrafficWithoutDate(NetworkTraffic expectedNetworkTraffic,
        NetworkTraffic networkTraffic)
    {
        Assert.AreEqual(expectedNetworkTraffic.BytesDownloaded, networkTraffic.BytesDownloaded);
        Assert.AreEqual(expectedNetworkTraffic.BytesUploaded, networkTraffic.BytesUploaded);
    }

    private void AssertSpeedHistoryIsEmpty()
    {
        IReadOnlyList<NetworkTraffic> speedHistory = _networkTrafficManager!.GetSpeedHistory();
        Assert.AreEqual(NetworkTrafficManager.HISTORY_LENGTH_IN_SECONDS, speedHistory.Count);
        for (int i = 0; i < NetworkTrafficManager.HISTORY_LENGTH_IN_SECONDS; i++)
        {
            AssertEqualNetworkTrafficWithoutDate(NetworkTraffic.Zero, speedHistory[i]);
        }
    }

    [TestMethod]
    public void Test_WhenFirstNetworkTrafficResponseIsNonZero()
    {
        NetworkTraffic networkTraffic = new(12345, 23456);

        _mockOfNetworkTrafficScheduler!.InvokeEvent(networkTraffic);
        _eventMessageSender!.Received(1).Send(Arg.Any<NetworkTrafficChangedMessage>());

        AssertEqualNetworkTrafficWithoutDate(NetworkTraffic.Zero, _networkTrafficManager!.GetSpeed());
        Assert.AreEqual(networkTraffic, _networkTrafficManager.GetVolume());
        AssertSpeedHistoryIsEmpty();
    }

    [TestMethod]
    public void Test_NetworkTrafficResponses_TooOldIncrement()
    {
        DateTime utcNow = DateTime.UtcNow;
        NetworkTraffic networkTraffic1 = new(100, 200, utcNow);
        NetworkTraffic networkTraffic2 = new(300, 300, utcNow.AddSeconds(1).AddTicks(-1));
        NetworkTraffic expectedSpeed = NetworkTraffic.Zero;

        _mockOfNetworkTrafficScheduler!.InvokeEvent(networkTraffic1);
        _eventMessageSender!.Received(1).Send(Arg.Any<NetworkTrafficChangedMessage>());
        _mockOfNetworkTrafficScheduler!.InvokeEvent(networkTraffic2);
        _eventMessageSender!.Received(1).Send(Arg.Any<NetworkTrafficChangedMessage>());

        AssertEqualNetworkTrafficWithoutDate(expectedSpeed, _networkTrafficManager!.GetSpeed());
        Assert.AreEqual(networkTraffic1, _networkTrafficManager.GetVolume());
        AssertSpeedHistoryIsEmpty();
    }

    [TestMethod]
    public void Test_NetworkTrafficResponses_FromZero_OneSecondIncrement()
    {
        DateTime utcNow = DateTime.UtcNow;
        NetworkTraffic networkTraffic1 = new(0, 0, utcNow);
        NetworkTraffic networkTraffic2 = new(100, 200, utcNow.AddSeconds(1));
        NetworkTraffic expectedSpeed = networkTraffic2;

        _mockOfNetworkTrafficScheduler!.InvokeEvent(networkTraffic1);
        _eventMessageSender!.Received(1).Send(Arg.Any<NetworkTrafficChangedMessage>());
        _mockOfNetworkTrafficScheduler!.InvokeEvent(networkTraffic2);
        _eventMessageSender!.Received(2).Send(Arg.Any<NetworkTrafficChangedMessage>());

        AssertEqualNetworkTrafficWithoutDate(expectedSpeed, _networkTrafficManager!.GetSpeed());
        Assert.AreEqual(networkTraffic2, _networkTrafficManager.GetVolume());
        AssertLastSpeedHistoryItems(expectedSpeed, 1);
    }

    [TestMethod]
    public void Test_NetworkTrafficResponses_FromNonZero_OneSecondIncrement()
    {
        DateTime utcNow = DateTime.UtcNow;
        NetworkTraffic networkTraffic1 = new(100, 200, utcNow);
        NetworkTraffic networkTraffic2 = new(500, 300, utcNow.AddSeconds(1));
        NetworkTraffic expectedSpeed = new(400, 100);

        _mockOfNetworkTrafficScheduler!.InvokeEvent(networkTraffic1);
        _eventMessageSender!.Received(1).Send(Arg.Any<NetworkTrafficChangedMessage>());
        _mockOfNetworkTrafficScheduler!.InvokeEvent(networkTraffic2);
        _eventMessageSender!.Received(2).Send(Arg.Any<NetworkTrafficChangedMessage>());

        AssertEqualNetworkTrafficWithoutDate(expectedSpeed, _networkTrafficManager!.GetSpeed());
        Assert.AreEqual(networkTraffic2, _networkTrafficManager.GetVolume());
        AssertLastSpeedHistoryItems(expectedSpeed, 1);
    }

    [TestMethod]
    public void Test_NetworkTrafficResponses_FromZero_HighIncrementButBelowLength()
    {
        DateTime utcNow = DateTime.UtcNow;
        ulong timeDifferenceInSeconds = NetworkTrafficManager.HISTORY_LENGTH_IN_SECONDS / 2;
        ulong downloadVolume = NetworkTrafficManager.HISTORY_LENGTH_IN_SECONDS * 20;
        ulong uploadVolume = NetworkTrafficManager.HISTORY_LENGTH_IN_SECONDS * 40;

        NetworkTraffic networkTraffic1 = new(0, 0, utcNow);
        NetworkTraffic networkTraffic2 = new(downloadVolume, uploadVolume, utcNow.AddSeconds(timeDifferenceInSeconds));
        NetworkTraffic expectedSpeed = new(downloadVolume / timeDifferenceInSeconds, uploadVolume / timeDifferenceInSeconds);

        _mockOfNetworkTrafficScheduler!.InvokeEvent(networkTraffic1);
        _eventMessageSender!.Received(1).Send(Arg.Any<NetworkTrafficChangedMessage>());
        _mockOfNetworkTrafficScheduler!.InvokeEvent(networkTraffic2);
        _eventMessageSender!.Received(2).Send(Arg.Any<NetworkTrafficChangedMessage>());

        AssertEqualNetworkTrafficWithoutDate(expectedSpeed, _networkTrafficManager!.GetSpeed());
        Assert.AreEqual(networkTraffic2, _networkTrafficManager.GetVolume());
        AssertLastSpeedHistoryItems(expectedSpeed, (int)timeDifferenceInSeconds);
    }

    private void AssertLastSpeedHistoryItems(NetworkTraffic expectedSpeed, int numOfItems)
    {
        IReadOnlyList<NetworkTraffic> speedHistory = _networkTrafficManager!.GetSpeedHistory();
        Assert.AreEqual(NetworkTrafficManager.HISTORY_LENGTH_IN_SECONDS, speedHistory.Count);

        int numOfEmptyItems = NetworkTrafficManager.HISTORY_LENGTH_IN_SECONDS - numOfItems;
        for (int i = 0; i < NetworkTrafficManager.HISTORY_LENGTH_IN_SECONDS; i++)
        {
            NetworkTraffic innerExpectedSpeed = i < numOfEmptyItems
                ? NetworkTraffic.Zero
                : expectedSpeed;

            AssertEqualNetworkTrafficWithoutDate(innerExpectedSpeed, speedHistory[i]);
        }
    }

    [TestMethod]
    public void Test_NetworkTrafficResponses_FromZero_HighIncrementAboveLength()
    {
        DateTime utcNow = DateTime.UtcNow;
        ulong timeDifferenceInSeconds = NetworkTrafficManager.HISTORY_LENGTH_IN_SECONDS * 2;
        ulong downloadVolume = NetworkTrafficManager.HISTORY_LENGTH_IN_SECONDS * 20;
        ulong uploadVolume = NetworkTrafficManager.HISTORY_LENGTH_IN_SECONDS * 40;

        NetworkTraffic networkTraffic1 = new(0, 0, utcNow);
        NetworkTraffic networkTraffic2 = new(downloadVolume, uploadVolume, utcNow.AddSeconds(timeDifferenceInSeconds));
        NetworkTraffic expectedSpeed = new(downloadVolume / timeDifferenceInSeconds, uploadVolume / timeDifferenceInSeconds);

        _mockOfNetworkTrafficScheduler!.InvokeEvent(networkTraffic1);
        _eventMessageSender!.Received(1).Send(Arg.Any<NetworkTrafficChangedMessage>());
        _mockOfNetworkTrafficScheduler!.InvokeEvent(networkTraffic2);
        _eventMessageSender!.Received(2).Send(Arg.Any<NetworkTrafficChangedMessage>());

        AssertEqualNetworkTrafficWithoutDate(expectedSpeed, _networkTrafficManager!.GetSpeed());
        Assert.AreEqual(networkTraffic2, _networkTrafficManager.GetVolume());
        AssertLastSpeedHistoryItems(expectedSpeed, NetworkTrafficManager.HISTORY_LENGTH_IN_SECONDS);
    }
}