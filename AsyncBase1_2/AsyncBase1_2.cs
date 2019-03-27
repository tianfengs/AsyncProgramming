using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 1.本例是在子线程运行期间Sleep(2000)，
/// 2.在主线程调用t.Abort()销毁线程
/// 3.子线程中断运行，被子线程catch块捕获，catch块调用Thread.ResetAbort()取消销毁操作
/// 4.子线程继续运行完当前catch块，并运行finally，并继续运行线程其它部分，直到结束
/// </summary>
namespace AsyncBase1_2
{
    using System.Threading;

    class AsyncBase1_2
    {
        static void Main(string[] args)
        {
            //SynchronizationContext sc = SynchronizationContext.Current; 
            //sc.Post(new SendOrPostCallback((result) => progressBar1.Value = (int)result), p);       // 通过 SynchronizationContext 实例 sc 异步 跨线程更新UI界面progressBar1控件

            Thread t = new Thread(() =>
            {
                try
                {
                    Console.WriteLine("try内部，调用Abort前。");
                    // ……等待其他线程调用该线程的Abort()
                    Thread.Sleep(2000);                             // 1.
                    Console.WriteLine("try内部，调用Abort后。");
                }
                catch (ThreadAbortException abortEx)
                {
                    Console.WriteLine("catch:" + abortEx.GetType());
                    Thread.ResetAbort();  // 此处取消另一个线程（此处为主线程）的Abort()操作    // 3.
                    Console.WriteLine("catch：调用ResetAbort()。");                       // 4.
                }
                catch (Exception ex)
                {
                    Console.WriteLine("catch：" + ex.GetType());
                }
                finally
                {
                    Console.WriteLine("finally");                                       // 4.
                    // 在finally中调用Thread.ResetAbort()不能取消线程的销毁
                    //Thread.ResetAbort();
                    //Console.WriteLine("调用ResetAbort()。");
                }

                // 若在catch中没有调用Thread.ResetAbort()，哪么try块外面的代码（下面的代码）就不会输出
                Console.WriteLine("try外面，调用Abort后（若再catch中调用了ResetAbort，则try块外面的代码依旧执行，即：线程没有终止）。");    // 4.
            });

            // 其他线程调用该线程的Abort()
            t.Start();

            Thread.Sleep(1000);
            t.Abort(); // 在t线程运行期间，取消这个线程                   // 2. 
            Console.WriteLine("主线程，调用Abort。");

            Console.ReadKey();
        }
    }
}
