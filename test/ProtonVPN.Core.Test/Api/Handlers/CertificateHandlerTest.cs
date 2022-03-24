/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using ProtonVPN.Common.Configuration.Api.Handlers.TlsPinning;
using ProtonVPN.Core.Api.Handlers;
using ProtonVPN.Core.Api.Handlers.TlsPinning;

namespace ProtonVPN.Core.Test.Api.Handlers
{
    [TestClass]
    public class CertificateHandlerTest
    {
        private IReportClient _reportClient;
        private X509Certificate _apiCert;
        private X509Certificate _alternativeHostCert;

        private readonly string _unknownHost = "unknown.host.com";
        private readonly string _apiHost = "api.protonvpn.ch";
        private readonly string _alternativeHost = "alternative.host.com";

        [TestInitialize]
        public void TestInitialize()
        {
            _reportClient = Substitute.For<IReportClient>();
            _apiCert = X509Certificate.CreateFromCertFile("TestData\\api.protonvpn.ch.cer");
            _alternativeHostCert = X509Certificate.CreateFromCertFile("TestData\\alternative.host.cer");
        }

        #region Unknown domain tests

        [TestMethod]
        public void ItShouldReturnTrue()
        {
            var certificateHandler = new TestCertificateHandler(GetApiTlsPinningConfig(false), _reportClient);

            certificateHandler.GetValidationResult(_unknownHost, _apiCert, SslPolicyErrors.None).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnFalseWhenEnforceIsOn()
        {
            var certificateHandler = new TestCertificateHandler(GetApiTlsPinningConfig(true), _reportClient);

            certificateHandler.GetValidationResult(_unknownHost, _apiCert, SslPolicyErrors.None).Should().BeFalse();
        }

        [TestMethod]
        public void ItShouldReturnFalseWhenEnforceIsOnAndSslError()
        {
            var certificateHandler = new TestCertificateHandler(GetApiTlsPinningConfig(true), _reportClient);

            certificateHandler.GetValidationResult(_unknownHost, _apiCert, SslPolicyErrors.RemoteCertificateNameMismatch).Should().BeFalse();
        }

        #endregion


        #region Known domain tests

        [TestMethod]
        public void ItShouldReturnFalseWhenSslError()
        {
            var certificateHandler = new TestCertificateHandler(GetApiTlsPinningConfig(true), _reportClient);

            certificateHandler.GetValidationResult(_apiHost, _apiCert, SslPolicyErrors.RemoteCertificateNameMismatch).Should().BeFalse();
        }

        [TestMethod]
        public void ItShouldReturnTrueWhenEnforceIsOff()
        {
            var certificateHandler = new TestCertificateHandler(GetApiTlsPinningConfig(false), _reportClient);

            certificateHandler.GetValidationResult(_apiHost, _apiCert, SslPolicyErrors.None).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnTrueWhenPinIsValid()
        {
            var certificateHandler = new TestCertificateHandler(GetApiTlsPinningConfig(true), _reportClient);

            certificateHandler.GetValidationResult(_apiHost, _apiCert, SslPolicyErrors.None).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnFalseWhenPinIsNotValid()
        {
            var config = GetApiTlsPinningConfig(true);
            config.PinnedDomains = new List<TlsPinnedDomain>();

            var certificateHandler = new TestCertificateHandler(config, _reportClient);

            certificateHandler.GetValidationResult(_apiHost, _apiCert, SslPolicyErrors.None).Should().BeFalse();
        }

        #endregion

        #region Alternative api tests

        [TestMethod]
        public void ItShouldReturnTrueWhenAlternativeHostPinIsValid()
        {
            var config = GetAlternativeApiTlsPinningConfig(true);
            var certificateHandler = new TestCertificateHandler(config, _reportClient);

            certificateHandler.GetValidationResult(_alternativeHost, _alternativeHostCert, SslPolicyErrors.None).Should().BeTrue();
        }

        [TestMethod]
        public void ItShouldReturnFalseWhenAlternativeHostPinIsInvalid()
        {
            var config = GetAlternativeApiTlsPinningConfig(true);
            config.PinnedDomains = new List<TlsPinnedDomain>();
            var certificateHandler = new TestCertificateHandler(config, _reportClient);

            certificateHandler.GetValidationResult(_alternativeHost, _alternativeHostCert, SslPolicyErrors.None).Should().BeFalse();
        }

        #endregion

        [TestMethod]
        public void ItShouldSendTlsPinReportWhenPinIsNotValid()
        {
            var config = GetIncorrectTlsPinningConfig(true);
            var certificateHandler = new TestCertificateHandler(config, _reportClient);
            certificateHandler.GetValidationResult(_apiHost, _apiCert, SslPolicyErrors.None);

            _reportClient.ReceivedWithAnyArgs().Send(null);
        }

        private TlsPinningConfig GetApiTlsPinningConfig(bool enforce)
        {
            var builder = new PinConfigBuilder(enforce);
            builder.AddDomain(_apiHost, enforce, new List<string>
            {
                "IEwk65VSaxv3s1/88vF/rM8PauJoIun3rzVCX5mLS3M=",
                "drtmcR2kFkM8qJClsuWgUzxgBkePfRCkRpqUesyDmeE=",
                "YRGlaY0jyJ4Jw2/4M8FIftwbDIQfh8Sdro96CeEel54=",
                "AfMENBVvOS8MnISprtvyPsjKlPooqh8nMB/pvCrpJpw=",
            });

            return builder.Config();
        }

        private TlsPinningConfig GetIncorrectTlsPinningConfig(bool enforce)
        {
            var builder = new PinConfigBuilder(enforce);
            builder.AddDomain(_apiHost, enforce, new List<string>
            {
                "wrong pin",
                "another wrong pin"
            });

            return builder.Config();
        }

        private TlsPinningConfig GetAlternativeApiTlsPinningConfig(bool enforce)
        {
            var builder = new PinConfigBuilder(enforce);
            builder.AddDomain(_alternativeHost, enforce, new List<string>
            {
                "EU6TS9MO0L/GsDHvVc9D5fChYLNy5JdGYpJw0ccgetM=",
                "iKPIHPnDNqdkvOnTClQ8zQAIKG0XavaPkcEo0LBAABA=",
                "MSlVrBCdL0hKyczvgYVSRNm88RicyY04Q2y5qrBt0xA=",
                "C2UxW0T1Ckl9s+8cXfjXxlEqwAfPM4HiW2y3UdtBeCw=",
            });

            return builder.Config();
        }
    }

    internal class PinConfigBuilder
    {
        private readonly TlsPinningConfig _config;
        private readonly List<TlsPinnedDomain> _domains = new List<TlsPinnedDomain>();

        public PinConfigBuilder(bool enforce)
        {
            _config = new TlsPinningConfig {PinnedDomains = new List<TlsPinnedDomain>(), Enforce = enforce};
        }

        public PinConfigBuilder AddDomain(string domain, bool enforce, List<string> pins)
        {
            _domains.Add(new TlsPinnedDomain
            {
                Name = domain,
                PublicKeyHashes = pins,
                Enforce = enforce,
                SendReport = true
            });

            return this;
        }

        public TlsPinningConfig Config()
        {
            _config.PinnedDomains = _domains;
            return _config;
        }
    }

    internal class TestCertificateHandler : CertificateHandler
    {
        public TestCertificateHandler(TlsPinningConfig config, IReportClient reportClient) : base(config, reportClient)
        {
        }

        public bool GetValidationResult(string host, X509Certificate cert, SslPolicyErrors sslPolicyErrors)
        {
            return CertificateCustomValidationCallback(
                new HttpRequestMessage { Headers = { Host = host }, RequestUri = new UriBuilder(new Uri("https://host.com")).Uri},
                cert,
                new X509Chain(),
                sslPolicyErrors);
        }
    }
}
