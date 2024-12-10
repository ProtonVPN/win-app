/*
 * Copyright (c) 2024 Proton AG
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

using System.Reflection;
using NSubstitute;
using ProtonVPN.Client.Contracts.Profiles;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Browsing;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Connection.ConnectionErrors;
using ProtonVPN.Client.Logic.Connection.Contracts;
using ProtonVPN.Client.Logic.Connection.Contracts.Enums;
using ProtonVPN.Client.Logic.Connection.Contracts.Extensions;
using ProtonVPN.Client.Logic.Profiles.Contracts.Models;
using ProtonVPN.Client.Settings.Contracts;

namespace ProtonVPN.Client.Logic.Connection.Tests;

[TestClass]
public class ConnectionErrorFactoryTest
{
    private readonly Dictionary<VpnError, Type> _connectionErrorMap = new()
    {
        { VpnError.None, typeof(NoConnectionError) },
        { VpnError.NoneKeepEnabledKillSwitch, typeof(NoConnectionError) },
        { VpnError.NoServers, typeof(NoServersConnectionError) },
        { VpnError.WireGuardAdapterInUseError, typeof(WireGuardAdapterInUseConnectionError) },
        { VpnError.MissingConnectionCertificate, typeof(MissingConnectionCertificateError) },
        { VpnError.TlsCertificateError, typeof(TlsCertificateConnectionError) },
        { VpnError.NoTapAdaptersError, typeof(NoTapAdaptersConnectionError) },
        { VpnError.TapAdapterInUseError, typeof(TapAdapterInUseConnectionError) },
        { VpnError.TapRequiresUpdateError, typeof(TapRequiresUpdateConnectionError) },
        { VpnError.RpcServerUnavailable, typeof(RpcServerUnavailableConnectionError) },
        { VpnError.SessionLimitReachedBasic, typeof(SessionLimitReachedConnectionError) },
        { VpnError.SessionLimitReachedFree, typeof(SessionLimitReachedConnectionError) },
        { VpnError.SessionLimitReachedPlus, typeof(SessionLimitReachedConnectionError) },
        { VpnError.SessionLimitReachedPro, typeof(SessionLimitReachedConnectionError) },
        { VpnError.SessionLimitReachedVisionary, typeof(SessionLimitReachedConnectionError) },
        { VpnError.SessionLimitReachedUnknown, typeof(SessionLimitReachedConnectionError) },
    };

    private ISettings? _settings;
    private IConnectionManager? _connectionManager;
    private ILocalizationProvider? _localizer;
    private IProfileEditor? _profileEditor;
    private IUrlsBrowser? _urlsBrowser;
    private IReportIssueWindowActivator? _reportIssueWindowActivator;

    [TestInitialize]
    public void TestInitialize()
    {
        _settings = Substitute.For<ISettings>();
        _connectionManager = Substitute.For<IConnectionManager>();
        _localizer = Substitute.For<ILocalizationProvider>();
        _profileEditor = Substitute.For<IProfileEditor>();
        _urlsBrowser = Substitute.For<IUrlsBrowser>();
        _reportIssueWindowActivator = Substitute.For<IReportIssueWindowActivator>();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _settings = null;
        _connectionManager = null;
        _localizer = null;
        _profileEditor = null;
        _urlsBrowser = null;
        _reportIssueWindowActivator = null;
    }

    [TestMethod]
    public void GetConnectionError_ShouldReturn_ConnectionErrorForEachUserError()
    {
        FieldInfo? errorsForUserField = typeof(VpnErrorExtensions).GetField("_errorsForUser", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(errorsForUserField);

        List<VpnError>? errorsForUser = (List<VpnError>?)errorsForUserField?.GetValue(null);
        Assert.IsNotNull(errorsForUser);

        ConnectionErrorFactory connectionErrorFactory = new(_connectionManager!, GetConnectionErrors(), GetUnknownConnectionError());

        foreach (VpnError error in errorsForUser)
        {
            IConnectionError connectionError = connectionErrorFactory.GetConnectionError(error);
            Assert.IsInstanceOfType(connectionError, _connectionErrorMap[error]);
        }
    }

    [TestMethod]
    public void GetConnectionError_ShouldReturn_NoServersForProfileConnectionError()
    {
        // Arrange
        _connectionManager!.CurrentConnectionIntent.Returns(ConnectionProfile.Default);
        ConnectionErrorFactory connectionErrorFactory = new(_connectionManager!, GetConnectionErrors(), GetUnknownConnectionError());

        // Act
        IConnectionError connectionError = connectionErrorFactory.GetConnectionError(VpnError.NoServers);

        // Assert
        Assert.IsInstanceOfType<NoServersForProfileConnectionError>(connectionError);
    }

    private Lazy<IEnumerable<IConnectionError>> GetConnectionErrors()
    {
        return new Lazy<IEnumerable<IConnectionError>>(() => [
            new NoConnectionError(),
            new NoTapAdaptersConnectionError(_localizer!, _urlsBrowser!),
            new TapAdapterInUseConnectionError(_localizer!, _urlsBrowser!),
            new TapRequiresUpdateConnectionError(_localizer!, _urlsBrowser!),
            new RpcServerUnavailableConnectionError(_localizer!, _urlsBrowser!),
            new NoServersForProfileConnectionError(_localizer!, _profileEditor!, _connectionManager!),
            new NoServersConnectionError(_localizer!, _reportIssueWindowActivator!),
            new WireGuardAdapterInUseConnectionError(_localizer!, _connectionManager!),
            new MissingConnectionCertificateError(_localizer!, _reportIssueWindowActivator!),
            new TlsCertificateConnectionError(_localizer!, _reportIssueWindowActivator!),
            new SessionLimitReachedConnectionError(_localizer!, _settings!, _reportIssueWindowActivator!),
        ]);
    }

    private Lazy<UnknownConnectionError> GetUnknownConnectionError()
    {
        return new(() => new UnknownConnectionError(_localizer!, _reportIssueWindowActivator!));
    }
}