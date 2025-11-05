using System;
using System.Windows;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using NotepadEx.MVVM.View.UserControls;
using NotepadEx.MVVM.ViewModels;
using NotepadEx.Properties;
using NotepadEx.Services;

namespace NotepadEx
{
    public partial class MainWindow : Window, IDisposable
    {
        readonly MainWindowViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            // Load the C# syntax highlighting definition from the embedded resource
            LoadSyntaxHighlighting();

            var windowService = new WindowService(this);
            var documentService = new DocumentService();
            var themeService = new ThemeService(Application.Current);
            var fontService = new FontService(Application.Current);
            fontService.LoadCurrentFont();

            Settings.Default.MenuBarAutoHide = false;

            DataContext = viewModel = new MainWindowViewModel(windowService, documentService, themeService, fontService, MenuItemFileDropDown, textEditor, () => SettingsManager.SaveSettings(this, textEditor, themeService.CurrentThemeName));
            viewModel.TitleBarViewModel = CustomTitleBar.InitializeTitleBar(this, "NotepadEx", onClose: Application.Current.Shutdown);

            InitializeWindowEvents();
        }

        private void LoadSyntaxHighlighting()
        {
            // The C# definition is an embedded resource in the AvalonEdit library.
            var resourceName = "ICSharpCode.AvalonEdit.Highlighting.C#.xshd";

            using(var stream = typeof(TextEditor).Assembly.GetManifestResourceStream(resourceName))
            {
                if(stream != null)
                {
                    using(var reader = new XmlTextReader(stream))
                    {
                        // This registers the highlighting definition with the HighlightingManager
                        // and applies it to the editor instance.
                        textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
            }
        }

        void InitializeWindowEvents()
        {
            StateChanged += (s, e) =>
            {
                if(WindowState != WindowState.Minimized)
                    viewModel.UpdateWindowState(WindowState);
            };

            Closed += (s, e) => viewModel.PromptToSaveChanges();
        }

        public void Dispose() => viewModel?.Cleanup();
    }
}