using System.Text.RegularExpressions;
using System.Windows;

namespace Image_Transformation.Views
{
    /// <summary>
    /// Interaction logic for Image3DView.xaml
    /// </summary>
    public partial class Image3DView : Window
    {
        private static readonly Regex _regex = new Regex("[^0-9.-]+");

        public Image3DView()
        {
            InitializeComponent();
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = Width;
            double windowHeight = Height;
            Left = (screenWidth / 2) - (windowWidth / 2);
            Top = (screenHeight / 2) - (windowHeight / 2);
        }
    }
}