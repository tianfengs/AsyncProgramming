using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsyncParallel5_1For
{
    using System.Threading;
    using System.Threading.Tasks;

    class AsyncParallel5_1For
    {
        static void Main(string[] args)
        {
            Parallel_For_Local_Test();

            Console.ReadKey();
        }

        public static void Parallel_For_Local_Test()
        {
            int[] nums = Enumerable.Range(0, 100).ToArray<int>();   // 生成指定范围内的整数序列

            long total = 0;

            // For<TLocal>(Int32, Int32, Func<TLocal>, Func<Int32,ParallelLoopState,TLocal,TLocal>, Action<TLocal>)
            ParallelLoopResult result = Parallel.For<long>(0, nums.Length,  // 前两个参数都指定起始迭代值和结束迭代值
                () => { return 0; },    // Func<TLocal> localInit
                                        // 在此示例中，表达式 () => 0（在 Visual Basic 中为 Function() 0）将线程本地变量初始化为零。 
                                        // 如果泛型类型参数是引用类型或用户定义的值类型，表达式将如下所示：() => new MyClass()  
                (j, loop, subtotal) =>  // Func<Int32,ParallelLoopState,TLocal,TLocal> body
                                        // 第一个参数是针对循环的该次迭代的循环计数器的值。
                                        // 第二个参数是可用于中断循环的 ParallelLoopState 对象；此对象由 Parallel 类提供给循环的每个匹配项。
                                        // 第三个参数是线程本地变量。即For<long>中的long定义的变量
                                        // 最后一个参数是返回类型。该变量名为 subtotal 并且由 lambda 表达式返回。 返回值用于在循环的每个后续迭代上初始化 subtotal。 
                                        //      你也可以将最后一个参数看作传递到每个迭代，然后在最后一个迭代完成时传递到 localFinally 委托的值。

                {
                    Thread.SpinWait(200);

                    Console.WriteLine($"当前线程ID为：{Thread.CurrentThread.ManagedThreadId}, j为{j.ToString()}, subtotal为：{subtotal.ToString()}。");

                    if (j == 23)
                    {
                        //loop.Break();       // loop是 ParallelLoopState 对象
                        loop.Stop();
                    }
                    if (j > loop.LowestBreakIteration)//获取调用ParallelLoopState.Break()后最低循环迭代次数
                    {
                        Thread.Sleep(4000);
                        Console.WriteLine("j为{0},等待4s种，用于判断已开启且大于阻断迭代是否会运行完。", j.ToString());
                    }
                    Console.WriteLine("j为{0},LowestBreakIteration为：{1}", j.ToString(), loop.LowestBreakIteration);
                    subtotal += nums[j];
                    return subtotal;
                },
                (finalResult) => Interlocked.Add(ref total, finalResult)    // Action<TLocal> localFinally
                                                                            // 定义在特定线程上的所有迭代都完成后，将调用一次的方法。
                                                                            // 此示例中，通过调用 Interlocked.Add 方法，采用线程安全的方式在类范围将值添加到变量。 
                                                                            // 通过使用线程本地变量，我们避免了在循环的每个迭代上写入此类变量。
                );

            Console.WriteLine("total值为：{0}", total.ToString());
            if (result.IsCompleted)
                Console.WriteLine("循环执行完毕");
            else
                Console.WriteLine("{0}"
                    , result.LowestBreakIteration.HasValue ? "调用了Break()阻断循环." : "调用了Stop()终止循环.");
        }
    }
}
