﻿/*
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

using Autofac;

namespace ProtonVPN.Client.Notifications.Installers;

public class NotificationsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PortForwardingNewPortNotificationSender>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ConnectionStatusNotificationSender>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<ConnectionErrorNotificationSender>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<SubscriptionExpiredNotificationSender>().AsImplementedInterfaces().SingleInstance();
        builder.RegisterType<NotificationActivationHandler>().AsImplementedInterfaces().SingleInstance().AutoActivate();
        builder.RegisterType<UnsecuredWifiNotificationSender>().AsImplementedInterfaces().SingleInstance().AutoActivate();
    }
}