using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Image_Transformation.Views
{
    public class CommandSlider : Slider
    {
        public static readonly DependencyProperty CommandProperty =
                               DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(CommandSlider));

        public CommandSlider()
        {
            ValueChanged += Slider_ValueChanged;
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Command != null && Command.CanExecute(null))
            {
                Command.Execute(null);
            }
        }
    }
}