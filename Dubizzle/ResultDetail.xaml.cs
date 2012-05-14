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
using HtmlAgilityPack;
using System.IO;
using Microsoft.Phone.Shell;
using System.Collections.ObjectModel;
using Microsoft.Phone.Controls.Maps;

namespace Dubizzle
{
    public partial class ResultDetail : PhoneApplicationPage
    {
        string resultlocation;
        ProgressIndicator prog;
        string baseuri = "http://dubai.dubizzle.com";
        ResultDetails data;

        public ResultDetail()
        {
            InitializeComponent();

            prog = new ProgressIndicator();
            prog.IsIndeterminate = true;
            prog.Text = "loading...";
            prog.IsVisible = true;

            SystemTray.SetProgressIndicator(this, prog);
            data = new ResultDetails();

            this.DataContext = data;

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            
            base.OnNavigatedTo(e);
            resultlocation = this.NavigationContext.QueryString["x"];

            HttpWebRequest req = HttpWebRequest.Create(new Uri(baseuri + resultlocation)) as HttpWebRequest;
            req.BeginGetResponse(new AsyncCallback(reqResponseHandler), req);

        }

        public void reqResponseHandler(IAsyncResult e)
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

            ///Parsing the title
            try
            {
                var x = (doc.DocumentNode.SelectSingleNode(".//span[@class='details-title ']").InnerHtml);
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    data.Title = x;
                });
            }
            catch (NullReferenceException) { }
            //foo.Description = 
            ///Parsing the Location
            var temp = doc.DocumentNode.SelectSingleNode(".//a[@id='view-map']").Attributes["href"].Value;
            if (temp != "")
            {
                var temp2 = (temp.Substring(/*temp.IndexOf("?q=", 0, 1) + 3)*/26).Split(','));
                if (!(temp2[0] == "" || temp2[1] == ""))
                {
                    var y = double.Parse(temp2[0]);
                    var z = double.Parse(temp2[1]);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        //data.Latitude = y;
                        //data.Longitude = z;
                        data.CustLocation.Latitude = y;
                        data.CustLocation.Longitude = z;
                        p.Location = data.CustLocation;
                        Map_CustomerLocation.Center = data.CustLocation;

                        if ((y == 0.0 && z== 0.0))
                        {
                            p.Visibility = Visibility.Collapsed;
                            TextBlock_islocationspecified.Visibility = Visibility.Visible;
                        }
                    });

                }

            }
            ///Parsing Location Text
            temp =HttpUtility.HtmlDecode((doc.DocumentNode.SelectSingleNode(".//span[@id='location-text']")).InnerText);
            temp = temp.Replace("\n", "");
            temp = temp.Replace("   ", "  ");
            temp = temp.Replace("  ", " ");
            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    data.LocationText = temp;
                });
            ///Parsing the details table
            HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(".//div[@id='details-table']//ul//li");
            var w = new Dictionary<string, string>();

            foreach (HtmlNode node in nodes)
            {
                try
                {
                    w.Add(HttpUtility.HtmlDecode(node.SelectSingleNode(".//strong").InnerText), HttpUtility.HtmlDecode(node.SelectSingleNode(".//span").InnerText));
                }
                catch (NullReferenceException) { }
            }
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                data.Details = w;
            });
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                prog.IsVisible = false;
            });
        }

    }
}