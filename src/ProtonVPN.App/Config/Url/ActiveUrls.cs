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

using ProtonVPN.Common.Configuration;
using ProtonVPN.Common.OS.Processes;

namespace ProtonVPN.Config.Url
{
    public class ActiveUrls : IActiveUrls
    {
        private readonly UrlConfig _config;
        private readonly IOsProcesses _processes;

        public ActiveUrls(IConfiguration config, IOsProcesses processes)
        {
            _config = config.Urls;
            _processes = processes;
        }

        public IActiveUrl BfeArticleUrl                           => Url(_config.BfeArticleUrl);
        public IActiveUrl PasswordResetUrl                        => Url(_config.PasswordResetUrl);
        public IActiveUrl ForgetUsernameUrl                       => Url(_config.ForgetUsernameUrl);
        public IActiveUrl DownloadUrl                             => Url(_config.DownloadUrl);
        public IActiveUrl ApiUrl                                  => Url(_config.ApiUrl);
        public IActiveUrl TlsReportUrl                            => Url(_config.TlsReportUrl);
        public IActiveUrl HelpUrl                                 => Url(_config.HelpUrl);
        public IActiveUrl AccountUrl                              => Url(_config.AccountUrl);
        public IActiveUrl AboutSecureCoreUrl                      => Url(_config.AboutSecureCoreUrl);
        public IActiveUrl RegisterUrl                             => Url(_config.RegisterUrl);
        public IActiveUrl TroubleShootingUrl                      => Url(_config.TroubleShootingUrl);
        public IActiveUrl P2PStatusUrl                            => Url(_config.P2PStatusUrl);
        public IActiveUrl ProtonMailPricingUrl                    => Url(_config.ProtonMailPricingUrl);
        public IActiveUrl PublicWifiSafetyUrl                     => Url(_config.PublicWifiSafetyUrl);
        public IActiveUrl ProtonStatusUrl                         => Url(_config.ProtonStatusUrl);
        public IActiveUrl TorBrowserUrl                           => Url(_config.TorBrowserUrl);
        public IActiveUrl ProtonTwitterUrl                        => Url(_config.ProtonTwitterUrl);
        public IActiveUrl SupportFormUrl                          => Url(_config.SupportFormUrl);
        public IActiveUrl AlternativeRoutingUrl                   => Url(_config.AlternativeRoutingUrl);
        public IActiveUrl AboutNetShieldUrl                       => Url(_config.AboutNetShieldUrl);
        public IActiveUrl AboutKillSwitchUrl                      => Url(_config.AboutKillSwitchUrl);
        public IActiveUrl AboutPortForwardingUrl                  => Url(_config.AboutPortForwardingUrl);
        public IActiveUrl PortForwardingRisksUrl                  => Url(_config.PortForwardingRisksUrl);
        public IActiveUrl AboutModerateNatUrl                     => Url(_config.AboutModerateNatUrl);
        public IActiveUrl InvoicesUrl                             => Url(_config.InvoicesUrl);
        public IActiveUrl SmartRoutingUrl                         => Url(_config.SmartRoutingUrl);
        public IActiveUrl StreamingUrl                            => Url(_config.StreamingUrl);
        public IActiveUrl P2PUrl                                  => Url(_config.P2PUrl);
        public IActiveUrl TorUrl                                  => Url(_config.TorUrl);
        public IActiveUrl AboutSmartProtocolUrl                   => Url(_config.AboutSmartProtocolUrl);
        public IActiveUrl IncorrectSystemTimeArticleUrl           => Url(_config.IncorrectSystemTimeArticleUrl);
        public IActiveUrl AssignVpnConnectionsUrl                 => Url(_config.AssignVpnConnectionsUrl);
        public IActiveUrl NonStandardPortsUrl                     => Url(_config.NonStandardPortsUrl);
        public IActiveUrl LoginProblemsUrl                        => Url(_config.LoginProblemsUrl);
        public IActiveUrl RebrandingUrl                           => Url(_config.RebrandingUrl);
        public IActiveUrl RpcServerProblemUrl                     => Url(_config.RpcServerProblemUrl);

        private ActiveUrl Url(string url)
        {
            return new(_processes, url);
        }
    }
}