using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;using System.Threading.Tasks;

/// <summary>
/// 设置和检索线程数据:
/// 线程使用托管线程本地存储区 (TLS，Thread-Local Storage)来存储线程特定的数据，托管 TLS 中的数据都是线程和应用程序域组合所独有的，其他任何线程（即使是子线程）都无法获取这些数据。
/// </summary>
namespace AsyncBase1_3
{
    using System.Threading;

    class AsyncBase1_3TLS4Data
    {
        static void Main(string[] args)
        {
            Console.WriteLine("数据槽：");
            TLS4DataSlot();

            Console.WriteLine();

            Console.WriteLine("线程相关静态字段：");
            TLS4StaticField();

            Console.ReadKey();
        }

        /// <summary>
        /// 数据槽  的使用示例
        /// 托管 TLS 中的数据都是线程和应用程序域组合所独有的
        /// </summary>
        private static void TLS4DataSlot()
        {
            LocalDataStoreSlot slot = Thread.AllocateNamedDataSlot("Name");     // 创建数据槽
            Console.WriteLine(String.Format("ID为{0}的线程，命名为\"Name\"的数据槽，开始设置数据。", Thread.CurrentThread.ManagedThreadId));
            Thread.SetData(slot, "小丽");                                        // 为数据槽设置数据
            Console.WriteLine(String.Format("ID为{0}的线程，命名为\"Name\"的数据槽，数据是\"{1}\"。"
                             , Thread.CurrentThread.ManagedThreadId, Thread.GetData(slot)));

            Thread newThread = new Thread(                                      // 创建新线程，为新线程创建自己的数据槽，并设置数据槽数据
                () =>
                {
                    LocalDataStoreSlot storeSlot = Thread.GetNamedDataSlot("Name");
                    Console.WriteLine(String.Format("ID为{0}的线程，命名为\"Name\"的数据槽，在新线程为其设置数据 前 为\"{1}\"。"
                                     , Thread.CurrentThread.ManagedThreadId, Thread.GetData(storeSlot)));
                    Console.WriteLine(String.Format("ID为{0}的线程，命名为\"Name\"的数据槽，开始设置数据。", Thread.CurrentThread.ManagedThreadId));
                    Thread.SetData(storeSlot, "小红");
                    Console.WriteLine(String.Format("ID为{0}的线程，命名为\"Name\"的数据槽，在新线程为其设置数据 后 为\"{1}\"。"
                                     , Thread.CurrentThread.ManagedThreadId, Thread.GetData(storeSlot)));

            // 命名数据槽中分配的数据必须用 FreeNamedDataSlot() 释放。未命名的数据槽数据随线程的销毁而释放
            Thread.FreeNamedDataSlot("Name");
                }
            );
            newThread.Start();
            newThread.Join();

            Console.WriteLine(String.Format("执行完新线程后，ID为{0}的线程，命名为\"Name\"的数据槽，在新线程为其设置数据 后 为\"{1}\"。"
                             , Thread.CurrentThread.ManagedThreadId, Thread.GetData(slot)));
        }

        [ThreadStatic]
        static string name = String.Empty;
        /// <summary>
        /// 线程相关静态字段  的使用示例
        /// 某个线程和应用程序域组合所独有的（即不是共享的）
        /// </summary>
        private static void TLS4StaticField()
        {
            Console.WriteLine(String.Format("ID为{0}的线程，开始为name静态字段设置数据。", Thread.CurrentThread.ManagedThreadId));
            name = "小丽";
            Console.WriteLine(String.Format("ID为{0}的线程，name静态字段数据为\"{1}\"。", Thread.CurrentThread.ManagedThreadId, name));

            Thread newThread = new Thread(
                () =>
                {
                    Console.WriteLine(String.Format("ID为{0}的线程，为name静态字段设置数据 前 为\"{1}\"。", Thread.CurrentThread.ManagedThreadId, name));
                    Console.WriteLine(String.Format("ID为{0}的线程，开始为name静态字段设置数据。", Thread.CurrentThread.ManagedThreadId));
                    name = "小红";
                    Console.WriteLine(String.Format("ID为{0}的线程，为name静态字段设置数据 后 为\"{1}\"。", Thread.CurrentThread.ManagedThreadId, name));
                }
            );
            newThread.Start();
            newThread.Join();

            Console.WriteLine(String.Format("执行完新线程后，ID为{0}的线程，name静态字段数据为\"{1}\"。", Thread.CurrentThread.ManagedThreadId, name));
        }
    }
}
