using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static App2.MainPage;

namespace App2
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DetailPage1 : ContentPage
	{
        ObservableCollection<Data> menu_array = new ObservableCollection<Data>();//メニュー配列
        class Data
        { // <-1
            public String title { get; set; }
            public String detail { get; set; }
            public String time { get; set; }
        }
        
        public DetailPage1 (string title,string url, Dictionary<string, string> feed_url_title)
		{
			InitializeComponent ();

            string path;
            string path_feedurl;
            string path_newfeed;

            //android/ios ファイルパス
            path = PCLStorage.FileSystem.Current.LocalStorage.Path;

            var feed_txt=File.ReadAllLines(path + Regex.Replace(title, @"\.|""|\/|\[|\]|:|;|=", "") + ".txt");

            List<FeedData> Feed_List = new List<FeedData>();
            FeedData hoo = new FeedData();
            for (int i = 0; i < feed_txt.Length; i++)
            {
                if (feed_txt[i].Contains("readed:"))
                {
                    var read_1 = Regex.Replace(feed_txt[i], @"readed:", "");
                }
                
                if (feed_txt[i].Contains("title:"))
                {
                    hoo.title = Regex.Replace(feed_txt[i], @"title:|", "");
                    hoo.title = hoo.title.Replace("<![CDATA[", "");
                    hoo.title = hoo.title.Replace("]]>", "");
                }
                if (feed_txt[i].Contains("link:"))
                {
                    hoo.link = Regex.Replace(feed_txt[i], @"link:", "");
                }
                if (feed_txt[i].Contains("desc:"))
                {
                    hoo.desc = Regex.Replace(feed_txt[i], @"desc:", "");
                    i++;
                    for (; ; i++)
                    {
                        if (feed_txt[i].Contains("putdate:"))
                        {
                            hoo.desc = hoo.desc.Replace("&lt;", "<");
                            hoo.desc = hoo.desc.Replace("&gt;", ">");
                            hoo.desc = hoo.desc.Replace("&quot;", @"""");
                            hoo.desc = hoo.desc.Replace("&amp;#32;", " ");
                            hoo.desc = hoo.desc.Replace("&amp;#39;", "'");
                            break;
                        }
                        hoo.desc += feed_txt[i];
                    }
                }
                if (feed_txt[i].Contains("putdate:"))
                {
                    hoo.putdate = Regex.Replace(feed_txt[i], @"putdate:", "");
                    Feed_List.Add(hoo);
                }
            }            
            
            foreach (var item in Feed_List)
            {
                menu_array.Add(new Data { title=item.title, detail=item.desc,time = item.putdate});
            }

            listView.ItemsSource = menu_array;
            listView.ItemSelected += (sender, e) =>
            {
                foreach (var item in Feed_List)
                {
                    if (item.title== ((App2.DetailPage1.Data)e.SelectedItem).title)
                    {
                        Navigation.PushAsync(new DetailPage2(item.link));
                        return;
                    }
                }
            };
        }
	}
}