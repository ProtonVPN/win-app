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
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

namespace ProtonVPN.Client.UI.Login.Forms;

public sealed partial class TwoFactorForm
{
    private VirtualKey _lastKey;

    public TwoFactorForm()
    {
        ViewModel = App.GetService<TwoFactorFormViewModel>();
        InitializeComponent();
    }

    public TwoFactorFormViewModel ViewModel { get; }

    public static readonly DependencyProperty TwoFactorCodeProperty =
        DependencyProperty.Register(nameof(TwoFactorCode), typeof(string), typeof(TwoFactorForm),
            new PropertyMetadata(string.Empty));

    public string TwoFactorCode
    {
        get => (string)GetValue(TwoFactorCodeProperty);
        set => SetValue(TwoFactorCodeProperty, value);
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
                    if (textBox.Name == LastDigit.Name && AuthenticateButton.Command.CanExecute(TwoFactorCode))
                    {
                        AuthenticateButton.Command.Execute(TwoFactorCode);
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

    private async void OnPaste(object sender, TextControlPasteEventArgs e)
    {
        if (sender is not TextBox)
        {
            return;
        }

        e.Handled = true;

        string text = await GetClipboardStringAsync();
        text = FilterNonDigits(text);
        text = text.Substring(0, Math.Min(text.Length, 6));

        int i = 0;
        foreach (UIElement? child in DigitsContainer.Children)
        {
            if (child is TextBox textBox)
            {
                textBox.Text = text[i++].ToString();
                if (i >= text.Length)
                {
                    break;
                }
            }
        }
    }

    private async Task<string> GetClipboardStringAsync()
    {
        DataPackageView? dataPackageView = Clipboard.GetContent();
        if (dataPackageView.Contains(StandardDataFormats.Text))
        {
            try
            {
                return (await dataPackageView.GetTextAsync()).Trim();
            }
            catch
            {
            }
        }

        return string.Empty;
    }

    private void OnGotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        FirstDigit.Focus(FocusState.Programmatic);
    }
}