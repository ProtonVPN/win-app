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

using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Features;
using ProtonVPN.Client.Logic.Connection.Contracts.Models.Intents.Locations;

namespace ProtonVPN.Client.Logic.Profiles.Contracts.Models;

public class ConnectionProfile : ConnectionIntentBase, IConnectionProfile
{
    public const ProfileColor DEFAULT_COLOR = ProfileColor.Purple;
    public const ProfileCategory DEFAULT_CATEGORY = ProfileCategory.Speed;

    public static IConnectionProfile Default => new ConnectionProfile(new CountryLocationIntent());

    public Guid Id { get; }
    public string Name { get; set; }

    public IProfileSettings Settings { get; set; }

    public DateTime CreationDateTimeUtc { get; }
    public DateTime UpdateDateTimeUtc { get; set; }
    public ProfileCategory Category { get; set; }
    public ProfileColor Color { get; set; }

    public ConnectionProfile(
        Guid id,
        DateTime creationDateTimeUtc,
        IProfileSettings settings,
        ILocationIntent location,
        IFeatureIntent feature = null,
        string name = "",
        ProfileCategory category = DEFAULT_CATEGORY,
        ProfileColor color = DEFAULT_COLOR)
        : base(location, feature)
    {
        Id = id;
        CreationDateTimeUtc = creationDateTimeUtc;
        UpdateDateTimeUtc = creationDateTimeUtc;
        Settings = settings;
        Name = name;
        Category = category;
        Color = color;
    }

    public ConnectionProfile(ILocationIntent location, IFeatureIntent feature = null)
        : this(Guid.NewGuid(), DateTime.UtcNow, ProfileSettings.Default, location, feature)
    { }

    public override bool IsSameAs(IConnectionIntent intent)
    {
        return intent is ConnectionProfile profile
            && profile.Id == Id;
    }

    public void UpdateIntent(ILocationIntent locationIntent, IFeatureIntent featureIntent = null)
    {
        Location = locationIntent;
        Feature = featureIntent;
    }

    public override string ToString()
    {
        return $"Profile {Name}: {base.ToString()}";
    }
}