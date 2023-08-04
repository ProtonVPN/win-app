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

using System.Windows.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class StepContentControl : ContentControl
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(StepContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty CurrentStepProperty =
        DependencyProperty.Register(nameof(CurrentStep), typeof(int), typeof(StepContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty TotalStepsProperty =
        DependencyProperty.Register(nameof(TotalSteps), typeof(int), typeof(StepContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty MoveBackwardCommandProperty =
        DependencyProperty.Register(nameof(MoveBackwardCommand), typeof(ICommand), typeof(StepContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty MoveForwardCommandProperty =
        DependencyProperty.Register(nameof(MoveForwardCommand), typeof(ICommand), typeof(StepContentControl), new PropertyMetadata(default));

    public static readonly DependencyProperty IsHeaderVisibleProperty =
        DependencyProperty.Register(nameof(IsHeaderVisible), typeof(bool), typeof(StepContentControl), new PropertyMetadata(true));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public int CurrentStep
    {
        get => (int)GetValue(CurrentStepProperty);
        set => SetValue(CurrentStepProperty, value);
    }

    public int TotalSteps
    {
        get => (int)GetValue(TotalStepsProperty);
        set => SetValue(TotalStepsProperty, value);
    }

    public ICommand MoveBackwardCommand
    {
        get => (ICommand)GetValue(MoveBackwardCommandProperty);
        set => SetValue(MoveBackwardCommandProperty, value);
    }

    public ICommand MoveForwardCommand
    {
        get => (ICommand)GetValue(MoveForwardCommandProperty);
        set => SetValue(MoveForwardCommandProperty, value);
    }

    public bool IsHeaderVisible
    {
        get => (bool)GetValue(IsHeaderVisibleProperty);
        set => SetValue(IsHeaderVisibleProperty, value);
    }

    public StepContentControl()
    {
        DefaultStyleKey = typeof(StepContentControl);
    }
}