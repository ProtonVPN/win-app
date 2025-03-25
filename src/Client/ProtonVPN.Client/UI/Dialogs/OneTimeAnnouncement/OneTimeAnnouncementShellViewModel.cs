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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Common.UI.Extensions;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.Client.Logic.Announcements.Contracts.Messages;
using ProtonVPN.Client.Models.Announcements;

namespace ProtonVPN.Client.UI.Dialogs.OneTimeAnnouncement;

public partial class OneTimeAnnouncementShellViewModel : ShellViewModelBase<IOneTimeAnnouncementWindowActivator>,
    IEventMessageReceiver<AnnouncementListChangedMessage>
{
    private readonly IAnnouncementsProvider _announcementsProvider;
    private readonly IAnnouncementActivator _announcementActivator;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ButtonText))]
    [NotifyPropertyChangedFor(nameof(ImageSource))]
    private Announcement? _activeAnnouncement;

    public string? ButtonText => ActiveAnnouncement?.Panel?.Button?.Text ?? string.Empty;
    public ImageSource? ImageSource => ActiveAnnouncement?.Panel?.FullScreenImage?.Image?.LocalPath?.ToImageSource();

    public OneTimeAnnouncementShellViewModel(
        IUIThreadDispatcher uIThreadDispatcher,
        IAnnouncementsProvider announcementsProvider,
        IAnnouncementActivator announcementActivator,
        IOneTimeAnnouncementWindowActivator windowActivator,
        IViewModelHelper viewModelHelper)
        : base(windowActivator, viewModelHelper)
    {
        _announcementsProvider = announcementsProvider;
        _announcementActivator = announcementActivator;
    }

    private void InvalidateCurrentAnnouncement()
    {
        Announcement? currentAnnouncement = ActiveAnnouncement is null ? null : _announcementsProvider.GetActiveById(ActiveAnnouncement.Id);
        Announcement? newAnnouncement = _announcementsProvider.GetActiveAndUnseenByType(AnnouncementType.OneTime);

        if (currentAnnouncement is null)
        {
            ActiveAnnouncement = newAnnouncement;
        }

        if (ActiveAnnouncement?.Panel?.FullScreenImage?.Image is null)
        {
            Hide();
        }
    }

    public void Receive(AnnouncementListChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateCurrentAnnouncement);
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        if (ActiveAnnouncement is not null)
        {
            _announcementsProvider.MarkAsSeen(ActiveAnnouncement.Id);
        }
    }

    [RelayCommand]
    public async Task OpenAnnouncementAsync()
    {
        await _announcementActivator.ActivateAsync(ActiveAnnouncement);
    }
}