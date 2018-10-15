using Image_Transformation.ViewModels;
using Image_Transformation.Views;
using System.Windows;

namespace Image_Transformation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow = new MainView
            {
                DataContext = new MainViewModel(new ImageLoaderBuilder())
            };

            MainWindow.Show();
        }
    }
}