using System.Windows;
using ICSharpCode.AvalonEdit;
using NotepadEx.Properties;
using NotepadEx.Util;

namespace NotepadEx
{
    public static class SettingsManager
    {
        public static void SaveSettings(Window window, TextEditor textEditor, string themeName)
        {
            Settings.Default.RecentFiles = string.Join(",", RecentFileManager.RecentFiles);
            Settings.Default.WindowSizeX = window.Width;
            Settings.Default.WindowSizeY = window.Height;
            Settings.Default.TextWrapping = textEditor.WordWrap;
            // Settings.Default.ThemeName = themeName; // This is already handled in ThemeService
            Settings.Default.FontSize = textEditor.FontSize;
            Settings.Default.FontFamily = textEditor.FontFamily.Source;
            Settings.Default.FontWeight = textEditor.FontWeight.ToString();
            Settings.Default.FontStyle = textEditor.FontStyle.ToString();

            // Note: Underline and Strikethrough are more complex in AvalonEdit and
            // require a custom IVisualLineTransformer. This functionality is omitted for now.
            Settings.Default.Underline = false;
            Settings.Default.Strikethrough = false;

            Settings.Default.Save();
        }
    }
}