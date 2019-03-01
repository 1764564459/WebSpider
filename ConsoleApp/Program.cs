using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        Dictionary<string, List<string>> Keys = new Dictionary<string, List<string>>();
        static readonly Object locker = new Object();
        static int nCount = 100;
        public static FileStream fileStream = new FileStream("./Log.txt",FileMode.OpenOrCreate,FileAccess.ReadWrite);
        static ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        static List<Socket> socketList = new List<Socket>();
        public static void Main(string[] args)
        {

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(address: IPAddress.Parse("192.168.1.156"),port:457);
            socket.Bind(endPoint);
            socket.Listen(10);

            Console.WriteLine("各部门已做好准备，请发送指令。");
            while (true)
            {
                Socket accept_Socket = socket.Accept();
                while (true)
                {
                    if (!accept_Socket.Connected)
                    {
                        Console.WriteLine("客户端断开链接");
                        Console.ReadKey();
                        break;
                    }
                    ThreadPool.QueueUserWorkItem(item =>
                    {
                        //accept_Socket.Receive
                        byte[] bytes = new byte[10240];
                        int nLen = accept_Socket.Receive(bytes);
                        string content = Encoding.UTF8.GetString(bytes, 0, nLen);
                        queue.Enqueue(content);
                        Console.WriteLine($"接收到的信息：{content}");
                    });
                    if (queue.Count > 30)
                        DeQueue();
                }
             
            }
            socket.Close();
            socket.Dispose();
            //fileStream.Close();
            //fileStream.Dispose();
        }
        
        public static void DeQueue()
        {
            Parallel.ForEach(queue, (item) => {
                lock (locker)
                {
                    string tempArr = string.Empty;
                    queue.TryDequeue(out tempArr);
                    tempArr += "\r\n";
                    byte[] bytes = Encoding.UTF8.GetBytes(tempArr);
                    fileStream.Write(bytes);
                }
            });
         
        }

        
    }

    public class Concurrent
    {
        public string UserName { get; set; }
        public Concurrent()
        {
        }
        public void Excute()
        {
        }
    }
}
