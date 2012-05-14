using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace Dubizzle
{
    public partial class ChooserPage : PhoneApplicationPage
    {
        public ChooserPage()
        {
            InitializeComponent();
        }

        private void MultiLevelAnimatingListBox_DoneSelecting(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/NewAd.xaml", UriKind.Relative));
        }
    }
}