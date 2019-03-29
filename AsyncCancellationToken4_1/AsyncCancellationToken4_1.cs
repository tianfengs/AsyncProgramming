using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 两个协作对象：CancellationTokenSource 和 CancellationToken
/// 协作式取消：
/// 
/// </summary>
namespace AsyncCancellationToken4_1
{
    using System.Threading;

    class AsyncCancellationToken4_1
    {
        static void Main(string[] args)
        {
            ThreadPool_Cancel_Test();

            Console.ReadKey();
        }

        public static void ThreadPool_Cancel_Test()
        {
            // 创建协作取消源
            CancellationTokenSource cts = new CancellationTokenSource();

            ThreadPool.QueueUserWorkItem(
                token =>
                { 
                    CancellationToken curCancelToken = (CancellationToken)token;

                    while (true)
                    {
                        // 耗时操作
                        Thread.Sleep(400);
                        if (curCancelToken.IsCancellationRequested)     // 当所有注册此取消对象都获得了取消通知，则为真，对应的线程不需要结束
                        {
                            break;
                        }
                    }

                    Console.WriteLine(String.Format("线程{0}上，CancellationTokenSource操作已取消，退出循环"
                        , Thread.CurrentThread.ManagedThreadId));
                }, cts.Token);

            ThreadPool.QueueUserWorkItem(
                token =>
                {
                    Console.WriteLine(String.Format("线程{0}上，调用CancellationToken实例的WaitHandle.WaitOne() "
                         , Thread.CurrentThread.ManagedThreadId));
                    CancellationToken curCancelToken = (CancellationToken)token;

                    curCancelToken.WaitHandle.WaitOne();    // 阻塞，等待信号

                    // 收到信号后，阻塞1000毫秒，继续执行
                    Thread.Sleep(1000);
                    Console.WriteLine(String.Format("线程{0}上，CancellationTokenSource操作已取消，WaitHandle获得信号"
                         , Thread.CurrentThread.ManagedThreadId));
                }
                , cts.Token
             );

            ThreadPool.QueueUserWorkItem(
                token =>
                {
                    Console.WriteLine(String.Format("线程{0}上，调用CancellationToken实例的WaitHandle.WaitOne() "
                         , Thread.CurrentThread.ManagedThreadId));
                    CancellationToken curCancelToken = (CancellationToken)token;

                    curCancelToken.WaitHandle.WaitOne();    // 阻塞，等待信号

                    // 收到信号后，立刻执行
                    Console.WriteLine(String.Format("线程{0}上，CancellationTokenSource操作已取消，WaitHandle获得信号"
                         , Thread.CurrentThread.ManagedThreadId));
                }
                , cts.Token
             );

            Thread.Sleep(2000);
            Console.WriteLine("执行CancellationTokenSource实例的Cancel()");

            // 向所有此取消标记，发送信号以解除阻塞
            cts.Cancel();   // Cancel()导致CancellationToken对象的ManualResetEvent对象调用Set()，发送信号，解除阻塞。
        }
    }
}
