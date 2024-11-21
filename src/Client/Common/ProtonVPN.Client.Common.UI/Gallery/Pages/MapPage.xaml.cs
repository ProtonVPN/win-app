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

using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ProtonVPN.Client.Common.UI.Gallery.Controls;
using Windows.Foundation;

namespace ProtonVPN.Client.Common.UI.Gallery.Pages;

public sealed partial class MapPage
{
    public MapPage()
    {
        InitializeComponent();
    }

    private void AddMapPin(Point p)
    {
        foreach (MapPinControl child in canvas.Children.OfType<MapPinControl>())
        {
            child.IsTarget = false;
        }

        MapPinControl pin = new()
        {
            Position = p,
            IsTarget = true
        };
        canvas.Children.Add(pin);
    }

    private void OnCanvasPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (canvas.Children.OfType<MapPinControl>().Count() >= 10)
        {
            canvas.Children.Clear();
        }

        AddMapPin(e.GetCurrentPoint(canvas).Position);

        ConnectMapPins();
    }

    private void ConnectMapPins()
    {
        List<MapConnectionControl> existingConnections = canvas.Children.OfType<MapConnectionControl>().ToList();
        foreach (MapConnectionControl connection in existingConnections)
        {
            canvas.Children.Remove(connection);
        }

        MapPinControl? previousPin = null;
        foreach (MapPinControl pin in canvas.Children.OfType<MapPinControl>().ToList())
        {
            if (previousPin != null)
            {
                MapConnectionControl connection = new()
                {
                    SourcePin = previousPin,
                    TargetPin = pin
                };
                canvas.Children.Insert(0, connection);
            }

            previousPin = pin;
        }
    }
}