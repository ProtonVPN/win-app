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

using ProtonVPN.Client.Common.Models;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts.Conflicts.Bases;
using ProtonVPN.Client.Settings.Contracts.Enums;
using ProtonVPN.Client.Settings.Contracts.RequiredReconnections;

namespace ProtonVPN.Client.Settings.Contracts.Conflicts;

public class NatTypeSettingsConflict : SettingsConflictBase
{
    private readonly IRequiredReconnectionSettings _requiredReconnectionSettings;

    public NatTypeSettingsConflict(
        IRequiredReconnectionSettings requiredReconnectionSettings,
        ILocalizationProvider localizer,
        ISettings settings) : base(localizer, settings)
    {
        _requiredReconnectionSettings = requiredReconnectionSettings;
    }

    public override string SettingsName => nameof(ISettings.NatType);

    public override dynamic? SettingsValue => NatType.Moderate;

    public override dynamic? SettingsResetValue => NatType.Strict;

    public override bool IsConflicting => Settings.IsPortForwardingEnabled;

    public override MessageDialogParameters MessageParameters => new()
    {
        Title = Localizer.Get("Settings_Connection_Advanced_NatType_Conflict_Title"),
        Message = Localizer.Get("Settings_Connection_Advanced_NatType_Conflict_Description"),
        PrimaryButtonText = Localizer.Get("Common_Actions_Enable"),
        CloseButtonText = Localizer.Get("Common_Actions_Cancel"),
    };

    public override Action ResolveAction => () => Settings.IsPortForwardingEnabled = false;

    public override bool IsReconnectionRequired => _requiredReconnectionSettings.IsReconnectionRequired(nameof(ISettings.IsPortForwardingEnabled)) && IsConflicting;
}