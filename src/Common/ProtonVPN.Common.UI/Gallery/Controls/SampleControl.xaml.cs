using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ProtonVPN.Common.UI.Gallery.Controls;

[ContentProperty(Name = "Sample")]
public sealed partial class SampleControl : UserControl
{



    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(SampleControl), new PropertyMetadata(default));


    public object Sample
    {
        get => (object)GetValue(SampleProperty);
        set => SetValue(SampleProperty, value);
    }

    public static readonly DependencyProperty SampleProperty =
        DependencyProperty.Register(nameof(Sample), typeof(object), typeof(SampleControl), new PropertyMetadata(default));


    public object Options
    {
        get => (object)GetValue(OptionsProperty);
        set => SetValue(OptionsProperty, value);
    }

    public static readonly DependencyProperty OptionsProperty =
        DependencyProperty.Register(nameof(Options), typeof(object), typeof(SampleControl), new PropertyMetadata(default));



    public SampleControl()
    {
        InitializeComponent();
    }

    private void OnToggleViewboxChecked(object sender, RoutedEventArgs e)
    {
        vbSample.Stretch = tgViewbox.IsChecked.GetValueOrDefault() ? Stretch.Uniform : Stretch.None;
    }
}
