using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Image_Transformation.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class Image2DView : Window
    {
        private static readonly Regex _regex = new Regex("[^0-9.-]+");

        public Image2DView()
        {
            InitializeComponent();
            CenterWindow();
        }

        /// <summary>
        /// Check if the user entered a number!
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
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

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
    }
}