using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace NotepadEx.MVVM.Behaviors
{
    public class WindowMouseMoveBehavior : Behavior<Window>
    {
        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.Register(nameof(MouseMoveCommand), typeof(ICommand),
                typeof(WindowMouseMoveBehavior));

        // REMOVE the ResizeCommandProperty
        // public static readonly DependencyProperty ResizeCommandProperty = ...

        public ICommand MouseMoveCommand
        {
            get => (ICommand)GetValue(MouseMoveCommandProperty);
            set => SetValue(MouseMoveCommandProperty, value);
        }

        // REMOVE the ResizeCommand property
        // public ICommand ResizeCommand { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseMove += Window_MouseMove;
        }

        protected override void OnDetaching() // Changed from Cleanup to override OnDetaching for proper behavior lifecycle
        {
            if(AssociatedObject != null)
                AssociatedObject.MouseMove -= Window_MouseMove;
            base.OnDetaching();
        }

        void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var window = AssociatedObject;
            var position = e.GetPosition(window);

            if(MouseMoveCommand?.CanExecute(position.Y) == true)
                MouseMoveCommand.Execute(position.Y);

            // REMOVE the resize logic
            // if (window.WindowState == WindowState.Normal && ResizeCommand?.CanExecute(position) == true)
            //     ResizeCommand.Execute(position);
        }
    }
}