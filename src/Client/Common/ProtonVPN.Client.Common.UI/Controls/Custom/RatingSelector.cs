/*
 * Copyright (c) 2025 Proton AG
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

using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using ProtonVPN.Client.Common.Constants;

namespace ProtonVPN.Client.Common.UI.Controls.Custom;

public class RatingSelector : Control
{
    public static readonly DependencyProperty ScoreProperty = DependencyProperty.Register(
        nameof(Score),
        typeof(int),
        typeof(RatingSelector),
        new PropertyMetadata(NpsSurveyConstants.DEFAULT_SCORE, OnScoreChanged));

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum),
        typeof(int),
        typeof(RatingSelector),
        new PropertyMetadata(0, OnMinMaxChanged));

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum),
        typeof(int),
        typeof(RatingSelector),
        new PropertyMetadata(10, OnMinMaxChanged));

    private ItemsRepeater? _repeater;

    public int Score
    {
        get => (int)GetValue(ScoreProperty);
        set => SetValue(ScoreProperty, value);
    }

    public int Minimum
    {
        get => (int)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _repeater = GetTemplateChild("PART_Rep") as ItemsRepeater;

        if (_repeater is not null)
        {
            _repeater.ElementPrepared -= OnElementPrepared;
            _repeater.ElementPrepared += OnElementPrepared;
        }

        RefreshItems();
        SynchroniseVisualState();
    }

    private void OnElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is ToggleButton btn)
        {
            btn.Click -= OnScoreButtonClick;
            btn.Click += OnScoreButtonClick;
        }
    }

    private void OnScoreButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton btn && int.TryParse(btn.Tag?.ToString(), out int v))
        {
            if (v == Score)
            {
                btn.IsChecked = true;
            }
            else
            {
                Score = v;
            }
        }
    }

    private void RefreshItems()
    {
        if (_repeater is not null)
        {
            _repeater.ItemsSource = Enumerable.Range(Minimum, Maximum - Minimum + 1).ToArray();
        }
    }

    private void SynchroniseVisualState()
    {
        if (_repeater is null)
        {
            return;
        }

        for (int i = 0; i < _repeater.ItemsSourceView.Count; i++)
        {
            if (_repeater.TryGetElement(i) is ToggleButton btn)
            {
                btn.IsChecked = (i + Minimum) == Score;
            }
        }
    }

    private static void OnScoreChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        (d as RatingSelector)?.SynchroniseVisualState();
    }

    private static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        (d as RatingSelector)?.RefreshItems();
    }
}