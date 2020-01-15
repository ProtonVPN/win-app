/*
 * Copyright (c) 2020 Proton Technologies AG
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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProtonVPN.SpeedGraph
{
    public partial class SpeedGraph
    {
        private int _columnCount = 35;
        private int _rowCount = 5;

        public SpeedGraph()
        {
            InitializeComponent();
            DrawGrid();
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SessionStats.Visibility = e.NewSize.Width > 160 ? Visibility.Visible : Visibility.Hidden;
            Graph.Visibility = e.NewSize.Width > 350 ? Visibility.Visible : Visibility.Hidden;
            TopLabels.Visibility = Graph.Visibility;
            BottomLabels.Visibility = Graph.Visibility;
        }

        private void DrawGrid()
        {
            for (var i = 0; i < _columnCount; i++)
            {
                Grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (var i = 0; i < _rowCount; i++)
            {
                Grid.RowDefinitions.Add(new RowDefinition());
            }

            for (var i = 0; i < _columnCount; i++)
            {
                for (var j = 0; j < _rowCount; j++)
                {
                    var border = new Border();
                    var left = 0;
                    var top = 1;
                    var right = 1;
                    var bottom = 0;

                    if (i == 0) left = 1;
                    if (j + 1 == _rowCount) bottom = 1;

                    var brush = (SolidColorBrush)Application.Current.Resources["PrimaryColor"];
                    var transparentColor = Color.FromArgb(26, brush.Color.R, brush.Color.G, brush.Color.B);

                    border.BorderBrush = new SolidColorBrush(transparentColor);
                    border.BorderThickness = new Thickness(left, top, right, bottom);

                    Grid.Children.Add(border);
                    Grid.SetColumn(border, i);
                    Grid.SetRow(border, j);
                }
            }
        }
    }
}
