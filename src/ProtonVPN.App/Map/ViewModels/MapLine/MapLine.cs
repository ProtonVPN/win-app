/*
 * Copyright (c) 2022 Proton Technologies AG
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
using System.Windows.Media;
using ProtonVPN.Core.MVVM;

namespace ProtonVPN.Map.ViewModels.MapLine
{
    public abstract class MapLine : ViewModel
    {
        public double HScale { get; set; } = 1;

        private double _x1;
        private double _y1;
        private double _x2;
        private double _y2;
        private double _strokeThickness;

        public double X1
        {
            get => _x1;
            set => Set(ref _x1, value);
        }

        public double Y1
        {
            get => _y1;
            set => Set(ref _y1, value);
        }
        public double X2
        {
            get => _x2;
            set => Set(ref _x2, value);
        }
        public double Y2
        {
            get => _y2;
            set => Set(ref _y2, value);
        }

        public double StrokeThickness
        {
            get => _strokeThickness;
            set => Set(ref _strokeThickness, value);
        }

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set
            {
                if (_connected == value) return;
                _connected = value;
                Visible = value;
                OnPropertyChanged("Color");
            }
        }

        private bool _active;
        public bool Active
        {
            get => _active;
            set
            {
                if (_active == value) return;
                _active = value;
                OnPropertyChanged("Color");
            }
        }

        private bool _visible;
        public bool Visible
        {
            get => _visible;
            set => Set(ref _visible, value);
        }

        public string Color
        {
            get
            {
                var findResource = (Brush)Application.Current.FindResource("PrimaryColor");
                var primaryColor = findResource?.ToString();
                if (this is SecureCoreLine)
                {
                    return primaryColor;
                }

                if (Active)
                {
                    return "#767682";
                }

                if (Connected)
                {
                    return primaryColor;
                }

                return "#55565c";
            }
        }

        public abstract void ApplyMapScale(double scale);
    }
}
