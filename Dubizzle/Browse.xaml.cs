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
using Newtonsoft.Json;
using HtmlAgilityPack;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Data;
using Microsoft.Phone.Shell;
namespace Dubizzle
{
    public partial class Browse : PhoneApplicationPage
    {
        ObservableCollection<ObservableCollection<MenuItem>> menuscollection;
        ObservableCollection<ResultMenuItem> resultsCollection; 

        string categoryuribase;
        string sectionuribase = "http://dubai.dubizzle.com/";
        string currentsectionuri;
        ProgressIndicator prog;
        string currentresultsuri;
        int currentpage;
        bool isdownloadingdata;
        public Browse()
        {
            InitializeComponent();
           // DubizzleURIBuilder dub = new DubizzleURIBuilder(HTTPProtocol.HTTP, Localization.UAE_Dubai, MainSection.Classified);
           // HttpWebRequest req = HttpWebRequest.Create(dub.Url) as HttpWebRequest;
            //Pivot_Browse.ItemsSource = menuscollection;
            resultsCollection = new ObservableCollection<ResultMenuItem>();
            currentpage = 1;
            isdownloadingdata = false;
            prog = new ProgressIndicator();
            prog.IsIndeterminate = true;
            prog.Text = "loading...";
            prog.IsVisible = true;

            SystemTray.SetProgressIndicator(this, prog);

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            var text = this.NavigationContext.QueryString["cat"];
            categoryuribase = "http://dubai.dubizzle.com/m/" + text;
            HttpWebRequest req = HttpWebRequest.Create(new Uri(categoryuribase)) as HttpWebRequest;
            req.BeginGetResponse(new AsyncCallback(reqResponseHandler), req);

            base.OnNavigatedTo(e);
        }

        private void ListBox_Level1_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
           // var foo = e.AddedItems[0] as MenuItem;
            var foo = ListBox_Level1.SelectedItem as MenuItem;
            if (foo != null)
            {
                currentsectionuri = sectionuribase + foo.Anchorlink;
                HttpWebRequest req = HttpWebRequest.Create(new Uri(currentsectionuri)) as HttpWebRequest;
                req.BeginGetResponse(new AsyncCallback(req2ResponseHandler), req);

                prog.IsVisible = true;
            }
        }

        private void ListBox_Level2_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // TODO: Add event handler implementation here.
            //var foo = e.AddedItems[0] as MenuItem;
            var foo = ListBox_Level2.SelectedItem as MenuItem;
            if (foo != null)
            {
                currentresultsuri = currentsectionuri + "/" + foo.Anchorlink;
                HttpWebRequest req = HttpWebRequest.Create(new Uri(currentresultsuri)) as HttpWebRequest;
                req.BeginGetResponse(new AsyncCallback(req3ResponseHandler), req);
                prog.IsVisible = true;
            }

        }

        private void ListBox_Level3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selecteditem = ListBox_Level3.SelectedItem as ResultMenuItem;
            if (selecteditem != null)
            {
                var a = (App)Application.Current;
                a.selectedItem = selecteditem;
                NavigationService.Navigate(new Uri("/ResultDetail.xaml?x=" + selecteditem.Anchorlink, UriKind.Relative));
            }
        }

        public void reqResponseHandler(IAsyncResult e)
        {
            //try
            //{
                HttpWebRequest req = e.AsyncState as HttpWebRequest;
                HttpWebResponse res ;
                string html ="";
                try
                {
                    res = req.EndGetResponse(e) as HttpWebResponse;
                    html = new StreamReader(res.GetResponseStream()).ReadToEnd();
                    if (res.StatusCode != HttpStatusCode.OK)
                    {
                        MessageBox.Show("Error - Could not connect to server");
                        return;
                    }
                }
                catch (WebException)
                {
                    MessageBox.Show("There was error connecting to the network. Please check your network connection and try again.","Network connection error",MessageBoxButton.OK);
                }
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                ObservableCollection<MenuItem> tempCollection = new ObservableCollection<MenuItem>();
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    ListBox_Level1.ItemsSource = tempCollection;
                });

                HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(".//ul[@class='categories-col sub-cats']//li//a");
                if (nodes != null)
                {
                    foreach (HtmlNode node in nodes)
                    {
                        var y = new MenuItem
                        {
                            Title = HttpUtility.HtmlDecode(node.InnerText),
                            Anchorlink = node.Attributes["href"].Value

                        };

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            tempCollection.Add(y);

                            //menuItems
                        });
                    }
                }
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    prog.IsVisible = false;
                });
            //}
            //catch (Exception)
            //{
            //    Deployment.Current.Dispatcher.BeginInvoke(() =>
            //        {
            //            MessageBox.Show("Please check your network connection and try again", "Error Connecting to server", MessageBoxButton.OK);
            //        });
            //}
                
        }

        public void req2ResponseHandler(IAsyncResult e)
        {
            HttpWebRequest req = e.AsyncState as HttpWebRequest;
            HttpWebResponse res = req.EndGetResponse(e) as HttpWebResponse;
            if (res.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show("Error - Could not connect to server");
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        prog.IsVisible = false;
                    });
                return;
            }
            HtmlDocument doc = new HtmlDocument();
            string html = new StreamReader(res.GetResponseStream()).ReadToEnd();
            doc.LoadHtml(html);
            ObservableCollection<MenuItem> tempCollection = new ObservableCollection<MenuItem>();
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ListBox_Level2.ItemsSource = tempCollection;
            });

            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(".//ul[@class='categories-col sub-cats']//li//a");
            if (nodes != null)
            {
                foreach (HtmlNode node in nodes)
                {
                    var y = new MenuItem
                    {
                        Title = HttpUtility.HtmlDecode(node.InnerText),
                        Anchorlink = node.Attributes["href"].Value

                    };

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        tempCollection.Add(y);

                        //menuItems
                    });
                }
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    PivotItem_Level2.Visibility = Visibility.Visible;
                    Pivot_Browse.SelectedItem = PivotItem_Level2;
                });
                
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    prog.IsVisible = false;
                });

            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    HttpWebRequest req2 = HttpWebRequest.Create(new Uri(currentsectionuri)) as HttpWebRequest;
                    req2.BeginGetResponse(new AsyncCallback(req3ResponseHandler), req2);
                    prog.IsVisible = true;
                });

            }

        }

        public void req3ResponseHandler(IAsyncResult e)
        {
            HttpWebRequest req = e.AsyncState as HttpWebRequest;
            HttpWebResponse res = req.EndGetResponse(e) as HttpWebResponse;
            if (res.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show("Error - Could not connect to server");
                return;
            }
            HtmlDocument doc = new HtmlDocument();
            string html = new StreamReader(res.GetResponseStream()).ReadToEnd();
            doc.LoadHtml(html);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ListBox_Level3.ItemsSource = resultsCollection;
            });
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(".//ul[@id='listing-table']//li//a");
            foreach (HtmlNode node in nodes)
            {
                    var y = new ResultMenuItem();
                    //{
                        //Title = HttpUtility.HtmlDecode(node.InnerText),
                        try
                        {
                           y.Anchorlink = node.Attributes["href"].Value;

                        }
                        catch (NullReferenceException) { }
                        try
                        {
                            y.Title = HttpUtility.HtmlDecode(node.SelectSingleNode(".//span[@class='title']").InnerText);
                        }
                        catch (Exception) { }
                        try
                        {
                            y.Price = HttpUtility.HtmlDecode(node.SelectSingleNode(".//span[@class='price']").InnerText);

                        }
                        catch (NullReferenceException) { }
                        try
                        {
                            y.Date = HttpUtility.HtmlDecode(node.SelectSingleNode(".//span[@class='date']").InnerText);

                        }
                        catch (NullReferenceException) { }
                        try
                        {
                            y.Imgurl = node.SelectSingleNode(".//img").Attributes["src"].Value;
                        }
                        catch (NullReferenceException) { }
                    //};
                //y.Title = HttpUtility.HtmlDecode(node.SelectSingleNode("//span[@class='title']").InnerText);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    resultsCollection.Add(y);
                });
            }
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                PivotItem_Level3.Visibility = Visibility.Visible;
                Pivot_Browse.SelectedItem = PivotItem_Level3;
            });
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                prog.IsVisible = false;
            });
            currentpage++;
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MessageBox.Show("Added to favorites");
        }

        private void ListBox_Level3_StretchingBottom(object sender, EventArgs e)
        {
            if (!isdownloadingdata)
            {
                isdownloadingdata = true;

                HttpWebRequest req = HttpWebRequest.Create(new Uri(currentresultsuri+"/?page="+(currentpage+1).ToString())) as HttpWebRequest;
                req.BeginGetResponse(new AsyncCallback(req3ResponseHandler), req);
                prog.IsVisible = true;
            }

        }

        private void ListBox_Level3_StretchingCompleted(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ListBox_Level3_SizeChanged(object sender, SizeChangedEventArgs e)
        {
           // throw new NotImplementedException();
        }







    }

    public class NullToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    } 


    public class MenuItem
    {
        public string Anchorlink { get; set; }
        public string Title { get; set; }
    }


    public class Menu
    {
        public ObservableCollection<MenuItem> menucollection { get; set; }
    }

    //public class 
}