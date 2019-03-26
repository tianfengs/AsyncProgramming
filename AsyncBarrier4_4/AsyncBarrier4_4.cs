using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncBarrier4_4
{
    using System.Threading;

    class AsyncBarrier4_4
    {
        static void Main(string[] args)
        {
            BarrierTest();

            Console.ReadKey();
        }

        private static int m_count = 3;
        private static int m_curCount = 0;
        private static Barrier pauseBarr = new Barrier(2);  // 栅栏1：分割两个阶段

        public static void BarrierTest()
        {
            Thread.VolatileWrite(ref m_curCount, 0);
            Barrier barr = new Barrier(m_count, new Action<Barrier>(Write_PhaseNumber));    // 栅栏2：拦截不同阶段的线程，使同一阶段的线程都结束了才进入下一阶段

            Console.WriteLine("Barrier开始第一阶段");
            AsyncSignalAndWait(barr, m_count);

            pauseBarr.SignalAndWait();

            Console.WriteLine("Barrier开始第二阶段");
            Thread.VolatileWrite(ref m_curCount, 0);
            AsyncSignalAndWait(barr, m_count);

            // 暂停等待 barr 第二阶段执行完毕
            pauseBarr.SignalAndWait();
            pauseBarr.Dispose();
            barr.Dispose();
            Console.WriteLine("Barrier两个阶段执行完毕");
        }

        static Random random = new Random();

        /// <summary>
        /// 每个阶段都建立三个线程，并用统一的Barrier来控制都完成后进入后期阶段、和下一阶段
        /// </summary>
        /// <param name="barr"></param>
        /// <param name="m_count"></param>
        private static void AsyncSignalAndWait(Barrier barr, int m_count)
        {
            for (int i = 1; i <= m_count; i++)
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    Thread.Sleep(random.Next(500));
                    Interlocked.Increment(ref m_curCount);
                    Console.WriteLine($"  Thread{m_curCount}已经到达barr");
                    barr.SignalAndWait();
                }
                );
            }
        }

        /// <summary>
        /// 所有线程都到达栅栏后，屏障进行的操作，是Action<Barrier>()委托的实际方法
        /// </summary>
        /// <param name="obj"></param>
        private static void Write_PhaseNumber(Barrier obj)
        {
            Console.WriteLine(String.Format("Barrier调用完{0}次SignalAndWait()", m_curCount));
            Console.WriteLine("阶段编号为：" + obj.CurrentPhaseNumber);
            Console.WriteLine("ParticipantsRemaining属性值为：" + obj.ParticipantsRemaining);
            Console.WriteLine();
            pauseBarr.SignalAndWait();
        }
    }
}
