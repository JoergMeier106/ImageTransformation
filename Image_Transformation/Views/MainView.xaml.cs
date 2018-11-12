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
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            Left = (screenWidth / 2) - (Width / 2);
            Top = (screenHeight / 2) - (Height / 2);
        }

        private void On2DButtonClicked(object sender, RoutedEventArgs e)
        {
            Image2DView image2DView = new Image2DView
            {
                DataContext = new Image2DViewModel()
            };
            image2DView.Show();
            Close();
        }

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