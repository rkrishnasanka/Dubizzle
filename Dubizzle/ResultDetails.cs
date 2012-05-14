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
using System.ComponentModel;
using System.Collections.ObjectModel;
using Microsoft.Phone.Controls.Maps.Platform;

namespace Dubizzle
{
    public class ResultMenuItem : MenuItem
    {
        public string Imgurl { get; set; }
        public string Price { get; set; }
        public string Date { get; set; }
    }

    public class ResultDetails : INotifyPropertyChanged , INotifyPropertyChanging
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ResultDetails()
        {
            this._custLocation = new Location();
        }
        private string _locationText;

        public string LocationText
        {
            get { return _locationText; }
            set 
            {
                NotifyPropertyChanging("LocationText");
                _locationText = value;
                NotifyPropertyChanged("LocationText");
            }
        }

        public string Price
        {
            get
            {
                if (this.Details != null)
                {
                    if (this.Details.ContainsKey("Price:"))
                    {
                        return this.Details["Price:"];
                    }
                }
                return null;
            }
        }
        private string _link;
        public string Link 
        {
            get
            {
               return _title;
            }
            
            set 
            {
                NotifyPropertyChanging("Link");
                _link = value;
                NotifyPropertyChanged("Link");
            } 
        }

        private string _title;
        public string Title 
        {
            get
            {
                return _title;
            }
            set 
            {
                NotifyPropertyChanging("Title");
                _title = value;
                NotifyPropertyChanged("Title");
            }
        }

        private Dictionary<string, string> _details; 
        public Dictionary<string, string> Details 
        {
            get
            {
                return _details;
            }
            set
            {
                NotifyPropertyChanging("Details");
                _details = value;
                NotifyPropertyChanged("Details");
            }

        }

        public ObservableCollection<string> ImageUrls { get; set; }
        //private List<string> _imageUrls;
        //public List<string> ImageUrls {
        //    get
        //    {
        //        return _imageUrls;
        //    }
        //    set
        //    {
        //        NotifyPropertyChanging("ImageUrls");
        //        _imageUrls = value;
        //        NotifyPropertyChanged("ImageUrls");
        //    }
        //}

        private string _actionEmail;
        public string ActionEmail
        {
            get
            {
                return _actionEmail;
            }
            set
            {
                NotifyPropertyChanging("ActionEmail");
                _actionEmail = value;
                NotifyPropertyChanged("ActionEmail");
            }
        }

        private string _actionPhoneno;
        public string ActionPhoneno
        {
            get
            {
                return _actionPhoneno;
            }
            set
            {
                NotifyPropertyChanging("ActionPhoneno");
                _actionPhoneno = value;
                NotifyPropertyChanged("ActionPhoneno");
            }
        }

        private string _description;
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                NotifyPropertyChanging("Description");
                _description = value;
                NotifyPropertyChanged("Description");
            }
        }

        private double _latitude;
        public double Latitude
        {
            get
            {
                return _latitude;
            }
            set
            {
                NotifyPropertyChanging("Latitude");
                _latitude = value;
                NotifyPropertyChanged("Latitude");
            }
        }

        private double _longitude;
        public double Longitude
        {
            get
            {
                return _longitude;
            }
            set
            {
                NotifyPropertyChanging("Longitude");
                _longitude = value;
                NotifyPropertyChanged("Longitude");
            }
        }

        private Location _custLocation;

        public Location CustLocation
        {
            get { return _custLocation; }
            set 
            {
                NotifyPropertyChanging("CustLocation");
                _custLocation = value;
                NotifyPropertyChanged("CustLocation");
            }
        }


        #region INotifyPropertyChanged Members

        public event PropertyChangingEventHandler PropertyChanging;

        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
        
    }
}
