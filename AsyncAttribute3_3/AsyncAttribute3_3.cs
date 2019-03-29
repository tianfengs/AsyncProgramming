using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 八、1.利用特性（属性标签）进行类上下文同步
/// 应用SynchronizaitonAttribute的类，CLR会自动对这个类实施同步机制。
/// 为当前上下文和所有共享同一实例的上下文强制一个同步域(同步域之所以有意义就在于它不能被多个线程所共享。
/// 换句话说，一个处在同步域中的对象的方法是不能被多个线程同时执行的。这也意味着在任一时刻，最多只有一个线程处于同步域中)。
/// 被应用SynchronizationAttribute的类必须是上下文绑定的。换句话说，它必须继承于System.ContextBoundObject类。
/// </summary>
namespace AsyncAttribute3_3
{
    using System.Runtime.Remoting.Contexts;
    using System.Threading;

    class AsyncAttribute3_3
    {
        static void Main(string[] args)
        {
            class1 c = new class1();
            ThreadPool.QueueUserWorkItem(o => { c.Test1(); });
            Thread.Sleep(100);
            ThreadPool.QueueUserWorkItem(o => { c.Test2(); });

            Console.ReadKey();
        }


        [Synchronization(SynchronizationAttribute.REQUIRED)]    // 特性（属性标签）进行类上下文同步
        internal class class1 : ContextBoundObject // 必须继承于System.ContextBoundObject类！！！
        {
            public void Test1()
            {
                Thread.Sleep(1000);
                Console.WriteLine("Test1");
                Console.WriteLine("1秒后");
            }

            public void Test2()
            {
                Thread.Sleep(1000);
                Console.WriteLine("Test2");
            }
        }
    }


}
