using Image_Transformation.ViewModels;
using System.Windows;

namespace Image_Transformation.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            CenterWindow();
        }

        /// <summary>
        /// Move the this window to the center of the main screen.
        /// </summary>
        private void CenterWindow()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = Width;
            double windowHeight = Height;
            Left = (screenWidth / 2) - (windowWidth / 2);
            Top = (screenHeight / 2) - (windowHeight / 2);
        }

        /// <summary>
        /// Opens the Image2DView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void On2DButtonClicked(object sender, RoutedEventArgs e)
        {
            Image2DView image2DView = new Image2DView
            {
                DataContext = new Image2DViewModel()
            };
            image2DView.Show();
            Close();
        }

        /// <summary>
        /// Opens the Image3DView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void On3DButtonClicked(object sender, RoutedEventArgs e)
        {
            Image3DView image3DView = new Image3DView
            {
                DataContext = new Image3DViewModel()
            };
            image3DView.Show();
            Close();
        }
    }
}