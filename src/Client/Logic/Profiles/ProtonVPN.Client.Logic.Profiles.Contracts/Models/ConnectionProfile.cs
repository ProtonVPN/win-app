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

using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations.Countries;

namespace ProtonVPN.Client.Logic.Profiles.Contracts.Models;

public class ConnectionProfile : ConnectionIntentBase, IConnectionProfile
{
    public static IConnectionProfile Default => new ConnectionProfile(MultiCountryLocationIntent.Default);

    public Guid Id { get; }

    public string Name { get; set; }

    public IProfileIcon Icon { get; set; }

    public IProfileSettings Settings { get; set; }

    public IProfileOptions Options { get; set; }

    public DateTime CreationDateTimeUtc { get; }

    public DateTime UpdateDateTimeUtc { get; set; }

    public ConnectionProfile(
        Guid id,
        DateTime creationDateTimeUtc,
        IProfileIcon icon,
        IProfileSettings settings,
        IProfileOptions options,
        ILocationIntent location,
        IFeatureIntent? feature = null,
        string name = "")
        : base(location, feature)
    {
        Id = id;
        CreationDateTimeUtc = creationDateTimeUtc;
        UpdateDateTimeUtc = creationDateTimeUtc;
        Icon = icon;
        Settings = settings;
        Options = options;
        Name = name;
    }

    public ConnectionProfile(
        IProfileIcon icon,
        IProfileSettings settings,
        IProfileOptions options,
        ILocationIntent location,
        IFeatureIntent? feature = null,
        string name = "")
        : this(Guid.NewGuid(), DateTime.UtcNow, icon, settings, options, location, feature, name)
    { }

    public ConnectionProfile(ILocationIntent location, IFeatureIntent? feature = null)
        : this(ProfileIcon.Default, ProfileSettings.Default, ProfileOptions.Default, location, feature)
    { }

    public override bool IsSameAs(IConnectionIntent? intent)
    {
        return intent is ConnectionProfile profile
            && profile.Id == Id;
    }

    public void UpdateIntent(ILocationIntent locationIntent, IFeatureIntent? featureIntent = null)
    {
        Location = locationIntent;
        Feature = featureIntent;

        UpdateDateTimeUtc = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return $"Profile {Name}: {base.ToString()}";
    }

    public IConnectionProfile Duplicate(string name)
    {
        return new ConnectionProfile(
            Icon.Copy(),
            Settings.Copy(),
            Options.Copy(),
            Location.Copy(),
            Feature?.Copy(),
            name);
    }
}