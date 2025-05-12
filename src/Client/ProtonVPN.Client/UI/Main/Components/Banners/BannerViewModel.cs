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

using Microsoft.UI.Xaml.Media;
using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Common.UI.Extensions;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Localization.Extensions;
using ProtonVPN.Client.Logic.Announcements.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.Client.Models.Announcements;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.Client.UI.Main.Components.Banners;

public partial class BannerViewModel : BannerViewModelBase
{
    private readonly IDispatcherTimer _countdownTimer;

    public bool ShowCountdown => ActiveAnnouncement != null && ActiveAnnouncement.ShowCountdown;

    public string? Footer => GetFooter();

    public ImageSource? LargeIllustrationSource => ActiveAnnouncement?.Panel?.FullScreenImage?.Image?.LocalPath?.ToImageSource();

    protected override AnnouncementType AnnouncementType { get; } = AnnouncementType.Banner;

    protected override ModalSource ModalSource { get; } = ModalSource.PromoOffer;

    public BannerViewModel(
        IUIThreadDispatcher uiThreadDispatcher,
        IAnnouncementActivator announcementActivator,
        IAnnouncementsProvider announcementsProvider,
        IUpsellDisplayStatisticalEventSender upsellDisplayStatisticalEventSender,
        IViewModelHelper viewModelHelper)
        : base(announcementActivator, announcementsProvider, upsellDisplayStatisticalEventSender, viewModelHelper)
    {
        _countdownTimer = uiThreadDispatcher.GetTimer(TimeSpan.FromSeconds(1));
        _countdownTimer.Tick += OnCountdownTimerTick;
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

    private void OnCountdownTimerTick(object? sender, object e)
    {
        OnPropertyChanged(nameof(Footer));
    }

    protected override void BeforeAnnouncementChange()
    {
        if (_countdownTimer.IsEnabled)
        {
            _countdownTimer?.Stop();
        }
    }

    protected override void AfterAnnouncementChange()
    {
        if (ShowCountdown)
        {
            _countdownTimer?.Start();
        }

        OnPropertyChanged(nameof(ShowCountdown));
        OnPropertyChanged(nameof(Footer));
        OnPropertyChanged(nameof(LargeIllustrationSource));
    }
}