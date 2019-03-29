using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 验证
/// "双检锁在执行完构造函数后才会将变量引用返回"
/// 
/// </summary>
namespace AsyncDoubleCheckLocking3_2
{
    using System.Threading;

    /// <summary>
    /// 采取第二种方式：跑两个线程，一个线程创建实例，并在构造函数中加入耗时操作Thread.Spin(Int32.MaxValue)，
    /// 另一个线程不断访问s_value。得出结果是执行完构造函数后才会将变量引用返回。
    /// </summary>
    class AsyncDoubleCheckLocking3_2
    {
        private static Singleton s_value = null;

        static void Main(string[] args)
        {
            // 一个线程创建单例对象
            ThreadPool.QueueUserWorkItem((obj) => Singleton.GetSingleton());
            // 第二个线程不停读取这个单例对象：结论：执行完构造函数后，其它线程才会将变量引用返回
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                while (true)
                {
                    if (s_value != null)
                    {
                        Console.WriteLine("s_value不为null。");
                        break;
                    }
                }
            });

            Console.ReadKey();
        }

        public sealed class Singleton
        {
            private static Object s_lock = new object();

            // 私有构造器，阻止这个类外部的任何代码创建实例
            private Singleton()
            {
                Thread.SpinWait(50000000);
                Console.WriteLine("对象创建完成");
            }

            public static Singleton GetSingleton()
            {
                if (s_value != null) return s_value;

                Monitor.Enter(s_lock);
                if (s_value == null)
                {
                    s_value = new Singleton();
                    //Singleton temp = new Singleton();
                    //Interlocked.Exchange(ref s_value, temp);
                    Console.WriteLine("赋值完成");
                }
                Monitor.Exit(s_lock);
                return s_value;
            }
        }
    }

}
