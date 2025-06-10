using Gma.System.MouseKeyHook;
using GongSolutions.Wpf.DragDrop;
using StickyNotesEdge.Models;
using StickyNotesEdge.Services;
using StickyNotesEdge.ViewModels;
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
        private MainViewModel viewModel;
        private NoteManager _noteManager = new();

        private bool _visible = false;
        private IKeyboardMouseEvents _globalHook;

        private const double ArrowButtonWidth = 30 + 10; // Width + margin
        private const double CloseButtonWidth = 30 + 10; // Width + margin
        private const double AddButtonWidth = 48 + 10;   // Width + margin

        //private DispatcherTimer _scrollTimer;
        private Button? _currentScrollButton;

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

            viewModel = new MainViewModel(new DialogService());
            DataContext = viewModel;
            var dropHandler = new NotesDropHandler(viewModel);

            viewModel.RequestLargeNoteWindow += ShowLargeNoteWindow;
            viewModel.RequestScrollToEnd += () =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    NotesScrollViewer.ScrollToRightEnd();
                }), DispatcherPriority.Background);
            };

            viewModel.NewNoteAdded += note =>
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    // Wait a tiny bit more to ensure the container is ready
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var container = NotesPanel.ItemContainerGenerator.ContainerFromItem(note) as ContentPresenter;
                        if (container != null)
                        {
                            var noteControl = VisualTreeHelper.GetChild(container, 0) as StickyNoteControl;
                            var textBox = noteControl?.FindName("NoteTextBox") as TextBox; // Adjust name if needed
                            textBox?.Focus();
                        }
                    }), DispatcherPriority.Loaded);
                }), DispatcherPriority.Background);
            };


            this.Visibility = Visibility.Hidden;

            // Hook up global mouse event listener
            _globalHook = Hook.GlobalEvents();
            _globalHook.MouseDownExt += GlobalHook_MouseDownExt;

            this.Left = 0;
            this.Width = SystemParameters.WorkArea.Width;
            this.Top = SystemParameters.WorkArea.Bottom - this.Height - 10;

            AdjustNotesScrollViewerWidth();
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
            //save
            viewModel.SaveChanges(viewModel.Notes);

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (s, e) => this.Visibility = Visibility.Hidden;
            this.BeginAnimation(Window.OpacityProperty, fadeOut);
            _visible = false;
        }

        private void GlobalHook_MouseDownExt(object? sender, MouseEventExtArgs e)
        {
            if (viewModel.NotePopupIsOpen)
                return; // Ignore mouse events while popup is open

            var workArea = SystemParameters.WorkArea;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            // Check if the mouse is physically at the very bottom of the screen
            if (screenHeight - e.Y == 1 && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                ShowWithAnimation();
                e.Handled = true;
                return;
            }

            // Get the window's screen rectangle
            var windowRect = new System.Drawing.Rectangle(
                (int)this.Left,
                (int)this.Top,
                (int)this.ActualWidth,
                (int)this.ActualHeight);

            // Check if the mouse is outside the window and the panel is visible
            if (this.IsVisible && !windowRect.Contains(e.X, e.Y))
            {
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

        private void LeftArrowButton_Click(object sender, RoutedEventArgs e)
        {
            NotesScrollViewer.AnimateScroll(NotesScrollViewer.HorizontalOffset - 250);
        }

        private void RightArrowButton_Click(object sender, RoutedEventArgs e)
        {
            NotesScrollViewer.AnimateScroll(NotesScrollViewer.HorizontalOffset + 250);
        }

        public class NotesDropHandler(MainViewModel viewModel) : IDropTarget
        {
            private MainViewModel _viewModel = viewModel;

            public event Action? DropCompleted;

            public void DragOver(IDropInfo dropInfo)
            {
                // Allow move operations
                dropInfo.Effects = DragDropEffects.Move;
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
            }

            public void Drop(IDropInfo dropInfo)
            {
                var sourceItem = dropInfo.Data as StickyNote;
                var targetList = dropInfo.TargetCollection as IList<StickyNote>;

                if (sourceItem == null || targetList == null) return;

                // Handle drop logic. For example, move the sourceItem to the new position.
                // Note: If you are using a collection that does not support direct reordering,
                // you may need to remove and re-insert the item at the new index.

                // Example: Remove and re-insert at the correct position (assuming single selection)
                if (dropInfo.InsertIndex != -1)
                {
                    targetList.Remove(sourceItem);
                    targetList.Insert(dropInfo.InsertIndex, sourceItem);
                }

                // Now update SequenceNumber for all notes to match their new order
                for (int i = 0; i < targetList.Count; i++)
                {
                    targetList[i].SequenceNumber = i; // or i + 1 if you want to start from 1
                }

                _viewModel.SaveChanges(targetList);
            }
        }

        public void ShowLargeNoteWindow(StickyNote note)
        {
            var largeNote = new Window
            {
                Title = "Sticky Note (View Large)",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResize,
                Owner = this,
                Content = new Border
                {
                    Padding = new Thickness(20),
                    Background = new SolidColorBrush(Color.FromRgb(255, 255, 200)),
                    Child = new TextBox
                    {
                        Text = note.Text,
                        FontSize = 24,
                        Background = Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        IsReadOnly = true,
                        TextWrapping = TextWrapping.Wrap,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                    }
                }
            };

            var vm = DataContext as MainViewModel;
            if (vm != null)
                vm.NotePopupIsOpen = true;

            largeNote.Closed += (s, args) =>
            {
                if (vm != null)
                    vm.NotePopupIsOpen = false;
            };

            largeNote.ShowDialog();
        }
    }
}
