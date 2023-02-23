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

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ProtonVPN.Resource.Colors;
using ProtonVPN.Resources.Colors;

namespace ProtonVPN.SpeedGraph
{
    public partial class SpeedGraph
    {
        private int _columnCount = 35;
        private int _rowCount = 5;
        
        private static readonly IColorPalette _colorPalette = ColorPaletteFactory.Create();
        private readonly Lazy<SolidColorBrush> _graphStrokeColorBrush = new(() => _colorPalette.GetSolidColorBrushByResourceName("BorderWeakBrushColor"));

        public SpeedGraph()
        {
            InitializeComponent();
            DrawGrid();
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SessionStats.Visibility = e.NewSize.Width > 200 ? Visibility.Visible : Visibility.Hidden;
            Graph.Visibility = e.NewSize.Width > 350 ? Visibility.Visible : Visibility.Hidden;
            TopLabels.Visibility = Graph.Visibility;
            BottomLabels.Visibility = Graph.Visibility;
        }

        private void DrawGrid()
        {
            for (int i = 0; i < _columnCount; i++)
            {
                Grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < _rowCount; i++)
            {
                Grid.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < _columnCount; i++)
            {
                for (int j = 0; j < _rowCount; j++)
                {
                    Border border = new();
                    int left = 0;
                    int top = 1;
                    int right = 1;
                    int bottom = 0;

                    if (i == 0)
                    {
                        left = 1;
                    }

                    if (j + 1 == _rowCount)
                    {
                        bottom = 1;
                    }

                    border.BorderBrush = _graphStrokeColorBrush.Value;
                    border.BorderThickness = new Thickness(left, top, right, bottom);

                    Grid.Children.Add(border);
                    Grid.SetColumn(border, i);
                    Grid.SetRow(border, j);
                }
            }
        }
    }
}
