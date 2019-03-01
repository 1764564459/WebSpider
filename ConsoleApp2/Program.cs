using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        public static int timer = 0;
        //Delegate del=Delegate.CreateDelegate()
        public delegate string myDel();
        static readonly Object locker = new Object();
       static ConcurrentQueue<Concurrent> ts = new ConcurrentQueue<Concurrent>();
        private static int ncount = 2000,Sum=2000;
       static Queue<string> queues = new Queue<string>();
        public static int nCount { get { Thread.Sleep(250); return ncount; }set { Thread.Sleep(250); ncount = value; } }
        static Queue<Object> queue = new Queue<Object>();

        static FileStream stream = new FileStream("./1.txt", FileMode.OpenOrCreate, FileAccess.Write);
        static void Main(string[] args)
        {
            //return;
            // queue.Enqueue(new Concurrent() { UserName = "张三" });
            myDel del;
            Console.Write("请输入你要发送的信息：");
            string getStr = Console.ReadLine();
            return;
            switch (getStr)
            {
                case "1":
                    {
                        del = new myDel(Real);
                        timer = 1 * 1000;
                    }
                    break;
                case "2":
                    {
                        del = new myDel(TenSecond);
                        timer = 1 * 1000 * 10;
                    }
                    //getStr = "Hour##0376QN=20180914090600630;ST=31;CN=2011;PW=123456;MN=010AQMS01000GD0201804060;Flag=4;CP=&&DataTime=20180914090600;a34001-Rtd=67.04,a34001-Flag=N;a34004-Rtd=39.60,a34004-Flag=N;a34002-Rtd=53.00,a34002-Flag=N;Leq-Rtd=60.97,Leq-Flag=N;a01007-Rtd=0.0,a01007-Flag=N;a01008-Rtd=335,a01008-Flag=N;a01006-Rtd=30.4,a01006-Flag=N;B00008-Rtd=51.3,B00008-Flag=N;B00009-Rtd=99.3,B00009-Flag=N&&2580";
                    break;
                case "3":
                    {
                        del = new myDel(Minute);
                        timer = 1000 * 60;
                    }
                    //getStr = "Real##0375QN=20180914090704440;ST=31;CN=2011;PW=123456;MN=010AQMS01000GD0201804071;Flag=4;CP=&&DataTime=20180914090700;a34001-Rtd=60.56,a34001-Flag=N;a34004-Rtd=36.90,a34004-Flag=N;a34002-Rtd=47.00,a34002-Flag=N;Leq-Rtd=61.65,Leq-Flag=N;a01007-Rtd=0.0,a01007-Flag=N;a01008-Rtd=53,a01008-Flag=N;a01006-Rtd=32.1,a01006-Flag=N;B00008-Rtd=46.3,B00008-Flag=N;B00009-Rtd=99.3,B00009-Flag=N&&24C0";
                    break;
                default:
                    {
                        del = new myDel(Real);
                        timer = 1 * 1000;
                    }
                    //getStr = "d##0375QN=20180914090704440;ST=31;CN=2011;PW=123456;MN=010AQMS01000GD0201804071;Flag=4;CP=&&DataTime=20180914090700;a34001-Rtd=60.56,a34001-Flag=N;a34004-Rtd=36.90,a34004-Flag=N;a34002-Rtd=47.00,a34002-Flag=N;Leq-Rtd=61.65,Leq-Flag=N;a01007-Rtd=0.0,a01007-Flag=N;a01008-Rtd=53,a01008-Flag=N;a01006-Rtd=32.1,a01006-Flag=N;B00008-Rtd=46.3,B00008-Flag=N;B00009-Rtd=99.3,B00009-Flag=N&&24C0";
                    break;
            }
          
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(IPAddress.Parse("192.168.1.156"), 457);
            while (true)
            {
                    getStr = del();
                    Byte[] bytes = Encoding.UTF8.GetBytes(getStr, 0, getStr.Length);
                    int nRst = socket.Send(bytes);
                    if (nRst == (int)SocketError.SocketError)
                        Console.WriteLine("发送失败");
                    //socket.Close();

                Thread.Sleep(timer);
            }
        }

        public static void HighConcurrent()
        {
            /*创建任务 t1  t1 执行 数据集合添加操作*/
            Task t1 = Task.Factory.StartNew(() =>
            {
                AddProducts(0, 1000);
            });
            /*创建任务 t2  t2 执行 数据集合添加操作*/
            Task t2 = Task.Factory.StartNew(() =>
            {
                AddProducts(1001, 2000);
            });
            /*创建任务 t3  t3 执行 数据集合添加操作*/
            Task t3 = Task.Factory.StartNew(() =>
            {
                AddProducts(2001, 3000);
            });
            Task.WaitAll(t1, t2, t3);
            Console.WriteLine($"nCount is{nCount} ");
        }
        public static string Real()
        { return $"Real##0382QNDateTime={DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}"; }
        public static string TenSecond()
        { return $"TenSecond##0382QNDateTime={DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}"; }
        public static string Minute()
        { return $"Minute##0382QNDateTime={DateTime.Now.ToString("yyyy-MM-dd HH:mm")}"; }
        /*执行集合数据添加操作*/
        static async void AddProducts(int s,int e)
        {
            bool bRst=true;
            for (int i = s; i < e; i++)
            {
                ThreadPool.QueueUserWorkItem(item => {  bRst = by((int)item).Result;},i);
                if (!bRst) {  Console.ReadKey(); break; }
            }
            //Parallel.For(s, e, (i) =>
            //{
            //    by(i);
              

            //});

        }

        public async static Task<bool> by(int i)
        {
            lock (locker)
            {
                Random rand = new Random();
                int count = rand.Next(1, 4), tempNum = Sum;
                if (tempNum - count < 0)
                {
                    Console.WriteLine("当前余量不足。");
                    Console.ReadKey();
                    return false;
                }

                Sum -= count;
                Concurrent concurrent = new Concurrent() { UserName = i + "", count = count };
                Console.WriteLine($"{concurrent.UserName} by {concurrent.count} ,nCount of {Sum},queue count of {ts.Count}");
                ts.Enqueue(concurrent);
                if (tempNum == 0)
                {
                    Console.WriteLine("已售罄。");
                    Console.ReadKey();
                    return false;
                }
                if (ts.Count > 30)
                    save();
                return true;
            }
        }

        public async static void save()
        {
            //lock (locker)
            //{
            //    for (int i = 0; i < 30; i++)
            //    {
            //        Concurrent concurrent = new Concurrent();
            //        ts.TryDequeue(out concurrent);
            //        int tempCount = nCount;
            //        tempCount -= concurrent.count;
            //        nCount = tempCount;
            //        Console.WriteLine($"{concurrent.UserName} by {concurrent.count} ,nCount of {nCount},queue count of {ts.Count}");
            //    }
            //}
        }
        
        
        public async static  void Concurrence(Object i)
        {

            Console.WriteLine($"count:{ts.Count}");
            for (int j = 0; j < ts.Count; j++)
            {
                string name = "";
                   // ts.TryDequeue(out name);
                int temp = nCount;
                temp -= 1;
                nCount = temp;
                //queues.Enqueue(name);
                //Thread thread = new Thread(Concurrence) ;
                //thread.Start(i);
                //if(queues.Count>=20)
                name = $"Name={name};ncount={ncount},temp={temp},time={DateTime.Now.ToString("HH:mm:ss.ffff")}\r\n";
            }
            //lock (locker)
            //{


            //    if (nCount > 0)
            //    {
            //        string name = ((int)i) + "";
            //        int temp = nCount;
            //        temp -= 1;
            //        nCount = temp;
            //        //queues.Enqueue(name);
            //        //Thread thread = new Thread(Concurrence) ;
            //        //thread.Start(i);
            //        //if(queues.Count>=20)
            //        name = $"Name={name};ncount={ncount},temp={temp},time={DateTime.Now.ToString("HH:mm:ss.ffff")}\r\n";
            //        Console.WriteLine(name);
            //        //Byte[] bytes = Encoding.UTF8.GetBytes(name, 0, name.Length);
            //        //stream.Write(bytes, 0, bytes.Length);
            //        //stream.Close();
            //        //stream.Dispose();
            //    }
            //}
        }

        public async static void Excute(Object items)
        {
           
            lock (locker)
            {
           
                // Console.WriteLine($"Queue Count of {queues.Count}");
                for (int i = 0; i < queues.Count; i++)
                {
                    string str = queues.Dequeue();
                    int temp = nCount;
                    if (nCount > 0)
                    {
                        temp -= 1;
                        nCount = temp;
                        str = $"Name={str};ncount={ncount},temp={temp},time={DateTime.Now.ToString("HH:mm:ss.ffff")}";
                        Byte[] bytes = Encoding.UTF8.GetBytes(str,0,str.Length);
                        stream.Write(bytes,0,bytes.Length);
                    }
                }
            }
            stream.Close();
            stream.Dispose();
            
        }
    }
    
    public class Concurrent
    {
        public Concurrent()
        { }
        public string UserName { get; set; }

        public int count { get; set; }
        public Concurrent(string name,int nCount)
        {
            this.UserName = name;
            this.count = nCount;
        }
    }
}
