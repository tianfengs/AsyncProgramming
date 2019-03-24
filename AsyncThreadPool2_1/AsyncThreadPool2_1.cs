using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            //Console.WriteLine("主线程开始WaitOne()");
            //endWaitHandle.WaitOne();
            //Console.WriteLine("主线程结束WaitOne()");
            Console.ReadKey();
        }

        private static void Example_RegisterWaitForSingleObject()
        {
            // AutoResetEvent 常常被用来在两个线程之间进行信号发送
            // 线程可以通过调用AutoResetEvent对象的WaitOne()方法进入等待状态，然后另外一个线程通过调用AutoResetEvent对象的Set()方法取消等待的状态。
            // 在内存中保持着一个bool值，如果bool值为False，则使线程阻塞，反之，如果bool值为True,则使线程退出阻塞。
            // 当我们创建AutoResetEvent对象的实例时，我们在函数构造中传递默认的bool值，以下是实例化AutoResetEvent的例子。

            AutoResetEvent notificWaitHandle = new AutoResetEvent(false);
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            
            ///////////////////////////////////////
            /// 在线程池加入一个线程
            /// 通过调用 ThreadPool.QueueUserWorkItem 并传递 WaitCallback 委托来使用线程池。
            /// 也可以通过使用 ThreadPool.RegisterWaitForSingleObject 并传递 WaitHandle 来将与等待操作相关的工作项排队到线程池中(此处使用这个方法)
            RegisteredWaitHandle registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(
                waitHandle,
                (Object state, bool timeOut) =>     // state为传递进来的变量，此处为null；timeOut为：如果 WaitHandle 在指定时间内没有收到信号（即，超时），则为 true，否则为 false
                {
                    if (timeOut)
                    {
                        Console.WriteLine("RegisterWaitForSingleObject因超时而执行");
                    }
                    else
                    {
                        Console.WriteLine("RegisterWaitForSingleObject收到WaitHandle信号");
                    }
                },
                null,                               // 传递个委托的变量 state=null
                TimeSpan.FromSeconds(2),
                true                                // executeOnlyOnce的Boolean=true表示线程池线程只执行回调方法一次；若false则表示内核对象每次收到信号，线程池线程都会执行回调方法。
                                                    // 等待一个AutoResetEvent对象时，这个功能尤其有用。
                );


            // 取消等待操作（即不再执行WaitOrTimerCallback委托）
            registeredWaitHandle.Unregister(notificWaitHandle);

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

            //endWaitHandle.WaitOne();    // WaitOne()阻止当前线程继续执行，并使线程进入等待状态以获取其他线程发送的信号。直到超时或者其它线程发送Set()
            //Console.WriteLine("子线程开始Set()");
            //endWaitHandle.Set();
        }
    }
}
