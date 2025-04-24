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
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.Client.Logic.Announcements.Contracts.Messages;
using ProtonVPN.Client.Models.Announcements;

namespace ProtonVPN.Client.UI.Main.Components.Banners;

public abstract partial class BannerViewModelBase : ViewModelBase,
    IEventMessageReceiver<AnnouncementListChangedMessage>
{
    private readonly IAnnouncementsProvider _announcementsProvider;
    private readonly IAnnouncementActivator _announcementActivator;

    protected BannerViewModelBase(
        IAnnouncementActivator announcementActivator,
        IAnnouncementsProvider announcementsProvider,
        IViewModelHelper viewModelHelper)
        : base(viewModelHelper)
    {
        _announcementsProvider = announcementsProvider;
        _announcementActivator = announcementActivator;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenAnnouncementCommand))]
    [NotifyCanExecuteChangedFor(nameof(DismissAnnouncementCommand))]
    [NotifyPropertyChangedFor(nameof(ActionButtonText))]
    [NotifyPropertyChangedFor(nameof(IsActionButtonVisible))]
    [NotifyPropertyChangedFor(nameof(IsDismissButtonVisible))]
    private Announcement? _activeAnnouncement;

    public bool IsDismissButtonVisible => ActiveAnnouncement?.IsDismissible ?? false;

    public string? ActionButtonText => ActiveAnnouncement?.Panel?.Button?.Text;

    public bool IsActionButtonVisible => !string.IsNullOrWhiteSpace(ActiveAnnouncement?.Panel?.Button?.Text);

    protected abstract AnnouncementType AnnouncementType { get; }

    public void Receive(AnnouncementListChangedMessage message)
    {
        Announcement? announcement = _announcementsProvider.GetActiveAndUnseenBanner();
        if (announcement?.Type != AnnouncementType)
        {
            ActiveAnnouncement = null;
            return;
        }

        ExecuteOnUIThread(() =>
        {
            BeforeAnnouncementChange();
            ActiveAnnouncement = announcement;
            AfterAnnouncementChange();
        });
    }

    protected virtual void BeforeAnnouncementChange()
    { }

    protected virtual void AfterAnnouncementChange()
    { }

    [RelayCommand(CanExecute = nameof(CanOpenAnnouncement))]
    private async Task OpenAnnouncementAsync()
    {
        await _announcementActivator.ActivateAsync(ActiveAnnouncement);
    }

    private bool CanOpenAnnouncement()
    {
        return ActiveAnnouncement != null;
    }

    [RelayCommand(CanExecute = nameof(CanDismissAnnouncement))]
    private void DismissAnnouncement()
    {
        Announcement? announcement = ActiveAnnouncement;
        if (announcement is not null)
        {
            _announcementsProvider.MarkAsSeen(announcement.Id);
        }
    }

    private bool CanDismissAnnouncement()
    {
        return ActiveAnnouncement != null && IsDismissButtonVisible;
    }
}