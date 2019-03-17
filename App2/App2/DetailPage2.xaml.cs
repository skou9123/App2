using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App2
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DetailPage2 : ContentPage
	{
		public DetailPage2 (string url)
		{
			InitializeComponent();
            var webView = new WebView
            {
                Source = new UrlWebViewSource
                {
                    Url = url,

                },
            };
            
            Content = webView;
        }
	}
}