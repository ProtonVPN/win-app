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

using System.Collections.Concurrent;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.Settings.Contracts.Messages;
using ProtonVPN.Client.Settings.Files;
using ProtonVPN.Client.Settings.Repositories.Contracts;
using ProtonVPN.Crypto.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.SettingsLogs;
using ProtonVPN.Serialization.Contracts;

namespace ProtonVPN.Client.Settings.Repositories;

public class UserSettingsRepository : SettingsRepositoryBase, IUserSettingsRepository, IEventMessageReceiver<SettingChangedMessage>
{
    private const string FILE_NAME = "UserSettings.{0}.json";

    private readonly ISettingsFileManager _settingsFileManager;
    private readonly IGlobalSettings _globalSettings;
    private readonly ISha1Calculator _sha1Calculator;

    private readonly object _lock = new();
    private Lazy<string?> _fileName;
    private Lazy<ConcurrentDictionary<string, string?>> _cache;

    public UserSettingsRepository(ILogger logger,
        IJsonSerializer jsonSerializer,
        IEventMessageSender eventMessageSender,
        ISettingsFileManager settingsFileManager,
        IGlobalSettings globalSettings,
        ISha1Calculator sha1Calculator)
        : base(logger, jsonSerializer, eventMessageSender)
    {
        _settingsFileManager = settingsFileManager;
        _globalSettings = globalSettings;
        _sha1Calculator = sha1Calculator;

        _fileName = new(GenerateFileName);
        _cache = new(new ConcurrentDictionary<string, string?>());
        GenerateNewCache(_fileName);
    }

    private string? GenerateFileName()
    {
        string? username = _globalSettings.Username?.ToLower();
        string? fileName = username is null ? null : string.Format(FILE_NAME, _sha1Calculator.Hash(username));
        return fileName;
    }

    private void GenerateNewCache(Lazy<string?> fileName)
    {
        Lazy<ConcurrentDictionary<string, string?>>? oldCache = _cache;
        _cache = new Lazy<ConcurrentDictionary<string, string?>>(() => ReadFileOrEmpty(fileName.Value));
        oldCache?.Value.Clear();
    }

    private ConcurrentDictionary<string, string?> ReadFileOrEmpty(string? fileName)
    {
        if (fileName is null)
        {
            Logger.Warn<SettingsChangeLog>("Cannot read user settings file because the file name is null.");
            return new();
        }
        return new(_settingsFileManager.Read(fileName));
    }

    protected override string? Get(string propertyName)
    {
        string? result;
        result = _cache.Value.TryGetValue(propertyName, out string? value) ? value : null;
        return result;
    }

    protected override void Set(string propertyName, string? value)
    {
        lock (_lock)
        {
            string? fileName = _fileName.Value;
            if (fileName is null)
            {
                Logger.Warn<SettingsChangeLog>($"Cannot write user setting '{propertyName}' to file because the file name is null.");
            }
            else
            {
                _cache.Value.AddOrUpdate(propertyName, value, (_, _) => value);
                _settingsFileManager.Save(fileName, _cache.Value);
            }
        }
    }

    public void Receive(SettingChangedMessage message)
    {
        if (message.PropertyName == nameof(ISettings.Username))
        {
            lock (_lock)
            {
                Lazy<string?> fileName = new(GenerateFileName);
                _fileName = fileName;
                GenerateNewCache(fileName);
            }
        }
    }
}