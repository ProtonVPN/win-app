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

        Loaded += OnLoaded;
    }

    public object GetContext()
    {
        return ViewModel;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.InvalidateUpdateCommands();
    }
}
