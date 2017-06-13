using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DSLink.UWP.Example
{
    public sealed partial class MainPage : Page
    {
        private readonly UniversalWindowsDSLink _link;

        public MainPage()
        {
            InitializeComponent();
            
            _link = new UniversalWindowsDSLink(new Configuration(new List<string>(), "UWP-DSLink", true, true));
        }

        private void StartStopButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_link.Connector.Connected())
            {
                _link.Disconnect();
            }
            else
            {
                _link.Config.BrokerUrl = BrokerURLText.Text;
                _link.Connect();
            }
        }
    }
}
