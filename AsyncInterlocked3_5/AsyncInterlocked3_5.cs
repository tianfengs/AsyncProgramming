using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Interlocked.Exchange 方法
/// https://docs.microsoft.com/zh-cn/dotnet/api/system.threading.interlocked.exchange?redirectedfrom=MSDN&view=netframework-4.7.2#System_Threading_Interlocked_Exchange_System_Int32__System_Int32_
/// 原子操作赋值
/// public static int Interlocked.Exchange(ref int location1, int value); 以原子操作的形式，将 32 位有符号整数设置为指定的值并返回原始值。
/// ref int location1:要设置为指定值的变量。int value:参数要设置成的值。
/// 返回值：location1 的原始值。
/// </summary>
namespace AsyncInterlocked3_5
{
    using System.Threading;

    class AsyncInterlocked3_5
    {
        //0 for false, 1 for true.
        private static int usingResource = 0;   // 设置一个资源，将用互锁方式进行单元操作赋值，避免线程争用错误

        private const int numThreadIterations = 5;
        private const int numThreads = 10;      

        static void Main(string[] args)
        {
            Thread myThread;
            Random rnd = new Random();

            /// 创建10个线程，并随机间隔0-1s的时间依次启动
            for (int i = 0; i < numThreads; i++)
            {
                myThread = new Thread(new ThreadStart(MyThreadProc));
                myThread.Name = String.Format("Thread{0}", i + 1);

                //Wait a random amount of time before starting next thread.
                Thread.Sleep(rnd.Next(0, 1000));
                myThread.Start();
            }

            Console.ReadKey();
        }

        /// <summary>
        /// 每个线程，每间隔1s的时间，对资源进行一次读写操作，即调用UserResource()方法
        /// </summary>
        private static void MyThreadProc()
        {
            for (int i = 0; i < numThreadIterations; i++)
            {
                UseResource();

                //Wait 1 second before next attempt.
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// 一个简单的对资源使用的方法——拒绝重入方法
        /// </summary>
        /// <returns></returns>
        //A simple method that denies reentrancy.
        static bool UseResource()
        {
            // 0 表示资源没有被使用
            //0 indicates that the method is not in use. 
            if (0 == Interlocked.Exchange(ref usingResource, 1))
            {
                Console.WriteLine("{0} 获得锁 acquired the lock", Thread.CurrentThread.Name);

                //Code to access a resource that is not thread safe would go here.

                //Simulate some work
                Thread.Sleep(500);

                Console.WriteLine("{0} 结束锁 exiting lock", Thread.CurrentThread.Name);

                //Release the lock
                Interlocked.Exchange(ref usingResource, 0);
                return true;
            }
            else
            {
                Console.WriteLine("   {0} 被锁拒绝 was denied the lock", Thread.CurrentThread.Name);
                return false;
            }
        }
    }
}
