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

using System;
using ProtonVPN.Common.Extensions;

namespace ProtonVPN.Core.Profiles
{
    public static class ProfileExtensions
    {
        public static bool Exists(this Profile profile)
        {
            return profile != null;
        }

        public static bool IsValid(this Profile profile)
        {
            if (profile == null) 
            {
                return false;
            }

            // The old ServerId (mapped to LogicalID on the API) value might be not mapped by the profile migration.
            if (!string.IsNullOrEmpty(profile.ServerId) && profile.ServerId.Length < 5)
            {
                return false;
            }

            return profile.ColorCode.IsColorCodeValid();
        }

        public static bool HasElapsed(this Profile profile, TimeSpan timeSpan)
        {
            if (profile == null)
                return false;

            return DateTime.UtcNow - profile.ModifiedAt > timeSpan;
        }

        public static bool ModifiedLaterThan(this Profile profile, Profile another)
        {
            if (profile == null || another == null)
                return false;

            return profile.ModifiedAt > another.ModifiedAt;
        }

        public static Profile WithIdFrom(this Profile profile, Profile another)
        {
            if (profile == null) return null;

            if (another != null)
            {
                profile.Id = another.Id;
            }

            return profile;
        }

        public static Profile WithExternalIdFrom(this Profile profile, Profile another)
        {
            if (profile == null) return null;

            if (another != null)
            {
                profile.ExternalId = another.ExternalId;
            }

            return profile;
        }

        public static Profile WithModifiedAt(this Profile profile, DateTime value)
        {
            profile.ModifiedAt = value;

            return profile;
        }

        public static Profile WithStatus(this Profile profile, ProfileStatus value)
        {
            Profile p = profile.Clone();
            p.Status = value;

            if (value == ProfileStatus.Synced)
            {
                p.OriginalName = null;
                p.UniqueNameIndex = 0;
            }

            return p;
        }

        public static Profile WithSyncStatus(this Profile profile, ProfileSyncStatus value)
        {
            profile.SyncStatus = value;

            return profile;
        }

        public static Profile WithStatusMergedFrom(this Profile profile, Profile another)
        {
            if (another != null && profile.Status == ProfileStatus.Updated)
                profile.Status = another.Status;

            return profile;
        }

        public static Profile WithUniqueNameCandidate(this Profile profile, int maxLength)
        {
            Profile p = profile.Clone();

            if (string.IsNullOrEmpty(p.OriginalName))
            {
                p.OriginalName = p.Name;
                p.UniqueNameIndex = 0;
            }

            p.Name = NameCandidate(p.OriginalName, p.UniqueNameIndex, maxLength);

            return p;
        }

        public static Profile WithNextUniqueNameCandidate(this Profile profile, int maxLength)
        {
            profile.UniqueNameIndex++;
            return profile.WithUniqueNameCandidate(maxLength);
        }

        private static string NameCandidate(string originalName, int index, int maxLength)
        {
            string suffix = index == 0 ? "" : $" ({index})";
            int excess = originalName.Length + suffix.Length - maxLength;
            return (excess > 0 ? originalName.Remove(originalName.Length - excess) : originalName) + suffix;
        }
    }
}
