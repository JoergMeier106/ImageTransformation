using System;
using System.Windows;

namespace Image_Transformation.Views
{
    /// <summary>
    /// Interaction logic for Image3DView.xaml
    /// </summary>
    public partial class Image3DView : Window
    {
        public Image3DView()
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

        private void OnBackClicked(object sender, EventArgs e)
        {
            MainView mainView = new MainView(); ;
            mainView.Show();
            Close();
        }
    }
}