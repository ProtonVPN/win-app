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
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.ProcessCommunicationLogs;

namespace ProtonVPN.ProcessCommunication.Common.Registration
{
    public abstract class ServerPortRegisterBase
    {
        private const string PATH = "SOFTWARE\\Proton AG\\Proton VPN\\gRPC";

        protected ILogger Logger { get; private set; }

        protected ServerPortRegisterBase(ILogger logger)
        {
            Logger = logger;
        }

        public void Write(int serverBoundPort)
        {
            RegistryKey key = OpenBaseKey().CreateSubKey(PATH);
            if (key == null)
            {
                string errorMessage = "Failed to open registry path before writing the gRPC server port.";
                Logger.Error<ProcessCommunicationErrorLog>(errorMessage);
                throw new Exception(errorMessage);
            }
            try
            {
                key.SetValue(GetKey(), serverBoundPort, RegistryValueKind.DWord);
            }
            catch (Exception ex)
            {
                Logger.Error<ProcessCommunicationErrorLog>("Failed when writing the gRPC server port.", ex);
            }
            finally
            {
                key?.Close();
            }
        }

        private RegistryKey OpenBaseKey()
        {
            return RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        }

        protected abstract string GetKey();

        public void Delete()
        {
            RegistryKey key = OpenBaseKey().OpenSubKey(PATH, RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (key != null)
            {
                try
                {
                    key.DeleteValue(GetKey());
                }
                catch (Exception ex)
                {
                    Logger.Error<ProcessCommunicationErrorLog>("Failed when deleting the gRPC server port.", ex);
                }
                finally
                {
                    key?.Close();
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
                    Logger.Error<ProcessCommunicationErrorLog>("Failed to open registry path before reading the gRPC server port.");
                    return null;
                }
                object? serverBoundPortObject = key.GetValue(GetKey());
                return serverBoundPortObject;
            }
            catch (Exception ex)
            {
                Logger.Error<ProcessCommunicationErrorLog>("Failed to read gRPC server port from registry.", ex);
                return null;
            }
            finally
            {
                key?.Close();
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
                    if (int.TryParse(registryValue.ToString(), out serverBoundPort))
                    {
                        return serverBoundPort;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error<ProcessCommunicationErrorLog>("Failed when parsing the gRPC server port.", ex);
                }
            }
            return null;
        }
    }
}
