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

using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.EventMessaging.Contracts;
using ProtonVPN.Client.Logic.Auth.Contracts.Enums;
using ProtonVPN.Client.Logic.Auth.Contracts.Messages;
using ProtonVPN.Client.Mappers;
using ProtonVPN.Client.Models.Navigation;
using ProtonVPN.Client.UI.Upsell.Carousel;
using ProtonVPN.Client.UI.Upsell.Carousel.Models;

namespace ProtonVPN.Client.Models.Activation.Custom;

public class UpsellCarouselDialogActivator : IUpsellCarouselDialogActivator,
    IEventMessageReceiver<LoggingOutMessage>
{
    private readonly IDialogActivator _dialogActivator;
    private readonly IUIThreadDispatcher _uiThreadDispatcher;
    private readonly UpsellCarouselShellViewModel _shellViewModel;

    public UpsellCarouselDialogActivator(
        IUIThreadDispatcher uiThreadDispatcher,
        IDialogActivator dialogActivator,
        UpsellCarouselShellViewModel shellViewModel)
    {
        _dialogActivator = dialogActivator;
        _shellViewModel = shellViewModel;
        _uiThreadDispatcher = uiThreadDispatcher;
    }

    public void ShowDialog(ModalSources modalSources)
    {
        _dialogActivator.ShowDialog<UpsellCarouselShellViewModel>();

        _shellViewModel.OriginalModalSources = modalSources;

        UpsellFeature feature = modalSources.Map();
        _shellViewModel.MoveToFeature(feature);
    }

    public void ShowDialog(UpsellFeature? feature)
    {
        _dialogActivator.ShowDialog<UpsellCarouselShellViewModel>();

        _shellViewModel.OriginalModalSources = feature.Map();

        _shellViewModel.MoveToFeature(feature ?? UpsellFeature.WorldwideCoverage);
    }

    public void CloseDialog()
    {
        _dialogActivator.CloseDialog<UpsellCarouselShellViewModel>();
    }

    public void Receive(LoggingOutMessage message)
    {
        _uiThreadDispatcher.TryEnqueue(CloseDialog);
    }
}