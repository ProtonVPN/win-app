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

using ProtonVPN.Client.Common.Enums;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Logic.Announcements.Contracts;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.Client.Models.Announcements;
using ProtonVPN.StatisticalEvents.Contracts;

namespace ProtonVPN.Client.UI.Main.Components.Banners;

public partial class ProminentBannerViewModel : BannerViewModelBase
{
    public string? Header => ActiveAnnouncement?.Panel?.Title;

    public string? Description => ActiveAnnouncement?.Panel?.Description;

    public ProminentBannerStyle BannerStyle => ActiveAnnouncement?.Panel is null ? ProminentBannerStyle.Regular : ActiveAnnouncement.Panel.Style;

    protected override AnnouncementType AnnouncementType { get; } = AnnouncementType.ProminentBanner;

    protected override ModalSource ModalSource { get; } = ModalSource.Account;

    public ProminentBannerViewModel(
        IAnnouncementActivator announcementActivator,
        IAnnouncementsProvider announcementsProvider,
        IUpsellDisplayStatisticalEventSender upsellDisplayStatisticalEventSender,
        IViewModelHelper viewModelHelper)
        : base(announcementActivator, announcementsProvider, upsellDisplayStatisticalEventSender, viewModelHelper)
    { }

    protected override void AfterAnnouncementChange()
    {
        OnPropertyChanged(nameof(Header));
        OnPropertyChanged(nameof(Description));
        OnPropertyChanged(nameof(BannerStyle));
    }
}