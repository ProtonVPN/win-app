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

namespace ProtonVPN.Client.UI.Update;

public sealed partial class UpdateComponent : IContextAware
{
    public static readonly DependencyProperty IsImageVisibleProperty = DependencyProperty.Register(
        nameof(IsImageVisible),
        typeof(bool),
        typeof(UpdateComponent),
        new PropertyMetadata(default));

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(UpdateComponent),
        new PropertyMetadata(default));

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description),
        typeof(string),
        typeof(UpdateComponent),
        new PropertyMetadata(default));

    public bool IsImageVisible
    {
        get => (bool)GetValue(IsImageVisibleProperty);
        set => SetValue(IsImageVisibleProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public UpdateViewModel ViewModel { get; }

    public UpdateComponent()
    {
        ViewModel = App.GetService<UpdateViewModel>();

        InitializeComponent();
    }

    public object GetContext()
    {
        return ViewModel;
    }
}