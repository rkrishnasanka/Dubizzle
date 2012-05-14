using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Device.Location;

namespace Dubizzle
{
    public class Item
    {
        public string Title { get; set; }
        public decimal Price { get; set; }
        public String Phone_Number { get; set; }
        public BitmapImage Image { get; set; }
        public GeoCoordinate Location { get; set; }
        public string Description { get; set; }
    }
}
