/*
 * Copyright (c) 2020 Proton Technologies AG
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

using ProtonVPN.Common.Helpers;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProtonVPN.Core.Profiles
{
    public class ApiProfiles : IProfileStorageAsync
    {
        private static readonly Dictionary<HttpStatusCode, ProfileError> HttpStatusCodeToProfileError = new Dictionary<HttpStatusCode, ProfileError>
        {
            { HttpStatusCode.NotFound, ProfileError.NotFound }
        };

        private static readonly Dictionary<int, ProfileError> ResponseCodeToProfileError = new Dictionary<int, ProfileError>
        {
            { ResponseCodes.InvalidProfileIdOnDelete, ProfileError.NotFound },
            { ResponseCodes.InvalidProfileIdOnUpdate, ProfileError.NotFound },
            { ResponseCodes.ProfileNameConflict, ProfileError.NameConflict }
        };

        private readonly IApiClient _apiClient;

        public ApiProfiles(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IReadOnlyList<Profile>> GetAll()
        {
            var response = await HandleErrors(() => _apiClient.GetProfiles());

            return ToProfiles(response.Value.Profiles);
        }

        public async Task Create(Profile profile)
        {
            Ensure.NotNull(profile, nameof(profile));
            Ensure.IsTrue(!profile.IsPredefined, "Can't create predefined profile");
            Ensure.IsTrue(profile.IsColorCodeValid());

            ApiResponseResult<ProfileResponse> response = await HandleErrors(
                () => _apiClient.CreateProfile(ToApiProfile(profile)));

            UpdatePropertiesFromApiProfile(profile, response.Value.Profile);
        }

        public async Task Update(Profile profile)
        {
            Ensure.NotNull(profile, nameof(profile));
            Ensure.IsTrue(!profile.IsPredefined, "Can't update predefined profile");
            Ensure.IsTrue(profile.IsColorCodeValid());

            ApiResponseResult<ProfileResponse> response = await HandleErrors(
                () => _apiClient.UpdateProfile(profile.ExternalId, ToApiProfile(profile)));

            UpdatePropertiesFromApiProfile(profile, response.Value.Profile);
        }

        public async Task Delete(Profile profile)
        {
            Ensure.NotNull(profile, nameof(profile));
            Ensure.IsTrue(!profile.IsPredefined, "Can't delete predefined profile");

            await HandleErrors(() => _apiClient.DeleteProfile(profile.ExternalId));
        }

        private List<Profile> ToProfiles(IEnumerable<Api.Contracts.Profile> profiles)
        {
            return profiles.Select(ToProfile).ToList();
        }

        private Profile ToProfile(Api.Contracts.Profile apiProfile)
        {
            var profile = new Profile { IsPredefined = false };
            UpdatePropertiesFromApiProfile(profile, apiProfile);
            return profile;
        }

        private void UpdatePropertiesFromApiProfile(Profile profile, Api.Contracts.Profile apiProfile)
        {
            profile.ExternalId = apiProfile.Id;
            profile.Name = apiProfile.Name;
            profile.Protocol = MapProtocol(apiProfile.Protocol);
            profile.ColorCode = apiProfile.Color?.ToUpperInvariant();
            profile.Features = MapFeatures(apiProfile.Features);
            profile.ProfileType = MapType(apiProfile.Type);
            profile.CountryCode = apiProfile.Country;
            profile.ServerId = apiProfile.LogicalId;
        }

        private BaseProfile ToApiProfile(Profile profile)
        {
            return new BaseProfile
            {
                Name = profile.Name,
                Protocol = MapProtocol(profile.Protocol),
                Color = profile.ColorCode,
                Features = MapFeatures(profile.Features),
                Type = MapType(profile.ProfileType),
                Country = !string.IsNullOrEmpty(profile.CountryCode) ? profile.CountryCode : null,
                LogicalId = !string.IsNullOrEmpty(profile.ServerId) ? profile.ServerId : null
            };
        }

        private Protocol MapProtocol(int value)
        {
            return Enum.IsDefined(typeof(Protocol), value)
                ? (Protocol) value 
                : Protocol.Auto;
        }

        // Don't change "default(Protocol)" to "default".
        // This would change default value from 0 to null and lead to incorrect behaviour.
        private int MapProtocol(Protocol protocol) => (int)(protocol != default(Protocol) ? protocol : Protocol.Auto);

        private Features MapFeatures(int value)
        {
            var features = new ServerFeatures(value);
            if (features.IsSecureCore())
                return Features.SecureCore;
            if (features.SupportsTor())
                return Features.Tor;
            if (features.SupportsP2P())
                return Features.P2P;

            return Features.None;
        }

        private int MapFeatures(Features features) => (int)features;

        private ProfileType MapType(int value) => (ProfileType) value;

        private int MapType(ProfileType type) => (int)type;

        private async Task<ApiResponseResult<T>> HandleErrors<T>(Func<Task<ApiResponseResult<T>>> function) where T: BaseResponse
        {
            try
            {
                var response = await function();

                if (!response.Success)
                    throw new ProfileException(ToError(response), response.Error);

                return response;
            }
            catch (HttpRequestException e)
            {
                throw new ProfileException(ProfileError.Failure, e.Message, e);
            }
        }

        private ProfileError ToError<T>(ApiResponseResult<T> response) where T: BaseResponse
        {
            if (response.Value != null)
            {
                if (ResponseCodeToProfileError.ContainsKey(response.Value.Code))
                    return ResponseCodeToProfileError[response.Value.Code];

                return ProfileError.Other;
            }

            if (HttpStatusCodeToProfileError.ContainsKey(response.StatusCode))
                return HttpStatusCodeToProfileError[response.StatusCode];

            return ProfileError.Failure;
        }
    }
}
