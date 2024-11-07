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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.History;
using ProtonVPN.Client.Logic.Connection.Contracts.Messages;
using ProtonVPN.Common.Core.Networking;
using ProtonVPN.Common.Core.Queues;

namespace ProtonVPN.Client.Logic.Connection.NetworkingTraffic;

public class NetworkTrafficManager : INetworkTrafficManager,
    IEventMessageReceiver<ConnectionStatusChangedMessage>
{
    public const int HISTORY_LENGTH_IN_SECONDS = 60;

    private readonly IEventMessageSender _eventMessageSender;
    private readonly INetworkTrafficScheduler _networkTrafficScheduler;

    private readonly FixedSizeQueue<NetworkTraffic> _speedHistory = new(HISTORY_LENGTH_IN_SECONDS, NetworkTraffic.Zero);
    private readonly object _lock = new();

    private NetworkTraffic? _lastNetworkTrafficVolume = null;
    private NetworkTraffic _lastNetworkTrafficSpeed = NetworkTraffic.Zero;

    public NetworkTrafficManager(IEventMessageSender eventMessageSender,
        INetworkTrafficScheduler networkTrafficScheduler)
    {
        _eventMessageSender = eventMessageSender;
        _networkTrafficScheduler = networkTrafficScheduler;
        _networkTrafficScheduler.NetworkTrafficChanged += OnNetworkTrafficChanged;
    }

    private void OnNetworkTrafficChanged(object? sender, NetworkTraffic networkTraffic)
    {
        bool hasChanges;
        lock (_lock)
        {
            hasChanges = RefreshTrafficBytes(networkTraffic);
        }
        if (hasChanges)
        {
            _eventMessageSender.Send(new NetworkTrafficChangedMessage());
        }
    }

    /// <returns>True if the values changed. False if no changes were made.</returns>
    private bool RefreshTrafficBytes(NetworkTraffic networkTrafficVolume)
    {
        NetworkTraffic networkTrafficSpeed = NetworkTraffic.Zero;

        if (_lastNetworkTrafficVolume is null)
        {
            _speedHistory.Reset();
        }
        else if (_lastNetworkTrafficVolume.Value.UtcDate + TimeSpan.FromSeconds(1) > networkTrafficVolume.UtcDate)
        {
            // Ignore network traffic older than the last we received
            // We also don't want network traffic less than one second from the last one, as we only measure second increments
            return false;
        }
        else
        {
            NetworkTraffic volumeDifference = networkTrafficVolume - _lastNetworkTrafficVolume.Value;

            double timeDifferenceInSeconds = (networkTrafficVolume.UtcDate - _lastNetworkTrafficVolume.Value.UtcDate).TotalSeconds;

            double bytesDownloadedPerSecond = volumeDifference.BytesDownloaded / timeDifferenceInSeconds;
            double bytesUploadedPerSecond = volumeDifference.BytesUploaded / timeDifferenceInSeconds;
            networkTrafficSpeed = new(bytesDownloaded: (ulong)bytesDownloadedPerSecond, bytesUploaded: (ulong)bytesUploadedPerSecond);

            if (networkTrafficVolume.UtcDate >= _lastNetworkTrafficVolume.Value.UtcDate + TimeSpan.FromSeconds(HISTORY_LENGTH_IN_SECONDS))
            {
                _speedHistory.Reset();
                for (int i = HISTORY_LENGTH_IN_SECONDS - 1; i >= 0; i--)
                {
                    DateTime date = networkTrafficVolume.UtcDate.AddSeconds(-i);
                    _speedHistory.Enqueue(networkTrafficSpeed.Copy(date));
                }
            }
            else
            {
                for (DateTime i = _lastNetworkTrafficVolume.Value.UtcDate; i < networkTrafficVolume.UtcDate; i = i.AddSeconds(1))
                {
                    _speedHistory.Enqueue(networkTrafficSpeed.Copy(i));
                }
            }
        }

        _lastNetworkTrafficVolume = networkTrafficVolume;
        _lastNetworkTrafficSpeed = networkTrafficSpeed;

        return true;
    }

    public NetworkTraffic GetSpeed()
    {
        lock (_lock)
        {
            return _lastNetworkTrafficSpeed;
        }
    }

    public NetworkTraffic GetVolume()
    {
        lock (_lock)
        {
            return _lastNetworkTrafficVolume ?? NetworkTraffic.Zero;
        }
    }

    public IReadOnlyList<NetworkTraffic> GetSpeedHistory()
    {
        lock (_lock)
        {
            return _speedHistory.ToList();
        }
    }

    public void Receive(ConnectionStatusChangedMessage message)
    {
        if (message.ConnectionStatus is not ConnectionStatus.Connected)
        {
            lock (_lock)
            {
                _speedHistory.Reset();
            }

            _eventMessageSender.Send(new NetworkTrafficChangedMessage());
        }
    }
}