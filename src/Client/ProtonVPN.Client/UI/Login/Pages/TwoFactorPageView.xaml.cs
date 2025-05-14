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

using System.Text;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ProtonVPN.Client.Core.Bases;
using ProtonVPN.Client.Contracts.Services.Edition;

namespace ProtonVPN.Client.UI.Login.Pages;

public sealed partial class TwoFactorPageView : IContextAware
{
    private const int NUM_OF_DIGITS = 6;

    private readonly IClipboardEditor _clipboardEditor;

    private VirtualKey _lastKey;

    public TwoFactorPageViewModel ViewModel { get; }

    public static readonly DependencyProperty TwoFactorCodeProperty = DependencyProperty.Register(
        nameof(TwoFactorCode),
        typeof(string),
        typeof(TwoFactorPageView),
        new PropertyMetadata(string.Empty));

    public string TwoFactorCode
    {
        get => (string)GetValue(TwoFactorCodeProperty);
        set => SetValue(TwoFactorCodeProperty, value);
    }

    public TwoFactorPageView()
    {
        ViewModel = App.GetService<TwoFactorPageViewModel>();
        ViewModel.OnTwoFactorFailure += OnTwoFactorFailure;
        ViewModel.OnTwoFactorSuccess += OnTwoFactorSuccess;

        _clipboardEditor = App.GetService<IClipboardEditor>();

        InitializeComponent();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        FirstDigit.Loaded += OnFirstDigitLoaded;
    }

    private void OnFirstDigitLoaded(object sender, RoutedEventArgs e)
    {
        ResetForm();
    }

    private void ResetForm()
    {
        ClearAllDigits();
        FirstDigit.Focus(FocusState.Programmatic);
    }

    private void ClearAllDigits()
    {
        foreach (UIElement? child in DigitsContainer.Children)
        {
            if (child is TextBox textBox)
            {
                textBox.Text = string.Empty;
            }
        }
        TwoFactorCode = GetTwoFactorCode();
    }

    public string GetTwoFactorCode()
    {
        StringBuilder sb = new();
        foreach (UIElement? child in DigitsContainer.Children)
        {
            if (child is TextBox textBox)
            {
                sb.Append(textBox.Text);
            }
        }

        return sb.ToString();
    }

    private void OnDigitChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                return;
            }

            string text = textBox.Text;
            string filteredText = FilterNonDigits(text);
            if (filteredText.Length > 1)
            {
                filteredText = GetNumberFromVirtualKey(_lastKey);
            }

            if (text != filteredText)
            {
                textBox.Text = filteredText;
            }
            else if (text.Length > 0)
            {
                if (textBox.Name != LastDigit.Name)
                {
                    FocusManager.TryMoveFocus(FocusNavigationDirection.Next, GetNextElementOptions(textBox));
                }
            }

            TwoFactorCode = GetTwoFactorCode();

            if (textBox.Name == LastDigit.Name)
            {
                SubmitTwoFactorCodeIfPossible();
            }
        }
    }

    private string FilterNonDigits(string text)
    {
        StringBuilder filteredText = new();

        foreach (char c in text.Where(c => char.IsDigit(c) && c is >= '0' and <= '9'))
        {
            filteredText.Append(c);
        }

        return filteredText.ToString();
    }

    private void SubmitTwoFactorCodeIfPossible()
    {
        string twoFactorCode = TwoFactorCode;
        if (AuthenticateButton.Command.CanExecute(twoFactorCode) && twoFactorCode.Length == 6)
        {
            AuthenticateButton.Command.Execute(twoFactorCode);
        }
    }

    private void OnDigitBoxKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            switch (e.Key)
            {
                case VirtualKey.Back:
                    textBox.Text = string.Empty;
                    if (textBox.Name != FirstDigit.Name)
                    {
                        FocusManager.TryMoveFocus(FocusNavigationDirection.Previous, GetNextElementOptions(textBox));
                    }

                    TwoFactorCode = GetTwoFactorCode();
                    break;

                case VirtualKey.Enter:
                    if (textBox.Name == LastDigit.Name)
                    {
                        SubmitTwoFactorCodeIfPossible();
                    }
                    break;

                default:
                    _lastKey = e.Key;
                    break;
            }
        }
    }

    private FindNextElementOptions GetNextElementOptions(TextBox textBox)
    {
        return new() { SearchRoot = textBox.XamlRoot.Content };
    }

    private string GetNumberFromVirtualKey(VirtualKey key)
    {
        int keyInt = (int)key;
        return key >= VirtualKey.Number0 && key <= VirtualKey.Number9
            ? (keyInt - (int)VirtualKey.Number0).ToString()
            : (keyInt - (int)VirtualKey.NumberPad0).ToString();
    }

    private async void OnPasteAsync(object sender, TextControlPasteEventArgs e)
    {
        if (sender is not TextBox)
        {
            return;
        }

        e.Handled = true;

        string clipboardText = await _clipboardEditor.GetTextAsync();
        clipboardText = FilterNonDigits(clipboardText);
        clipboardText = clipboardText.Substring(0, Math.Min(clipboardText.Length, NUM_OF_DIGITS));

        if (string.IsNullOrEmpty(clipboardText))
        { 
            return;
        }

        int i = 0;
        foreach (UIElement? child in DigitsContainer.Children)
        {
            if (child is TextBox textBox)
            {
                textBox.Text = clipboardText[i++].ToString();
                if (i >= clipboardText.Length)
                {
                    break;
                }
            }
        }
    }

    private void OnGotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }

    private void OnTwoFactorFailure(object? sender, EventArgs e)
    {
        ResetForm();
    }

    private void OnTwoFactorSuccess(object? sender, EventArgs e)
    {
        ClearAllDigits();
    }

    public object GetContext()
    {
        return ViewModel;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Activate();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Deactivate();
        ClearAllDigits();
    }
}