using StickyNotesEdge.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StickyNotesEdge
{
    public partial class StickyNoteControl : UserControl
    {
        public StickyNoteControl()
        {
            InitializeComponent();
        }

        public event RoutedEventHandler? DeleteRequested;

        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            DeleteRequested?.Invoke(this, e);
        }

        private void MagnifyButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
                mainWindow._notePopupIsOpen = true;

            var largeNote = new Window
            {
                Title = "Sticky Note (View Large)",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResize,
                Owner = mainWindow,
                Content = new Border
                {
                    Padding = new Thickness(20),
                    Background = new SolidColorBrush(Color.FromRgb(255, 255, 200)),
                    Child = new TextBox
                    {
                        Text = (this.DataContext as StickyNote)?.Text ?? "",
                        FontSize = 24,
                        Background = Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        IsReadOnly = true,
                        TextWrapping = TextWrapping.Wrap,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                    }
                }
            };

            largeNote.Closed += (s, args) =>
            {
                if (mainWindow != null)
                    mainWindow._notePopupIsOpen = false;
            };

            largeNote.ShowDialog();
        }
    }
}
