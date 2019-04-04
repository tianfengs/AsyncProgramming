using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 本例演示，线程的执行上下文切换
/// 确保辅助线程使用的是和初始线程相同的“安全设置”、“宿主设置”和“逻辑调用上下文”
/// </summary>
namespace AsyncThreadPoolContextChg2_2
{
    using System.Runtime.Remoting.Messaging;
    using System.Threading;

    class AsyncThreadPoolContextChg2_2
    {
        static void Main(string[] args)
        {
            // 示例：ExecutionContext
            Example_ExecutionContext();

            Console.ReadKey();
        }

        /// <summary>
        /// 示例：ExecutionContext 
        /// 1)	在线程间共享逻辑调用上下文数据（CallContext）。
        /// 2)	为了提升性能，取消\恢复执行上下文的流动。
        /// 3)	在当前线程上的指定执行上下文中运行某个方法。
        /// </summary>
        private static void Example_ExecutionContext()
        {
            // CallContext 是类似于方法调用的线程本地存储区的专用集合对象，并提供对每个逻辑执行线程都唯一的数据槽。
            // 数据槽不在其他逻辑线程上的调用上下文之间共享。
            CallContext.LogicalSetData("Name", "小红");       // 在逻辑调用上下文里存储一个对象
                                                            
            Console.WriteLine("主线程中Name为：{0}", CallContext.LogicalGetData("Name"));     // 显示此上下文

            // 1)   在线程间共享逻辑调用上下文数据（CallContext）。
            Console.WriteLine("1)在线程间共享逻辑调用上下文数据（CallContext）。");
            ThreadPool.QueueUserWorkItem((Object state) =>
            {
                Console.WriteLine("ThreadPool线程中Name为：\"{0}\"", CallContext.LogicalGetData("Name"));
            });
            Thread.Sleep(500);
            Console.WriteLine();

            // 2)   为了提升性能，取消\恢复执行上下文的流动。
            ThreadPool.QueueUserWorkItem((Object state) =>
            {
                Console.WriteLine("ThreadPool线程使用Unsafe异步执行方法来取消执行上下文的流动。Name为：\"{0}\"",
                    CallContext.LogicalGetData("Name"));
            }, null);

            Console.WriteLine("2)为了提升性能，取消/恢复执行上下文的流动。");
            AsyncFlowControl flowControl = ExecutionContext.SuppressFlow();
            ThreadPool.QueueUserWorkItem((Object obj)
                => Console.WriteLine("(取消ExecutionContext流动)ThreadPool线程中Name为：\"{0}\""
                , CallContext.LogicalGetData("Name")));
            Thread.Sleep(500);
            // 恢复，不推荐使用ExecutionContext.RestoreFlow()
            flowControl.Undo();
            ThreadPool.QueueUserWorkItem((Object obj)
                => Console.WriteLine("(恢复ExecutionContext流动)ThreadPool线程中Name为：\"{0}\"",
                CallContext.LogicalGetData("Name")));
            Thread.Sleep(500);
            Console.WriteLine();

            // 3)   在当前线程上的指定执行上下文中运行某个方法。(通过获取调用上下文数据验证)
            Console.WriteLine("3)在当前线程上的指定执行上下文中运行某个方法。(通过获取调用上下文数据验证)");
            // 若使用 Thread.CurrentThread.ExecutionContext 会报“无法应用以下上下文: 跨 AppDomains 封送的上下文、不是通过捕获操作获取的上下文或已作为 Set 调用的参数的上下文。”
            // 若使用Thread.CurrentThread.ExecutionContext.CreateCopy()会报“只能复制新近捕获(ExecutionContext.Capture())的上下文”。 
            ExecutionContext curExecutionContext = ExecutionContext.Capture();      // 使用ExecutionContext.Capture()获取当前执行上下文的一个副本
            ExecutionContext.SuppressFlow();        // 抑制了ExecutionContext执行上下文流动
            ThreadPool.QueueUserWorkItem(
                (Object obj) =>
                {
                    ExecutionContext innerExecutionContext = obj as ExecutionContext;
                    ExecutionContext.Run(innerExecutionContext, (Object state)
                        => Console.WriteLine("ThreadPool线程中Name为：\"{0}\"", CallContext.LogicalGetData("Name")), null);
                }
                , curExecutionContext
             );
        }
    }
}
