using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSpider
{
    class SpiderModel
    {
    }


    /// <summary>
    /// 城市
    /// </summary>
    public class City
    {
        /// <summary>
        /// 民称
        /// </summary>
        public string CityName { get; set; }

        /// <summary>
        /// url
        /// </summary>
        public Uri Uri { get; set; }
    }

    /// <summary>
    /// 酒店
    /// </summary>
    public class Hotel
    {

       /// <summary>
       /// 酒店名称
       /// </summary>
        public string HotelName { get; set; }

        /// <summary>
        /// 酒店价格
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 酒店url
        /// </summary>
        public Uri Uri { get; set; }


    }

    public class SimpleCrawler : ICrawler
    {
        public event EventHandler<OnStartEventArgs> OnStart;//爬虫启动事件

        public event EventHandler<OnCompletedEventArgs> OnCompleted;//爬虫完成事件

        public event EventHandler<OnErrorEventArgs> OnError;//爬虫出错事件

        //public event EventHandler<>
        public CookieContainer CookiesContainer { get; set; }//定义Cookie容器

        public SimpleCrawler() { }

        public static FileStream file = new FileStream("./error.txt", FileMode.OpenOrCreate, FileAccess.Write);
        /// <summary>
        /// 异步创建爬虫
        /// </summary>
        /// <param name="uri">爬虫URL地址</param>
        /// <param name="proxy">代理服务器</param>
        /// <returns>网页源代码</returns>
        public async Task<string> Start(Uri uri, string proxy = null)
        {
            return await Task.Run(() =>
            {
                var pageSource = string.Empty;
                try
                {
                    if (this.OnStart != null) this.OnStart(this, new OnStartEventArgs(uri));
                    var watch = new Stopwatch();
                    watch.Start();
                    var request = (HttpWebRequest)WebRequest.Create(uri);
                    request.Accept = "*/*";
                    request.ServicePoint.Expect100Continue = false;//加快载入速度
                    request.ServicePoint.UseNagleAlgorithm = false;//禁止Nagle算法加快载入速度
                    request.AllowWriteStreamBuffering = false;//禁止缓冲加快载入速度
                    request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");//定义gzip压缩页面支持
                    request.ContentType = "application/x-www-form-urlencoded";//定义文档类型及编码
                    request.AllowAutoRedirect = false;//禁止自动跳转
                    //设置User-Agent，伪装成Google Chrome浏览器
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
                    request.Timeout = 5000;//定义请求超时时间为5秒
                    request.KeepAlive = true;//启用长连接
                    request.Method = "GET";//定义请求方式为GET              
                    if (proxy != null) request.Proxy = new WebProxy(proxy);//设置代理服务器IP，伪装请求地址
                    request.CookieContainer = this.CookiesContainer;//附加Cookie容器
                    request.ServicePoint.ConnectionLimit = int.MaxValue;//定义最大连接数

                    using (var response = (HttpWebResponse)request.GetResponse())
                    {//获取请求响应

                        foreach (Cookie cookie in response.Cookies) this.CookiesContainer.Add(cookie);//将Cookie加入容器，保存登录状态

                        if (response.ContentEncoding.ToLower().Contains("gzip"))//解压
                        {
                            using (GZipStream stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                {
                                    pageSource = reader.ReadToEnd();
                                }
                            }
                        }
                        else if (response.ContentEncoding.ToLower().Contains("deflate"))//解压
                        {
                            using (DeflateStream stream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress))
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                {
                                    pageSource = reader.ReadToEnd();
                                }

                            }
                        }
                        else
                        {
                            using (Stream stream = response.GetResponseStream())//原始
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                {

                                    pageSource = reader.ReadToEnd();
                                }
                            }
                        }
                    }
                    request.Abort();
                    watch.Stop();
                  
                    var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;//获取当前任务线程ID
                    var milliseconds = watch.ElapsedMilliseconds;//获取请求执行时间
                    if (this.OnCompleted != null) this.OnCompleted(this, new OnCompletedEventArgs(uri, threadId, milliseconds, pageSource));
                }
                catch (Exception ex)
                {
                    if (this.OnError != null) this.OnError(this, new OnErrorEventArgs(uri, ex));
                }
                return pageSource;
            });
        }

        /// <summary>
        /// 下载图片
        /// </summary>
        /// <param name="Uri">地址</param>
        /// <param name="proxy"></param>
        public async static void DownLoadFile(string Uri, string proxy = null)
        {
            await Task.Run(() =>
            {
                try
                {
                    //获取图片名称
                    string fileName = Uri.Substring((Uri.LastIndexOf("/") + 1));
                    List<string> list = new List<string> { "jpg", "png", "gif" };
                    int index = list.IndexOf(fileName.Substring(fileName.LastIndexOf(".") + 1));
                    if (index == -1)
                        return;
                    var request = (HttpWebRequest)WebRequest.Create(Uri);
                    request.Accept = "*/*";
                    request.ServicePoint.Expect100Continue = false;//加快载入速度
                    request.ServicePoint.UseNagleAlgorithm = false;//禁止Nagle算法加快载入速度
                    request.AllowWriteStreamBuffering = false;//禁止缓冲加快载入速度
                    request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");//定义gzip压缩页面支持
                    request.ContentType = "application/x-www-form-urlencoded";//定义文档类型及编码
                    request.AllowAutoRedirect = false;//禁止自动跳转
                    //设置User-Agent，伪装成Google Chrome浏览器
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
                    request.Timeout = 5000;//定义请求超时时间为5秒
                    request.KeepAlive = true;//启用长连接
                    request.Method = "GET";//定义请求方式为GET              
                    if (proxy != null) request.Proxy = new WebProxy(proxy);//设置代理服务器IP，伪装请求地址
                                                                           //    request.CookieContainer = this.CookiesContainer;//附加Cookie容器
                    request.ServicePoint.ConnectionLimit = int.MaxValue;//定义最大连接数

                    //获取响
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        Stream stream = response.GetResponseStream();//获取响应流
                                                                     //StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
                        Image image = Image.FromStream(stream);//获取图片信息
                                                               //构建图片大小的位图
                        Bitmap bitmap = new Bitmap(image.Width, image.Height);
                        bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                        Graphics graphics = Graphics.FromImage(bitmap);
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
                        bitmap.Save($"./Image/{fileName}");

                    }
                }
                catch (Exception ex)
                {

                    //写入错误信息
                    //byte[] bytes = Encoding.UTF8.GetBytes((Uri + "\r\n"), 0, (Uri + "\r\n").Length);
                    //file.Write(bytes, 0, bytes.Length);

                    Console.WriteLine($"下载图片出错，下载地址为：{Uri},errorMessage:{ex.Message}");
                }
                ////关闭并销毁文件句柄
                //file.Close();
                //file.Dispose();
                GC.Collect();
            });
        }
    }

}
