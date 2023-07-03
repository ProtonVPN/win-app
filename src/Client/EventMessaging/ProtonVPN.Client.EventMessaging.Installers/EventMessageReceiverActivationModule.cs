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

using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;
using CommunityToolkit.Mvvm.Messaging;
using ProtonVPN.Client.EventMessaging.Contracts;

namespace ProtonVPN.Client.EventMessaging.Installers;

public class EventMessageReceiverActivationModule : Module
{
    private readonly IMessenger _messenger = MessengerFactory.Get();

    protected override void Load(ContainerBuilder builder)
    {
        _messenger.Reset();
        builder.RegisterCallback((IComponentRegistryBuilder componentRegistryBuilder) =>
        {
            componentRegistryBuilder.Registered += (object? sender, ComponentRegisteredEventArgs args) =>
            {
                args.ComponentRegistration.ConfigurePipeline((IResolvePipelineBuilder resolvePipelineBuilder) =>
                {
                    resolvePipelineBuilder.Use(PipelinePhase.Activation,
                        (ResolveRequestContext context, Action<ResolveRequestContext> next) =>
                    {
                        // Before activation
                        next(context); // Call the next middleware in the pipeline to activate.

                        // After activation
                        if (context.Instance is IEventMessageReceiver eventMessageReceiver)
                        {
                            try
                            {
                                _messenger.RegisterAll(eventMessageReceiver);
                            }
                            catch (InvalidOperationException ex)
                             when (ex.Message == "The target recipient has already subscribed to the target message.")
                            {
                            }
                        }
                    });
                });
            };
        });
    }
}