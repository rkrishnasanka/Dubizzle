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
using System.Windows.Controls.Primitives;
using System.Collections;
using System.IO.IsolatedStorage;
using System.ComponentModel;
using System.Diagnostics;

namespace Dubizzle
{
    public partial class Browse : PhoneApplicationPage
    {
        ObservableCollection<ResultMenuItem> resultsCollection;
        ObservableCollection<MenuItem> subsectionCollection;

        Dictionary<string, string> FilterOptions;

        ObservableCollection<ResultMenuItem> Favorites;
        string refineoptionsstring;
        HtmlNode FilterOptionHtmlNodes;
        bool flag_filtercontrolsloaded;
        string categoryuribase;
        string sectionuribase = "http://dubai.dubizzle.com/";
        string currentsectionuri;
        ProgressIndicator prog;
        string currentresultsuri;
        int currentpage;
        bool isdownloadingdata;
        //Scroll state fields
        private ScrollBar sb = null;
        private ScrollViewer sv = null;
        private bool _isBouncy = false;
        private bool alreadyHookedScrollEvents = false;
        private bool alreadyHookedVisualStates = false;
        //
        /// <summary>
        /// Constructor
        /// </summary>

        public Browse()
        {
            InitializeComponent();
           // DubizzleURIBuilder dub = new DubizzleURIBuilder(HTTPProtocol.HTTP, Localization.UAE_Dubai, MainSection.Classified);
           // HttpWebRequest req = HttpWebRequest.Create(dub.Url) as HttpWebRequest;
            //Pivot_Browse.ItemsSource = menuscollection;
            resultsCollection = new ObservableCollection<ResultMenuItem>();
            subsectionCollection = new ObservableCollection<MenuItem>();
            FilterOptions = new Dictionary<string, string>();
            Favorites = (Application.Current as App).Favorites;
            currentpage = 1;
            isdownloadingdata = false;
            prog = new ProgressIndicator();
            prog.IsIndeterminate = true;
            prog.Text = "loading...";
            prog.IsVisible = true;

            SystemTray.SetProgressIndicator(this, prog);

            Pivot_Browse.Items.Remove(PivotItem_Level2);
            Pivot_Browse.Items.Remove(PivotItem_Level3);
            ApplicationBar.IsVisible = false;

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
                subsectionCollection.Clear();
                resultsCollection.Clear();
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
                resultsCollection.Clear();
                currentresultsuri = currentsectionuri + "/" + foo.Anchorlink;
                HttpWebRequest req = HttpWebRequest.Create(new Uri(currentresultsuri)) as HttpWebRequest;
                req.BeginGetResponse(new AsyncCallback(req3ResponseHandler), req);
                isdownloadingdata = true;
                prog.IsVisible = true;
            }

        }

        private void ListBox_Level3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
                catch (WebException ex)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        MessageBox.Show("There was error connecting to the network. Please check your network connection and try again.", "Network connection error", MessageBoxButton.OK);
                        prog.IsVisible = false;

                    });
                    Debug.WriteLine(ex.Message);
                    return;
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
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                ListBox_Level2.ItemsSource = subsectionCollection;
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
                        subsectionCollection.Add(y);

                        //menuItems
                    });
                }
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    PivotItem_Level2.Visibility = Visibility.Visible;
                    if (!Pivot_Browse.Items.Contains(PivotItem_Level2))
                        Pivot_Browse.Items.Add(PivotItem_Level2);
                    if (Pivot_Browse.Items.Contains(PivotItem_Level3))
                    {
                        Pivot_Browse.Items.Remove(PivotItem_Level3);
                        ApplicationBar.IsVisible = false;
                    }
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
                    currentresultsuri = currentsectionuri;
                    HttpWebRequest req2 = HttpWebRequest.Create(new Uri(currentresultsuri)) as HttpWebRequest;
                    req2.BeginGetResponse(new AsyncCallback(req3ResponseHandler), req2);
                    prog.IsVisible = true;
                });

            }

        }

        private void refinesearch(string filteroptions)
        {
            resultsCollection.Clear();
            refineoptionsstring = "&" + filteroptions.Substring(2);
            HttpWebRequest req2 = HttpWebRequest.Create(new Uri(currentresultsuri+filteroptions)) as HttpWebRequest;
            req2.BeginGetResponse(new AsyncCallback(req3ResponseHandler), req2);
            prog.IsVisible = true;
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
                    ListBox_Level3.ItemsSource = resultsCollection;
                });
                FilterOptionHtmlNodes = doc.DocumentNode.SelectSingleNode(".//div[@class='fields']");

                //FilterOptionHtmlNodes = doc.DocumentNode.SelectSingleNode(".//div[@id='header-search-options']");
                HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes(".//ul[@id='listing-table']//li//a");
                if (nodes == null)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            MessageBox.Show("No reults were found , try again !");
                            StackPanel_RefiningOptions.Children.Clear();
                        });

                    return;
                }
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
                        StackPanel_RefiningOptions.Children.Clear();
                        var inputelements = FilterOptionHtmlNodes.SelectNodes(".//input[@type='text']");
                        var selectelements = FilterOptionHtmlNodes.SelectNodes(".//div[@class='range-fields']");
                        List<EDAdvancedTextbox> Textboxes;
                        List<StackPanel> Listpickers;
                        if (inputelements != null)
                        {
                            Textboxes = new HTMLtoXAML().ConvertInputFields(inputelements);
                            foreach (var item in Textboxes)
                            {
                                item.KeyUp += (sender, xe) =>
                                {
                                    if (xe.Key == Key.Enter)
                                        this.Focus();
                                };

                                StackPanel_RefiningOptions.Children.Add(item);
                            }
                        }

                        if (selectelements != null)
                        {
                            Listpickers = new HTMLtoXAML().ConvertSelectwithLabel(selectelements);
                            foreach (var item2 in Listpickers)
                            {
                                StackPanel_RefiningOptions.Children.Add(item2);
                            }
                        }


                        Button refine_button = new Button();
                        refine_button.BorderThickness = new Thickness(2.0);
                        refine_button.Content = "submit";
                        refine_button.Click +=new RoutedEventHandler((o,ex)=>
                        {
                            string filteroptions = "/?";
                            filteroptions += filteroptionsfromPanel(StackPanel_RefiningOptions);

                            refinesearch(filteroptions);
                        });
                        StackPanel_RefiningOptions.Children.Add(refine_button);

                    });

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    PivotItem_Level3.Visibility = Visibility.Visible;
                    if (!Pivot_Browse.Items.Contains(PivotItem_Level3))
                    {
                        Pivot_Browse.Items.Add(PivotItem_Level3);
                        ApplicationBar.IsVisible = true;
                    }
                    Pivot_Browse.SelectedItem = PivotItem_Level3;
                });
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    Grid_Refine.Visibility = Visibility.Collapsed;
                    prog.IsVisible = false;
                });
                currentpage++;
                isdownloadingdata = false;
            }
            catch (WebException)
            {
                isdownloadingdata = false;
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    prog.IsVisible = false;

                });

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

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            ObservableCollection<ResultMenuItem> x;
            if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue("favorites", out x))
            {
                x = new ObservableCollection<ResultMenuItem>();
                IsolatedStorageSettings.ApplicationSettings["favorites"] = x;
            }

            x.Add(ListBox_Level3.SelectedItem as ResultMenuItem);
            IsolatedStorageSettings.ApplicationSettings.Save();
            MessageBox.Show("Added to favorites");
        }


        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (alreadyHookedScrollEvents)
                return;

            alreadyHookedScrollEvents = true;
            ListBox_Level3.AddHandler(ListBox.ManipulationCompletedEvent, (EventHandler<ManipulationCompletedEventArgs>)LB_ManipulationCompleted, true);
            sb = (ScrollBar)FindElementRecursive(ListBox_Level3, typeof(ScrollBar));
            sv = (ScrollViewer)FindElementRecursive(ListBox_Level3, typeof(ScrollViewer));

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

        #region strechinglistbox
        private void LB_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (_isBouncy)
            {
                ReleaseBounce();
            }

            if (!alreadyHookedVisualStates)
            {
                sb = (ScrollBar)FindElementRecursive(ListBox_Level3, typeof(ScrollBar));
                sv = (ScrollViewer)FindElementRecursive(ListBox_Level3, typeof(ScrollViewer));

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
                    HttpWebRequest req = HttpWebRequest.Create(new Uri(currentresultsuri+"/?page="+currentpage+refineoptionsstring)) as HttpWebRequest;
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
            var selecteditem = ListBox_Level3.SelectedItem as ResultMenuItem;
            if (selecteditem != null)
            {
                var a = (App)Application.Current;
                a.selectedItem = selecteditem;
                NavigationService.Navigate(new Uri("/ResultDetail.xaml?x=" + selecteditem.Anchorlink, UriKind.Relative));
            }

        }

        private void ApplicationBarIconButton_search_Click(object sender, EventArgs e)
        {
            if (Grid_Refine.Visibility == Visibility.Collapsed)
            {
                Grid_Refine.Visibility = Visibility.Visible;
                //prog.IsVisible = true;
                //// Grid_RefiningOptions = new HTMLtoXAML().ConvertHTMLtoXAMLGrid(FilterOptionHtmlNodes);

                //var inputelements = FilterOptionHtmlNodes.SelectNodes(".//input[@type='text']");
                ////  var selectelements = FilterOptionHtmlNodes.SelectNodes(".//select");
                //List<EDAdvancedTextbox> Textboxes;
                //// List<ListPicker> Listpickers;
                //if (inputelements != null)
                //{
                //    Textboxes = new HTMLtoXAML().ConvertInputFields(inputelements);
                //    foreach (var item in Textboxes)
                //    {
                //        StackPanel_RefiningOptions.Children.Add(item);
                //    }
                //}

                //prog.IsVisible = false;

            }
            else
            {
                Grid_Refine.Visibility = Visibility.Collapsed;

            }
        }


    }





}