using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using NotepadEx.Util;
using Point = System.Windows.Point;
namespace NotepadEx.MVVM.ViewModels;

public class ScrollManager
{
    ScrollViewer _scrollViewer;
    ScrollBar _verticalScrollBar;
    ScrollBar _horizontalScrollBar;
    readonly TextBox _textBox;
    const double PADDING = 20;
    const double SCROLL_ZONE_PERCENTAGE  = 0.20;
    bool _isInitialized;
    bool isMouseDown;
    DispatcherTimer _scrollTimer;
    double _scrollSpeed = 25;
    bool isAutoScrolling;
    double scrollPositionBeforeRightClick;
    bool expectingRightClickScroll;
    DispatcherTimer _rightClickScrollFixTimer;

    bool _isSelecting => _textBox.SelectionLength > 0 && isMouseDown;

    public ScrollManager(TextBox textBox)
    {
        _textBox = textBox;
        _textBox.Loaded += TextBox_Loaded;
        _textBox.PreviewMouseWheel += HandleMouseWheel;
        InitializeTimer();
        InitializeMouseEvents();
    }

    void ScrollTimer_Tick(object sender, EventArgs e)
    {
        if(!_isSelecting || _scrollViewer == null) return;

        double speed = (double)_scrollTimer.Tag;

        double newOffset = _scrollViewer.VerticalOffset + (speed / 2400.0);
        newOffset = Math.Max(0, Math.Min(newOffset, _scrollViewer.ScrollableHeight));

        _scrollViewer.ScrollToVerticalOffset(newOffset);
    }

    void RightClickScrollFixTimer_Tick(object sender, EventArgs e)
    {
        if(!expectingRightClickScroll || _scrollViewer == null)
        {
            _rightClickScrollFixTimer?.Stop();
            return;
        }

        double currentPosition = _scrollViewer.VerticalOffset;
        if(Math.Abs(currentPosition - scrollPositionBeforeRightClick) > 0.1)
        {
            // Scroll position changed unexpectedly during right-click - restore it
            _scrollViewer.ScrollToVerticalOffset(scrollPositionBeforeRightClick);

            // Stop monitoring now that we've corrected it
            expectingRightClickScroll = false;
            _rightClickScrollFixTimer?.Stop();
        }
    }

    void TextBox_MouseMove(object sender, MouseEventArgs e)
    {
        if(_isSelecting && _scrollViewer != null)
        {
            Point mousePosition = e.GetPosition(_scrollViewer);
            double zoneHeight = _scrollViewer.ActualHeight * SCROLL_ZONE_PERCENTAGE;

            // Top scroll zone check
            if(mousePosition.Y >= 0 && mousePosition.Y <= zoneHeight)
            {
                double scrollFactor = 1 - (mousePosition.Y / zoneHeight);
                isAutoScrolling = true;
                StartScrolling(-_scrollSpeed * Math.Pow(scrollFactor, 3));
            }
            // Bottom scroll zone check
            else if(mousePosition.Y >= _scrollViewer.ActualHeight - zoneHeight &&
                    mousePosition.Y <= _scrollViewer.ActualHeight)
            {
                double scrollFactor = (mousePosition.Y - (_scrollViewer.ActualHeight - zoneHeight)) / zoneHeight;
                isAutoScrolling = true;
                StartScrolling(_scrollSpeed * Math.Pow(scrollFactor, 3));
            }
            else
            {
                isAutoScrolling = false;
                StopScrolling();
            }
        }
    }

    void ScrollToCaretPosition(bool ensureVisible = false)
    {
        if(_scrollViewer == null || isAutoScrolling || _isSelecting || expectingRightClickScroll) return;

        try
        {
            var rect = _textBox.GetRectFromCharacterIndex(_textBox.CaretIndex);

            // If caret is outside the visible area (either above or below), scroll to it
            if(rect.Top < 0 ||
               rect.Bottom > _scrollViewer.ViewportHeight)
            {
                double offset;

                if(rect.Top < 0)
                    offset = _scrollViewer.VerticalOffset + rect.Top - PADDING; // Scroll up to show caret with padding
                else
                    offset = _scrollViewer.VerticalOffset + rect.Bottom - _scrollViewer.ViewportHeight + PADDING; // Scroll down

                _scrollViewer.ScrollToVerticalOffset(offset);
            }

            // Also check horizontal scrolling
            if(rect.Left < 0 ||
               rect.Right > _scrollViewer.ViewportWidth)
            {
                double offset;

                if(rect.Left < 0)
                    offset = _scrollViewer.HorizontalOffset + rect.Left - PADDING;
                else
                    offset = _scrollViewer.HorizontalOffset + rect.Right - _scrollViewer.ViewportWidth + PADDING;

                _scrollViewer.ScrollToHorizontalOffset(offset);
            }
        }
        catch(Exception)
        {
            // Caret index might be invalid, just ignore
        }
    }

    void TextBox_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if(e.RightButton == MouseButtonState.Pressed && _textBox.SelectionLength > 0 && _scrollViewer != null)
        {
            // Store scroll position before right-click
            scrollPositionBeforeRightClick = _scrollViewer.VerticalOffset;
            expectingRightClickScroll = true;

            // Start monitoring for scroll changes due to right-click
            if(_rightClickScrollFixTimer == null)
            {
                _rightClickScrollFixTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
                _rightClickScrollFixTimer.Tick += RightClickScrollFixTimer_Tick;
            }

            _rightClickScrollFixTimer.Start();

            // Schedule a cleanup after a reasonable timeout for context menu operations
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                // After 2 seconds, we assume the context menu operation is done
                expectingRightClickScroll = false;
                _rightClickScrollFixTimer?.Stop();
            }));
        }

        isMouseDown = true;
        isAutoScrolling = false;
    }

    void TextBox_MouseUp(object sender, MouseButtonEventArgs e)
    {
        // If this was the completion of a right-click, don't reset yet
        if(e.RightButton != MouseButtonState.Pressed)
        {
            expectingRightClickScroll = false;
            _rightClickScrollFixTimer?.Stop();
        }

        isMouseDown = false;
        isAutoScrolling = false;
        StopScrolling();
    }

    void StartScrolling(double speed)
    {
        _scrollTimer.Tag = speed;
        if(!_scrollTimer.IsEnabled)
            _scrollTimer.Start();
    }

    void StopScrolling() => _scrollTimer.Stop();

    void InitializeTimer()
    {
        _scrollTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(20)
        };
        _scrollTimer.Tick += ScrollTimer_Tick;
    }

    void InitializeMouseEvents()
    {
        _textBox.PreviewMouseDown += TextBox_MouseDown;
        _textBox.PreviewMouseUp += TextBox_MouseUp;
        _textBox.PreviewMouseMove += TextBox_MouseMove;
    }

    void TextBox_Loaded(object sender, RoutedEventArgs e)
    {
        if(!_isInitialized)
        {
            InitializeScrollViewer();
            _isInitialized = true;
        }
    }

    void InitializeScrollViewer()
    {
        _scrollViewer = VisualTreeUtil.FindVisualChildren<ScrollViewer>(_textBox).FirstOrDefault();
        if(_scrollViewer == null) return;

        // We no longer need to manually handle scrollbar events as the default
        // template and bindings will take care of it. We just need the reference
        // for programmatic scrolling.
        _verticalScrollBar = _scrollViewer.Template.FindName("PART_VerticalScrollBar", _scrollViewer) as ScrollBar;
        _horizontalScrollBar = _scrollViewer.Template.FindName("PART_HorizontalScrollBar", _scrollViewer) as ScrollBar;
    }

    public void HandleNavigationKey(Key key, ModifierKeys modifiers) =>
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
        {
            ScrollToCaretPosition((modifiers & ModifierKeys.Control) == ModifierKeys.Control);
        }));

    public void HandleTextChanged() => ScrollToCaretPosition(false);

    public void HandleSelectionChanged() => ScrollToCaretPosition(false);

    public void HandleMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if(_scrollViewer == null) return;

        // A standard mouse wheel delta is 120. We can simulate the default 
        // scroll amount of 3 lines for a smoother, more standard feel.
        double lineHeight = _textBox.FontSize * _textBox.FontFamily.LineSpacing;
        double scrollAmount = (double)e.Delta / 120.0 * (3 * lineHeight);

        double newOffset = _scrollViewer.VerticalOffset - scrollAmount;

        _scrollViewer.ScrollToVerticalOffset(newOffset);
        e.Handled = true;
    }
}