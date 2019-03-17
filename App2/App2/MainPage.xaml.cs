using PCLStorage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace App2
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            ToolbarItem tItem1 = new ToolbarItem
            {
                Icon = "",
                Text = "更新",
                Priority = 1,
                Order = ToolbarItemOrder.Primary,
                Command = new Command(async () =>
                {
                    await ReloadFeed(menu_array);
                }),
            };
            
            ToolbarItem tItem2 = new ToolbarItem
            {
                Text = "初期化",
                Priority = 2,
                Order = ToolbarItemOrder.Secondary,
                Command = new Command(() =>
                {
                    menu_array.Clear();
                    feed_url_title.Clear();
                    IsLoad = false;
                    IsAll = false;
                    newfeed.IsEnabled = false;
                    allfeed.IsEnabled = true;
                    if (feed_array!=null)
                    {
                        feed_array.Clear();
                    }
                    File.Delete(path_feedurl);
                    File.Delete(path_newfeed);
                    Init();
                }),
            };
            //ツールバーとして設定します。
            this.ToolbarItems.Add(tItem1);
            this.ToolbarItems.Add(tItem2);
        }

        async Task ReloadFeed(ObservableCollection<String> menu_array)
        {
            if (IsAll)
            {
                menu_array.Clear();
                feed_url_title.Clear();
                File.Delete(path_newfeed);
            }

            var feedurl_array = File.ReadAllLines(path_feedurl);

            label.Text = "更新中";
            await Task.Run(async () =>
            {
                foreach (var item in feedurl_array)
                {
                    try
                    {
                        using (HttpClient httpClient = new HttpClient())
                        {
                            //feed取得
                            var html = await httpClient.GetStringAsync(item);

                            var title = Regex.Match(html, @"<title>.*?</title>").Value;
                            title = Regex.Replace(title, @"<title>|</title>|<!\[CDATA\[|\]\]>", "");

                            //rss2
                            var feed = Regex.Matches(html, @"<item>.*?</item>|<item .*?>.*?</item>", RegexOptions.Singleline);
                            if (feed.Count == 0)
                            {
                                //atom
                                feed = Regex.Matches(html, @"<entry>.*?</entry>|<entry .*?>.*?</entry>", RegexOptions.Singleline);
                                Atom(feed, out feed_array);
                            }
                            else
                            {
                                //rss2
                                Rss2(feed, out feed_array);
                            }

                            if (IsAll)//全て更新
                            {
                                feed_url_title.Add(title, item);
                                menu_array.Add(title);
                                File.AppendAllText(path_newfeed, Regex.Replace(title, @"\r|\n", "") + "\r\n");//title newfeed
                                File.AppendAllText(path_newfeed, Regex.Replace(item, @"\r|\n", "") + "\r\n");//url newfeed

                                title = Regex.Replace(title, @"\.|""|\/|\[|\]|:|;|=", "");
                                File.WriteAllText(path + title + ".txt", feed_array[0].putdate + "\r\n\r\n");

                                for (int i = 0; i < feed_array.Count; i++)
                                {
                                    File.AppendAllText(path + title + ".txt", "title:" + (i + 1).ToString() + "." + feed_array[i].title + "\r\nlink:" + feed_array[i].link + "\r\ndesc:");
                                    File.AppendAllText(path + title + ".txt", feed_array[i].desc + "\r\nputdate:");
                                    File.AppendAllText(path + title + ".txt", feed_array[i].putdate + "\r\n\r\n");
                                }
                            }
                            else//新着のみ表示
                            {
                                if (!File.Exists(path + Regex.Replace(title, @"\.|""|\/|\[|\]|:|;|=", "") + ".txt"))
                                {
                                    feed_url_title.Add(title, item);
                                    menu_array.Add(title);
                                    File.AppendAllText(path_newfeed, Regex.Replace(title + ".txt", @"\r|\n", "") + "\r\n");//title newfeed
                                    File.AppendAllText(path_newfeed, Regex.Replace(item, @"\r|\n", "") + "\r\n");//url newfeed

                                    title = Regex.Replace(title, @"\.|""|\/|\[|\]|:|;|=", "");
                                    File.WriteAllText(path + title + ".txt", feed_array[0].putdate + "\r\n\r\n");

                                    for (int i = 0; i < feed_array.Count; i++)
                                    {
                                        File.AppendAllText(path + title + ".txt", "title:" + (i + 1).ToString() + "." + feed_array[i].title + "\r\nlink:" + feed_array[i].link + "\r\ndesc:");
                                        File.AppendAllText(path + title + ".txt", feed_array[i].desc + "\r\nputdate:");
                                        File.AppendAllText(path + title + ".txt", feed_array[i].putdate + "\r\n\r\n");
                                    }
                                }
                                else
                                {
                                    var putdate = File.ReadAllLines(path + Regex.Replace(title, @"\.|""|\/|\[|\]|:|;|=", "") + ".txt");
                                    if (feed_array[0].putdate != putdate[0])//更新された
                                    {
                                        feed_url_title.Add(title, item);
                                        menu_array.Add(title);
                                        var feed_a=File.ReadAllLines(path_newfeed);
                                        if (!feed_a.Contains(title))
                                        {
                                            File.AppendAllText(path_newfeed, Regex.Replace(title, @"\r|\n", "") + "\r\n");//title newfeed
                                            File.AppendAllText(path_newfeed, Regex.Replace(item, @"\r|\n", "") + "\r\n");//url newfeed
                                        }

                                        title = Regex.Replace(title, @"\.|""|\/|\[|\]|:|;|=", "");
                                        File.WriteAllText(path + title + ".txt", feed_array[0].putdate + "\r\n\r\n");

                                        for (int i = 0; i < feed_array.Count; i++)
                                        {
                                            File.AppendAllText(path + title + ".txt", "title:" + (i + 1).ToString() + "." + feed_array[i].title + "\r\nlink:" + feed_array[i].link + "\r\ndesc:");
                                            File.AppendAllText(path + title + ".txt", feed_array[i].desc + "\r\nputdate:");
                                            File.AppendAllText(path + title + ".txt", feed_array[i].putdate + "\r\n\r\n");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            });

            label.Text = "更新完了";
        }

        void Rss2(MatchCollection re, out List<FeedData> rsslist)
        {
            rsslist = new List<FeedData>();//最新Feed

            foreach (Match itesm in re)
            {
                FeedData hoge = new FeedData();
                //<title>
                hoge.title = Regex.Match(itesm.Value, @"<title>.*?</title>", RegexOptions.Singleline).Value;
                hoge.title = Regex.Replace(hoge.title, @"<title>|</title>", "", RegexOptions.Singleline);
                hoge.title = hoge.title.Replace("<![CDATA[", "");
                hoge.title = hoge.title.Replace("]]>", "");
                hoge.title = hoge.title.Replace("\r", "");
                hoge.title = hoge.title.Replace("\n", "");

                hoge.title = hoge.title.Replace("&lt;", "<");
                hoge.title = hoge.title.Replace("&gt;", ">");
                hoge.title = hoge.title.Replace("&quot;", @"""");
                //&quot;
                hoge.title = hoge.title.Replace("&#32", " ");
                hoge.title = hoge.title.Replace("&#39", "'");
                hoge.title = Regex.Replace(hoge.title, @"^ +", "", RegexOptions.Singleline);
                //<link>
                hoge.link = Regex.Match(itesm.Value, @"<link>.*?</link>", RegexOptions.Singleline).Value;
                hoge.link = Regex.Replace(hoge.link, @"<link>|</link>", "", RegexOptions.Singleline);
                hoge.link = hoge.link.Replace("<![CDATA[", "");
                hoge.link = hoge.link.Replace("]]>", "");
                //<pubDate>
                hoge.putdate = Regex.Match(itesm.Value, @"<pubDate>.*?</pubDate>", RegexOptions.Singleline).Value;
                hoge.putdate = Regex.Replace(hoge.putdate, @"<pubDate>|</pubDate>", "", RegexOptions.Singleline);
                hoge.putdate = hoge.putdate.Replace("<![CDATA[", "");
                hoge.putdate = hoge.putdate.Replace("]]>", "");
                //<description>
                hoge.desc = Regex.Match(itesm.Value, @"<description>.*?</description>", RegexOptions.Singleline).Value;
                hoge.desc = Regex.Replace(hoge.desc, @"<description>|</description>", "", RegexOptions.Singleline);

                //<content>
                hoge.desc += Regex.Match(itesm.Value, @"<content.*?</content.*?>", RegexOptions.Singleline).Value;
                hoge.desc = Regex.Replace(hoge.desc, @"<content.*?>|</content.*?>", "", RegexOptions.Singleline);
                hoge.desc = hoge.desc.Replace("<![CDATA[", "");
                hoge.desc = hoge.desc.Replace("]]>", "");


                if (hoge.putdate == "")
                {
                    //<pubDate>
                    hoge.putdate = Regex.Match(itesm.Value, @"<dc:date>.*?</dc:date>", RegexOptions.Singleline).Value;
                    hoge.putdate = Regex.Replace(hoge.putdate, @"<dc:date>|</dc:date>", "", RegexOptions.Singleline);
                    hoge.putdate = hoge.putdate.Replace("<![CDATA[", "");
                    hoge.putdate = hoge.putdate.Replace("]]>", "");
                }

                rsslist.Add(hoge);
            }
        }

        void Atom(MatchCollection re, out List<FeedData> rsslist)
        {
            rsslist = new List<FeedData>();//最新Feed

            foreach (Match itesm in re)
            {
                FeedData hoge = new FeedData();
                //<title>
                hoge.title = Regex.Match(itesm.Value, @"<title>.*?</title>", RegexOptions.Singleline).Value;
                hoge.title = Regex.Replace(hoge.title, @"<title>|</title>", "", RegexOptions.Singleline);
                hoge.title = hoge.title.Replace("&lt;", "<");
                hoge.title = hoge.title.Replace("&gt;", ">");
                hoge.title = hoge.title.Replace("&quot;", @"""");
                hoge.title = hoge.title.Replace("&#32", " ");
                hoge.title = hoge.title.Replace("&#39", "'");
                //<link>
                hoge.link = Regex.Match(itesm.Value, @"<link .*?href.*?/>", RegexOptions.Singleline).Value;
                hoge.link = Regex.Replace(hoge.link, @"<link .*?href=""|"" />", "");
                hoge.link = hoge.link.Replace(@"""/>", "");
                hoge.link = hoge.link.Replace("<![CDATA[", "");
                hoge.link = hoge.link.Replace("]]>", "");
                //<<updated>>
                hoge.putdate = Regex.Match(itesm.Value, @"<updated>.*?</updated>", RegexOptions.Singleline).Value;
                hoge.putdate = Regex.Replace(hoge.putdate, @"<updated>|</updated>", "", RegexOptions.Singleline);
                hoge.putdate = hoge.putdate.Replace("<![CDATA[", "");
                hoge.putdate = hoge.putdate.Replace("]]>", "");
                //<description>
                hoge.desc = Regex.Match(itesm.Value, @"<summary>.*?</summary>", RegexOptions.Singleline).Value;
                hoge.desc = Regex.Replace(hoge.desc, @"<summary>|</summary>", "", RegexOptions.Singleline);
                hoge.desc = hoge.desc.Replace("<![CDATA[", "");
                hoge.desc = hoge.desc.Replace("]]>", "");

                if (hoge.putdate == "")
                {
                    //<pubDate>
                    hoge.putdate = Regex.Match(itesm.Value, @"<dc:date>.*?</dc:date>", RegexOptions.Singleline).Value;
                    hoge.putdate = Regex.Replace(hoge.putdate, @"<dc:date>|</dc:date>", "", RegexOptions.Singleline);
                    hoge.putdate = hoge.putdate.Replace("<![CDATA[", "");
                    hoge.putdate = hoge.putdate.Replace("]]>", "");
                }
                if (hoge.desc == "")
                {
                    //<pubDate>
                    hoge.desc = Regex.Match(itesm.Value, @"<content.*?</content>", RegexOptions.Singleline).Value;
                    hoge.desc = Regex.Replace(hoge.desc, @"<content.*?>|</content>", "", RegexOptions.Singleline);
                    hoge.desc = hoge.desc.Replace("<![CDATA[", "");
                    hoge.desc = hoge.desc.Replace("]]>", "");
                }

                rsslist.Add(hoge);
            }
        }

        void Init()
        {
            if (IsLoad)
            {
                return;
            }
            
            //android/ios ファイルパス
            path = PCLStorage.FileSystem.Current.LocalStorage.Path;
            path_feedurl = path + "FeedUrl.txt";
            path_newfeed = path + "NewFeed.txt";

            try
            {
                var feed_a = File.ReadAllLines(path_newfeed);
                string title="";
                foreach (var item in feed_a)
                {
                    if (Regex.IsMatch(item, @"http.*?"))
                    {
                        feed_url_title.Add(title, item);
                    }
                    else
                    {
                        title = item;
                        menu_array.Add(item);
                    }
                }
            }
            catch (Exception)
            {
            }

            if (!File.Exists(path_feedurl))
            {
                //初回起動
                string feedurl = "";
                feedurl += "https://www.moguravr.com/feed\r\n";
                feedurl += "http://www.pixivision.net/ja/rss\r\n";
                feedurl += "http://pc.watch.impress.co.jp/sublink/pc.rdf\r\n";
                feedurl += "http://www.choke-point.com/?feed=rss2\r\n";
                feedurl += "http://www.forest.impress.co.jp/rss.xml\r\n";
                feedurl += "http://www.lifehacker.jp/index.xml\r\n";
                feedurl += "http://www.4gamer.net/rss/index.xml\r\n";
                feedurl += "http://feed.japan.cnet.com/rss/index.rdf\r\n";
                feedurl += "http://rss.itmedia.co.jp/rss/1.0/news_bursts.xml\r\n";
                feedurl += "http://rss.itmedia.co.jp/rss/1.0/techtarget.xml\r\n";
                feedurl += "http://jp.gamesindustry.biz/rss/index.xml\r\n";



                File.WriteAllText(path_feedurl, feedurl);
            }
        }

        public struct FeedData
        {
            public string title;
            public string link;
            public string putdate;
            public string desc;
        }

        bool IsLoad = false;
        bool IsAll = false;
        List<FeedData> feed_array;
        Dictionary<string,string> feed_url_title = new Dictionary<string,string>();
        ObservableCollection<String> menu_array = new ObservableCollection<String>();//メニュー配列
        string path;
        string path_feedurl;
        string path_newfeed;

        async protected override void OnAppearing()//ページロード後イベント
        {
            if (IsLoad)
            {
                return;
            }
            Init();

            listView.ItemsSource = menu_array;
            listView.ItemSelected += (sender, e) =>
            {
                Navigation.PushAsync(new DetailPage1((string)e.SelectedItem, feed_url_title[(string)e.SelectedItem], feed_url_title));
            };

            newfeed.Clicked += (sender, e) =>
            {
                IsAll = false;
                newfeed.IsEnabled = false;
                allfeed.IsEnabled = true;
            };

            allfeed.Clicked += (sender, e) =>
            {
                IsAll = true;
                newfeed.IsEnabled = true;
                allfeed.IsEnabled = false;
            };

            reloadfeed.Clicked += async (sender, e) => 
            {
                await ReloadFeed(menu_array);
            };            

            IsLoad = true;
        }


    }
}
