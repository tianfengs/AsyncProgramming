using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 本例演示了用ThreadPool.RegisterWaitForSingleObject()方法在线程池里加入线程，
/// 并进行超时和获得信号量的多种方法解除阻塞的方法
/// 
/// 还有一种建立线程池线程的方法：ThreadPool.QueueUserWorkItem(WaitCallback, state),比较简单，只有回调函数和传递给回调函数的参数。
/// 
/// 下面讲解一下信号量设置的的原理：（摘自 第四章 异步编程：轻量级线程同步基元对象）
/// - ManualResetEventSlim是基于自旋+Monitor完成。
/// - 可在ManualResetEventSlim的构造函数中指定切换为内核模式之前需发生的自旋等待数量（只读的SpinCount属性），默认为10。
/// - 访问WaitHandle属性会延迟创建一个ManualResetEvent(false)对象。在调用ManualResetEventSlim的set()方法时通知WaitHandle.WaitOne() 获得信号。
/// 
/// ManualResetEvent的** 终止状态和非终止状态**
/// 
/// AutoResetEvent和ManualResetEvent 常常被用来在两个线程之间进行信号发送
/// 
/// >用Set方法后:
/// >自动开关量（自动信号量，AutoResetEvent）调用Set()方法后，值变为true后，立即变为false,可以将它理解为一个脉冲
/// >手动开关量（手动信号量，ManualResetEvent）值将一直保持为true,直到调用Reset()把开关量设置回false
/// 
/// 参考：https://www.cnblogs.com/huangcong/p/5633456.html
/// 
/// > 0. 在内存中保持着一个bool值，如果bool值为False，则使线程阻塞，（非终止状态）
/// >    反之，如果bool值为True,则使线程退出阻塞。                （终止状态）
/// > 1. 我们在函数构造中传递默认的bool值，以下是实例化AutoResetEvent的例子
/// >    ManualResetEventSlim manualSlim = new ManualResetEventSlim(false);// 非终止状态 false
/// >    ManualResetEventSlim manualSlim = new ManualResetEventSlim(true); // 终止状态	  true
/// > 2. 非终止状态时 WaitOne()阻塞线程，不允许线程访问下边的语句
/// >    终止状态时 WaitOne()允许线程访问下边的语句
/// > 3. 非终止状态 --> 终止状态 : Set()方法，即 false --> true
/// >    终止状态  --> 非终止状态: Reset()方法，即 true --> false
/// </summary>
namespace AsyncThreadPool2_1
{
    using System.Threading;

    class AsyncThreadPool2_1
    {
        static AutoResetEvent endWaitHandle = new AutoResetEvent(false);  // 加endWaitHandle的原因：如果执行过快退出方法会导致一些东西被释放，造成排队的任务不能执行，原因还在研究

        static void Main(string[] args)
        {
            // 示例：ThreadPool.RegisterWaitForSingleObject 
            Example_RegisterWaitForSingleObject();

            // 示例：ExecutionContext
            //Example_ExecutionContext();

            Console.ReadKey();
        }

        private static void Example_RegisterWaitForSingleObject()
        {
            // - AutoResetEvent 常常被用来在两个线程之间进行信号发送
            // - 线程可以通过调用AutoResetEvent对象的WaitOne()方法进入等待状态，
            //   然后另外一个线程通过调用AutoResetEvent对象的Set()方法取消等待的状态。
            // - 在内存中保持着一个bool值，如果bool值为False，则使线程阻塞，（非终止状态）Set()方法 使 false-->true 
            //   反之，如果bool值为True,则使线程退出阻塞。                （终止状态） Reset()方法 使 true-->false
            // - 当我们创建AutoResetEvent对象的实例时，我们在函数构造中传递默认的bool值，以下是实例化AutoResetEvent的例子。

            AutoResetEvent notificWaitHandle = new AutoResetEvent(false);
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            
            ///////////////////////////////////////
            /// 在线程池加入一个线程
            /// 通过调用 ThreadPool.QueueUserWorkItem 并传递 WaitCallback 委托来使用线程池。
            /// 也可以通过使用 ThreadPool.RegisterWaitForSingleObject 并传递 WaitHandle 来将与等待操作相关的工作项排队到线程池中(此处使用这个方法)
            RegisteredWaitHandle registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                waitHandle,
                (object state, bool timeOut) => // WaitOrTimerCallback(Object state, bool timeOut)
                                                // state为传递进来的变量，此处为null；
                                                // timeOut为：如果 WaitHandle 在指定时间内没有收到信号（即，超时），则为 true，否则为 false
                {
                    if (timeOut)
                    {
                        Console.WriteLine("RegisterWaitForSingleObject因超时而执行");
                    }
                    else
                    {
                        Console.WriteLine("RegisterWaitForSingleObject收到WaitHandle信号");
                    }
                    //endWaitHandle.Set(); // 如果本身没有被取消，运行了回调函数，则可以在此设置开关量（信号量）为终止状态true，
                },
                null,                               // 传递个委托的变量 state=null
                TimeSpan.FromSeconds(2),
                true                                // executeOnlyOnce的Boolean=true表示线程池线程只执行回调方法一次；若false则表示内核对象每次收到信号，线程池线程都会执行回调方法。
                                                    // 等待一个AutoResetEvent对象时，这个功能尤其有用。
                );

            //Thread.Sleep(3000);   // 超时，大于2秒的调用间隔TimeSpan.FromSeconds(2)，就会超时调用回调函数
            waitHandle.Set();       // 设置等待句柄为终止状态，信号量bool值为true，取消其线程阻塞，运行回调函数

            //endWaitHandle.WaitOne();    // WaitOne()阻止当前线程继续执行，并使线程进入等待状态以获取其他线程发送的信号。直到超时或者其它线程发送Set()

            // 取消等待操作（即不再执行WaitOrTimerCallback委托）
            registeredWaitHandle.Unregister(notificWaitHandle);
            //notificWaitHandle.Set();

            ////////////////////////////////////////
            // 在线程池加入另一个线程
            // 通知
            ThreadPool.RegisterWaitForSingleObject(notificWaitHandle,
                (Object state, bool timeOut) =>
                {
                    if (timeOut)
                        Console.WriteLine("第一个RegisterWaitForSingleObject没有调用Unregister()");
                    else
                        Console.WriteLine("第一个RegisterWaitForSingleObject调用了Unregister()");

                    
                }, null,
                TimeSpan.FromSeconds(4), true);

        }
    }
}
