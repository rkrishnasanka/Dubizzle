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
using System.IO.IsolatedStorage;
using System.Device.Location;
using Microsoft.Phone.Controls.Maps;
using System.Windows.Navigation;

namespace Dubizzle
{
    public partial class MainPage : PhoneApplicationPage
    {
        bool FirstTime = true;
        GeoCoordinateWatcher _watcher;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            _watcher = new GeoCoordinateWatcher();
            _watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(_watcher_PositionChanged);
            _watcher.Start();
            foreach (Item i in (App.Current as App).Uploaded_Items)
            {
                ListBox_MyDeals.Items.Add(i);
            }


        }


        void _watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (_watcher.Status == GeoPositionStatus.Ready)
            {
                //throw new NotImplementedException();
                Pushpin me = new Pushpin();
                me.Background = new SolidColorBrush(Colors.Orange);
                me.Content = new TextBlock { Text = "me!" };
                me.Location = e.Position.Location;
                Map_NearbyDeals.Children.Add(me);
                PopulateMap();
                Map_NearbyDeals.Center = e.Position.Location;
                Map_NearbyDeals.ZoomLevel = 16;
            }
            
        }

        private void PopulateMap()
        {
            //throw new NotImplementedException();
            Map_NearbyDeals.Children.Add(new Pushpin { Location = new GeoCoordinate { Latitude = 25.0899099110523, Longitude = 55.1501983661919 } });
            Map_NearbyDeals.Children.Add(new Pushpin { Location = new GeoCoordinate { Latitude = 25.0894046519822, Longitude = 55.1563352604179 } });
            Map_NearbyDeals.Children.Add(new Pushpin { Location = new GeoCoordinate { Latitude = 25.0928442590252, Longitude = 55.1539963741569 } });
            Map_NearbyDeals.Children.Add(new Pushpin { Location = new GeoCoordinate { Latitude = 25.0902208386739, Longitude = 55.15382471278   } });
            Map_NearbyDeals.Children.Add(new Pushpin { Location = new GeoCoordinate { Latitude = 25.0848377926733, Longitude = 55.1557129879265 } });
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            IDictionary<string, string> queryStrings = this.NavigationContext.QueryString;


            // Ensure that there is at least one key in the query string, and check whether the "token" key is present.

            if (FirstTime)
            {
                if (queryStrings.ContainsKey("token"))
                {
                    FirstTime = false;
                    NavigationService.Navigate(new Uri("/NewAd.xaml" + "?token=" + queryStrings["token"], UriKind.Relative));
                    //// Retrieve the picture from the media library using the token passed to the application.
                    //MediaLibrary library = new MediaLibrary();
                    //Picture picture = library.GetPictureFromToken(queryStrings["token"]);


                    //// Create a WriteableBitmap object and add it to the Image control Source property.
                    //BitmapImage bitmap = new BitmapImage();
                    //bitmap.CreateOptions = BitmapCreateOptions.None;
                    //bitmap.SetSource(picture.GetImage());

                    //WriteableBitmap picLibraryImage = new WriteableBitmap(bitmap);
                    //retrievePic.Source = picLibraryImage;
                }

                LoadingAnimation.Begin();
                FirstTime = !FirstTime;
            }
        }

        private void StackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //throw new NotImplementedException();
            NavigationService.Navigate(new Uri("/NewAd.xaml", UriKind.Relative));
        }

        private void HubTile_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
        	// TODO: Add event handler implementation here.
            NavigationService.Navigate(new Uri("/Browse.xaml?cat=classified", UriKind.Relative));
        }
    }
}