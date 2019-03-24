using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncDoubleCheckLocking3_1
{
    using System.Threading;

    class AsyncDoubleCheckLocking3_1
    {
        static void Main(string[] args)
        {
        }
    }

    /// <summary>
    /// 创建一个单例类
    /// </summary>
    public sealed class Singleton
    {
        private static Object s_lock = new object();
        private static Singleton s_value = null;          // 懒汉加载方式 lazy
        //private static Singleton s_value = new Singleton(); // 饿汉加载方式 延迟加载

        // 私有构造器，阻止外部创建实例
        private Singleton() { }

        public static Singleton GetSingleton()
        {
            // 如果只加同步锁，会使效率低下，下一个线程想获取对象，就要等待上一个线程结束，因此使用双if方式解决，即双重检测

            ///////////////////////////////
            // 双if+Monitor方法
            if (s_value != null) return s_value;

            Monitor.Enter(s_lock);

            if (s_value == null)
            {
                s_value = new Singleton();
            }

            Monitor.Exit(s_lock);
            //////////////////////////////////

            //////////////////////////////////
            // 双if+lock方法，此方法会产生异常，因此要在try块中使用，但会破坏线程同步，因此尽量避免使用
            if (s_value == null)
            {
                lock (s_lock)
                {
                    if (s_value == null)
                    {
                        s_value = new Singleton();
                    }
                }
            }
            /////////////////////////////////

            return s_value;
        }

        
    }
}
