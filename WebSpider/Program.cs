using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WebSpider
{
    class Program
    {
        public static Object locker = new Object();
        public static Object QueueLocker = new Object();
        public static ConcurrentQueue<string> queue = new ConcurrentQueue<string>() { };

        public static ConcurrentQueue<Hotel> ImageQueue = new ConcurrentQueue<Hotel>();
        public static ConcurrentQueue<Hotel> LinkQueue = new ConcurrentQueue<Hotel>();
        
        public static Dictionary<string, string[]> pairs = new Dictionary<string, string[]>() {
            {"A",new string[]{"1","2","3","4","5" } },
            {"B",new string[]{"6","7","8","9","10" }  },
            {"C",new string[]{"11","12","13","14","15" }  },
            {"D",new string[]{"16","17","18","19","20" }  },
            {"E",new string[]{"21","22","23","24","25" }  },
            {"F",new string[]{"26","27","28","29","30" }  }
        };
       static List<string> tempList = new List<string>() { "A","B","C","D","E","F"};


        static List<Hotel> ImageList = new List<Hotel>(),HttpList=new List<Hotel> ();
        static void Main(string[] args)
        {
            //测试代理IP是否生效：http://1212.ip138.com/ic.asp

            //测试当前爬虫的User-Agent：http://www.whatismyuseragent.net

            string html = @"<img src='//pic.c-ctrip.com/common/loading_50.gif' />";

            MatchCollection regex = Regex.Matches(html, @"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>");
            var value = regex[0].Groups["imgUrl"].Value;
            //1.抓取城市
            // CityCrawler();


            //2.抓取酒店
             HotelCrawler();
            //queue.Enqueue("A");
            //queue.Enqueue("B");
            //queue.Enqueue("C");
            //queue.Enqueue("D");
            //queue.Enqueue("E");
            //queue.Enqueue("F");
            //test();

            //3.并发抓取示例
            //ConcurrentCrawler();


            //SimpleCrawler.DownLoadFile("http://img-ads.csdn.net/2016/201608021757063065.png");

            Console.ReadKey();
        }

        public    static void FindChild(string key)
        {
            lock (locker)
            {
               // Task.Run(() =>
               //{
                   //  Console.WriteLine($"key:{key}");

                   try
                   {
                       string[] strArr = pairs[key];
                       foreach (string item in strArr)
                       {
                           queue.Enqueue(item);
                       }
                   }
                   catch (Exception ex)
                   {
                       //return;

                   }
                   test();
               //});
            }
        }
        public static void test()
        {
            Parallel.For(0,queue.Count, (index) => {
                lock (locker)
                {
                    string tempStr = string.Empty;
                    queue.TryDequeue(out tempStr);
                    if (!string.IsNullOrWhiteSpace(tempStr))
                        Console.WriteLine($"str:{tempStr},equeue count :{queue.Count}");
                    FindChild(tempStr);
                    //   tempList.Remove(tempList[index]);
                }
            });
         
        }

        /// <summary>
        /// 抓取城市列表
        /// </summary>
        public static void CityCrawler()
        {

            var cityUrl = "http://hotels.ctrip.com/citylist";//定义爬虫入口URL
            var cityList = new List<City>();//定义泛型列表存放城市名称及对应的酒店URL
            var cityCrawler = new SimpleCrawler();//调用刚才写的爬虫程序
            cityCrawler.OnStart += (s, e) =>
            {
                Console.WriteLine("爬虫开始抓取地址：" + e.Uri.ToString());
            };
            cityCrawler.OnError += (s, e) =>
            {
                Console.WriteLine("爬虫抓取出现错误：" + e.Uri.ToString() + "，异常消息：" + e.Exception.Message);
            };
            cityCrawler.OnCompleted += (s, e) =>
            {
                //使用正则表达式清洗网页源代码中的数据
                var links = Regex.Matches(e.PageSource, @"<a[^>]+href=""*(?<href>/hotel/[^>\s]+)""\s*[^>]*>(?<text>(?!.*img).*?)</a>", RegexOptions.IgnoreCase);
                foreach (Match match in links)
                {
                    var city = new City
                    {
                        CityName = match.Groups["text"].Value,
                        Uri = new Uri("http://hotels.ctrip.com" + match.Groups["href"].Value
                    )
                    };
                    if (!cityList.Contains(city)) cityList.Add(city);//将数据加入到泛型列表
                    Console.WriteLine(city.CityName + "|" + city.Uri);//将城市名称及URL显示到控制台
                }
                Console.WriteLine("===============================================");
                Console.WriteLine("爬虫抓取任务完成！合计 " + links.Count + " 个城市。");
                Console.WriteLine("耗时：" + e.Milliseconds + "毫秒");
                Console.WriteLine("线程：" + e.ThreadId);
                Console.WriteLine("地址：" + e.Uri.ToString());
            };
            cityCrawler.Start(new Uri(cityUrl)).Wait();//没被封锁就别使用代理：60.221.50.118:8090
        }



        /// <summary>
        /// 抓取酒店列表
        /// </summary>
        public static void HotelCrawler()
        {
            FileStream file = new FileStream("./1.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var hotelUrl = "https://blog.csdn.net/sqldebug_fan/article/details/20465455";//"http://hotels.ctrip.com/hotel/zunyi558";
            var hotelList = new List<Hotel>();
            var hotelCrawler = new SimpleCrawler();
            hotelCrawler.OnStart += (s, e) =>
            {
                Console.WriteLine("爬虫开始抓取地址：" + e.Uri.ToString());
            };
            hotelCrawler.OnError += (s, e) =>
            {
                Console.WriteLine("爬虫抓取出现错误：" + e.Uri.ToString() + "，异常消息：" + e.Exception.Message);

                //写入错误信息
                byte[] bytes = Encoding.UTF8.GetBytes(e.Uri.ToString() + "\r\n", 0, (e.Uri.ToString() + "\r\n").Length);
                file.Write(bytes, 0, bytes.Length);
                //关闭并销毁文件句柄
                file.Close();
                file.Dispose();
            };
            hotelCrawler.OnCompleted += (s, e) =>
            {
                //正则表达式列表
                Dictionary<string, string> list = new Dictionary<string, string>()
                {
                    {"Link_url",@"(?i)<a\s[^>]*?href=(['""]?)(?!javascript|__doPostBack)(?<url>[^'""\s*#<>]+)[^>]*>" },//a标签中的链接
                    { "Http",@"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?"},//http链接
                    { "Image_imgUrl",@"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>"}//image链接
                };

                //遍历所有正则、并匹配
                foreach (KeyValuePair<string,string> link in list)
                {
                    //分割名称、占位
                    string[] strArr = link.Key.Split('_');
                    string Uri = "";

                    //正则匹配
                    var links = Regex.Matches(e.PageSource, link.Value, RegexOptions.IgnoreCase);
                    foreach (Match match in links)
                    {
                       
                        var hotel = new Hotel
                        {
                            //HotelName = match.Groups["text"].Value,
                            HotelName = strArr[0],
                            // Uri = new Uri("http://hotels.ctrip.com" + match.Groups["href"].Value)
                        };
                        
                        //获取匹配占位符的数据
                        if (strArr.Length > 1)
                            Uri = match.Groups[strArr[1]].Value;
                        else
                            Uri = match.Value;

                        //不包含http加上网页的根网址
                        if (!Uri.Contains("http"))
                            Uri = Uri.StartsWith(@"//") ? "http:" + Uri : "http:/" + Uri;

                        //验证是否属于网站地址
                        MatchCollection regex = Regex.Matches(Uri,list["Http"]);
                        if (regex.Count < 1)//匹配的地址与原地址不相同
                            continue;
                        hotel.Uri = new Uri(Uri);
                        if (!ImageList.Contains(hotel) && hotel.HotelName.Contains("Image"))
                        {
                            ImageList.Add(hotel);//将数据加入到泛型列表
                            ImageQueue.Enqueue(hotel);
                        }
                        if (!HttpList.Contains(hotel) && !hotel.HotelName.Contains("Image"))
                        {
                            HttpList.Add(hotel);//将链接加到HttpList中
                            LinkQueue.Enqueue(hotel);//将连接加入到队列中
                        }
                        Console.WriteLine(hotel.HotelName + "|" + hotel.Uri);//将酒店名称及详细页URL显示到控制台
                      
                    }

                    Console.WriteLine();
                    Console.WriteLine("===============================================");
                    Console.WriteLine("爬虫抓取任务完成！合计 " + links.Count + " 个酒店。");
                    Console.WriteLine("耗时：" + e.Milliseconds + "毫秒");
                    Console.WriteLine("线程：" + e.ThreadId);
                    Console.WriteLine("地址：" + e.Uri.ToString());
                }

               
                //并发下载图片
                Console.WriteLine($"开始下载图片...,Image Count is:{ImageList.Count}");
                Parallel.For(0,ImageQueue.Count, (index) =>
                {
                    Hotel hotel = new Hotel();
                    ImageQueue.TryDequeue(out hotel);
                    Console.WriteLine($"正在下载第{index}张图片");
                    SimpleCrawler.DownLoadFile(hotel.Uri.ToString());
                    ImageList.Remove(hotel);

                });

                //并发遍历链接
                Parallel.For(0,LinkQueue.Count, (index) =>
                {
                    Hotel hotel = new Hotel();
                    LinkQueue.TryDequeue(out hotel);
                    hotelCrawler.Start(hotel.Uri).Wait();
                    HttpList.Remove(hotel);
                });
                

                //回收资源
                GC.Collect();
            };
          
            hotelCrawler.Start(new Uri(hotelUrl)).Wait();//没被封锁就别使用代理：60.221.50.118:8090
        }


        /// <summary>
        /// 并发抓取示例
        /// </summary>
        public static void ConcurrentCrawler()
        {
            var hotelList = new List<Hotel>() {
                new Hotel { HotelName="遵义浙商酒店", Uri=new Uri("http://hotels.ctrip.com/hotel/4983680.html?isFull=F") },
                new Hotel { HotelName="遵义森林大酒店", Uri=new Uri("http://hotels.ctrip.com/hotel/1665124.html?isFull=F") },
            };
            var hotelCrawler = new SimpleCrawler();
            hotelCrawler.OnStart += (s, e) =>
            {
                Console.WriteLine("爬虫开始抓取地址：" + e.Uri.ToString());
            };
            hotelCrawler.OnError += (s, e) =>
            {
                Console.WriteLine("爬虫抓取出现错误：" + e.Uri.ToString() + "，异常消息：" + e.Exception.Message);
            };
            hotelCrawler.OnCompleted += (s, e) =>
            {
                Console.WriteLine();
                Console.WriteLine("===============================================");
                Console.WriteLine("爬虫抓取任务完成！");
                Console.WriteLine("耗时：" + e.Milliseconds + "毫秒");
                Console.WriteLine("线程：" + e.ThreadId);
                Console.WriteLine("地址：" + e.Uri.ToString());
            };
            Parallel.For(0, 2, (i) =>
            {
                var hotel = hotelList[i];
                hotelCrawler.Start(hotel.Uri);
            });
        }
    }
}
