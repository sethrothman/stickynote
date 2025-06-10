using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace StickyNotesEdge
{
    public partial class App : System.Windows.Application
    {
        private MainWindow? _mainWindow;
        private static Mutex? _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string mutexName = "StickyNotesEdgeSingletonAppMutex"; // use a unique name
            bool createdNew;

            _mutex = new Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                // Another instance is already running
                MessageBox.Show("Sticky Notes is already running.", "Already Running", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();

                return;
            }

            base.OnStartup(e);

            _mainWindow = new MainWindow();
            _mainWindow.Hide();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }
    }
}
