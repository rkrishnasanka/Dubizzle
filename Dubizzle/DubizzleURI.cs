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

namespace Dubizzle
{
    public class DubizzleURIBuilder
    {


        const string baseuri = "dubizzle.com/m";

        public Uri Url {  get; set; }

        public MainSection Section
        {
            get { return Section; }
            set 
            {
                Section = value;
                switch(Section)
                {
                    case MainSection.Classified : _sectionString = "classified";
                        break;
                    case MainSection.Property_for_Rent: _sectionString = "property_for_rent";
                        break;
                    case MainSection.Property_for_Sale: _sectionString = "property_for_sale";
                        break;
                    case MainSection.Jobs: _sectionString = "jobs";
                        break;
                    case MainSection.Jobs_Wanted: _sectionString = "jobs_wanted";
                        break;
                }
            }
        }

        public string Selection { get; set; }

        public ResourceDictionary Filteroptions { get; set; }
        public HTTPProtocol Protocol
        {
            get { return Protocol; }
            set
            {
                Protocol = value;
                switch (Protocol)
                {
                    case HTTPProtocol.HTTP : _protocolString="http://";
                        break;
                    case HTTPProtocol.HTTPS : _protocolString="https://";
                        break;
                }
            }
        }
        public Localization Location { get; set; }

        string _protocolString;
        string _locationString;
        string _sectionString;
        string _filteroptionString;

        public DubizzleURIBuilder(HTTPProtocol protocol, Localization location, MainSection section)
        {
            Location = location ;
            Protocol = protocol;
            Section = section;
            this.Url = new Uri(string.Format("{0}{1}.{2}/{3}", _protocolString, _locationString, baseuri, _sectionString));
        }

        public DubizzleURIBuilder(HTTPProtocol protocol, Localization location, MainSection section , string selection)
        {
            Location = location;
            Protocol = protocol;
            Section = section;
            Selection = selection;
            this.Url = new Uri(string.Format("{0}{1}.{2}/{3}/{4}", _protocolString, _locationString, baseuri, _sectionString, selection));
        }

    }

    public enum Localization
    {
        UAE_All,
        UAE_Dubai,
        UAE_AbuDhabi,
        UAE_Sharjah,
        UAE_Fujairah,
        UAE_RAK
    }

    public enum HTTPProtocol
    {
        HTTP ,
        HTTPS 
    }

    public enum MainSection
	{
	    Classified ,
        Property_for_Sale,
        Property_for_Rent ,
        Jobs ,
        Jobs_Wanted 
	}

}
