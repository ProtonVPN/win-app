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
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Common;
using ProtonVPN.Api.Contracts.Profiles;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Helpers;
using ProtonVPN.Common.Networking;
using ProtonVPN.Core.Servers;

namespace ProtonVPN.Core.Profiles
{
    public class ApiProfiles : IProfileStorageAsync
    {
        private static readonly Dictionary<HttpStatusCode, ProfileError> HttpStatusCodeToProfileError = new()
        {
            { HttpStatusCode.NotFound, ProfileError.NotFound }
        };

        private static readonly Dictionary<int, ProfileError> ResponseCodeToProfileError = new()
        {
            { ResponseCodes.InvalidProfileIdOnDelete, ProfileError.NotFound },
            { ResponseCodes.InvalidProfileIdOnUpdate, ProfileError.NotFound },
            { ResponseCodes.ProfileNameConflict, ProfileError.NameConflict }
        };

        private readonly IApiClient _apiClient;
        private readonly IProfileFactory _profileFactory;
        private readonly ColorProvider _colorProvider;

        public ApiProfiles(IApiClient apiClient,
            IProfileFactory profileFactory,
            ColorProvider colorProvider)
        {
            _apiClient = apiClient;
            _profileFactory = profileFactory;
            _colorProvider = colorProvider;
        }

        public async Task<IReadOnlyList<Profile>> GetAll()
        {
            ApiResponseResult<ProfilesResponse> response = await HandleErrors(() => _apiClient.GetProfiles());

            return ToProfiles(response.Value.Profiles);
        }

        public async Task Create(Profile profile)
        {
            Ensure.NotNull(profile, nameof(profile));
            Ensure.IsTrue(!profile.IsPredefined, "Can't create predefined profile");
            Ensure.IsTrue(profile.ColorCode.IsColorCodeValid());

            ApiResponseResult<ProfileWrapperResponse> response = await HandleErrors(
                () => _apiClient.CreateProfile(ToApiProfile(profile)));

            UpdatePropertiesFromApiProfile(profile, response.Value.Profile);
        }

        public async Task Update(Profile profile)
        {
            Ensure.NotNull(profile, nameof(profile));
            Ensure.IsTrue(!profile.IsPredefined, "Can't update predefined profile");
            Ensure.IsTrue(profile.ColorCode.IsColorCodeValid());

            ApiResponseResult<ProfileWrapperResponse> response = await HandleErrors(
                () => _apiClient.UpdateProfile(profile.ExternalId, ToApiProfile(profile)));

            UpdatePropertiesFromApiProfile(profile, response.Value.Profile);
        }

        public async Task Delete(Profile profile)
        {
            Ensure.NotNull(profile, nameof(profile));
            Ensure.IsTrue(!profile.IsPredefined, "Can't delete predefined profile");

            await HandleErrors(() => _apiClient.DeleteProfile(profile.ExternalId));
        }

        private List<Profile> ToProfiles(IEnumerable<ProfileResponse> profiles)
        {
            return profiles.Select(ToProfile).ToList();
        }

        private Profile ToProfile(ProfileResponse apiProfile)
        {
            Profile profile = _profileFactory.Create();
            profile.IsPredefined = false;
            UpdatePropertiesFromApiProfile(profile, apiProfile);
            return profile;
        }

        private void UpdatePropertiesFromApiProfile(Profile profile, ProfileResponse apiProfile)
        {
            profile.ExternalId = apiProfile.Id;
            profile.Name = apiProfile.Name;
            profile.VpnProtocol = MapVpnProtocol(apiProfile.Protocol);
            profile.ColorCode = _colorProvider.GetRandomColorIfInvalid(apiProfile.Color?.ToUpperInvariant());
            profile.Features = MapFeatures(apiProfile.Features);
            profile.ProfileType = MapType(apiProfile.Type);
            profile.CountryCode = apiProfile.Country;
            profile.ServerId = apiProfile.LogicalId;
        }

        private BaseProfileResponse ToApiProfile(Profile profile)
        {
            return new BaseProfileResponse
            {
                Name = profile.Name,
                Protocol = MapVpnProtocol(profile.VpnProtocol),
                Color = profile.ColorCode,
                Features = MapFeatures(profile.Features),
                Type = MapType(profile.ProfileType),
                Country = !string.IsNullOrEmpty(profile.CountryCode) ? profile.CountryCode : null,
                LogicalId = !string.IsNullOrEmpty(profile.ServerId) ? profile.ServerId : null
            };
        }

        private VpnProtocol MapVpnProtocol(int value)
        {
            return value switch
            {
                1 => VpnProtocol.OpenVpnTcp,
                2 => VpnProtocol.OpenVpnUdp,
                3 => VpnProtocol.Smart,
                4 => VpnProtocol.WireGuard,
                _ => VpnProtocol.Smart
            };
        }

        private int MapVpnProtocol(VpnProtocol vpnProtocol)
        {
            return vpnProtocol switch
            {
                VpnProtocol.OpenVpnTcp => 1,
                VpnProtocol.OpenVpnUdp => 2,
                VpnProtocol.Smart => 3,
                VpnProtocol.WireGuard => 4,
                _ => throw new NotImplementedException()
            };
        }

        private Features MapFeatures(int value)
        {
            ServerFeatures features = new(value);
            return features.IsSecureCore() ? Features.SecureCore :
                features.SupportsTor() ? Features.Tor :
                features.SupportsP2P() ? Features.P2P :
                Features.None;
        }

        private int MapFeatures(Features features) => (int)features;

        private ProfileType MapType(int value) => (ProfileType)value;

        private int MapType(ProfileType type) => (int)type;

        private async Task<ApiResponseResult<T>> HandleErrors<T>(Func<Task<ApiResponseResult<T>>> function) where T : BaseResponse
        {
            try
            {
                ApiResponseResult<T> response = await function();

                if (!response.Success)
                {
                    throw new ProfileException(ToError(response), response.Error);
                }

                return response;
            }
            catch (Exception e)
            {
                throw new ProfileException(ProfileError.Failure, e.Message, e);
            }
        }

        private ProfileError ToError<T>(ApiResponseResult<T> response) where T : BaseResponse
        {
            if (response.Value != null)
            {
                if (ResponseCodeToProfileError.ContainsKey(response.Value.Code))
                {
                    return ResponseCodeToProfileError[response.Value.Code];
                }

                return ProfileError.Other;
            }

            if (HttpStatusCodeToProfileError.ContainsKey(response.ResponseMessage.StatusCode))
            {
                return HttpStatusCodeToProfileError[response.ResponseMessage.StatusCode];
            }

            return ProfileError.Failure;
        }
    }
}