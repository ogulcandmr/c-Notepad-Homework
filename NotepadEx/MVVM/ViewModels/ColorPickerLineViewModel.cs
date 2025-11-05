using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using NotepadEx.MVVM.View;
using NotepadEx.Services.Interfaces;
using NotepadEx.Theme;
using NotepadEx.Util;
using Brush = System.Windows.Media.Brush;

namespace NotepadEx.MVVM.ViewModels
{
    public class ColorPickerLineViewModel : ViewModelBase
    {
        private string themeName;
        private string themePath;
        private ThemeObject themeObj;
        private bool isGradient;
        private Brush previewImage;
        private IThemeService themeService;

        public string ThemeName
        {
            get => themeName;
            set => SetProperty(ref themeName, value);
        }

        public Brush PreviewImage
        {
            get => previewImage;
            set => SetProperty(ref previewImage, value);
        }

        public bool IsGradient
        {
            get => isGradient;
            set
            {
                if(SetProperty(ref isGradient, value))
                {
                    if(themeObj != null)
                        themeObj.isGradient = value;

                    UpdatePreviewColor();
                }
            }
        }

        public ICommand RandomizeCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }

        public ColorPickerLineViewModel()
        {
            RandomizeCommand = new RelayCommand(ExecuteRandomize);
            EditCommand = new RelayCommand(ExecuteEdit);
            CopyCommand = new RelayCommand(ExecuteCopy);
            PasteCommand = new RelayCommand(ExecutePaste);
        }

        public void SetupThemeObj(ThemeObject obj, string themePath, string friendlyThemeName, IThemeService service)
        {
            ThemeName = friendlyThemeName;
            this.themePath = themePath;
            themeObj = obj;
            this.themeService = service;

            if(obj == null)
            {
                IsGradient = false;
                return;
            }

            IsGradient = obj.isGradient;
            UpdatePreviewColor();
        }

        private void UpdatePreviewColor()
        {
            if(themeObj == null) return;

            if(IsGradient)
                PreviewImage = themeObj.gradient ?? CreateDefaultGradient();
            else
                PreviewImage = new SolidColorBrush(themeObj.color ?? Colors.Gray);

            UpdateResourceBrush();
        }

        private LinearGradientBrush CreateDefaultGradient() => new LinearGradientBrush
        {
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Colors.White, 0),
                new GradientStop(Colors.Black, 1)
            }
        };

        private void UpdateResourceBrush()
        {
            if(IsGradient)
            {
                AppResourceUtil<LinearGradientBrush>.TrySetResource(Application.Current, themePath, PreviewImage as LinearGradientBrush);
                if(themeObj != null) themeObj.gradient = PreviewImage as LinearGradientBrush;
            }
            else
            {
                AppResourceUtil<SolidColorBrush>.TrySetResource(Application.Current, themePath, PreviewImage as SolidColorBrush);
                if(themeObj != null) themeObj.color = (PreviewImage as SolidColorBrush)?.Color;
            }

            // This is the crucial step that notifies MainWindow to update non-dependency properties
            // like the AvalonEdit brushes.
            themeService?.TriggerLiveUpdate();
        }

        private void ExecuteRandomize()
        {
            if(IsGradient)
            {
                var brush = ColorUtil.GetRandomLinearGradientBrush(180);
                PreviewImage = brush;
                if(themeObj != null) themeObj.gradient = brush;
            }
            else
            {
                byte minAlpha = System.Random.Shared.NextDouble() >= 0.5 ? (byte)128 : (byte)255;
                var brush = ColorUtil.GetRandomColorBrush(minAlpha);
                PreviewImage = brush;
                if(themeObj != null) themeObj.color = brush.Color;
            }
            UpdateResourceBrush();
        }

        private void ExecuteEdit()
        {
            if(!IsGradient)
            {
                HandleColorEdit();
            }
            else
            {
                HandleGradientEdit();
            }
        }

        private void HandleColorEdit()
        {
            var colorPickerWindow = new ColorPickerWindow();
            colorPickerWindow.myColorPicker.SetInitialColor(themeObj?.color ?? Colors.Gray);
            colorPickerWindow.myColorPicker.OnSelectedColorChanged += () =>
            {
                var newBrush = new SolidColorBrush(colorPickerWindow.SelectedColor);
                PreviewImage = newBrush;
                if(themeObj != null) themeObj.color = colorPickerWindow.SelectedColor;
                UpdateResourceBrush();
            };

            // We only need to "confirm" the change if the user clicks OK.
            // The live updates happen via the OnSelectedColorChanged event.
            colorPickerWindow.ShowDialog();
        }

        private void HandleGradientEdit()
        {
            var gradientPickerWindow = new GradientPickerWindow();

            if(PreviewImage is LinearGradientBrush gradientBrush)
                gradientPickerWindow.SetGradient(gradientBrush);
            else if(themeObj?.gradient != null)
                gradientPickerWindow.SetGradient(themeObj.gradient);

            gradientPickerWindow.OnSelectedColorChanged += () =>
            {
                PreviewImage = gradientPickerWindow.GradientBrush;
                UpdateResourceBrush();
            };

            if(gradientPickerWindow.ShowDialog() == true)
            {
                if(themeObj != null) themeObj.gradient = gradientPickerWindow.GradientBrush;
                UpdateResourceBrush();
            }
            // On cancel, no need to do anything as live updates were temporary
        }

        private void ExecuteCopy()
        {
            if(IsGradient)
            {
                if(PreviewImage is LinearGradientBrush gradient)
                {
                    var serializedGradient = ColorUtil.SerializeGradient(gradient);
                    Clipboard.SetText(serializedGradient);
                }
            }
            else
            {
                if(PreviewImage is SolidColorBrush colorBrush)
                {
                    Clipboard.SetText(ColorUtil.ColorToHexString(colorBrush.Color));
                }
            }
        }

        private void ExecutePaste()
        {
            var clipboardText = Clipboard.GetText();
            var gradient = ColorUtil.DeserializeGradient(clipboardText);

            if(gradient != null)
            {
                PreviewImage = gradient;
                if(themeObj != null) themeObj.gradient = gradient;
                IsGradient = true; // This will trigger UpdateResourceBrush via its setter
            }
            else
            {
                var color = ColorUtil.HexStringToColor(clipboardText);
                if(color.HasValue)
                {
                    var brush = new SolidColorBrush(color.Value);
                    PreviewImage = brush;
                    if(themeObj != null) themeObj.color = color.Value;
                    IsGradient = false; // This will trigger UpdateResourceBrush via its setter
                }
            }
        }
    }
}