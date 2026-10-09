/*
 * Copyright (c) 2026 Proton AG
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

using System.Net;
using System.Net.Http;
using NSubstitute;
using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Servers;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Servers.Cache;
using ProtonVPN.Client.Logic.Servers.Contracts;
using ProtonVPN.Client.Logic.Servers.Contracts.Models;
using ProtonVPN.Client.Logic.Servers.Files;
using ProtonVPN.Client.Logic.Servers.Loads;
using ProtonVPN.Client.Logic.Users.Contracts.Messages;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Common.Core.Geographical;
using ProtonVPN.Configurations.Contracts;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Logic.Servers.Tests;

[TestClass]
public class StatusRefreshTests
{
    private const string STATUS_ID = "status-id";
    private const string NEW_STATUS_ID = "new-status-id";

    private IApiClient? _apiClient;
    private IEntityMapper? _entityMapper;
    private IServersFileReaderWriter? _serversFileReaderWriter;
    private IEventMessageSender? _eventMessageSender;
    private IConfiguration? _config;
    private ISettings? _settings;
    private ILogger? _logger;
    private IIssueReporter? _issueReporter;
    private IFavoriteServersStorage? _favoriteServersStorage;
    private IServerLoadsCalculator? _serverLoadsCalculator;
    private IServerCountCache? _serverCountCache;

    private IServersCache? _serversCache;
    private IServersUpdater? _serversUpdater;

    [TestInitialize]
    public void Initialize()
    {
        _apiClient = Substitute.For<IApiClient>();
        _entityMapper = Substitute.For<IEntityMapper>();
        _serversFileReaderWriter = Substitute.For<IServersFileReaderWriter>();
        _eventMessageSender = Substitute.For<IEventMessageSender>();
        _config = Substitute.For<IConfiguration>();
        _settings = Substitute.For<ISettings>();
        _logger = Substitute.For<ILogger>();
        _issueReporter = Substitute.For<IIssueReporter>();
        _favoriteServersStorage = Substitute.For<IFavoriteServersStorage>();
        _serverLoadsCalculator = Substitute.For<IServerLoadsCalculator>();
        _serverCountCache = Substitute.For<IServerCountCache>();

        // Zero intervals keep the tests about the control flow instead of the wall clock.
        _config.ServerUpdateInterval.Returns(TimeSpan.Zero);
        _config.ServerLoadUpdateInterval.Returns(TimeSpan.Zero);
        _config.OverdueServerLoadUpdateTimeout.Returns(TimeSpan.FromSeconds(10));

        _favoriteServersStorage.Get().Returns([]);
        _serversFileReaderWriter.Read().Returns(new ServersFile());
        _settings.LastLogicalsStatusId.Returns(STATUS_ID);

        _settings.DeviceLocation.Returns(new DeviceLocation { CountryCode = "CH" });

        SetStatusResponse(isSuccess: true);
        SetLoadsCalculatorResult(areAllServersUnderMaintenance: false);

        _serversCache = new ServersCache(_apiClient, _entityMapper, _serversFileReaderWriter, _eventMessageSender,
            _config, _settings, _logger, _favoriteServersStorage, _serverLoadsCalculator);
        _serversUpdater = new ServersUpdater(_logger, _issueReporter, _serversCache, _serverCountCache, _settings, _config);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _apiClient = null;
        _entityMapper = null;
        _serversFileReaderWriter = null;
        _eventMessageSender = null;
        _config = null;
        _settings = null;
        _logger = null;
        _issueReporter = null;
        _favoriteServersStorage = null;
        _serverLoadsCalculator = null;
        _serverCountCache = null;
        _serversCache = null;
        _serversUpdater = null;
    }

    [TestMethod]
    public async Task IsStale_ReturnsFalse_WhenDeviceLocationIsUnknownOnBothSides()
    {
        _settings!.DeviceLocation.Returns((DeviceLocation?)null);

        await GivenAPopulatedServerCacheAsync();

        Assert.IsFalse(_serversCache!.IsStale(),
            "An unset device location must not compare as stale against itself.");
    }

    [TestMethod]
    public async Task UpdateAsync_RefreshesLoads_AfterLogicalsRefresh()
    {
        SetLogicalsResponse(ApiResponseResult<ServersResponse>.Ok(CreateHttpResponse(), CreateServersResponse(NEW_STATUS_ID)));

        await _serversUpdater!.ForceUpdateAsync();

        await _apiClient!.Received().GetServerLoadsAndStatusBinaryStringAsync(NEW_STATUS_ID, Arg.Any<CancellationToken>());
        Assert.IsFalse(_serversCache!.IsEmpty(), "The server list must be published once its loads are applied.");
    }

    [TestMethod]
    public async Task UpdateAsync_KeepsRefreshingLoads_AfterNotModifiedLogicalsResponse()
    {
        await GivenAPopulatedServerCacheAsync();

        SetLogicalsResponse(ApiResponseResult<ServersResponse>.NotModified(CreateHttpResponse(HttpStatusCode.NotModified)));
        _apiClient!.ClearReceivedCalls();

        await _serversUpdater!.UpdateAsync();

        await _apiClient!.Received().GetServerLoadsAndStatusBinaryStringAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task UpdateAsync_KeepsRefreshingLoads_AfterFailedLogicalsResponse()
    {
        await GivenAPopulatedServerCacheAsync();

        SetLogicalsResponse(ApiResponseResult<ServersResponse>.Fail(CreateHttpResponse(HttpStatusCode.InternalServerError), "error"));
        _apiClient!.ClearReceivedCalls();

        await _serversUpdater!.UpdateAsync();

        await _apiClient!.Received().GetServerLoadsAndStatusBinaryStringAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task UpdateAsync_KeepsRefreshingLoads_AfterNoServersError()
    {
        SetLoadsCalculatorResult(areAllServersUnderMaintenance: true);
        await GivenAPopulatedServerCacheAsync();

        Assert.IsTrue(_serversCache!.HasNoServers(), "The cache must be in the state that raises the no servers error.");

        SetLogicalsResponse(ApiResponseResult<ServersResponse>.Fail(CreateHttpResponse(HttpStatusCode.InternalServerError), "error"));
        _apiClient!.ClearReceivedCalls();

        await _serversUpdater!.UpdateAsync();

        await _apiClient!.Received().GetServerLoadsAndStatusBinaryStringAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task UpdateOverdueLoadsAsync_RefreshesLoads_WhenOverdue()
    {
        await GivenAPopulatedServerCacheAsync();
        _apiClient!.ClearReceivedCalls();

        await _serversUpdater!.UpdateOverdueLoadsAsync();

        await _apiClient!.Received().GetServerLoadsAndStatusBinaryStringAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        _issueReporter!.Received().CaptureMessage(Arg.Any<string>(), Arg.Any<string>());
    }

    [TestMethod]
    public async Task UpdateOverdueLoadsAsync_DoesNothing_WhenLoadsAreRecent()
    {
        _config!.ServerLoadUpdateInterval.Returns(TimeSpan.FromMinutes(10));
        await GivenAPopulatedServerCacheAsync();
        _apiClient!.ClearReceivedCalls();

        await _serversUpdater!.UpdateOverdueLoadsAsync();

        await _apiClient!.DidNotReceive().GetServerLoadsAndStatusBinaryStringAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        _issueReporter!.DidNotReceive().CaptureMessage(Arg.Any<string>(), Arg.Any<string>());
    }

    [TestMethod]
    public async Task UpdateOverdueLoadsAsync_DoesNotWaitForTheUpdate_BeyondTheTimeout()
    {
        await GivenAPopulatedServerCacheAsync();
        _config!.OverdueServerLoadUpdateTimeout.Returns(TimeSpan.FromMilliseconds(50));
        TaskCompletionSource<ApiResponseResult<byte[]>> neverCompletes = new();
        _apiClient!.GetServerLoadsAndStatusBinaryStringAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(neverCompletes.Task);

        Task update = _serversUpdater!.UpdateOverdueLoadsAsync();

        Assert.AreSame(update, await Task.WhenAny(update, Task.Delay(TimeSpan.FromSeconds(5))),
            "The connect path must not wait for a slow loads update.");
    }

    [TestMethod]
    public async Task UpdateOverdueLoadsAsync_DoesNothing_WhenOnlyTheDeviceLocationChanged()
    {
        _config!.ServerLoadUpdateInterval.Returns(TimeSpan.FromMinutes(10));
        await GivenAPopulatedServerCacheAsync();
        _apiClient!.ClearReceivedCalls();

        _settings!.DeviceLocation.Returns(new DeviceLocation { CountryCode = "US" });

        await _serversUpdater!.UpdateOverdueLoadsAsync();

        await _apiClient.DidNotReceive().GetServerLoadsAndStatusBinaryStringAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task UpdateAsync_KeepsRetryingLogicals_WhenLoadsAreRefreshedAfterADeviceLocationChange()
    {
        // Long enough that only the staleness, and not the age of the cache, can trigger the retry.
        _config!.ServerUpdateInterval.Returns(TimeSpan.FromHours(12));
        await GivenAPopulatedServerCacheAsync();

        _settings!.DeviceLocation.Returns(new DeviceLocation { CountryCode = "US" });
        SetLogicalsResponse(ApiResponseResult<ServersResponse>.Fail(CreateHttpResponse(HttpStatusCode.InternalServerError), "error"));

        await _serversUpdater!.UpdateAsync();

        await _apiClient!.Received().GetServerLoadsAndStatusBinaryStringAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        Assert.IsTrue(_serversCache!.IsStale(),
            "A loads update must not clear the staleness of a list fetched for another device location.");

        _apiClient.ClearReceivedCalls();

        await _serversUpdater.UpdateAsync();

        await _apiClient.Received().GetServersAsync(Arg.Any<DeviceLocation?>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task UpdateAsync_KeepsRetryingLogicals_WhenLoadsAreRefreshedAfterAPlanChange()
    {
        // Long enough that only the staleness, and not the age of the cache, can trigger the retry.
        _config!.ServerUpdateInterval.Returns(TimeSpan.FromHours(12));
        await GivenAPopulatedServerCacheAsync();

        SetMaxTier(2);
        SetLogicalsResponse(ApiResponseResult<ServersResponse>.Fail(CreateHttpResponse(HttpStatusCode.InternalServerError), "error"));

        await _serversUpdater!.UpdateAsync();

        await _apiClient!.Received().GetServerLoadsAndStatusBinaryStringAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        Assert.IsTrue(_serversCache!.IsStale(),
            "A loads update must not clear the staleness of a list fetched for another plan. The logicals "
            + "response is user dependent, so the list has to be refetched, not just rescored.");

        _apiClient.ClearReceivedCalls();

        await _serversUpdater.UpdateAsync();

        await _apiClient.Received().GetServersAsync(Arg.Any<DeviceLocation?>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task UpdateLoadsAsync_DoesNotThrow_WhenTheLoadsCalculationFails()
    {
        await GivenAPopulatedServerCacheAsync();

        _serverLoadsCalculator!
            .UpdateServerLoads(Arg.Any<IReadOnlyList<Server>>(), Arg.Any<byte[]>(), Arg.Any<DeviceLocation?>())
            .Returns(_ => throw new InvalidOperationException("Native loads calculation failed."));

        // Reachable from a UI command that has no exception handling of its own.
        await _serversUpdater!.ForceUpdateLoadsAsync();
    }

    [TestMethod]
    public async Task ForceUpdateLoadsAsync_DoesNothing_WhenTheCacheIsEmpty()
    {
        Assert.IsTrue(_serversCache!.IsEmpty(), "The cache must be empty for the tested short circuit.");

        await _serversUpdater!.ForceUpdateLoadsAsync();

        await _apiClient!.DidNotReceive().GetServerLoadsAndStatusBinaryStringAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task UpdateAsync_RecomputesScoresWithoutARequest_WhenOnlyTheCoordinatesChanged()
    {
        _config!.ServerUpdateInterval.Returns(TimeSpan.FromHours(12));
        _config.ServerLoadUpdateInterval.Returns(TimeSpan.FromMinutes(10));
        SetDeviceLocation(countryCode: "CH", ipAddress: "1.2.3.4", latitude: 47.3, longitude: 8.5);
        await GivenAPopulatedServerCacheAsync();
        _apiClient!.ClearReceivedCalls();
        _serverLoadsCalculator!.ClearReceivedCalls();

        SetDeviceLocation(countryCode: "CH", ipAddress: "1.2.3.4", latitude: 46.2, longitude: 6.1);

        await _serversUpdater!.UpdateAsync();

        // The scores are computed on device from the distance to it, so the retained status blob is
        // enough. Spending a request on data that did not change would be waste.
        _serverLoadsCalculator!.Received().UpdateServerLoads(
            Arg.Any<IReadOnlyList<Server>>(), Arg.Any<byte[]>(), Arg.Any<DeviceLocation?>());
        await _apiClient!.DidNotReceive().GetServerLoadsAndStatusBinaryStringAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _apiClient!.DidNotReceive().GetServersAsync(Arg.Any<DeviceLocation?>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());

        Assert.IsFalse(_serversCache!.IsLoadStale(), "The recompute must move the stored coordinates on.");
    }

    [TestMethod]
    public async Task UpdateAsync_RefetchesLogicals_WhenTheDeviceIpAddressChanged()
    {
        // Long enough that only the staleness, and not the age of the cache, can trigger the refetch.
        _config!.ServerUpdateInterval.Returns(TimeSpan.FromHours(12));
        SetDeviceLocation(countryCode: "CH", ipAddress: "1.2.3.4");
        await GivenAPopulatedServerCacheAsync();
        _apiClient!.ClearReceivedCalls();

        // The IP address is sent as x-pm-netzone, so the list the API returned for it no longer applies.
        SetDeviceLocation(countryCode: "CH", ipAddress: "5.6.7.8");

        await _serversUpdater!.UpdateAsync();

        await _apiClient!.Received().GetServersAsync(Arg.Any<DeviceLocation?>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task UpdateAsync_DoesNotRecomputeScores_WhenTheCountryChangedTooAndTheListIsBeingRefetched()
    {
        _config!.ServerUpdateInterval.Returns(TimeSpan.FromHours(12));
        _config.ServerLoadUpdateInterval.Returns(TimeSpan.FromMinutes(10));
        SetDeviceLocation(countryCode: "CH", ipAddress: "1.2.3.4", latitude: 47.3, longitude: 8.5);
        await GivenAPopulatedServerCacheAsync();
        _apiClient!.ClearReceivedCalls();
        _serverLoadsCalculator!.ClearReceivedCalls();

        SetDeviceLocation(countryCode: "FR", ipAddress: "5.6.7.8", latitude: 48.8, longitude: 2.3);

        await _serversUpdater!.UpdateAsync();

        await _apiClient!.Received().GetServersAsync(Arg.Any<DeviceLocation?>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());

        // One scoring pass, from the refetch. A successful fetch stamps the current coordinates, and that
        // alone is what leaves the recompute nothing to do.
        _serverLoadsCalculator!.Received(1).UpdateServerLoads(
            Arg.Any<IReadOnlyList<Server>>(), Arg.Any<byte[]>(), Arg.Any<DeviceLocation?>());
    }

    [TestMethod]
    public async Task UpdateAsync_StillRecomputesScores_WhenTheLogicalsRequestFailed()
    {
        _config!.ServerUpdateInterval.Returns(TimeSpan.FromHours(12));
        _config.ServerLoadUpdateInterval.Returns(TimeSpan.FromMinutes(10));
        SetDeviceLocation(countryCode: "CH", ipAddress: "1.2.3.4", latitude: 47.3, longitude: 8.5);
        await GivenAPopulatedServerCacheAsync();
        _serverLoadsCalculator!.ClearReceivedCalls();

        // The country change makes the list stale, so the failing request is a forced one.
        SetDeviceLocation(countryCode: "FR", ipAddress: "1.2.3.4", latitude: 46.2, longitude: 6.1);
        SetLogicalsResponse(ApiResponseResult<ServersResponse>.Fail(CreateHttpResponse(HttpStatusCode.InternalServerError), "error"));

        await _serversUpdater!.UpdateAsync();

        // Rescoring the cached list costs no request, so a failed fetch must not starve it.
        _serverLoadsCalculator!.Received().UpdateServerLoads(
            Arg.Any<IReadOnlyList<Server>>(), Arg.Any<byte[]>(), Arg.Any<DeviceLocation?>());
        Assert.IsFalse(_serversCache!.IsLoadStale(), "The recompute must move the stored coordinates on.");
        Assert.IsTrue(_serversCache.IsStale(), "The list itself is still stale, it was never refetched.");
    }

    [TestMethod]
    public void IsStale_ReturnsTrue_WhenTheIpAddressChangedWhileTheAppWasClosed()
    {
        // The cache comes from a file written in an earlier session, for a netzone the device has left.
        GivenAServersFile(countryCode: "CH", ipAddress: "1.2.3.4");
        SetDeviceLocation(countryCode: "CH", ipAddress: "5.6.7.8");

        _serversCache!.LoadFromFileIfEmpty();

        // The IP address is sent as x-pm-netzone, so only a new logicals fetch can put the list right.
        // Seeding it from the current location instead would compare the value against itself.
        Assert.IsTrue(_serversCache.IsStale());
    }

    [TestMethod]
    public void IsStale_ReturnsFalse_WhenTheIpAddressIsUnchangedSinceTheFileWasWritten()
    {
        GivenAServersFile(countryCode: "CH", ipAddress: "1.2.3.4");
        SetDeviceLocation(countryCode: "CH", ipAddress: "1.2.3.4");

        _serversCache!.LoadFromFileIfEmpty();

        Assert.IsFalse(_serversCache.IsStale());
    }

    [TestMethod]
    public async Task UpdateAsync_PersistsTheIpAddressTheListWasFetchedWith()
    {
        SetDeviceLocation(countryCode: "CH", ipAddress: "1.2.3.4");

        await GivenAPopulatedServerCacheAsync();

        _serversFileReaderWriter!.Received().Save(Arg.Is<ServersFile>(f => f.DeviceIpAddress == "1.2.3.4"));
    }

    [TestMethod]
    public void RecomputeLoads_DoesNothing_WhenNoStatusFileHasBeenRetained()
    {
        SetDeviceLocation(countryCode: "CH", latitude: 47.3, longitude: 8.5);

        // Nothing has been fetched yet, so there is no blob to score against.
        _serversCache!.RecomputeLoads();

        _serverLoadsCalculator!.DidNotReceive().UpdateServerLoads(
            Arg.Any<IReadOnlyList<Server>>(), Arg.Any<byte[]>(), Arg.Any<DeviceLocation?>());
    }

    private void GivenAServersFile(string countryCode, string ipAddress)
    {
        _serversFileReaderWriter!.Read().Returns(new ServersFile
        {
            DeviceCountryLocation = countryCode,
            DeviceIpAddress = ipAddress,
            UserMaxTier = 0,
            Servers = [CreateServer()],
        });
    }

    private void SetDeviceLocation(string countryCode, string ipAddress = "", double? latitude = null, double? longitude = null)
    {
        _settings!.DeviceLocation.Returns(new DeviceLocation
        {
            CountryCode = countryCode,
            IpAddress = ipAddress,
            Isp = string.Empty,
            Latitude = latitude,
            Longitude = longitude,
        });
    }

    private void SetMaxTier(sbyte maxTier)
    {
        _settings!.VpnPlan.Returns(new VpnPlan(title: "Plus", name: "vpn2022", maxTier: maxTier, isB2B: false));
    }

    private async Task GivenAPopulatedServerCacheAsync()
    {
        SetLogicalsResponse(ApiResponseResult<ServersResponse>.Ok(CreateHttpResponse(), CreateServersResponse(STATUS_ID)));

        await _serversUpdater!.ForceUpdateAsync();

        Assert.IsFalse(_serversCache!.IsEmpty(), "The cache must be populated before the tested update.");
    }

    private void SetLogicalsResponse(ApiResponseResult<ServersResponse> response)
    {
        _apiClient!.GetServersAsync(Arg.Any<DeviceLocation?>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
            .Returns(response);
    }

    private void SetStatusResponse(bool isSuccess)
    {
        _apiClient!.GetServerLoadsAndStatusBinaryStringAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(isSuccess
                ? ApiResponseResult<byte[]>.Ok(CreateHttpResponse(), [1])
                : ApiResponseResult<byte[]>.Fail(CreateHttpResponse(HttpStatusCode.InternalServerError), "error"));
    }

    private void SetLoadsCalculatorResult(bool areAllServersUnderMaintenance)
    {
        _serverLoadsCalculator!
            .UpdateServerLoads(Arg.Any<IReadOnlyList<Server>>(), Arg.Any<byte[]>(), Arg.Any<DeviceLocation?>())
            .Returns(callInfo =>
            {
                foreach (Server server in callInfo.Arg<IReadOnlyList<Server>>())
                {
                    server.Status = (sbyte)(areAllServersUnderMaintenance ? 0 : 1);
                    server.IsVisible = true;
                    server.IsAutoconnectable = true;
                }
                return true;
            });
    }

    private ServersResponse CreateServersResponse(string statusId)
    {
        ServersResponse response = new()
        {
            Servers = [new LogicalServerResponse { Id = "server-id" }],
            StatusId = statusId,
        };

        _entityMapper!.Map<LogicalServerResponse, Server>(Arg.Any<IEnumerable<LogicalServerResponse>>())
            .Returns(_ => [CreateServer()]);

        return response;
    }

    private static Server CreateServer()
    {
        return new Server
        {
            Id = "server-id",
            Name = "CH#1",
            City = "Zurich",
            State = string.Empty,
            EntryCountry = "CH",
            ExitCountry = "CH",
            HostCountry = string.Empty,
            Domain = "node-ch-01.protonvpn.net",
            GatewayName = string.Empty,
            Status = 1,
            StatusReference = new StatusReference(),
            EntryLocation = new GeoLocation(),
            ExitLocation = new GeoLocation(),
            Servers =
            [
                new PhysicalServer
                {
                    Id = "physical-server-id",
                    EntryIp = "1.2.3.4",
                    ExitIp = "1.2.3.4",
                    Domain = "node-ch-01.protonvpn.net",
                    Label = "0",
                    Status = 1,
                    X25519PublicKey = string.Empty,
                    Signature = string.Empty,
                }
            ],
        };
    }

    private static HttpResponseMessage CreateHttpResponse(HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode) { Content = new StringContent(string.Empty) };
    }
}
