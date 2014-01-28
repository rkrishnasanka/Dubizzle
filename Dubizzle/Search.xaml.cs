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
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.Shell;
using System.Collections;
using HtmlAgilityPack;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO.IsolatedStorage;
using System.Net.NetworkInformation;

namespace Dubizzle
{
    public partial class Search : PhoneApplicationPage
    {
        ObservableCollection<ResultMenuItem> resultsCollection;
        int currentpage;
        bool isdownloadingdata;
        string currentresultsuri;
        string category;
        ProgressIndicator prog;
        List<Dictionary<string,string>> newSearchCategories;
        List<List<UIElement>> newRefineUIElements;
        Dictionary<string, string> SearchCategories;
        Dictionary<string, string> SearchSubCategories;
        int Level;
        List<KeyValuePair<string, string>> FilterOptions;
        //Scroll state fields

        private ScrollBar sb = null;
        private ScrollViewer sv = null;
        private bool _isBouncy = false;
        private bool alreadyHookedScrollEvents = false;
        private bool alreadyHookedVisualStates = false;

        public Search()
        {
            InitializeComponent();
            resultsCollection = new ObservableCollection<ResultMenuItem>();
            currentpage = 0;
            isdownloadingdata = false;
            //SearchCategories = new Dictionary<string, string>() { { "Classifieds", "CL" }, { "Property for Sale", "SP" }, {  "Property for Rent","RP" }, {  "Jobs","JB" }, {  "Jobs Wanted","JW" }, {  "Community" ,"CO" } };
            newSearchCategories = new List<Dictionary<string, string>>();
            newSearchCategories.Add(new Dictionary<string, string>() { { "Classifieds", "CL" }, { "Property for Sale", "SP" }, { "Property for Rent", "RP" }, { "Jobs", "JB" }, { "Jobs Wanted", "JW" }, { "Community", "CO" } });
            newRefineUIElements = new List<List<UIElement>>();
            newRefineUIElements.Add(new List<UIElement>());
            SearchSubCategories = new Dictionary<string, string>();
            FilterOptions = new List<KeyValuePair<string,string>>();
            Level = 0;
            prog = new ProgressIndicator();
            prog.IsIndeterminate = true;
            prog.Text = "loading...";
            prog.IsVisible = false;
            SystemTray.SetProgressIndicator(this, prog);
            ListBox_Results.ItemsSource = resultsCollection;
            ListBox_FilterOptions.ItemsSource = newSearchCategories[Level];
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (alreadyHookedScrollEvents)
                return;

            alreadyHookedScrollEvents = true;
            ListBox_Results.AddHandler(ListBox.ManipulationCompletedEvent, (EventHandler<ManipulationCompletedEventArgs>)LB_ManipulationCompleted, true);
            sb = (ScrollBar)FindElementRecursive(ListBox_Results, typeof(ScrollBar));
            sv = (ScrollViewer)FindElementRecursive(ListBox_Results, typeof(ScrollViewer));

            if (sv != null)
            {
                // Visual States are always on the first child of the control template 
                FrameworkElement element = VisualTreeHelper.GetChild(sv, 0) as FrameworkElement;
                if (element != null)
                {

                    //VisualStateGroup group = FindVisualState(element, "ScrollStates");
                    //if (group != null)
                    //{
                    //    group.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(group_CurrentStateChanging);
                    //}
                    VisualStateGroup vgroup = FindVisualState(element, "VerticalCompression");
                    //Horizontal change listener
                    //VisualStateGroup hgroup = FindVisualState(element, "HorizontalCompression");
                    if (vgroup != null)
                    {
                        vgroup.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(vgroup_CurrentStateChanging);
                        alreadyHookedVisualStates = true;
                    }
                    //Horizontal Group state Events
                    //if (hgroup != null)
                    //{
                    //    hgroup.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(hgroup_CurrentStateChanging);
                    //}
                }
            }


        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            //var text = this.NavigationContext.QueryString["cat"];
            //categoryuribase = "http://dubai.dubizzle.com/m/" + text;
            //HttpWebRequest req = HttpWebRequest.Create(new Uri(categoryuribase)) as HttpWebRequest;
            //req.BeginGetResponse(new AsyncCallback(reqResponseHandler), req);

            base.OnNavigatedTo(e);
        }

        private void Button_Search_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                // TODO: Add event handler implementation here.
                if (Level <= 1)
                {
                    MessageBox.Show("Search requires for you to select a category and a subcategory");
                    return;
                }
                currentpage = 1;
                string filterkeyvals = "";
                foreach (var foo in FilterOptions)
                {
                    filterkeyvals += "&" + foo.Key + "=" + foo.Value;
                }
                string morefilteroptions = filteroptionsfromPanel(StackPanel_MoreOptions);

                currentresultsuri = string.Format("http://dubai.dubizzle.com/m/search/js/?is_basic_search_widget=0&s={1}&site=2&keywords={0}&is_search=1{2}&{3}", EDAdvancedTextbox_SearhQuery.Text, category, filterkeyvals, morefilteroptions);
                HttpWebRequest req2 = HttpWebRequest.Create(new Uri(currentresultsuri)) as HttpWebRequest;
                req2.BeginGetResponse(new AsyncCallback(req3ResponseHandler), req2);
                prog.IsVisible = true;
            }
            else
            {
                MessageBox.Show("Could not connect to the server , please check your internet connection and try again");
            }
			
        }
        private string filteroptionsfromPanel(UIElement control)
        {
            string ret = "";

            foreach (var subcontrol in (control as StackPanel).Children)
            {
                if (subcontrol is StackPanel)
                    ret += filteroptionsfromPanel(subcontrol);
                else if (subcontrol is ListPicker)
                {
                    var lp = subcontrol as ListPicker;
                    if (lp == null)
                        continue;
                    if (lp.SelectedItem != null)
                    {
                        ret += lp.Name + "=" + ((KeyValuePair<string, string>)lp.SelectedItem).Value + "&";
                    }
                }
                else if (subcontrol is EDAdvancedTextbox)
                {
                    ret += ((EDAdvancedTextbox)subcontrol).Name + "=" + ((EDAdvancedTextbox)subcontrol).Text + "&";
                }

            }
            return ret;
        }

        public void req3ResponseHandler(IAsyncResult e)
        {
            try
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
                    ListBox_Results.ItemsSource = resultsCollection;
                });
                var testnode = doc.DocumentNode.SelectSingleNode(".//div[@id='no-listings']");
                if (testnode != null)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("No results found");
                        EDAdvancedTextbox_SearhQuery.Text = "";
                        prog.IsVisible = false;
                    });
                    return;
                }
                HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(".//ul[@id='listing-table']//li//a");
                foreach (HtmlNode node in nodes)
                {
                    var y = new ResultMenuItem();
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
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if(!resultsCollection.Contains(y))
                            resultsCollection.Add(y);
                    });
                }
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    prog.IsVisible = false;
                });
                currentpage++;
                isdownloadingdata = false;

            }catch(Exception)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("Please check your network connection and try again later", "Network Error",MessageBoxButton.OK);
                    });
            }
        }


        #region strechinglistbox
        private void LB_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (_isBouncy)
            {
                ReleaseBounce();
            }

            if (!alreadyHookedVisualStates)
            {
                sb = (ScrollBar)FindElementRecursive(ListBox_Results, typeof(ScrollBar));
                sv = (ScrollViewer)FindElementRecursive(ListBox_Results, typeof(ScrollViewer));

                if (sv != null)
                {
                    // Visual States are always on the first child of the control template 
                    FrameworkElement element = VisualTreeHelper.GetChild(sv, 0) as FrameworkElement;
                    if (element != null)
                    {

                        //VisualStateGroup group = FindVisualState(element, "ScrollStates");
                        //if (group != null)
                        //{
                        //    group.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(group_CurrentStateChanging);
                        //}
                        VisualStateGroup vgroup = FindVisualState(element, "VerticalCompression");
                        //Horizontal change listener
                        //VisualStateGroup hgroup = FindVisualState(element, "HorizontalCompression");
                        if (vgroup != null)
                        {
                            vgroup.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(vgroup_CurrentStateChanging);
                            alreadyHookedVisualStates = true;
                        }
                        //Horizontal Group state Events
                        //if (hgroup != null)
                        //{
                        //    hgroup.CurrentStateChanging += new EventHandler<VisualStateChangedEventArgs>(hgroup_CurrentStateChanging);
                        //}
                    }
                }
            }

        }

        private UIElement FindElementRecursive(FrameworkElement parent, Type targetType)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            UIElement returnElement = null;
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    Object element = VisualTreeHelper.GetChild(parent, i);
                    if (element.GetType() == targetType)
                    {
                        return element as UIElement;
                    }
                    else
                    {
                        returnElement = FindElementRecursive(VisualTreeHelper.GetChild(parent, i) as FrameworkElement, targetType);
                    }
                }
            }
            return returnElement;
        }

        private VisualStateGroup FindVisualState(FrameworkElement element, string name)
        {
            if (element == null)
                return null;

            IList groups = VisualStateManager.GetVisualStateGroups(element);
            foreach (VisualStateGroup group in groups)
                if (group.Name == name)
                    return group;

            return null;
        }

        private void ReleaseBounce()
        {
            _isBouncy = false;
        }

        private void ProcessBounce()
        {
            _isBouncy = true;
        }


        private void vgroup_CurrentStateChanging(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == "CompressionTop")
            {
                ProcessBounce();
                //VCompressionTxb.Foreground = new SolidColorBrush(Colors.White);
                //TopTxb.Foreground = new SolidColorBrush(Colors.Green);
            }

            if (e.NewState.Name == "CompressionBottom")
            {
                ProcessBounce();
                //VCompressionTxb.Foreground = new SolidColorBrush(Colors.White);
                //BottomTxb.Foreground = new SolidColorBrush(Colors.Green);

                /*
                 * As an example, this is where you can add code to load new items, have the progress bar animation and so on
                 *                  
                 */
                if (!isdownloadingdata)
                {
                    isdownloadingdata = true;
                    HttpWebRequest req = HttpWebRequest.Create(new Uri(currentresultsuri + "&?page=" + currentpage)) as HttpWebRequest;
                    req.BeginGetResponse(new AsyncCallback(req3ResponseHandler), req);
                    prog.IsVisible = true;

                }

            }
            if (e.NewState.Name == "NoVerticalCompression")
            {
                ReleaseBounce();
                //TopTxb.Foreground = new SolidColorBrush(Colors.White);
                //BottomTxb.Foreground = new SolidColorBrush(Colors.White);
                //VCompressionTxb.Foreground = new SolidColorBrush(Colors.Green);
            }
        }

        #endregion 

        private void Grid_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var selitem = (KeyValuePair<string, string>) ListBox_FilterOptions.SelectedItem;
            if (selitem.Value != "--")
            {
                if (Level == 0)
                    category = selitem.Value;
                else if (Level == 1)
                {
                    // Show the more options grid
                    Grid_MoreOptions.Visibility = Visibility.Visible;

                    KeyValuePair<string, string> temp = new KeyValuePair<string, string>("rc", selitem.Value);
                    for (int i = 0; i < FilterOptions.Count; i++)
                    {
                        var foo = FilterOptions[i];
                        if (foo.Key == temp.Key)
                        {
                            FilterOptions.Remove(foo);
                            break;
                        }
                    }
                    FilterOptions.Add(temp);
                }
                else
                {
                    KeyValuePair<string, string> temp = new KeyValuePair<string, string>("c" + (Level - 1).ToString(), selitem.Value);
                    for (int i = 0; i < FilterOptions.Count; i++)
                    {
                        var foo = FilterOptions[i];
                        if (foo.Key == temp.Key)
                        {
                            FilterOptions.Remove(foo);
                            break;
                        }
                    }
                    FilterOptions.Add(temp);
                }
                var subcaturl = string.Format("http://dubai.dubizzle.com/classified/get_category_models/{0}/?site=2&s={1}&for_mobile=1", selitem.Value,category);
                HttpWebRequest req2 = HttpWebRequest.Create(new Uri(subcaturl)) as HttpWebRequest;
                req2.BeginGetResponse(new AsyncCallback(req_filteroptionsResponseHandler), req2);
                prog.IsVisible = true;
            }
            else
            {
                Level--;
                ToggleButton_MoreOptions.IsChecked = false;
                ListBox_FilterOptions.ItemsSource = newSearchCategories[Level];
                FillnewMoreOptionsControls();

            }

            if (Level > 0)
            {
                // hide the more options grid
                Grid_MoreOptions.Visibility = Visibility.Visible;

            }
            else
            {
                // Show the more options grid
                Grid_MoreOptions.Visibility = Visibility.Collapsed;

            }

        }

        public void req_filteroptionsResponseHandler(IAsyncResult e)
        {
            try
            {
                HttpWebRequest req = e.AsyncState as HttpWebRequest;
                HttpWebResponse res = req.EndGetResponse(e) as HttpWebResponse;
                if (res.StatusCode != HttpStatusCode.OK)
                {
                    MessageBox.Show("Error - Could not connect to server");
                    return;
                }
                string json = new StreamReader(res.GetResponseStream()).ReadToEnd();
                JObject bar = JObject.Parse("{'root':" + json + "}");
                Dictionary<string, string> tempdict = new Dictionary<string, string>();
                foreach (var foo in bar["root"]["models"])
                {
                    var key = foo[1].Value<string>();
                    var val = foo[0].Value<string>();
                    if (val == "--")
                        key = "Go back";
                    tempdict.Add(key,val);
                }

                Level++;
                if (newSearchCategories.Count > Level)
                    newSearchCategories[Level] = tempdict;
                else
                    newSearchCategories.Add(tempdict);

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    ListBox_FilterOptions.ItemsSource = newSearchCategories[Level];
                });

                string html = bar["root"]["form_html"].Value<string>();

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);

                var fields = doc.DocumentNode.SelectSingleNode(".//div[@class='fields']");
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    //StackPanel_MoreOptions.Children.Clear();
                    //var temp = new HTMLtoXAML().ConvertInputFields(fields.SelectNodes(".//input[@type='text']"));
                    //if (temp != null)
                    //{

                    //        foreach (var item in temp)
                    //        {
                    //            if (!StackPanel_MoreOptions.Children.Contains(item))
                    //            {
                    //                StackPanel_MoreOptions.Children.Add(item);
                    //            }
                    //        }
                    //}

                    //Deployment.Current.Dispatcher.BeginInvoke(() =>
                    //{
                    //    StackPanel_MoreOptions.Children.Clear();
                    //    var temp2 = new HTMLtoXAML().ConvertSelectFields(fields.SelectNodes(".//select"));
                    //    if (temp2 != null)
                    //    {
                    //            foreach (var item2 in temp2)
                    //            {
                    //                if(!StackPanel_MoreOptions.Children.Contains(item2))
                    //                {
                    //                    StackPanel_MoreOptions.Children.Add(item2);
                    //                }

                    //            }
                    //    }
                    //});

                    StackPanel_MoreOptions.Children.Clear();
                    var inputelements = fields.SelectNodes(".//input[@type='text']");
                    var selectelements = fields.SelectNodes(".//div[@class='range-fields']");
                    List<EDAdvancedTextbox> Textboxes;
                    List<StackPanel> Listpickers;
                    List<UIElement> tempUIList = new List<UIElement>();
                    if (inputelements != null)
                    {
                        Textboxes = new HTMLtoXAML().ConvertInputFields(inputelements);
                        foreach (var item in Textboxes)
                        {
                            //StackPanel_MoreOptions.Children.Add(item);
                            item.KeyUp += (sender, xe) =>
                            {
                                if (xe.Key == Key.Enter)
                                    this.Focus();
                            };
                            tempUIList.Add(item);
                        }
                    }

                    if (selectelements != null)
                    {
                        Listpickers = new HTMLtoXAML().ConvertSelectwithLabel(selectelements);
                        foreach (var item2 in Listpickers)
                        {
                            //StackPanel_MoreOptions.Children.Add(item2);
                            tempUIList.Add(item2);
                        }
                    }

                    if (newRefineUIElements.Count > Level)
                        newRefineUIElements[Level] = tempUIList;
                    else
                        newRefineUIElements.Add(tempUIList);


                });

                FillnewMoreOptionsControls();

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    prog.IsVisible = false;
                });

            }
            catch (WebException)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Please check your network connection and try again later", "Network Error", MessageBoxButton.OK);
                });
            }
        }

        private void FillnewMoreOptionsControls()
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                StackPanel_MoreOptions.Children.Clear();
                if (newRefineUIElements[Level] == null)
                    return;
                foreach (var control in newRefineUIElements[Level])
                {
                    StackPanel_MoreOptions.Children.Add(control);
                }
            });
        }

        private void ApplicationBarIconButton_search_Click(object sender, System.EventArgs e)
        {
            if (Grid_SearchBox.Visibility == Visibility.Collapsed)
            {
                Grid_SearchBox.Visibility = Visibility.Visible;
            }
            else
            {
                Grid_SearchBox.Visibility = Visibility.Collapsed;
            }
        }

        private void Grid_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var selecteditem = ListBox_Results.SelectedItem as ResultMenuItem;
            if (selecteditem != null)
            {
                var a = (App)Application.Current;
                a.selectedItem = selecteditem;
                NavigationService.Navigate(new Uri("/ResultDetail.xaml?x=" + selecteditem.Anchorlink, UriKind.Relative));
            }
        }


        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ObservableCollection<ResultMenuItem> x;
            if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue("favorites", out x))
            {
                x = new ObservableCollection<ResultMenuItem>();
                IsolatedStorageSettings.ApplicationSettings["favorites"] = x;
            }

            x.Add(ListBox_Results.SelectedItem as ResultMenuItem);
            IsolatedStorageSettings.ApplicationSettings.Save();
            MessageBox.Show("Added to favorites");

        }

    }

}