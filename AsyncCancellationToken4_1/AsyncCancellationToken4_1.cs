using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    curCancelToken.WaitHandle.WaitOne();
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
                    curCancelToken.WaitHandle.WaitOne();
                    Console.WriteLine(String.Format("线程{0}上，CancellationTokenSource操作已取消，WaitHandle获得信号"
                         , Thread.CurrentThread.ManagedThreadId));
                }
                , cts.Token
             );

            Thread.Sleep(2000);
            Console.WriteLine("执行CancellationTokenSource实例的Cancel()");
            cts.Cancel();
        }
    }
}
