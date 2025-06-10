using Gma.System.MouseKeyHook;
using StickyNotesEdge.Models;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace StickyNotesEdge
{
    public partial class MainWindow : Window
    {
        private NoteManager _noteManager = new NoteManager();
        private bool _visible = false;
        private IKeyboardMouseEvents _globalHook;

        private const double ArrowButtonWidth = 30 + 10; // Width + margin
        private const double CloseButtonWidth = 30 + 10; // Width + margin
        private const double AddButtonWidth = 48 + 10;   // Width + margin

        private DispatcherTimer _scrollTimer;
        private Button _currentScrollButton;
        internal bool _notePopupIsOpen = false;

        private void AdjustNotesScrollViewerWidth()
        {
            double screenWidth = SystemParameters.WorkArea.Width;

            // Start with screen width, subtract close button and right arrow always,
            // and left arrow if it's visible
            double width = screenWidth;

            width -= CloseButtonWidth;
            width -= (RightArrowButton.Visibility == Visibility.Visible ? ArrowButtonWidth : 0);
            width -= (LeftArrowButton.Visibility == Visibility.Visible ? ArrowButtonWidth : 0);
            width -= AddButtonWidth; // Account for add button inside notes area

            // Optionally: subtract a little extra for padding
            width -= 40;

            if (width < 200) width = 200; // enforce a minimum width

            NotesScrollViewer.Width = width;
        }

        public MainWindow()
        {
            InitializeComponent();

            _scrollTimer = new DispatcherTimer();
            _scrollTimer.Interval = TimeSpan.FromMilliseconds(150);
            _scrollTimer.Tick += ScrollTimer_Tick;

            this.Visibility = Visibility.Hidden;

            // Hook up global mouse event listener
            _globalHook = Hook.GlobalEvents();
            _globalHook.MouseDownExt += GlobalHook_MouseDownExt;

            NotesPanel.ItemsSource = CreateStickyNoteControls();

            _noteManager.Notes.CollectionChanged += Notes_CollectionChanged;

            this.Left = 0;
            this.Width = SystemParameters.WorkArea.Width;
            this.Top = SystemParameters.WorkArea.Bottom - this.Height - 10;

            AdjustNotesScrollViewerWidth();
        }

        private List<StickyNoteControl> CreateStickyNoteControls()
        {
            return [.. _noteManager.Notes.Select(n => CreateStickyNoteControl(n))];
        }

        private StickyNoteControl CreateStickyNoteControl(StickyNote note)
        {
            var control = new StickyNoteControl { DataContext = note };
            control.DeleteRequested += (s, e) => OnDeleteRequest(note, e);
            return control;
        }

        private void OnDeleteRequest(StickyNote note, RoutedEventArgs e)
        {
            _notePopupIsOpen = true;
            e.Handled = true;

            if (MessageBox.Show("Delete this note?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                _noteManager.Notes.Remove(note);
                _noteManager.Save();

            }), DispatcherPriority.Background);
        }

        private void Notes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Simple refresh; in real app, use ObservableCollection binding
            NotesPanel.ItemsSource = CreateStickyNoteControls();
            AdjustNotesScrollViewerWidth();
        }

        public void ToggleShowHide()
        {
            if (_visible)
                HideWithAnimation();
            else
                ShowWithAnimation();
        }

        private void ShowWithAnimation()
        {
            this.Visibility = Visibility.Visible;
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            this.BeginAnimation(Window.OpacityProperty, fadeIn);
            _visible = true;
        }

        private void HideWithAnimation()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (s, e) => this.Visibility = Visibility.Hidden;
            this.BeginAnimation(Window.OpacityProperty, fadeOut);
            _visible = false;
        }

        private void AddNote_Click(object sender, RoutedEventArgs e)
        {
            var note = new StickyNote { Text = "New note" };
            _noteManager.Notes.Add(note);
            _noteManager.Save();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                // Scroll all the way to the right (show newest note)
                NotesScrollViewer.ScrollToRightEnd();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void GlobalHook_MouseDownExt(object? sender, MouseEventExtArgs e)
        {
            if (_notePopupIsOpen)
                return; // Ignore mouse events while popup is open

            // Get taskbar/work area info
            var workArea = SystemParameters.WorkArea;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            // Check if the mouse is physically BELOW the work area (i.e., over the taskbar or below)
            if (screenHeight - e.Y == 1)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    ShowWithAnimation();
                    e.Handled = true; // Optionally block further processing (not required)
                }
            }
            else if (this.IsVisible && e.Y < this.Top)
            {
                // If notes panel is visible and you click above it, hide it
                HideWithAnimation();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _globalHook.MouseDownExt -= GlobalHook_MouseDownExt;
            _globalHook.Dispose();
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustNotesScrollViewerWidth();
            NotesScrollViewer.ScrollToHorizontalOffset(0);

        }

        //private void LeftArrowButton_Click(object sender, RoutedEventArgs e)
        //{
        //    double offset = NotesScrollViewer.HorizontalOffset - 400; // scroll left by 200 px
        //    if (offset < 0) offset = 0;

        //    NotesScrollViewer.ScrollToHorizontalOffset(offset);
        //}

        //private void RightArrowButton_Click(object sender, RoutedEventArgs e)
        //{
        //    double maxOffset = NotesScrollViewer.ScrollableWidth;
        //    double offset = NotesScrollViewer.HorizontalOffset + 400; // scroll right by 200 px
        //    if (offset > maxOffset) offset = maxOffset;

        //    NotesScrollViewer.ScrollToHorizontalOffset(offset); e.Handled = true;
        //}

        private void LeftArrowButton_Click(object sender, RoutedEventArgs e)
        {
            NotesScrollViewer.ScrollToHorizontalOffset(NotesScrollViewer.HorizontalOffset - 30);
        }

        private void RightArrowButton_Click(object sender, RoutedEventArgs e)
        {
            NotesScrollViewer.ScrollToHorizontalOffset(NotesScrollViewer.HorizontalOffset + 30);
        }

        private void ScrollTimer_Tick(object sender, EventArgs e)
        {
            HandleScrollAction();
        }

        private void ArrowButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _currentScrollButton = sender as Button;
            _scrollTimer.Start();
            // Trigger immediate first scroll
            HandleScrollAction();
        }

        private void ArrowButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StopScrolling();
        }

        private void ArrowButton_PreviewMouseLeave(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                StopScrolling();
            }
        }

        private void HandleScrollAction()
        {
            if (_currentScrollButton == LeftArrowButton)
            {
                LeftArrowButton_Click(null, null);
            }
            else if (_currentScrollButton == RightArrowButton)
            {
                RightArrowButton_Click(null, null);
            }
        }

        private void StopScrolling()
        {
            _scrollTimer.Stop();
            _currentScrollButton = null;
        }
    }
}
