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

using System;
using System.Collections.Generic;
using System.Linq;
using ProtonVPN.Common.Extensions;
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
        private readonly ServerCandidatesFactory _serverCandidatesFactory;
        private readonly ServerManager _serverManager;
        private readonly IAppSettings _appSettings;

        public SimilarServerCandidatesGenerator(
            ProfileConnector profileConnector,
            ServerCandidatesFactory serverCandidatesFactory,
            ServerManager serverManager, 
            IAppSettings appSettings)
        {
            _profileConnector = profileConnector;
            _serverCandidatesFactory = serverCandidatesFactory;
            _serverManager = serverManager;
            _appSettings = appSettings;
        }

        public ServerCandidates Generate(bool isToIncludeOriginalServer, 
            Server originalServer = null, Profile baseProfile = null)
        {
            IList<Server> similarServers = GenerateList(isToIncludeOriginalServer, originalServer, baseProfile);
            return CreateServerCandidates(similarServers);
        }

        public IList<Server> GenerateList(bool isToIncludeOriginalServer, 
            Server originalServer = null, Profile baseProfile = null)
        {
            if (originalServer == null || originalServer.IsEmpty())
            {
                if (baseProfile == null)
                {
                    return new List<Server>();
                }

                if (baseProfile.ProfileType == ProfileType.Custom)
                {
                    originalServer = _serverManager.GetServer(new ServerById(baseProfile.ServerId));
                }
            }

            baseProfile = CreateProfile(originalServer);
            
            return GetSimilarServers(isToIncludeOriginalServer, originalServer, baseProfile);
        }

        private Profile CreateProfile(Server originalServer)
        {
            Profile profile = new()
            {
                Protocol = Protocol.Auto,
                ProfileType = ProfileType.Fastest,
                Features = (Features)(originalServer?.Features ?? (sbyte)Features.None),
                EntryCountryCode = originalServer?.EntryCountry,
                CountryCode = originalServer?.ExitCountry,
                City = originalServer?.City,
                ExactTier = originalServer == null ? null : (sbyte)originalServer.Tier
            };

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
            Profile profile = new()
            {
                ProfileType = ProfileType.Fastest,
                Features = baseProfile.Features,
                CountryCode = baseProfile.CountryCode
            };
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
            ServerCandidates candidates = _profileConnector.ServerCandidates(profile);
            IReadOnlyList<Server> candidateServers = _profileConnector.Servers(candidates);
            return _profileConnector.SortServers(candidateServers, profile.ProfileType);
        }

        private Server GetBestServerForSameEntryCountryAndDifferentExitCountry(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = new()
            {
                ProfileType = ProfileType.Fastest,
                Features = baseProfile.Features,
                EntryCountryCode = baseProfile.EntryCountryCode
            };
            return GetBestServer(originalServer, excludedServers, profile, s => s.ExitCountry != baseProfile.CountryCode);
        }

        private Server GetBestServerForDifferentEntryAndExitCountries(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = new()
            {
                ProfileType = ProfileType.Fastest,
                Features = baseProfile.Features
            };
            return GetBestServer(originalServer, excludedServers, profile,
                s => s.EntryCountry != baseProfile.EntryCountryCode && s.ExitCountry != baseProfile.CountryCode);
        }

        private Server GetBestServerForSameCityTierFeatures(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = new()
            {
                ProfileType = ProfileType.Fastest,
                Features = baseProfile.Features,
                CountryCode = baseProfile.CountryCode,
                City = baseProfile.City,
                ExactTier = baseProfile.ExactTier
            };
            return GetBestServer(originalServer, excludedServers, profile);
        }

        private Server GetBestServerForSameCountryTierFeatures(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = new()
            {
                ProfileType = ProfileType.Fastest,
                Features = baseProfile.Features,
                CountryCode = baseProfile.CountryCode,
                ExactTier = baseProfile.ExactTier
            };
            return GetBestServer(originalServer, excludedServers, profile);
        }

        private Server GetBestServerForSameCountryFeaturesAndDifferentCity(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = new()
            {
                ProfileType = ProfileType.Fastest,
                Features = baseProfile.Features,
                CountryCode = baseProfile.CountryCode
            };
            return GetBestServer(originalServer, excludedServers, profile, s => s.City != baseProfile.City);
        }

        private Server GetBestServerForSameFeaturesAndDifferentCountry(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = new()
            {
                ProfileType = ProfileType.Fastest,
                Features = baseProfile.Features
            };
            return GetBestServer(originalServer, excludedServers, profile, s => s.ExitCountry != baseProfile.CountryCode);
        }

        private Server GetBestServerForSameCityAndNoFeatures(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = new()
            {
                ProfileType = ProfileType.Fastest,
                Features = Features.None,
                CountryCode = baseProfile.CountryCode,
                City = baseProfile.City
            };
            return GetBestServer(originalServer, excludedServers, profile);
        }

        private Server GetBestServerForSameCountryAndNoFeaturesAndDifferentCity(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = new()
            {
                ProfileType = ProfileType.Fastest,
                Features = Features.None,
                CountryCode = baseProfile.CountryCode
            };
            return GetBestServer(originalServer, excludedServers, profile, s => s.City != baseProfile.City);
        }

        private Server GetBestServerForNoFeaturesAndDifferentCountry(Server originalServer, IList<Server> excludedServers, Profile baseProfile)
        {
            Profile profile = new()
            {
                ProfileType = ProfileType.Fastest,
                Features = Features.None
            };
            return GetBestServer(originalServer, excludedServers, profile, s => s.ExitCountry != baseProfile.CountryCode);
        }

        private ServerCandidates CreateServerCandidates(IList<Server> servers)
        {
            return _serverCandidatesFactory.ServerCandidates((IReadOnlyCollection<Server>)servers);
        }
    }
}
