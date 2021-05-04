/*
 * Copyright (c) 2021 Proton Technologies AG
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
using System.Net.Http;
using System.Threading.Tasks;
using ProtonVPN.Common.Extensions;
using ProtonVPN.Common.Threading;
using ProtonVPN.Core.Api;
using ProtonVPN.Core.Api.Contracts;
using ProtonVPN.Core.Auth;

namespace ProtonVPN.Streaming
{
    internal class StreamingServicesUpdater : ILoggedInAware, ILogoutAware
    {
        private readonly StreamingServicesStorage _streamingServicesStorage;
        private readonly IApiClient _apiClient;
        private readonly ISchedulerTimer _timer;
        private readonly SingleAction _updateAction;

        public StreamingServicesUpdater(
            StreamingServicesStorage streamingServicesStorage,
            IApiClient apiClient,
            IScheduler scheduler,
            Common.Configuration.Config config)
        {
            _streamingServicesStorage = streamingServicesStorage;
            _apiClient = apiClient;
            _timer = scheduler.Timer();
            _timer.Interval = config.StreamingServicesUpdateInterval.RandomizedWithDeviation(0.2);
            _timer.Tick += Timer_OnTick;
            _updateAction = new SingleAction(UpdateStreamingServices);
        }

        public event EventHandler<StreamingServicesResponse> StreamingServicesUpdated;

        public void OnUserLoggedIn()
        {
            _timer.Start();
        }

        public async Task Update()
        {
            await _updateAction.Run();
        }

        public void OnUserLoggedOut()
        {
            _timer.Stop();
        }

        private void Timer_OnTick(object sender, EventArgs eventArgs)
        {
            _updateAction.Run();
        }

        private async Task UpdateStreamingServices()
        {
            try
            {
                ApiResponseResult<StreamingServicesResponse> response = await _apiClient.GetStreamingServicesAsync();
                if (response.Success)
                {
                    _streamingServicesStorage.Set(response.Value);
                    NotifyStreamingServicesUpdate(response.Value);
                }
                else
                {
                    NotifyFromCache();
                }
            }
            catch (HttpRequestException)
            {
                NotifyFromCache();
            }
        }

        private void NotifyFromCache()
        {
            StreamingServicesResponse cachedStreamingServicesResponse = _streamingServicesStorage.Get();
            if (cachedStreamingServicesResponse != null)
            {
                NotifyStreamingServicesUpdate(cachedStreamingServicesResponse);
            }
        }

        private void NotifyStreamingServicesUpdate(StreamingServicesResponse response)
        {
            StreamingServicesUpdated?.Invoke(this, response);
        }
    }
}