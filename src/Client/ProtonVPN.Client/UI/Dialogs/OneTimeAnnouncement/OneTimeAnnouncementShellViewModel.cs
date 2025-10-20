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
using ProtonVPN.Client.Common.UI.Extensions;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Bases.ViewModels;
using ProtonVPN.Client.Core.Messages;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Extensions;
using ProtonVPN.Client.Logic.Announcements.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.Client.Logic.Announcements.Contracts.Messages;
using ProtonVPN.Client.Models.Announcements;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.Client.UI.Dialogs.OneTimeAnnouncement;

public partial class OneTimeAnnouncementShellViewModel : ShellViewModelBase<IOneTimeAnnouncementWindowActivator>,
    IEventMessageReceiver<AnnouncementListChangedMessage>,
    IEventMessageReceiver<ThemeChangedMessage>
{
    private readonly IAnnouncementsProvider _announcementsProvider;
    private readonly IAnnouncementActivator _announcementActivator;
    private readonly IUpsellDisplayStatisticalEventSender _upsellDisplayStatisticalEventSender;
    private readonly IApplicationThemeSelector _themeSelector;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ButtonText))]
    [NotifyPropertyChangedFor(nameof(ImageSource))]
    private Announcement? _activeAnnouncement;

    [ObservableProperty]
    private bool _fillTitleBarArea = true;

    public string? ButtonText => ActiveAnnouncement?.Panel?.Button?.Text ?? string.Empty;
    public ImageSource? ImageSource => ActiveAnnouncement?.Panel?.FullScreenImage
        .GetImageForTheme(_themeSelector.GetTheme())?.LocalPath?.ToImageSource();

    public OneTimeAnnouncementShellViewModel(
        IAnnouncementsProvider announcementsProvider,
        IAnnouncementActivator announcementActivator,
        IOneTimeAnnouncementWindowActivator windowActivator,
        IViewModelHelper viewModelHelper,
        IUpsellDisplayStatisticalEventSender upsellDisplayStatisticalEventSender,
        IApplicationThemeSelector themeSelector)
        : base(windowActivator, viewModelHelper)
    {
        _announcementsProvider = announcementsProvider;
        _announcementActivator = announcementActivator;
        _upsellDisplayStatisticalEventSender = upsellDisplayStatisticalEventSender;
        _themeSelector = themeSelector;
    }

    private void InvalidateCurrentAnnouncement()
    {
        Announcement? currentAnnouncement = ActiveAnnouncement is null ? null : _announcementsProvider.GetActiveById(ActiveAnnouncement.Id);
        Announcement? newAnnouncement = _announcementsProvider.GetActiveAndUnseenByType(AnnouncementType.OneTime);

        if (currentAnnouncement is null)
        {
            ActiveAnnouncement = newAnnouncement;
        }
        else if (newAnnouncement is not null && newAnnouncement.Id != ActiveAnnouncement?.Id)
        {
            ActiveAnnouncement = newAnnouncement;
        }

        if (ActiveAnnouncement?.Panel?.FullScreenImage.GetImageForTheme(_themeSelector.GetTheme()) is null)
        {
            Exit();
        }
    }

    public void Receive(AnnouncementListChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateCurrentAnnouncement);
    }

    public void Receive(ThemeChangedMessage message)
    {
        ExecuteOnUIThread(() => OnPropertyChanged(nameof(ImageSource)));
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        if (ActiveAnnouncement is not null)
        {
            _announcementsProvider.MarkAsSeen(ActiveAnnouncement.Id);
            _upsellDisplayStatisticalEventSender.Send(ModalSource.PromoOffer, ActiveAnnouncement.Reference);
        }
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();
        Exit();
    }

    [RelayCommand]
    public async Task OpenAnnouncementAsync()
    {
        await _announcementActivator.ActivateAsync(ActiveAnnouncement);

        Exit();
    }
}