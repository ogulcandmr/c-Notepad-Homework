using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using NotepadEx.Util;

namespace NotepadEx.MVVM.ViewModels
{
    public class CustomTitleBarViewModel : ViewModelBase
    {
        Window window;
        BitmapImage iconImage;
        string titleText;
        bool isResizeable;
        bool showMinimizeButton = true;
        bool showMaximizeButton = true;
        bool showCloseButton = true;
        Action onClose;

        public string TitleText
        {
            get => titleText;
            set => SetProperty(ref titleText, value);
        }

        public BitmapImage IconImage
        {
            get => iconImage;
            set => SetProperty(ref iconImage, value);
        }

        bool _isMaximized;

        public bool IsMaximized
        {
            get => _isMaximized;
            set => SetProperty(ref _isMaximized, value);
        }

        public bool ShowMinimizeButton
        {
            get => showMinimizeButton;
            set => SetProperty(ref showMinimizeButton, value);
        }

        public bool ShowMaximizeButton
        {
            get => showMaximizeButton;
            set => SetProperty(ref showMaximizeButton, value);
        }

        public bool ShowCloseButton
        {
            get => showCloseButton;
            set => SetProperty(ref showCloseButton, value);
        }

        public ICommand MinimizeCommand { get; }
        public ICommand MaximizeCommand { get; }
        public ICommand CloseCommand { get; }
        // REMOVED: public ICommand TitleBarMouseDownCommand { get; }

        public CustomTitleBarViewModel(Window window, bool isResizeable = true)
        {
            this.window = window;
            this.isResizeable = isResizeable; // Corrected variable name

            MinimizeCommand = new RelayCommand(ExecuteMinimize);
            MaximizeCommand = new RelayCommand(ExecuteMaximize);
            CloseCommand = new RelayCommand(ExecuteClose);
            // REMOVED: TitleBarMouseDownCommand = new RelayCommand<MouseButtonEventArgs>(ExecuteTitleBarMouseDown);
        }

        void ExecuteMinimize() => window.WindowState = WindowState.Minimized;

        void ExecuteMaximize()
        {
            IsMaximized = !IsMaximized;
            // The WindowState change should be handled by the system now, but we can keep this for visual state
            if(window.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Normal;
            }
            else
            {
                window.WindowState = WindowState.Maximized;
            }
        }

        void ExecuteClose()
        {
            if(onClose != null)
                onClose();
            else
                window.Close();
        }

        // REMOVED: The ExecuteTitleBarMouseDown method is no longer needed.
        // void ExecuteTitleBarMouseDown(MouseButtonEventArgs e) { ... }

        public void Initialize(string titleText, bool showMinimize = true, bool showMaximize = true, bool showClose = true, Action onClose = null)
        {
            TitleText = titleText;
            ShowMinimizeButton = showMinimize;
            ShowMaximizeButton = showMaximize;
            ShowCloseButton = showClose;
            this.onClose = onClose;
        }
    }
}