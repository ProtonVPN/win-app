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
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Networking;
using ProtonVPN.Core.Profiles;
using ProtonVPN.Core.Servers;
using ProtonVPN.Core.Servers.Models;
using ProtonVPN.Core.Servers.Specs;
using ProtonVPN.Core.Settings;
using ProtonVPN.Vpn.Connectors;
using Profile = ProtonVPN.Core.Profiles.Profile;

namespace ProtonVPN.Core.Service.Vpn
{
    public class SimilarServerCandidatesGenerator : ISimilarServerCandidatesGenerator
    {
        private readonly ProfileConnector _profileConnector;
        private readonly ServerManager _serverManager;
        private readonly IAppSettings _appSettings;
        private readonly IProfileFactory _profileFactory;

        public SimilarServerCandidatesGenerator(
            ProfileConnector profileConnector,
            ServerManager serverManager,
            IAppSettings appSettings,
            IProfileFactory profileFactory)
        {
            _profileConnector = profileConnector;
            _serverManager = serverManager;
            _appSettings = appSettings;
            _profileFactory = profileFactory;
        }

        public IList<Server> Generate(bool isToIncludeOriginalServer,
            Server originalServer = null, Profile originalProfile = null)
        {
            if (originalServer == null || originalServer.IsEmpty())
            {
                if (originalProfile == null)
                {
                    return new List<Server>();
                }

                if (originalProfile.ProfileType == ProfileType.Custom)
                {
                    originalServer = _serverManager.GetServer(new ServerById(originalProfile.ServerId));
                }
            }

            if (originalProfile != null && string.IsNullOrEmpty(originalProfile.ServerId))
            {
                originalServer = null;
            }

            Profile profile = CreateProfile(originalServer, originalProfile);
            return GetSimilarServers(isToIncludeOriginalServer, originalServer, profile);
        }

        private Profile CreateProfile(Server originalServer, Profile originalProfile)
        {
            Profile profile = originalProfile;
            if (originalProfile == null || !string.IsNullOrEmpty(originalProfile.ServerId))
            {
                profile = _profileFactory.Create();
                profile.VpnProtocol = VpnProtocol.Smart;
                profile.ProfileType = ProfileType.Fastest;
                profile.Features = (Features)(originalServer?.Features ?? (sbyte)Features.None);
                profile.EntryCountryCode = originalServer?.EntryCountry;
                profile.CountryCode = originalServer?.ExitCountry;
                profile.City = originalServer?.City;
                profile.ExactTier = originalServer == null ? null : (sbyte)originalServer.Tier;
            }

            if ((profile.Features & Features.SecureCore) > 0 != _appSettings.SecureCore)
            {
                profile.Features = _appSettings.SecureCore ? Features.SecureCore : profile.Features & ~Features.SecureCore;
            }

            if (profile.Features == Features.None && _appSettings.IsPortForwardingEnabled())
            {
                profile.Features = Features.P2P;
            }

            return profile;
        }

        private IList<Server> GetSimilarServers(bool isToIncludeOriginalServer, Server originalServer, Profile baseProfile)
        {
            IList<Server> servers = new List<Server>();
            servers.AddIfNotNull(GetOriginalServerIfRequired(isToIncludeOriginalServer, originalServer));

            if ((baseProfile.Features & Features.SecureCore) > 0)
            {
                servers.AddIfNotNull(GetBestServerForSameExitCountryAndDifferentEntryCountry(originalServer, servers, baseProfile));
                servers.AddIfNotNull(GetBestServerForSameEntryCountryAndDifferentExitCountry(originalServer, servers, baseProfile));
                servers.AddIfNotNull(GetBestServerForDifferentEntryAndExitCountries(originalServer, servers, baseProfile));
            }
            else
            {
                servers.AddIfNotNull(GetBestServerForSameCityTierFeatures(originalServer, servers, baseProfile));
                servers.AddIfNotNull(GetBestServerForSameCountryTierFeatures(originalServer, servers, baseProfile));
                servers.AddIfNotNull(GetBestServerForSameCountryFeaturesAndDifferentCity(originalServer, servers, baseProfile));
                if (baseProfile.Features > 0)
                {
                    servers.AddIfNotNull(GetBestServerForSameFeaturesAndDifferentCountry(originalServer, servers, baseProfile));
                }
                servers.AddIfNotNull(GetBestServerForSameCityAndNoFeatures(originalServer, servers, baseProfile));
                servers.AddIfNotNull(GetBestServerForSameCountryAndNoFeaturesAndDifferentCity(originalServer, servers, baseProfile));
                servers.AddIfNotNull(GetBestServerForNoFeaturesAndDifferentCountry(originalServer, servers, baseProfile));
            }

            return servers;
        }

        private Server GetOriginalServerIfRequired(bool isToIncludeOriginalServer, Server originalServer)
        {
            return isToIncludeOriginalServer &&
                   !originalServer.IsNullOrEmpty() &&
                   originalServer.IsSecureCore() == _appSettings.SecureCore &&
                   originalServer.Online()
                ? originalServer
                : null;
        }

        private Server GetBestServerForSameExitCountryAndDifferentEntryCountry(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = _profileFactory.Create();
            profile.ProfileType = ProfileType.Fastest;
            profile.Features = baseProfile.Features;
            profile.CountryCode = baseProfile.CountryCode;
            return GetBestServer(originalServer, excludedServers, profile, s => s.EntryCountry != baseProfile.EntryCountryCode);
        }

        private Server GetBestServer(Server originalServer, IList<Server> excludedServers, Profile profile, Func<Server, bool> filter = null)
        {
            IEnumerable<Server> sortedCandidateServers = GetSortedCandidateServers(profile);
            Server server = filter == null
                ? sortedCandidateServers.FirstOrDefault(s => !s.Equals(originalServer) && !excludedServers.Contains(s))
                : sortedCandidateServers.FirstOrDefault(s => !s.Equals(originalServer) && !excludedServers.Contains(s) && filter(s));
            return server;
        }

        private IEnumerable<Server> GetSortedCandidateServers(Profile profile)
        {
            IReadOnlyCollection<Server> candidates = _profileConnector.ServerCandidates(profile);
            IReadOnlyList<Server> candidateServers = _profileConnector.GetValidServers(candidates);
            return _profileConnector.SortServers(candidateServers, profile.ProfileType);
        }

        private Server GetBestServerForSameEntryCountryAndDifferentExitCountry(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = _profileFactory.Create();
            profile.ProfileType = ProfileType.Fastest;
            profile.Features = baseProfile.Features;
            profile.EntryCountryCode = baseProfile.EntryCountryCode;
            return GetBestServer(originalServer, excludedServers, profile, s => s.ExitCountry != baseProfile.CountryCode);
        }

        private Server GetBestServerForDifferentEntryAndExitCountries(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = _profileFactory.Create();
            profile.ProfileType = ProfileType.Fastest;
            profile.Features = baseProfile.Features;
            return GetBestServer(originalServer, excludedServers, profile,
                s => s.EntryCountry != baseProfile.EntryCountryCode && s.ExitCountry != baseProfile.CountryCode);
        }

        private Server GetBestServerForSameCityTierFeatures(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = _profileFactory.Create();
            profile.ProfileType = ProfileType.Fastest;
            profile.Features = baseProfile.Features;
            profile.CountryCode = baseProfile.CountryCode;
            profile.City = baseProfile.City;
            profile.ExactTier = baseProfile.ExactTier;
            return GetBestServer(originalServer, excludedServers, profile);
        }

        private Server GetBestServerForSameCountryTierFeatures(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = _profileFactory.Create();
            profile.ProfileType = ProfileType.Fastest;
            profile.Features = baseProfile.Features;
            profile.CountryCode = baseProfile.CountryCode;
            profile.ExactTier = baseProfile.ExactTier;
            return GetBestServer(originalServer, excludedServers, profile);
        }

        private Server GetBestServerForSameCountryFeaturesAndDifferentCity(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = _profileFactory.Create();
            profile.ProfileType = ProfileType.Fastest;
            profile.Features = baseProfile.Features;
            profile.CountryCode = baseProfile.CountryCode;
            return GetBestServer(originalServer, excludedServers, profile, s => s.City != baseProfile.City);
        }

        private Server GetBestServerForSameFeaturesAndDifferentCountry(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = _profileFactory.Create();
            profile.ProfileType = ProfileType.Fastest;
            profile.Features = baseProfile.Features;
            return GetBestServer(originalServer, excludedServers, profile, s => s.ExitCountry != baseProfile.CountryCode);
        }

        private Server GetBestServerForSameCityAndNoFeatures(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = _profileFactory.Create();
            profile.ProfileType = ProfileType.Fastest;
            profile.Features = Features.None;
            profile.CountryCode = baseProfile.CountryCode;
            profile.City = baseProfile.City;
            return GetBestServer(originalServer, excludedServers, profile);
        }

        private Server GetBestServerForSameCountryAndNoFeaturesAndDifferentCity(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = _profileFactory.Create();
            profile.ProfileType = ProfileType.Fastest;
            profile.Features = Features.None;
            profile.CountryCode = baseProfile.CountryCode;
            return GetBestServer(originalServer, excludedServers, profile, s => s.City != baseProfile.City);
        }

        private Server GetBestServerForNoFeaturesAndDifferentCountry(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = _profileFactory.Create();
            profile.ProfileType = ProfileType.Fastest;
            profile.Features = Features.None;
            return GetBestServer(originalServer, excludedServers, profile, s => s.ExitCountry != baseProfile.CountryCode);
        }
    }
}
