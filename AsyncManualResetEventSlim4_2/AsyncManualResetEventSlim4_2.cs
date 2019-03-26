using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/// <summary>
/// 1. ManualResetEventSlim manualSlim = new ManualResetEventSlim(false);// 非终止状态
///    ManualResetEventSlim manualSlim = new ManualResetEventSlim(true); // 终止状态
/// 2. 非终止状态时 WaitOne()阻塞线程，不允许线程访问下边的语句
///    终止状态时   WaitOne()允许线程访问下边的语句
/// 3. 非终止状态 -->终止状态 : Set()方法
///    终止状态  -->非终止状态: Reset()方法
/// 参考：https://www.cnblogs.com/huangcong/p/5633456.html
/// </summary>
namespace AsyncManualResetEventSlim4_2
{
    using System.Threading;

    class AsyncManualResetEventSlim4_2
    {
        static void Main(string[] args)
        {
            ManualResetEventSlimTest();

            Console.ReadKey();
        }

        public static  void ManualResetEventSlimTest()
        {
            ManualResetEventSlim manualSlim = new ManualResetEventSlim(true);    // 状态设置为非终止状态。true为终止状态

            Console.WriteLine("0.ManualResetEventSlim示例开始");
            Thread thread1 = new Thread(o =>
            {
                Thread.Sleep(500);
                Console.WriteLine("2.调用ManualResetEventSlim的Set()");  // 第二步，执行thread1中代码
                manualSlim.Set();                                     // thread1调用Set()，设置thread1为终止，其它等待线程获得信号
                                                                      // 通知WaitHandle.WaitOne()获得信号 非终止状态->终止状态
            });
            thread1.Start();

            Console.WriteLine("1.调用ManualResetEventSlim的Wait()"); // 第一步，执行主线程，因为thread1开始sleep了500毫秒
            manualSlim.Wait();                                    //        主线程调用Wait(),主线程被阻止

            Console.WriteLine("3.调用ManualResetEventSlim的Reset()");        // 第三步，主线程重新获得执行权，继续执行主线程代码
            manualSlim.Reset();  // 重置为非终止状态，以便下一次Wait()等待      // 主线程调用Reset(),终止状态->非终止状态

            CancellationTokenSource cts = new CancellationTokenSource();
            Thread thread2 = new Thread(obj =>
            {
                Thread.Sleep(500);
                CancellationTokenSource curCTS = obj as CancellationTokenSource;
                Console.WriteLine("5.调用CancellationTokenSource的Cancel()");
                Thread.Sleep(200);

                curCTS.Cancel();
            });
            thread2.Start(cts);
            try
            {
                Console.WriteLine("4.调用ManualResetEventSlim的Wait()");
                manualSlim.Wait(cts.Token);
                //cts.Token.WaitHandle.WaitOne();
                Console.WriteLine("6.调用CancellationTokenSource后的输出");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("6.异常：OperationCanceledException");
            }
        }
    }
}
