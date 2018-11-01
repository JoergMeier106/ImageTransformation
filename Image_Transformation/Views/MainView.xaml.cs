using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace Image_Transformation.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private static readonly Regex _regex = new Regex("[^0-9.-]+");

        public MainView()
        {
            InitializeComponent();
        }

        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
    }
}