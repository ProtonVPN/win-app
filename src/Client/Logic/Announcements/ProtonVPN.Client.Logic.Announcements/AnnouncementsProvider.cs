/*
 * Copyright (c) 2025 Proton AG
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

using ProtonVPN.Api.Contracts;
using ProtonVPN.Api.Contracts.Announcements;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.Client.Logic.Announcements.Contracts.Messages;
using ProtonVPN.Client.Logic.Announcements.Files;
using ProtonVPN.Client.Logic.Announcements.Images;
using ProtonVPN.Client.Logic.Auth.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Common.Legacy.Extensions;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;

namespace ProtonVPN.Client.Logic.Announcements;

public class AnnouncementsProvider : IAnnouncementsProvider, IAnnouncementsUpdater,
    IEventMessageReceiver<LoggedOutMessage>
{
    private readonly IApiClient _apiClient;
    private readonly IEntityMapper _entityMapper;
    private readonly IAnnouncementsFileReaderWriter _announcementsFileReaderWriter;
    private readonly IEventMessageSender _eventMessageSender;
    private readonly ILogger _logger;
    private readonly IUserAuthenticator _userAuthenticator;
    private readonly IAnnouncementImagesDeleter _announcementImagesDeleter;

    private readonly ReaderWriterLockSlim _readWriteLock = new();
    private readonly SemaphoreSlim _updateLock = new(1, 1);

    private List<Announcement> _announcements = [];

    public AnnouncementsProvider(IApiClient apiClient,
        IEntityMapper entityMapper,
        IAnnouncementsFileReaderWriter announcementsFileReaderWriter,
        IEventMessageSender eventMessageSender,
        ILogger logger,
        IUserAuthenticator userAuthenticator,
        IAnnouncementImagesDeleter announcementImagesDeleter)
    {
        _apiClient = apiClient;
        _entityMapper = entityMapper;
        _announcementsFileReaderWriter = announcementsFileReaderWriter;
        _eventMessageSender = eventMessageSender;
        _logger = logger;
        _userAuthenticator = userAuthenticator;
        _announcementImagesDeleter = announcementImagesDeleter;
    }

    public async Task UpdateAsync()
    {
        await _updateLock.WaitAsync();

        try
        {
            if (_userAuthenticator.IsLoggedIn)
            {
                LoadFromFileIfEmpty();
                AnnouncementsRequest request = CreateAnnouncementsRequest();
                ApiResponseResult<AnnouncementsResponse> response = await _apiClient.GetAnnouncementsAsync(request);
                if (response.Success)
                {
                    List<Announcement> announcements = _entityMapper.Map<AnnouncementResponse, Announcement>(response.Value.Announcements);
                    ProcessAnnouncements(announcements);
                    SetWithWriteLock(() => SaveToFile(announcements));
                }
            }
            else
            {
                _logger.Info<AppLog>("Ignoring announcements update because there is no user logged in");
            }

            _announcementImagesDeleter.DeleteUnused();
        }
        catch (Exception ex)
        {
            _logger.Error<AppLog>("Get announcements failed", ex);
        }
        finally
        {
            _updateLock.Release();
        }
    }

    private void LoadFromFileIfEmpty()
    {
        if (!HasAnyAnnouncements())
        {
            _logger.Info<AppLog>("Loading announcements from file as the user has none.");
            List<Announcement> announcements = _announcementsFileReaderWriter.Read();
            ProcessAnnouncements(announcements);
        }
    }

    public bool HasAnyAnnouncements()
    {
        return GetWithReadLock(() => _announcements is not null && _announcements.Count > 0);
    }

    private void ProcessAnnouncements(List<Announcement> newAnnouncements)
    {
        SetWithWriteLock(() =>
        {
            for (int i = 0; i < newAnnouncements.Count; i++)
            {
                Announcement newAnnouncement = newAnnouncements[i];
                newAnnouncement.Seen = _announcements.FirstOrDefault(a => a.Id == newAnnouncement.Id)?.Seen ?? newAnnouncement.Seen;
            }
            _announcements = newAnnouncements;
        });
    }

    private void SaveToFile(List<Announcement> announcements)
    {
        _announcementsFileReaderWriter.Save(announcements);
    }

    private AnnouncementsRequest CreateAnnouncementsRequest()
    {
        return new AnnouncementsRequest()
        {
            FullScreenImageWidth = 1024,
            FullScreenImageHeight = 768,
            FullScreenImageSupport = AnnouncementConstants.FULL_SCREEN_IMAGE_FORMAT,
        };
    }

    public IReadOnlyList<Announcement> GetAllActive()
    {
        return GetWithReadLock(() => _announcements.Where(a => a.IsActive())).ToList();
    }

    private T GetWithReadLock<T>(Func<T> func)
    {
        _readWriteLock.EnterReadLock();
        try
        {
            return func();
        }
        finally
        {
            _readWriteLock.ExitReadLock();
        }
    }

    public Announcement? GetActiveById(string id)
    {
        return GetWithReadLock(() => _announcements.FirstOrDefault(a => a.Id == id && a.IsActive()));
    }

    public Announcement? GetActiveAndUnseenByType(AnnouncementType type)
    {
        return GetWithReadLock(() => _announcements
            .OrderBy(a => a.EndDateTimeUtc)
            .FirstOrDefault(a => a.Type == type && a.IsActiveAndUnseen())
        );
    }

    public void MarkAsSeen(string id)
    {
        SetWithWriteLock(() =>
        {
            _announcements.Where(a => a.Id == id).ForEach(a => a.Seen = true);
            SaveToFile(_announcements);
        });
    }

    private void SetWithWriteLock(Action action)
    {
        _readWriteLock.EnterWriteLock();
        try
        {
            action();
        }
        finally
        {
            _readWriteLock.ExitWriteLock();
        }
        _eventMessageSender.Send(new AnnouncementListChangedMessage());
    }

    public void Delete(string id)
    {
        SetWithWriteLock(() =>
        {
            Announcement? announcement = _announcements.FirstOrDefault(a => a.Id == id);
            if (announcement is not null)
            {
                try
                {
                    _announcements.Remove(announcement);
                    SaveToFile(_announcements);
                }
                catch (Exception ex)
                {
                    _logger.Error<AppLog>($"The announcement with ID '{id}' failed to be removed.", ex);
                }
            }
            else
            {
                _logger.Info<AppLog>($"The announcement with ID '{id}' doesn't exist.");
            }
        });
    }

    public void Receive(LoggedOutMessage message)
    {
        SetWithWriteLock(() =>
        {
            _announcements = [];
        });
    }
}
