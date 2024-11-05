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
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Logging.Contracts;
using ProtonVPN.Client.Contracts.Messages;
using ProtonVPN.Client.Contracts.Services.Activation;
using ProtonVPN.Client.Contracts.Services.Activation.Bases;
using ProtonVPN.Client.Contracts.Services.Selection;
using ProtonVPN.Client.UI.Dialogs.Upsell;
using ProtonVPN.Client.Common.Messages;

namespace ProtonVPN.Client.Services.Activation;

public class UpsellCarouselWindowActivator : WindowActivatorBase<UpsellCarouselWindow>, IUpsellCarouselWindowActivator
{
    public override string WindowTitle => Localizer.Get("Upsell_Carousel_Title");

    public UpsellCarouselWindowActivator(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        ILocalizationProvider localizer,
        IApplicationIconSelector iconSelector)
        : base(logger, uiThreadDispatcher, themeSelector, settings, localizer, iconSelector)
    { }

    public void Receive(MainWindowVisibilityChangedMessage message)
    {
        if (Host != null)
        {
            if (message.IsMainWindowVisible)
            {
                Activate();
            }
            else
            {
                Hide();
            }
        }
    }

    public void Receive(ApplicationStoppedMessage message)
    {
        Exit();
    }
}