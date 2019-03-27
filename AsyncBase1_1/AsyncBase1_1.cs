using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 本例用帮助器类，使用无参数的Thread（ThreadStart start）方法，用无参数的Start()方法，
/// 为线程传递参数，
/// 以及使用委托的方式创建回调函数，是线程在异步执行完可以做出响应
/// </summary>
namespace AsyncBase1_1
{
    using System.Threading;

    /// <summary>
    /// 1.声明委托
    /// 包装异步方法的委托 用来代理回调函数的委托 处理异步执行完毕后，做出的响应
    /// Thread构造函数接收的ThreadStart或ParameterizedThreadStart委托参数，
    /// 这两个委托的声明都是返回void，即线程执行完后不会有数据返回。
    /// 那么如何在异步执行完时做出响应呢？使用回调方法。
    /// </summary>
    /// <param name="lineCount"></param>
    public delegate void ExampleCallback(int lineCount);

    class Program
    {
        static void Main(string[] args)
        {
            // 3. 链接“实例方法”到“委托对象”
            ExampleCallback ResultCallback = ResultCallBackInstance;

            // 异步调用
            // 将需传递给异步执行的“方法数据”及“委托”传递给帮助器类
            ThreadWithState tws = new ThreadWithState(
               "This report displays the number {0}.",
               42,
               new ExampleCallback(ResultCallback)          // 4. 把“委托对象”作为实参传递给帮助器类 对象
                                                            //    等待线程调用帮助器内的方法，来启动回调函数
            );

            // 创建线程，通过 public delegate void ThreadStart()委托 构造无参数传递线程
            // 参数传递，由帮助器类辅助完成
            Thread t = new Thread(new ThreadStart(tws.ThreadProc));
            //Thread t = new Thread(tws.ThreadProc);
            t.IsBackground = true;
            t.Start();

            Console.ReadKey();
        }

        /// <summary>
        /// 2. 建立“实例方法”
        /// 异步执行完，做出响应的回调函数实例函数
        /// </summary>
        /// <param name="lineCount"></param>
        static public void ResultCallBackInstance(int lineCount)
        {
            Console.WriteLine($"LineCount = {lineCount}");
        }
    }

    /// <summary>
    /// 帮助器类
    /// </summary>
    public class ThreadWithState
    {
        private string boilerplate;
        private int value;
        private ExampleCallback callback;

        /// <summary>
        /// 帮助器类构造函数，用于接收参数
        /// </summary>
        /// <param name="text">接收boilerplate，模板</param>
        /// <param name="number">接收value, 数据</param>
        /// <param name="callbackDelegate">接收回调函数</param>
        public ThreadWithState(string text, int number,
            ExampleCallback callbackDelegate)
        {
            boilerplate = text;
            value = number;
            callback = callbackDelegate;
        }

        /// <summary>
        /// 在帮助器类中，线程调用的方法，用于处理传递过来的参数，以及最后调用回调函数
        /// </summary>
        public void ThreadProc()
        {
            // 执行异步操作内容
            Console.WriteLine(boilerplate, value);

            // 异步执行完时调用回调函数
            //callback?.Invoke(1);
            if (callback != null)
                callback(1);
        }
    }
}
