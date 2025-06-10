using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace StickyNotesEdge
{
    public partial class App : System.Windows.Application
    {
        private NotifyIcon? _notifyIcon;
        private MainWindow? _mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _mainWindow = new MainWindow();
            _mainWindow.Hide();

            _notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Information, // Use a real icon for release!
                Visible = true,
                Text = "StickyNotesEdge"
            };
            _notifyIcon.Click += (s, args) => _mainWindow.ToggleShowHide();

            // Global hotkey: Ctrl+Alt+N (very simple version, only works if app is focused)
            EventManager.RegisterClassHandler(typeof(Window),
                Keyboard.KeyDownEvent,
                new System.Windows.Input.KeyEventHandler(OnKeyDown));
        }

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt) && e.Key == Key.N)
            {
                _mainWindow?.ToggleShowHide();
                e.Handled = true;
            }
        }
    }
}
