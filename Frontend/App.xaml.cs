using Frontend.View;
using Frontend.ViewModel;
using Frontend.Model;
using System.Windows;
using IntroSE.Kanban.Backend.DataAccessLayer;

namespace Frontend
{
    /// <summary>
    /// Application entry point and startup configuration.
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize backend logic
            var backendController = new BackendController();

            // Create main window and associate its view model
            var mainWindow = new MainWindow();
            var mainWindowViewModel = new MainWindowViewModel(backendController);
            mainWindow.DataContext = mainWindowViewModel;

            // Show the main window
            mainWindow.Show();
        }
    }
}
