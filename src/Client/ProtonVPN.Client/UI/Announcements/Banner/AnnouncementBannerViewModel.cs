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

using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Common.UI.Extensions;
using ProtonVPN.Client.Contracts.ViewModels;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Announcements.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.Client.Logic.Announcements.Contracts.Messages;
using ProtonVPN.IssueReporting.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.UI.Announcements.Banner;

public partial class AnnouncementBannerViewModel : ViewModelBase,
    IEventMessageReceiver<AnnouncementListChangedMessage>
{
    private readonly IAnnouncementsProvider _announcementsProvider;
    private readonly IAnnouncementActivator _announcementActivator;
    private readonly DispatcherTimer _countdownTimer;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenAnnouncementCommand))]
    [NotifyCanExecuteChangedFor(nameof(DismissAnnouncementCommand))]
    [NotifyPropertyChangedFor(nameof(Header))]
    [NotifyPropertyChangedFor(nameof(Description))]
    [NotifyPropertyChangedFor(nameof(ShowCountdown))]
    [NotifyPropertyChangedFor(nameof(Footer))]
    [NotifyPropertyChangedFor(nameof(ActionButtonText))]
    [NotifyPropertyChangedFor(nameof(IsActionButtonVisible))]
    [NotifyPropertyChangedFor(nameof(DismissButtonText))]
    [NotifyPropertyChangedFor(nameof(IsDismissButtonVisible))]
    [NotifyPropertyChangedFor(nameof(BannerStyle))]
    [NotifyPropertyChangedFor(nameof(IconSource))]
    [NotifyPropertyChangedFor(nameof(SmallIllustrationSource))]
    [NotifyPropertyChangedFor(nameof(LargeIllustrationSource))]
    private Announcement? _activeAnnouncement;

    public string? Header => ActiveAnnouncement?.Panel?.Title;

    public string? TooltipText
    {
        get
        {
            string? label = ActiveAnnouncement?.Label;
            string? title = ActiveAnnouncement?.Panel?.Title;
            return string.IsNullOrEmpty(label)
                ? string.IsNullOrEmpty(title)
                    ? null
                    : title
                : label;
        }
    }

    public string? Description => ActiveAnnouncement?.Panel?.Description;

    public bool ShowCountdown => ActiveAnnouncement != null && ActiveAnnouncement.ShowCountdown;

    public string? Footer => GetFooter();

    public string? ActionButtonText => ActiveAnnouncement?.Panel?.Button?.Text;

    public bool IsActionButtonVisible => !string.IsNullOrWhiteSpace(ActiveAnnouncement?.Panel?.Button?.Text);

    public string? DismissButtonText => Localizer.Get("Common_Actions_Close");

    public bool IsDismissButtonVisible => ActiveAnnouncement?.IsDismissible ?? false;

    /// <summary>
    /// TODO: Banner style can be set to regular / warning for subscription reminders and upsell for promo offers.
    /// </summary>
    public ProminentBannerStyle BannerStyle => ActiveAnnouncement?.Panel is null ? ProminentBannerStyle.Upsell : ActiveAnnouncement.Panel.Style;

    public ImageSource? IconSource => ActiveAnnouncement?.Icon?.LocalPath?.ToImageSource();

    public ImageSource? SmallIllustrationSource => ActiveAnnouncement?.Panel?.Picture?.LocalPath?.ToImageSource();

    public ImageSource? LargeIllustrationSource => ActiveAnnouncement?.Panel?.FullScreenImage?.Image?.LocalPath?.ToImageSource();

    public AnnouncementBannerViewModel(
        ILocalizationProvider localizationProvider,
        ILogger logger,
        IIssueReporter issueReporter,
        IAnnouncementsProvider announcementsProvider,
        IAnnouncementActivator announcementActivator)
        : base(localizationProvider, logger, issueReporter)
    {
        _announcementsProvider = announcementsProvider;
        _announcementActivator = announcementActivator;

        _countdownTimer = new()
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _countdownTimer.Tick += OnCountdownTimerTick;

        InvalidateActiveAnnouncement();
    }

    public void Receive(AnnouncementListChangedMessage message)
    {
        ExecuteOnUIThread(InvalidateActiveAnnouncement);
    }

    protected override void OnLanguageChanged()
    {
        base.OnLanguageChanged();

        OnPropertyChanged(nameof(DismissButtonText));
    }

    private string? GetFooter()
    {
        if (!ShowCountdown)
        {
            return null;
        }

        TimeSpan countdown = ActiveAnnouncement!.EndDateTimeUtc - DateTime.UtcNow;
        if (countdown < TimeSpan.FromSeconds(1))
        {
            return null;
        }

        return Localizer.GetFormattedTime(countdown);
    }

    private void OnCountdownTimerTick(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(Footer));
    }

    private void InvalidateActiveAnnouncement()
    {
        if (_countdownTimer.IsEnabled)
        {
            _countdownTimer.Stop();
        }

        ActiveAnnouncement = _announcementsProvider.GetActiveAndUnseenByType(AnnouncementType.Banner, AnnouncementType.ProminentBanner);

        if (ShowCountdown)
        {
            _countdownTimer.Start();
        }
    }

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