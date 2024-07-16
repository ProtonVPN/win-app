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

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using ProtonVPN.Api.Contracts.Announcements;
using ProtonVPN.Client.Files.Contracts.Images;
using ProtonVPN.Client.Logic.Announcements.Contracts.Entities;
using ProtonVPN.Client.Logic.Announcements.EntityMapping;
using ProtonVPN.EntityMapping.Contracts;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Logic.Announcements.Tests;

[TestClass]
public class AnnouncementMapperTest
{
    private ILogger _logger;
    private IEntityMapper _entityMapper;
    private IImageCache _imageCache;

    private AnnouncementMapper _announcementMapper;
    private PanelMapper _panelMapper;
    private PanelFeatureMapper _panelFeatureMapper;
    private PanelButtonMapper _panelButtonMapper;
    private FullScreenImageMapper _fullScreenImageMapper;

    [TestInitialize]
    public void TestInitialize()
    {
        CreateDependencies();
        SetUpMappers();
    }

    private void CreateDependencies()
    {
        _logger = Substitute.For<ILogger>();
        _entityMapper = Substitute.For<IEntityMapper>();
        _imageCache = Substitute.For<IImageCache>();
        _imageCache.Get(Arg.Any<string>(), Arg.Any<string?>()).Returns(args => new CachedImage() { LocalPath = (((string?)args[0]) ?? string.Empty).ToUpper() });

        _announcementMapper = new AnnouncementMapper(_logger, _entityMapper, _imageCache);
        _panelMapper = new PanelMapper(_entityMapper, _imageCache);
        _panelFeatureMapper = new PanelFeatureMapper(_imageCache);
        _panelButtonMapper = new PanelButtonMapper();
        _fullScreenImageMapper = new FullScreenImageMapper(_imageCache);
    }

    private void SetUpMappers()
    {
        _entityMapper
            .Map<OfferPanelResponse, Panel>(Arg.Any<OfferPanelResponse>())
            .Returns(args => _panelMapper.Map((OfferPanelResponse)args[0]));

        _entityMapper
            .Map<OfferPanelFeatureResponse, PanelFeature>(Arg.Any<OfferPanelFeatureResponse>())
            .Returns(args => _panelFeatureMapper.Map((OfferPanelFeatureResponse)args[0]));

        _entityMapper
            .Map<OfferPanelButtonResponse, PanelButton>(Arg.Any<OfferPanelButtonResponse>())
            .Returns(args => _panelButtonMapper.Map((OfferPanelButtonResponse)args[0]));

        _entityMapper
            .Map<FullScreenImageResponse, FullScreenImage>(Arg.Any<FullScreenImageResponse>())
            .Returns(args => _fullScreenImageMapper.Map((FullScreenImageResponse)args[0]));
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _logger = null;
        _entityMapper = null;
        _imageCache = null;

        _announcementMapper = null;
        _panelMapper = null;
        _panelFeatureMapper = null;
        _panelButtonMapper = null;
        _fullScreenImageMapper = null;
    }

    [TestMethod]
    public void Test_VpnBlackFriday2023Icon0()
    {
        AnnouncementResponse input = CreateResponse_VpnBlackFriday2023Icon0();

        Announcement result = _announcementMapper.Map(input);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Id == "vpn-black-friday-2023-icon-0");
        Assert.IsTrue(result.StartDateTimeUtc == new DateTime(2023, 11, 2, 13, 0, 0, 0, 0, DateTimeKind.Utc));
        Assert.IsTrue(result.EndDateTimeUtc == new DateTime(2023, 11, 24, 8, 59, 0, 0, 0, DateTimeKind.Utc));
        Assert.IsTrue(result.Type == (AnnouncementType)0);
        Assert.IsNotNull(result.Panel);
        Assert.IsNotNull(result.Panel.Button);
        Assert.IsTrue(result.Panel.Button.Text == "Get the deal now");
        Assert.IsTrue(result.Panel.Button.Url == "https://protonvpn.com/");
        Assert.IsTrue(result.Panel.Button.Action == "OpenURL");
        Assert.IsNotNull(result.Panel.Button.Behaviors);
        Assert.IsTrue(result.Panel.Button.Behaviors.Count > 0);
        Assert.IsNotNull(result.Panel.FullScreenImage);
        Assert.IsNotNull(result.Panel.FullScreenImage.Image);
    }

    private AnnouncementResponse CreateResponse_VpnBlackFriday2023Icon0()
    {
        return new AnnouncementResponse()
        {
            Id = "vpn-black-friday-2023-icon-0",
            StartTimestamp = 1698930000,
            EndTimestamp = 1700816340,
            Type = 0,
            Offer = new OfferResponse()
            {
                Url = "https://protonvpn.com/",
                Icon = "https://protonvpn.com/download/resources/youtube.png",
                Label = "Black Friday offer -60%",
                Panel = new OfferPanelResponse()
                {
                    Button = new OfferPanelButtonResponse()
                    {
                        Url = "https://protonvpn.com/",
                        Action = "OpenURL",
                        Behaviors = ["AutoLogin"],
                        Text = "Get the deal now"
                    },
                    FullScreenImage = new FullScreenImageResponse()
                    {
                        AlternativeText = "Black Friday offer -60%",
                        Source = new List<SourceResponse>
                         {
                             new SourceResponse()
                             {
                                 Url = "https://protonvpn.com/download/resources/promo/eoy2023/eoy23-en-desktop-modal-protonunlimited@3x.png",
                                 Type = "PNG",
                                 Width = 2244,
                                 Height = 1740
                             }
                         }
                    }
                }
            }
        };
    }

    [TestMethod]
    public void Test_VpnBlackFriday2023Splash0()
    {
        AnnouncementResponse input = CreateResponse_VpnBlackFriday2023Splash0();

        Announcement result = _announcementMapper.Map(input);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Id == "vpn-black-friday-2023-splash-0");
        Assert.IsTrue(result.StartDateTimeUtc == new DateTime(2023, 11, 2, 13, 0, 0, 0, 0, DateTimeKind.Utc));
        Assert.IsTrue(result.EndDateTimeUtc == new DateTime(2023, 11, 24, 8, 59, 0, 0, 0, DateTimeKind.Utc));
        Assert.IsTrue(result.Type == (AnnouncementType)1);
        Assert.IsNotNull(result.Panel);
        Assert.IsNotNull(result.Panel.Button);
        Assert.IsTrue(result.Panel.Button.Text == "Get the deal now");
        Assert.IsTrue(result.Panel.Button.Url == "https://protonvpn.com/");
        Assert.IsTrue(result.Panel.Button.Action == "OpenURL");
        Assert.IsNotNull(result.Panel.Button.Behaviors);
        Assert.IsTrue(result.Panel.Button.Behaviors.Count > 0);
        Assert.IsNotNull(result.Panel.FullScreenImage);
        Assert.IsNotNull(result.Panel.FullScreenImage.Image);
    }

    private AnnouncementResponse CreateResponse_VpnBlackFriday2023Splash0()
    {
        return new AnnouncementResponse()
        {
            Id = "vpn-black-friday-2023-splash-0",
            StartTimestamp = 1698930000,
            EndTimestamp = 1700816340,
            Type = 1,
            Offer = new OfferResponse()
            {
                Url = "https://protonvpn.com/",
                Icon = "https://protonvpn.com/download/resources/russia-emergency/phone-illustration.png",
                Label = "Get 60% off Proton VPN Plus!",
                Panel = new OfferPanelResponse()
                {
                    Button = new OfferPanelButtonResponse()
                    {
                        Url = "https://protonvpn.com/",
                        Action = "OpenURL",
                        Behaviors = ["AutoLogin"],
                        Text = "Get the deal now"
                    },
                    FullScreenImage = new FullScreenImageResponse()
                    {
                        AlternativeText = "Get 60% off Proton VPN Plus!",
                        Source = new List<SourceResponse>
                         {
                             new SourceResponse()
                             {
                                 Url = "https://protonvpn.com/download/resources/promo/welcome-offer/1month-desktop-30pct@3x.png",
                                 Type = "PNG",
                                 Width = 2244,
                                 Height = 1740
                             }
                         }
                    }
                }
            }
        };
    }
}