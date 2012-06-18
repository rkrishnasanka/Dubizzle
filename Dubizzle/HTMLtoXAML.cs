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
using System.Collections.Generic;
using Microsoft.Phone.Controls;

namespace Dubizzle
{
    public class HTMLtoXAML
    {
        public List<EDAdvancedTextbox> ConvertInputFields(HtmlAgilityPack.HtmlNodeCollection nodes)
        {
            if (nodes == null)
                return null;

            var ret = new List<EDAdvancedTextbox>();
            foreach (var item in nodes)
            {
                EDAdvancedTextbox foo = new EDAdvancedTextbox();
                foo.Name = item.Attributes["Name"].Value;
                try
                {
                    foo.DefaultText = item.Attributes["Title"].Value;
                }
                catch (NullReferenceException)
                {
                    foo.DefaultText = foo.Name;
                }
                foo.HorizontalAlignment = HorizontalAlignment.Stretch;
                foo.BorderBrush = new SolidColorBrush(Colors.Black);
                foo.BorderThickness = new Thickness(2.0);
                        
                ret.Add(foo);
            }
            return ret;
        }

        public List<ListPicker> ConvertSelectFields(HtmlAgilityPack.HtmlNodeCollection nodes)
        {
            if (nodes == null)
                return null;
            var ret = new List<ListPicker>();
            foreach (var item in nodes)
            {
                ListPicker foo = new ListPicker();
                foo.Name = item.Attributes["Name"].Value;
                foo.HorizontalAlignment = HorizontalAlignment.Stretch;
                var options = item.SelectNodes(".//option");
                var listboxitems = new List<KeyValuePair<string, string>>();
                foreach (var option in options)
                {
                    var bar = new KeyValuePair<string, string>(HttpUtility.HtmlDecode(option.NextSibling.OuterHtml), option.Attributes["value"].Value);
                    listboxitems.Add(bar);
                }
                foo.ItemsSource = listboxitems;
                foo.FullModeItemTemplate = (DataTemplate)Application.Current.Resources["SearchListBoxItemTemplate"];
                foo.ItemTemplate = (DataTemplate)Application.Current.Resources["SearchListBoxItemTemplateSelected"];
                foo.BorderThickness = new Thickness(2.0);
                foo.SelectedIndex = 0;
                ret.Add(foo);

            }
            return ret;
        }

        public List<StackPanel> ConvertSelectwithLabel(HtmlAgilityPack.HtmlNodeCollection nodes)
        {
            if (nodes == null)
                return null;
            List<StackPanel> ret = new List<StackPanel>();
            foreach (var divnode in nodes)
            {
                var sitem = divnode.SelectNodes(".//select");
                if (sitem == null)
                    continue;
                StackPanel stack = new StackPanel();
                stack.Children.Add(new TextBlock() { 
                    FontSize = 26, Foreground = new SolidColorBrush(Colors.White), 
                    Text = HttpUtility.HtmlDecode(divnode.SelectSingleNode(".//label[@class='heading']").InnerHtml) 
                });
                var listpickers = this.ConvertSelectFields(divnode.SelectNodes(".//select"));
                foreach (var listpicker in listpickers)
                {
                    stack.Children.Add(listpicker);
                }
                ret.Add(stack);
            }
            return ret;
        }
    }
}