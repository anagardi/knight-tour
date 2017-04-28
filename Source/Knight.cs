using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows;
using System;

namespace knights_tour
{
    public class Knight : Image
    {
        public Knight()
        {
            Source = new BitmapImage(new Uri("../Images/knight.png", UriKind.Relative));
            Width = 50;
            Height = 50;
            Margin = new Thickness(5);
        }
    }
}
