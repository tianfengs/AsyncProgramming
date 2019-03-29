using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 创建一个单例类，用10个线程并发调用里面的实例方法Increase()
/// 利用双检锁，保证单例模式，
/// </summary>
namespace AsyncDoubleCheckLocking3_1
{
    using System.Threading;

    class AsyncDoubleCheckLocking3_1
    {
        static void Main(string[] args)
        {
            // 1. 用带参线程传递处理对象进行处理
            //Singleton singleton = Singleton.GetSingleton();

            Thread myThread;
            Random rnd = new Random();

            for (int i = 0; i < 10; i++)
            {
                // 1. 用带参线程传递处理对象进行处理
                //myThread = new Thread(new ParameterizedThreadStart(MyThreadProc));
                //myThread.Name = String.Format("Thread{0}", i + 1);

                ////Wait a random amount of time before starting next thread.
                //Thread.Sleep(rnd.Next(0, 1000));
                //myThread.Start(singleton);

                // 2. 用无参线程启动，用单例模式的静态方法获取 Singleton 的实例对象，对其进行处理
                myThread = new Thread(new ThreadStart(MyThreadProc1));
                myThread.Name = String.Format("Thread{0}", i + 1);

                //Wait a random amount of time before starting next thread.
                Thread.Sleep(rnd.Next(0, 1000));
                myThread.Start();
            }

            Console.ReadKey();
        }

        // 1. 用带参线程传递处理对象进行处理
        private static void MyThreadProc(object obj)
        {
            for (int i = 0; i < 5; i++)
            {
                ((Singleton)obj).Increase(i + 1);

                //Wait 1 second before next attempt.
                Thread.Sleep(1000);
            }
        }

        // 2. 用无参线程启动，用单例模式的静态方法获取 Singleton 的实例对象，对其进行处理
        private static void MyThreadProc1()
        {
            for (int i = 0; i < 5; i++)
            {
                (Singleton.GetSingleton()).Increase(i+1);

                //Wait 1 second before next attempt.
                Thread.Sleep(1000);
            }
        }
    }

    /// <summary>
    /// 创建一个单例类
    /// </summary>
    public sealed class Singleton
    {        
        private static Object s_lock = new object();      // 定义一个锁定对象

        // 创建私有实例，为外界调用的实际单例对象
        private static Singleton s_value = null;          // 懒汉加载方式 lazy
        //private static Singleton s_value = new Singleton(); // 饿汉加载方式 延迟加载

        // 私有构造器，阻止外部创建实例
        private Singleton() { Count = 0; }

        // 设定一个自增变量
        public int Count { get; set; }
        // 实例函数，用于给变量自增
        public void Increase(int times)
        {
            Count++;
            Console.WriteLine($"{Thread.CurrentThread.Name}第{times}次执行后Count值是：{Count}");
        }

        // 静态方法，用于返回单例模式的单例实例
        public static Singleton GetSingleton()
        {
            // 如果只加同步锁，会使效率低下，下一个线程想获取对象，就要等待上一个线程结束，因此使用双if方式解决，即双重检测

            ///////////////////////////////
            // 方法一：双if+Monitor方法
            if (s_value != null) return s_value;

            Monitor.Enter(s_lock);

            if (s_value == null)
            {
                s_value = new Singleton();
            }

            Monitor.Exit(s_lock);
            //////////////////////////////////

            ////////////////////////////////////
            //// 方法二：双if+lock方法，此方法会产生异常，因此要在try块中使用，但会破坏线程同步，因此尽量避免使用
            //if (s_value == null)
            //{
            //    lock (s_lock)
            //    {
            //        if (s_value == null)
            //        {
            //            s_value = new Singleton();
            //        }
            //    }
            //}
            ///////////////////////////////////

            return s_value;
        }        
    }
}
