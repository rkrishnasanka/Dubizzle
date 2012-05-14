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
using Microsoft.Phone.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Phone;
using Microsoft.Phone.Shell;
using System.Device.Location;
using Microsoft.Phone.Controls.Maps;
using System.Windows.Navigation;
using Microsoft.Xna.Framework.Media;

namespace Dubizzle
{
    public partial class NewAd : PhoneApplicationPage
    {
        CameraCaptureTask cam;
        PhotoChooserTask phot=null;
        GeoCoordinateWatcher _watcher;
        Pushpin me;
        ProgressIndicator P;
        public NewAd()
        {
            _watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);

            InitializeComponent();
            //NavigationService.RemoveBackEntry();
            me = new Pushpin();
            me.Visibility = Visibility.Collapsed;
            Map_SaleLocation.Children.Add(me);
            P = new ProgressIndicator();
            SystemTray.ProgressIndicator = P;
            _watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(_watcher_PositionChanged);
            _watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(_watcher_StatusChanged);
            _watcher.Start();

			cam = new CameraCaptureTask();
            cam.Completed += new EventHandler<PhotoResult>(cam_Completed);
            phot = new PhotoChooserTask();
            phot.Completed += new EventHandler<PhotoResult>(phot_Completed);
            //ChooserPage.DoneSelecting +=
            CategoryChooser.DoneSelectingListBox4 += new EventHandler(CategoryChooser_DoneSelecting);
            //this.ApplicationBar.Mode = ApplicationBarMode.Minimized;
            this.ApplicationBar.IsVisible = false;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Get a dictionary of query string keys and values.
            IDictionary<string, string> queryStrings = this.NavigationContext.QueryString;


            // Ensure that there is at least one key in the query string, and check whether the "token" key is present.
            if (queryStrings.ContainsKey("token"))
            {

                // Retrieve the picture from the media library using the token passed to the application.
                MediaLibrary library = new MediaLibrary();
                Picture picture = library.GetPictureFromToken(queryStrings["token"]);


                // Create a WriteableBitmap object and add it to the Image control Source property.
                BitmapImage bitmap = new BitmapImage();
                bitmap.CreateOptions = BitmapCreateOptions.None;
                bitmap.SetSource(picture.GetImage());

               // WriteableBitmap picLibraryImage = new WriteableBitmap(bitmap);
                Image_ToBeUploaded.Source = bitmap;
            }

        }

        void CategoryChooser_DoneSelecting(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
            CategoryChooser.Visibility = Visibility.Collapsed;
            this.ApplicationBar.IsVisible = true;
            this.ApplicationBar.Mode = ApplicationBarMode.Default;
        }

        void _watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            if (e.Status == GeoPositionStatus.Ready)
            {
               P.IsIndeterminate = false;

            }
            else
            {
                P.IsIndeterminate = true;
            }
        }

        void _watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            //throw new NotImplementedException();
            if (_watcher.Status == GeoPositionStatus.Ready)
            {
                me.Location = _watcher.Position.Location;
                me.Visibility = Visibility.Visible;
                Map_SaleLocation.Center = _watcher.Position.Location;
                Map_SaleLocation.ZoomLevel = 12;
            }
            
        }

        private void Button_LaunchCameraTask_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
			cam.Show();
        }

        private void Button_LaunchPhotosTask_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
            phot.Show();
        }
		
		void phot_Completed(object sender, PhotoResult e)
        {
            //throw new NotImplementedException();
             if (e.TaskResult == TaskResult.OK)
             {
                BinaryReader reader = new BinaryReader(e.ChosenPhoto);
                Image_ToBeUploaded.Source = new BitmapImage(new Uri(e.OriginalFileName));
            }
        }
		
		void cam_Completed(object sender, PhotoResult e)
        {
            //throw new NotImplementedException();
            byte[] imgLocal;
            if (e.ChosenPhoto != null)
            {
                imgLocal= new  byte[(int)e.ChosenPhoto.Length];
                e.ChosenPhoto.Read(imgLocal, 0, imgLocal.Length);
                e.ChosenPhoto.Seek(0, System.IO.SeekOrigin.Begin);
                var bitmapImage = PictureDecoder.DecodeJpeg(e.ChosenPhoto);
                Image_ToBeUploaded.Source = bitmapImage;
            }
        }

		private void ApplicationBarIconButton_Click(object sender, System.EventArgs e)
		{
			// TODO: Add event handler implementation here.
            ApplicationBarIconButton pressedButton = sender as ApplicationBarIconButton;
            switch (pressedButton.Text)
            {
                case "Upload": 
                    {
                           string Title = EDATextBox_Title.Text;
                           
                        decimal Price =  decimal.TryParse(EDATextBox_Price.Text ,out Price) ? decimal.Parse(EDATextBox_Price.Text):500;
                           string Phone_Number = EDATextBox_PhoneNumber.Text; 
                           string Description = EDATextBox_Description.Text;
                           // Image img = Image_ToBeUploaded.
                           Item newItem = new Item { Title=Title,Price=Price, Phone_Number=Phone_Number , Description= Description , Location = me.Location };

                        
                        (App.Current as App).Uploaded_Items.Add(newItem);
                        NavigationService.GoBack();
                        break;                
                    }

                case "Cancel":
                    {
                        NavigationService.GoBack();
                        break; 
                    }
            }
		}

		private void ApplicationBarMenuItem_Click(object sender, System.EventArgs e)
		{
			// TODO: Add event handler implementation here.
            ApplicationBarMenuItem pressedItem = sender as ApplicationBarMenuItem;
            switch (pressedItem.Text)
            {
                case "Change Category":
                    {
                        CategoryChooser.Visibility = Visibility.Visible;
                        this.ApplicationBar.IsVisible = false;
                        break;
                    }
            }
		}

		private void Map_Tap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			// TODO: Add event handler implementation here.
		}

		private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
		{
			// TODO: Add event handler implementation here.
            _watcher.Start();
            _watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(_watcher_PositionChanged);
			
		}


    }
}