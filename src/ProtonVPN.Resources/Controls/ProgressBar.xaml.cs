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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ProtonVPN.Resource.Controls
{
    public partial class ProgressBar
    {
        public ProgressBar()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register("Step", typeof(int), typeof(ProgressBar), new PropertyMetadata(1, Draw));

        public static readonly DependencyProperty TotalStepsProperty =
            DependencyProperty.Register("TotalSteps", typeof(int), typeof(ProgressBar), new PropertyMetadata(1, Draw));

        public static readonly DependencyProperty BarColorProperty =
            DependencyProperty.Register("BarColor", typeof(Brush), typeof(ProgressBar),
                new PropertyMetadata(Brushes.Black, Draw));

        public static readonly DependencyProperty CompleteBarColorProperty =
            DependencyProperty.Register("CompleteBarColor", typeof(Brush), typeof(ProgressBar),
                new PropertyMetadata(Brushes.White, Draw));

        public int Step
        {
            get => (int)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        public int TotalSteps
        {
            get => (int)GetValue(TotalStepsProperty);
            set => SetValue(TotalStepsProperty, value);
        }

        public Brush BarColor
        {
            get => (Brush)GetValue(BarColorProperty);
            set => SetValue(BarColorProperty, value);
        }

        public Brush CompleteBarColor
        {
            get => (Brush)GetValue(CompleteBarColorProperty);
            set => SetValue(CompleteBarColorProperty, value);
        }

        private static void Draw(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProgressBar control = (ProgressBar)d;
            Grid grid = control.Grid;
            GridLength width = new(1, GridUnitType.Star);
            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();

            for (int i = 1; i <= control.TotalSteps; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
                grid.Children.Add(GetBar(i, control));
            }
        }

        private static Border GetBar(int step, ProgressBar control)
        {
            Brush background = step <= control.Step ? control.CompleteBarColor : control.BarColor;
            Border border = new Border { Background = background, Margin = new Thickness(2, 0, 2, 0) };
            Grid.SetColumn(border, step - 1);
            return border;
        }
    }
}