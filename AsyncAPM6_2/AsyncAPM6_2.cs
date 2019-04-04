using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// 使用异步方式调用同步方法, 利用IAsyncResult接口，实现APM模式异步方法
/// https://docs.microsoft.com/zh-cn/dotnet/standard/asynchronous-programming-patterns/calling-synchronous-methods-asynchronously?view=netframework-4.7.2
/// 三种阻塞式异步方法（1.2.3)，一种非阻塞式异步方法(4)：
/// 1. 用 EndInvoke 阻塞
/// 2. 用 WaitHandle 阻塞
/// 3. 用 IsCompleted轮询 阻塞
/// 4. 使用回调函数，非阻塞式响应异步调用方法
/// </summary>
namespace AsyncAPM6_2
{
    // 定义异步调用的测试方法
    public class AsyncDemo
    {
        public string  TestMethod(int callDuration, out int threadId)
        {
            Console.WriteLine("Test method begins.");
            Thread.Sleep(callDuration);
            threadId = Thread.CurrentThread.ManagedThreadId;
            return String.Format("My call time was {0}.", callDuration.ToString());
        }
    }

    // 声明一个委托，和异步调用的方法签名以及返回值相同
    public delegate string AsyncMethodCaller(int callDuration, out int threadId);
    
    class AsyncAPM6_2
    {
        static void Main(string[] args)
        {
            // 1. 用 EndInvoke 阻塞式响应异步调用方法
            Console.WriteLine("1. 用EndInvoke阻塞式响应异步调用方法");
            Async_EndInvoke();

            // 2. 用 WaitHandle 阻塞式响应异步调用方法
            Console.WriteLine("2. 用 WaitHandle 阻塞式响应异步调用方法");
            Async_WaitHandle();

            // 3. 用 IsCompleted轮询 阻塞式响应异步调用方法
            Console.WriteLine("3. 用 IsCompleted轮询 阻塞式响应异步调用方法");
            Async_IsCompleted();

            // 4. 使用回调函数，非阻塞式响应异步调用方法
            Console.WriteLine("4. 使用回调函数，非阻塞式响应异步调用方法");
            Async_NoStop();

            Console.ReadKey();
        }

        /// <summary>
        /// 1. 用EndInvoke阻塞线程
        /// 调用BeginInvoke后，立刻返回主线程，
        /// 遇到EndInvoke后，阻塞主线程，直到子线程运行结束
        /// </summary>
        static void Async_EndInvoke()
        {
            int threadId;

            AsyncDemo ad = new AsyncDemo();

            AsyncMethodCaller caller = new AsyncMethodCaller(ad.TestMethod);

            IAsyncResult result = caller.BeginInvoke(3000, out threadId, null, null);

            Thread.Sleep(0);
            Console.WriteLine($"主线程{Thread.CurrentThread.ManagedThreadId}做一些工作");

            string rtnValue = caller.EndInvoke(out threadId, result);

            Console.WriteLine($"这个调用运行在线程{threadId}，返回值是\"{rtnValue}\"");

            Console.WriteLine("主线程运行结束");
            Console.WriteLine();

        }

        /// <summary>
        /// 2. 使用 WaitHandle 阻塞等待异步调用
        /// </summary>
        static void Async_WaitHandle()
        {
            int threadId;

            AsyncDemo ad = new AsyncDemo();

            AsyncMethodCaller caller = new AsyncMethodCaller(ad.TestMethod);

            IAsyncResult result = caller.BeginInvoke(3000, out threadId, null, null);

            Thread.Sleep(0);
            Console.WriteLine($"主线程{Thread.CurrentThread.ManagedThreadId}做一些工作");

            result.AsyncWaitHandle.WaitOne();   // 阻塞

            string rtnValue = caller.EndInvoke(out threadId, result);

            result.AsyncWaitHandle.Close();

            Console.WriteLine($"这个调用运行在线程{threadId}，返回值是\"{rtnValue}\"");

            Console.WriteLine("主线程运行结束");
            Console.WriteLine();

        }

        /// <summary>
        /// 3. 使用 IsCompleted轮询 阻塞式响应异步调用方法
        /// </summary>
        static void Async_IsCompleted()
        {
            int threadId;

            AsyncDemo ad = new AsyncDemo();

            AsyncMethodCaller caller = new AsyncMethodCaller(ad.TestMethod);

            IAsyncResult result = caller.BeginInvoke(3000, out threadId, null, null);

            Thread.Sleep(0);
            Console.WriteLine($"主线程{Thread.CurrentThread.ManagedThreadId}做一些工作");

            while (!result.IsCompleted)
            {
                Thread.Sleep(250);
                Console.Write(".");
            }

            string returnValue = caller.EndInvoke(out threadId, result);

            Console.WriteLine($"\n这个调用运行在线程{threadId}，返回值是\"{returnValue}\"");

            Console.WriteLine("主线程运行结束");
            Console.WriteLine();
        }

        /// <summary>
        /// 4. 使用回调函数，非阻塞式响应异步调用方法
        /// </summary>
        static void Async_NoStop()
        {
            int threadId;

            AsyncDemo ad = new AsyncDemo();

            AsyncMethodCaller caller = new AsyncMethodCaller(ad.TestMethod);

            IAsyncResult result = caller.BeginInvoke(3000, out threadId, new AsyncCallback(CallBackMethod), "这个调用运行在线程{0}，返回值是\"{1}\"。");

            Console.WriteLine($"主线程{Thread.CurrentThread.ManagedThreadId}继续运行。。。");

            //Thread.Sleep(1000);
            Console.WriteLine("主线程运行结束");
            Console.WriteLine();
        }

        private static void CallBackMethod(IAsyncResult ar)
        {
            AsyncResult result = (AsyncResult)ar;
            AsyncMethodCaller caller = (AsyncMethodCaller)result.AsyncDelegate; // 调用

            string formatString = (string)ar.AsyncState;

            int threadId = 0;

            string rtnValue = caller.EndInvoke(out threadId, ar);

            Console.WriteLine(formatString, threadId, rtnValue);

            Console.WriteLine();
        }
    }
}
