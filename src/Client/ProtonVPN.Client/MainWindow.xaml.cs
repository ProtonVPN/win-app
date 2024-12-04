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

using Microsoft.UI.Xaml;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Core.Extensions;
using ProtonVPN.Client.Services.Activation;
using Windows.Graphics;

namespace ProtonVPN.Client;

public sealed partial class MainWindow : IActivationStateAware
{
    private const double TITLE_BAR_HEIGHT = 38.0;
    private static RectInt32 TITLE_BAR_INTERACTIVE_AREA = new(10, 6, 26, 26);

    public MainWindowActivator WindowActivator { get; }
    public MainWindowOverlayActivator OverlayActivator { get; }

    public MainWindow()
    {
        WindowActivator = App.GetService<MainWindowActivator>();
        OverlayActivator = App.GetService<MainWindowOverlayActivator>();

        InitializeComponent();

        WindowActivator.Initialize(this);
        OverlayActivator.Initialize(this);
    }

    public void InvalidateTitleBarOpacity(WindowActivationState activationState)
    {
        WindowContainer.TitleBarOpacity = activationState.GetTitleBarOpacity();
    }

    public void InvalidateTitleBarVisibility(bool isTitleBarVisible)
    {
        WindowContainer.IsTitleBarVisible = isTitleBarVisible;

        IsMinimizable = isTitleBarVisible;
        IsMaximizable = isTitleBarVisible;
        IsResizable = isTitleBarVisible;

        if (isTitleBarVisible)
        {
            this.SetDragArea(Width, TITLE_BAR_HEIGHT, TITLE_BAR_INTERACTIVE_AREA);
        }
        else
        {
            this.SetDragArea(Width, TITLE_BAR_HEIGHT);
        };
    }

    protected override bool OnSizeChanged(Windows.Foundation.Size newSize)
    {
        InvalidateTitleBarVisibility(WindowContainer.IsTitleBarVisible);

        return base.OnSizeChanged(newSize);
    }
}