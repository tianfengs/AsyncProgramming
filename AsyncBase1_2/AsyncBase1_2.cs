using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncBase1_2
{
    using System.Threading;

    class AsyncBase1_2
    {
        static void Main(string[] args)
        {
            SynchronizationContext sc = SynchronizationContext.Current;

            Thread t = new Thread(() =>
            {
                try
                {
                    Console.WriteLine("try内部，调用Abort前。");
                    // ……等待其他线程调用该线程的Abort()
                    Thread.Sleep(1000);
                    Console.WriteLine("try内部，调用Abort后。");
                }
                catch (ThreadAbortException abortEx)
                {
                    Console.WriteLine("catch:" + abortEx.GetType());
                    Thread.ResetAbort();  // 此处取消另一个线程（此处为主线程）的Abort()操作
                    Console.WriteLine("catch：调用ResetAbort()。");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("catch：" + ex.GetType());
                }
                finally
                {
                    Console.WriteLine("finally");
                    // 在finally中调用Thread.ResetAbort()不能取消线程的销毁
                    //Thread.ResetAbort();
                    //Console.WriteLine("调用ResetAbort()。");
                }

                // 若在catch中没有调用Thread.ResetAbort()，哪么try块外面的代码（下面的代码）就不会输出
                Console.WriteLine("try外面，调用Abort后（若再catch中调用了ResetAbort，则try块外面的代码依旧执行，即：线程没有终止）。");
            });

            // 其他线程调用该线程的Abort()
            t.Start();
            Thread.Sleep(200);
            t.Abort(); // 在t线程运行期间，取消这个线程
            Console.WriteLine("主线程，调用Abort。");

            Console.ReadKey();
        }
    }
}
