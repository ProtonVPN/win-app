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

using System.Collections;
using System.ComponentModel;
using Microsoft.UI.Xaml;

namespace ProtonVPN.Client.Common.UI.AttachedProperties;

public class FieldValidation
{
    /// <summary>
    /// Gets or sets a provider that implements input validation through <see cref="INotifyDataErrorInfo"/>.
    /// Must be used along with the <see cref="ValidationPropertyNameProperty"/>.
    /// </summary>
    public static readonly DependencyProperty ValidationProviderProperty
        = DependencyProperty.RegisterAttached("ValidationProvider", typeof(INotifyDataErrorInfo), typeof(FieldValidation), new(null, OnValidationProviderChanged));

    /// <summary>
    /// Gets or sets the name of the property to validate.
    /// The actual validation is done through the validation provider (see <see cref="ValidationProviderProperty"/>).
    /// </summary>
    public static readonly DependencyProperty ValidationPropertyNameProperty
        = DependencyProperty.RegisterAttached("ValidationPropertyName", typeof(string), typeof(FieldValidation), null);

    /// <summary>
    /// Gets an enumerable of all active validation errors from the provider.
    /// </summary>
    public static readonly DependencyProperty ErrorsProperty
        = DependencyProperty.RegisterAttached("Errors", typeof(IEnumerable), typeof(FieldValidation), null);

    /// <summary>
    /// Gets or sets a template used to display validation errors on the attached control.
    /// The control must handle showing the items on its own.
    /// </summary>
    public static readonly DependencyProperty ErrorTemplateProperty
        = DependencyProperty.RegisterAttached("ErrorTemplate", typeof(object), typeof(FieldValidation), null);

    /// <summary>
    /// Gets or sets a value indicating whether the control has errors.
    /// </summary>
    public static readonly DependencyProperty HasErrorsProperty =
        DependencyProperty.RegisterAttached("HasErrors", typeof(bool), typeof(FieldValidation), new PropertyMetadata(default));

    /// <summary>
    /// Gets or sets the global error message
    /// </summary>
    public static readonly DependencyProperty ErrorMessageProperty =
        DependencyProperty.RegisterAttached("ErrorMessage", typeof(string), typeof(FieldValidation), new PropertyMetadata(default));

    public static bool GetHasErrors(DependencyObject obj)
    {
        return (bool)obj.GetValue(HasErrorsProperty);
    }

    public static void SetHasErrors(DependencyObject obj, bool value)
    {
        obj.SetValue(HasErrorsProperty, value);
    }

    public static string GetErrorMessage(DependencyObject obj)
    {
        return (string)obj.GetValue(ErrorMessageProperty);
    }

    public static void SetErrorMessage(DependencyObject obj, string value)
    {
        obj.SetValue(ErrorMessageProperty, value);
    }

    public static string GetValidationPropertyName(DependencyObject obj)
    {
        return (string)obj.GetValue(ValidationPropertyNameProperty);
    }

    public static void SetValidationPropertyName(DependencyObject obj, string value)
    {
        obj.SetValue(ValidationPropertyNameProperty, value);
    }

    public static IEnumerable GetErrors(DependencyObject obj)
    {
        return (IEnumerable)obj.GetValue(ErrorsProperty);
    }

    public static void SetErrors(DependencyObject obj, IEnumerable errors)
    {
        obj.SetValue(ErrorsProperty, errors);
    }

    public static object GetErrorTemplate(DependencyObject obj)
    {
        return obj.GetValue(ErrorTemplateProperty);
    }

    public static void SetErrorTemplate(DependencyObject obj, object value)
    {
        obj.SetValue(ErrorTemplateProperty, value);
    }

    public static INotifyDataErrorInfo GetValidationProvider(DependencyObject obj)
    {
        return (INotifyDataErrorInfo)obj.GetValue(ValidationProviderProperty);
    }

    public static void SetValidationProvider(DependencyObject obj, INotifyDataErrorInfo value)
    {
        obj.SetValue(ValidationProviderProperty, value);
    }

    private static void OnValidationProviderChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        sender.SetValue(ErrorsProperty, null);
        if (args.NewValue is INotifyDataErrorInfo info)
        {
            string propertyName = GetValidationPropertyName(sender);
            if (!string.IsNullOrEmpty(propertyName))
            {
                info.ErrorsChanged += (source, eventArgs) =>
                {
                    if (eventArgs.PropertyName == propertyName)
                    {
                        sender.SetValue(ErrorsProperty, info.GetErrors(propertyName));
                    }
                };

                sender.SetValue(ErrorsProperty, info.GetErrors(propertyName));
            }
        }
    }
}