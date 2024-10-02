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

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.UI.Extensions;
using ProtonVPN.Client.Legacy.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.Client.Logic.Announcements.Contracts.Messages;
using ProtonVPN.Client.Legacy.Models.Activation;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Legacy.Models.Announcements;

namespace ProtonVPN.Client.Legacy.UI.Announcements.Modals;

public partial class AnnouncementModalViewModel : PageViewModelBase,
    IEventMessageReceiver<AnnouncementListChangedMessage>
{
    private readonly IAnnouncementsProvider _announcementsProvider;
    private readonly IAnnouncementActivator _announcementActivator;
    private readonly IDialogActivator _dialogActivator;

    public override string? Title => null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ButtonText))]
    [NotifyPropertyChangedFor(nameof(ImageSource))]
    private Announcement? _activeAnnouncement;

    public string? ButtonText => ActiveAnnouncement?.Panel?.Button?.Text ?? string.Empty;
    public ImageSource? ImageSource => ActiveAnnouncement?.Panel?.FullScreenImage?.Image?.LocalPath?.ToImageSource();

    public AnnouncementModalViewModel(
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        IAnnouncementsProvider announcementsProvider,
        IAnnouncementActivator announcementActivator,
        IDialogActivator dialogActivator)
        : base(localizationProvider,
               logger,
               issueReporter)
    {
        _announcementsProvider = announcementsProvider;
        _announcementActivator = announcementActivator;
        _dialogActivator = dialogActivator;

        InvalidateCurrentAnnouncement();
    }

    private void InvalidateCurrentAnnouncement()
    {
        Announcement? currentAnnouncement = ActiveAnnouncement is null ? null : _announcementsProvider.GetActiveById(ActiveAnnouncement.Id);
        Announcement? newAnnouncement = _announcementsProvider.GetActiveAndUnseenByType(AnnouncementType.OneTime);

        if (currentAnnouncement is null)
        {
            ActiveAnnouncement = newAnnouncement;

            if (newAnnouncement is not null)
            {
                _announcementsProvider.MarkAsSeen(newAnnouncement.Id);
            }
        }

        if (ActiveAnnouncement?.Panel?.FullScreenImage?.Image is null)
        {
            _dialogActivator.CloseDialog<AnnouncementModalViewModel>();
        }
    }

    public void Receive(AnnouncementListChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateCurrentAnnouncement);
    }

    [RelayCommand]
    public async Task OpenAnnouncementAsync()
    {
        await _announcementActivator.ActivateAsync(ActiveAnnouncement);
    }
}