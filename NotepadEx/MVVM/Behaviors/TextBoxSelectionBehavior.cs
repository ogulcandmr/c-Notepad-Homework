using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace NotepadEx.MVVM.Behaviors;

public class TextBoxSelectionBehavior : Behavior<TextBox>
{
    #region Dependency Properties
    public static readonly DependencyProperty SelectionChangedCommandProperty =
        DependencyProperty.Register(nameof(SelectionChangedCommand), typeof(ICommand),
            typeof(TextBoxSelectionBehavior));

    public static readonly DependencyProperty TextChangedCommandProperty =
        DependencyProperty.Register(nameof(TextChangedCommand), typeof(ICommand),
            typeof(TextBoxSelectionBehavior));

    public static readonly DependencyProperty PreviewKeyDownCommandProperty =
        DependencyProperty.Register(nameof(PreviewKeyDownCommand), typeof(ICommand),
            typeof(TextBoxSelectionBehavior));

    public ICommand SelectionChangedCommand
    {
        get => (ICommand)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    public ICommand TextChangedCommand
    {
        get => (ICommand)GetValue(TextChangedCommandProperty);
        set => SetValue(TextChangedCommandProperty, value);
    }

    public ICommand PreviewKeyDownCommand
    {
        get => (ICommand)GetValue(PreviewKeyDownCommandProperty);
        set => SetValue(PreviewKeyDownCommandProperty, value);
    }
    #endregion

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.SelectionChanged += TextBox_SelectionChanged;
        AssociatedObject.TextChanged += TextBox_TextChanged;
        AssociatedObject.PreviewKeyDown += TextBox_PreviewKeyDown;
    }

    protected override void OnDetaching()
    {
        if(AssociatedObject != null)
        {
            AssociatedObject.SelectionChanged -= TextBox_SelectionChanged;
            AssociatedObject.TextChanged -= TextBox_TextChanged;
            AssociatedObject.PreviewKeyDown -= TextBox_PreviewKeyDown;
        }
        base.OnDetaching();
    }

    void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if(PreviewKeyDownCommand?.CanExecute(e) == true)
            PreviewKeyDownCommand.Execute(e);
    }

    void TextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if(TextChangedCommand?.CanExecute(e) == true)
            TextChangedCommand.Execute(e);
    }

    void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        if(SelectionChangedCommand?.CanExecute(e) == true)
            SelectionChangedCommand.Execute(e);
    }
}