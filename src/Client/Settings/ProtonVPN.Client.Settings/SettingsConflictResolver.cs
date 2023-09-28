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

using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Conflicts.Bases;
using ProtonVPN.Client.Settings.Contracts.Messages;

namespace ProtonVPN.Client.Settings;

public class SettingsConflictResolver : ISettingsConflictResolver, IEventMessageReceiver<SettingChangedMessage>
{
    private readonly List<ISettingsConflict> _conflicts;

    public SettingsConflictResolver(IEnumerable<ISettingsConflict> conflicts)
    {
        _conflicts = conflicts.ToList();
    }

    public ISettingsConflict? GetConflict(string settingsName, dynamic? settingsValue)
    {
        return _conflicts.FirstOrDefault(c => c.Matches(settingsName, settingsValue) && c.IsConflicting);
    }

    public void ResolveConflict(string settingsName, dynamic? settingsValue)
    {
        ISettingsConflict? conflict = GetConflict(settingsName, settingsValue);

        conflict?.ResolveAction();
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message == null)
        {
            return;
        }

        ResolveConflict(message.PropertyName, message.NewValue);
    }
}