using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncBase1_1
{
    using System.Threading;

    class Program
    {
        static void Main(string[] args)
        {
            // 链接实例函数到委托
            ExampleCallback ResultCallback = ResultCallBackInstance;

            // 异步调用
            // 将需传递给异步执行方法数据及委托传递给帮助器类
            ThreadWithState tws = new ThreadWithState(
               "This report displays the number {0}.",
               42,
               new ExampleCallback(ResultCallback)
            );

            // 创建线程，通过 public delegate void ThreadStart()委托 构造无参数传递线程
            // 参数传递，由帮助器类辅助完成
            //Thread t = new Thread(new ThreadStart(tws.ThreadProc));
            Thread t = new Thread(tws.ThreadProc);
            t.IsBackground = true;
            t.Start();

            Console.ReadKey();
        }

        /// <summary>
        /// 异步执行完，做出响应的回调函数实例函数
        /// </summary>
        /// <param name="lineCount"></param>
        static public void ResultCallBackInstance(int lineCount)
        {
            Console.WriteLine($"LineCount = {lineCount}");
        }
    }

    
    /// <summary>
    /// 包装异步方法的委托 用来代理回调函数的委托 处理异步执行完毕后，做出的响应
    /// Thread构造函数接收的ThreadStart或ParameterizedThreadStart委托参数，这两个委托的声明都是返回void，即线程执行完后不会有数据返回。
    /// 那么如何在异步执行完时做出响应呢？使用回调方法。
    /// </summary>
    /// <param name="lineCount"></param>
    public delegate void ExampleCallback(int lineCount);

    /// <summary>
    /// 帮助器类
    /// </summary>
    public class ThreadWithState
    {
        private string boilerplate;
        private int value;
        private ExampleCallback callback;

        public ThreadWithState(string text, int number,
            ExampleCallback callbackDelegate)
        {
            boilerplate = text;
            value = number;
            callback = callbackDelegate;
        }

        public void ThreadProc()
        {
            Console.WriteLine(boilerplate, value);
            // 异步执行完时调用回调
            //callback?.Invoke(1);
            if (callback != null)
                callback(1);
        }
    }
}
