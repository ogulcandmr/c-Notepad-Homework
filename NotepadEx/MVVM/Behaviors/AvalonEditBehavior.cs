using System.Windows;
using ICSharpCode.AvalonEdit;
using Microsoft.Xaml.Behaviors;

namespace NotepadEx.MVVM.Behaviors
{
    public sealed class AvalonEditBehavior : Behavior<TextEditor>
    {
        public static readonly DependencyProperty DocumentTextProperty =
            DependencyProperty.Register(nameof(DocumentText), typeof(string), typeof(AvalonEditBehavior),
            new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback));

        public string DocumentText
        {
            get => (string)GetValue(DocumentTextProperty);
            set => SetValue(DocumentTextProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if(AssociatedObject != null)
            {
                AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if(AssociatedObject != null)
            {
                AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
            }
        }

        private void AssociatedObjectOnTextChanged(object sender, System.EventArgs e)
        {
            if(sender is TextEditor textEditor)
            {
                if(textEditor.Document != null)
                {
                    DocumentText = textEditor.Document.Text;
                }
            }
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var behavior = dependencyObject as AvalonEditBehavior;
            if(behavior?.AssociatedObject != null)
            {
                var editor = behavior.AssociatedObject;
                if(editor.Document != null)
                {
                    var caretOffset = editor.CaretOffset;
                    editor.Document.Text = dependencyPropertyChangedEventArgs.NewValue.ToString();
                    editor.CaretOffset = caretOffset > editor.Document.TextLength ? editor.Document.TextLength : caretOffset;
                }
            }
        }
    }
}