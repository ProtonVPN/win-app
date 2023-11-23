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
using ProtonVPN.Builds.Variables;
using ProtonVPN.Common.Configuration.Api.Handlers.TlsPinning;
using ProtonVPN.Common.Configuration.Source;
using ProtonVPN.Common.Configuration.Storage;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.OS.EnvironmentVariables;

namespace ProtonVPN.Common.Configuration.Environments
{
    public class EnvironmentVariableConfigStorage : IConfigStorage
    {
        private const string API_URL = "api.protonvpn.ch";

        private readonly IConfigSource _default;
        private readonly IConfigStorage _origin;

        public EnvironmentVariableConfigStorage(IConfigSource defaultSource, IConfigStorage origin)
        {
            _default = defaultSource;
            _origin = origin;
        }

        public void SaveIfNotExists(IConfiguration value)
        {
            _origin.SaveIfNotExists(value);
        }

        public void Save(IConfiguration value)
        {
            _origin.Save(value);
        }

        public IConfiguration Value()
        {
            IConfiguration config = _origin.Value();

#if DEBUG
            if (config is null)
            {
                config = _default.Value();
                Uri btiApiUri = SetApiUrl(config);
                SetApiTlsPinningPublicKeyHashes(config, btiApiUri);
                SetAlternativeRoutingTlsPinningPublicKeyHashes(config);
                SetCertificateValidation(config);
                SetDohProviders(config);
                SetServerSignaturePublicKey(config);
            }
#endif

            return config;
        }

        private Uri SetApiUrl(IConfiguration config)
        {
            string apiUrl = EnvironmentVariableLoader.GetOrNull("BTI_API_DOMAIN");
            if (apiUrl.IsNullOrEmpty())
            {
                apiUrl = GlobalConfig.BtiApiDomain;
                if (!apiUrl.IsNullOrEmpty())
                {
                    return SetApiUrl(config, apiUrl);
                }
            }
            else
            {
                return SetApiUrl(config, apiUrl);
            }
            return null;
        }

        private Uri SetApiUrl(IConfiguration config, string apiUrl)
        {
            if (apiUrl.IsHttpUri(out Uri uri))
            {
                config.Urls.ApiUrl = uri.AbsoluteUri;
                return uri;
            }
            return null;
        }

        private void SetApiTlsPinningPublicKeyHashes(IConfiguration config, Uri btiApiUri)
        {
            HashSet<string> apiTlsPinningPublicKeyHashes = 
                EnvironmentVariableLoader.GetOrNull("BTI_API_TLS_PINNINGS").SplitToHashSet(',');
            if (apiTlsPinningPublicKeyHashes.IsNullOrEmpty())
            {
                apiTlsPinningPublicKeyHashes = GlobalConfig.BtiApiTlsPinningPublicKeyHashes.SplitToHashSet(',');
                if (!apiTlsPinningPublicKeyHashes.IsNullOrEmpty())
                {
                    SetApiTlsPinningPublicKeyHashes(config, apiTlsPinningPublicKeyHashes, btiApiUri);
                }
            }
            else
            {
                SetApiTlsPinningPublicKeyHashes(config, apiTlsPinningPublicKeyHashes, btiApiUri);
            }
        }

        private void SetApiTlsPinningPublicKeyHashes(IConfiguration config, 
            HashSet<string> apiTlsPinningPublicKeyHashes, Uri btiApiUri)
        {
            CreateTlsPinningIfNull(config);
            string btiApiDomain = btiApiUri?.Host ?? API_URL;
            TlsPinnedDomain pinnedDomain = config.TlsPinningConfig.PinnedDomains.FirstOrDefault(pd => pd.Name == btiApiDomain);
            if (pinnedDomain is null)
            {
                List<TlsPinnedDomain> pinnedDomains = new()
                {
                    new TlsPinnedDomain()
                    {
                        Name = btiApiDomain,
                        PublicKeyHashes = apiTlsPinningPublicKeyHashes,
                        Enforce = true,
                        SendReport = true,
                    }
                };
                pinnedDomains.AddRange(config.TlsPinningConfig.PinnedDomains);
                config.TlsPinningConfig.PinnedDomains = pinnedDomains;
            }
            else
            {
                pinnedDomain.PublicKeyHashes = apiTlsPinningPublicKeyHashes;
            }
        }

        private void CreateTlsPinningIfNull(IConfiguration config)
        {
            config.TlsPinningConfig ??= new TlsPinningConfig()
            {
                Enforce = false,
            };

            if (config.TlsPinningConfig.PinnedDomains is null)
            {
                config.TlsPinningConfig.PinnedDomains = new List<TlsPinnedDomain>();
            }
        }

        private void SetAlternativeRoutingTlsPinningPublicKeyHashes(IConfiguration config)
        {
            HashSet<string> alternativeRoutingTlsPinningPublicKeyHashes = 
                EnvironmentVariableLoader.GetOrNull("BTI_ALT_ROUTE_TLS_PINNINGS").SplitToHashSet(',');
            if (alternativeRoutingTlsPinningPublicKeyHashes.IsNullOrEmpty())
            {
                alternativeRoutingTlsPinningPublicKeyHashes = 
                    GlobalConfig.BtiAlternativeRoutingTlsPinningPublicKeyHashes.SplitToHashSet(',');
                if (!alternativeRoutingTlsPinningPublicKeyHashes.IsNullOrEmpty())
                {
                    SetAlternativeRoutingTlsPinningPublicKeyHashes(config, alternativeRoutingTlsPinningPublicKeyHashes);
                }
            }
            else
            {
                SetAlternativeRoutingTlsPinningPublicKeyHashes(config, alternativeRoutingTlsPinningPublicKeyHashes);
            }
        }

        private void SetAlternativeRoutingTlsPinningPublicKeyHashes(IConfiguration config, 
            HashSet<string> alternativeRoutingTlsPinningPublicKeyHashes)
        {
            CreateTlsPinningIfNull(config);
            TlsPinnedDomain pinnedDomain = config.TlsPinningConfig.PinnedDomains
                .FirstOrDefault(pd => pd.Name == DefaultConfig.ALTERNATIVE_ROUTING_HOSTNAME);
            if (pinnedDomain is null)
            {
                List<TlsPinnedDomain> pinnedDomains = config.TlsPinningConfig.PinnedDomains.ToList();
                pinnedDomains.Add(new TlsPinnedDomain()
                {
                    Name = DefaultConfig.ALTERNATIVE_ROUTING_HOSTNAME,
                    PublicKeyHashes = alternativeRoutingTlsPinningPublicKeyHashes,
                    Enforce = true,
                    SendReport = true,
                });
                config.TlsPinningConfig.PinnedDomains = pinnedDomains;
            }
            else
            {
                pinnedDomain.PublicKeyHashes = alternativeRoutingTlsPinningPublicKeyHashes;
            }
        }

        private void SetCertificateValidation(IConfiguration config)
        {
            bool? btiCertificateValidation = EnvironmentVariableLoader.GetOrNull("BTI_CERT_VALIDATION").ToBoolOrNull();
            if (btiCertificateValidation.HasValue)
            {
                SetCertificateValidation(config, btiCertificateValidation.Value);
            }
            else
            {
                btiCertificateValidation = GlobalConfig.BtiCertificateValidation.ToBoolOrNull();
                if (btiCertificateValidation.HasValue)
                {
                    SetCertificateValidation(config, btiCertificateValidation.Value);
                }
            }
        }

        private void SetCertificateValidation(IConfiguration config, bool btiCertificateValidation)
        {
            config.IsCertificateValidationDisabled = !btiCertificateValidation;
        }

        private void SetDohProviders(IConfiguration config)
        {
            List<string> dohProviders = EnvironmentVariableLoader.GetOrNull("BTI_DOH_URLS").SplitToList(',');
            if (dohProviders.IsNullOrEmpty())
            {
                dohProviders = GlobalConfig.BtiDohProviders.SplitToList(',');
                if (!dohProviders.IsNullOrEmpty())
                {
                    SetDohProviders(config, dohProviders);
                }
            }
            else
            {
                SetDohProviders(config, dohProviders);
            }
        }

        private void SetDohProviders(IConfiguration config, List<string> dohProviders)
        {
            List<Uri> uris = new();
            foreach (string dohProvider in dohProviders)
            {
                if (dohProvider.IsHttpUri(out Uri uri))
                {
                    uris.Add(uri);
                }
            }
            if (uris.Any())
            {
                config.DoHProviders = uris.Select(u => u.AbsoluteUri).ToList();
            }
        }

        private void SetServerSignaturePublicKey(IConfiguration config)
        {
            string serverSignaturePublicKey = EnvironmentVariableLoader.GetOrNull("BTI_SERVER_SIGNATURE_PUBLIC_KEY");
            if (string.IsNullOrWhiteSpace(serverSignaturePublicKey))
            {
                serverSignaturePublicKey = GlobalConfig.BtiServerSignaturePublicKey;
                if (!string.IsNullOrWhiteSpace(serverSignaturePublicKey))
                {
                    SetServerSignaturePublicKey(config, serverSignaturePublicKey);
                }
            }
            else
            {
                SetServerSignaturePublicKey(config, serverSignaturePublicKey);
            }
        }

        private void SetServerSignaturePublicKey(IConfiguration config, string serverSignaturePublicKey)
        {
            config.ServerValidationPublicKey = serverSignaturePublicKey;
        }
    }
}