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

using ProtonVPN.Client.Common.Dispatching;
using ProtonVPN.Client.Core.Services.Activation;
using ProtonVPN.Client.Core.Services.Activation.Bases;
using ProtonVPN.Client.Core.Services.Selection;
using ProtonVPN.Client.Localization.Contracts;
using ProtonVPN.Client.Settings.Contracts;
using ProtonVPN.Client.UI.Dialogs.DebugTools;
using ProtonVPN.Logging.Contracts;

namespace ProtonVPN.Client.Services.Activation;

public class DebugToolsWindowActivator : DialogActivatorBase<DebugToolsWindow>, IDebugToolsWindowActivator
{
    public override string WindowTitle { get; } = "Debug Tools";

    public DebugToolsWindowActivator(
        ILogger logger,
        IUIThreadDispatcher uiThreadDispatcher,
        IApplicationThemeSelector themeSelector,
        ISettings settings,
        ILocalizationProvider localizer,
        IApplicationIconSelector iconSelector,
        IMainWindowActivator mainWindowActivator)
        : base(logger,
               uiThreadDispatcher,
               themeSelector,
               settings,
               localizer,
               iconSelector,
               mainWindowActivator)
    { }
}