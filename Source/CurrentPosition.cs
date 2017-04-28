using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace knights_tour
{
    class CurrentPosition : TextBlock
    {
        public CurrentPosition(int position)
        {
            Height = 30;
            Width = 30;
            Margin = new Thickness(15);
            Background = new SolidColorBrush(Colors.Transparent);
            TextAlignment = TextAlignment.Center;
            FontSize = 20;
            Foreground = new SolidColorBrush(Colors.Black);
            Text = position.ToString();
        }
    }
}
