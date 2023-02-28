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

using Microsoft.Win32;
using ProtonVPN.Common.Logging;
using ProtonVPN.Common.Logging.Categorization.Events.ProcessCommunicationLogs;
using ProtonVPN.ProcessCommunication.Contracts.Registration;

namespace ProtonVPN.ProcessCommunication.Common.Registration
{
    public class ServiceServerPortRegister : IServiceServerPortRegister
    {
        private const string PATH = "SOFTWARE\\Proton AG\\Proton VPN\\gRPC";
        private const string KEY = "ServiceServerPort";
        private static readonly TimeSpan DELAY = TimeSpan.FromMilliseconds(100);

        private readonly ILogger _logger;

        public ServiceServerPortRegister(ILogger logger)
        {
            _logger = logger;
        }

        public void Write(int serverBoundPort)
        {
            RegistryKey key = OpenBaseKey().CreateSubKey(PATH);
            if (key == null)
            {
                string errorMessage = "Failed to write gRPC server port to registry.";
                _logger.Error<ProcessCommunicationErrorLog>(errorMessage);
                throw new Exception(errorMessage);
            }
            try
            {
                key.SetValue(KEY, serverBoundPort, RegistryValueKind.DWord);
            }
            finally
            {
                key.Close();
            }
        }

        private RegistryKey OpenBaseKey()
        {
            return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        }

        public void Delete()
        {
            RegistryKey key = OpenBaseKey().OpenSubKey(PATH, RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (key != null)
            {
                try
                {
                    key.DeleteValue(KEY);
                }
                catch
                {
                }
                finally
                {
                    key.Close();
                }
            }
        }

        public int? ReadOnce()
        {
            object rawRegistryValue = ReadRawRegistryValue();
            return ParseRawRegistryValue(rawRegistryValue);
        }

        private object ReadRawRegistryValue()
        {
            RegistryKey key = null;
            try
            {
                key = OpenBaseKey().OpenSubKey(PATH);
                if (key == null)
                {
                    _logger.Error<ProcessCommunicationErrorLog>("Failed to open gRPC server port key from registry.");
                    return null;
                }
                object? serverBoundPortObject = key.GetValue(KEY);
                return serverBoundPortObject;
            }
            catch (Exception e)
            {
                _logger.Error<ProcessCommunicationErrorLog>("Failed to read gRPC server port from registry.", e);
                return null;
            }
            finally
            {
                try
                {
                    key?.Close();
                }
                catch
                {
                }
            }
        }

        private int? ParseRawRegistryValue(object registryValue)
        {
            if (registryValue is not null)
            {
                int serverBoundPort;
                try
                {
                    serverBoundPort = (int)registryValue;
                    return serverBoundPort;
                }
                catch
                {
                }
                try
                {
                    int.TryParse(registryValue.ToString(), out serverBoundPort);
                    return serverBoundPort;
                }
                catch
                {
                }
            }
            return null;
        }

        public async Task<int> ReadAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                int? serverBoundPort = ReadOnce();
                if (serverBoundPort.HasValue && serverBoundPort.Value > 0)
                {
                    return serverBoundPort.Value;
                }
                await Task.Delay(DELAY, cancellationToken);
            }
        }
    }
}