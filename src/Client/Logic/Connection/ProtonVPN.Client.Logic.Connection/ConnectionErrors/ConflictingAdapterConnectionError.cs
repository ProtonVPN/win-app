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

using System.Text;
using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.ConnectionErrors;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.OperatingSystems.Network.Contracts.NetworkInterfaces;

namespace ProtonVPN.Client.Logic.Connection.ConnectionErrors;

public class ConflictingAdapterConnectionError : ConnectionErrorBase, IConflictingAdapterConnectionError
{
    private const byte MAX_ADAPTERS_TO_SHOW = 3;

    private IReadOnlyList<NetworkInterfaceInfo> _conflictingAdapters = [];
    private Severity _severity;

    public override Severity Severity => _severity;

    public override string Title => Localizer.Get("Connection_Error_ConflictingAdapter_Title");

    public override string Message => CreateMessage();

    public override string ActionLabel => string.Empty;

    public override bool IsToCloseErrorOnDisconnect => false;

    public override bool IsToCloseErrorOnConnecting => false;

    public ConflictingAdapterConnectionError(
        ILocalizationProvider localizer)
        : base(localizer)
    {
    }

    private string CreateMessage()
    {
        if (_conflictingAdapters.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder stringBuilder = new();
        stringBuilder.Append(Localizer.Get("Connection_Error_ConflictingAdapter_Description"));

        List<NetworkInterfaceInfo> conflictingAdaptersToShow = _conflictingAdapters.Take(MAX_ADAPTERS_TO_SHOW).ToList();

        foreach (NetworkInterfaceInfo conflictingAdapter in conflictingAdaptersToShow)
        {
            stringBuilder.AppendLine().Append($"- {conflictingAdapter.Description} ({conflictingAdapter.Name})");
        }
        return stringBuilder.ToString();
    }

    public override Task ExecuteActionAsync()
    {
        return Task.CompletedTask;
    }

    public void SetConflictingAdapters(IReadOnlyList<NetworkInterfaceInfo> conflictingAdapters)
    {
        _conflictingAdapters = conflictingAdapters;
    }

    public void SetConnectionStatus(ConnectionStatus connectionStatus)
    {
        _severity = connectionStatus == ConnectionStatus.Disconnected ? Severity.Error : Severity.Warning;
    }
}