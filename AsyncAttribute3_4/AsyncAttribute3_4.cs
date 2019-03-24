using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 7、2. 利用特性进行上下文同步和方法同步  MethodImplAttribute(AttributeTargets.Constructor | AttributeTargets.Method)
/// </summary>
namespace AsyncAttribute3_4
{
    using System.Runtime.CompilerServices;
    using System.Threading;

    class AsyncAttribute3_4
    {
        static void Main(string[] args)
        {
            ThreadPool.QueueUserWorkItem(o => { class1.Static_Test1(); });
            Thread.Sleep(100);
            ThreadPool.QueueUserWorkItem(o => { class1.Static_Test2(); });
            ThreadPool.QueueUserWorkItem(o => { class1.Static_Test3(); });

            Console.ReadKey();
        }

        internal class class1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            public static void Static_Test1()
            {
                Thread.Sleep(100);
                Console.WriteLine("MethodImpl特性标注的静态方法----1");
                Console.WriteLine("1秒后释放lock (typeof(class1))");
                Thread.Sleep(1000);
            }

            public static void Static_Test2()
            {
                // MethodImplAttribute应用到static method相当于lock (typeof (该类))。
                lock (typeof(class1))
                {
                    Console.WriteLine("MethodImpl特性标注的静态方法----2");
                }
            }

            public static void Static_Test3()
            {
                Console.WriteLine("MethodImpl特性标注的静态方法----3");
            }
        }
    }
}
