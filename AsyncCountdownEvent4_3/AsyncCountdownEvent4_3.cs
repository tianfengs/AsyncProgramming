using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncCountdownEvent4_3
{
    using System.Threading;

    class AsyncCountdownEvent4_3
    {
        static void Main(string[] args)
        {
            CountdownEventTest();

            Console.ReadKey();
        }

        public static void CountdownEventTest()
        {
            Console.WriteLine("初始化CountdownEvent计数为1000");
            CountdownEvent cde = new CountdownEvent(1000);

            Console.WriteLine("增加CountdownEvent计数200至1200");
            cde.AddCount(200);                                          // AddCount()\TryAddCount()增加CurrentCount一个或指定数量信号
            // CurrentCount（当前值）1200允许大于InitialCount（初始值）1000

            Console.WriteLine("Reset()设置CountdownEvent计数1200至InitialCount");
            cde.Reset();                                                // Reset() 将 CurrentCount 重新设置为初始值或指定值 CurrentCount=InitialCount


            Thread thread = new Thread(o =>
              {
                  int i = cde.CurrentCount;                             // CurrentCount 属性标识剩余信号数（和InitialCount属性一起由构造函数初始化）
                  int count = 0;
                  for (int j = 1; j <= i; j++)
                  {
                      //Thread.Sleep(2);
                      if (j == i)
                      {
                          Console.WriteLine("CurrentCount为{0}，所以必须调用Signal(){1}次",count+1,count+1);
                      }
                      count++;
                      cde.Signal();                                     // Signal()向CountdownEvent注册一个或指定数量信号，通知任务完成并且将CurrentCount的值减少一或指定数量
                  }
              });

            thread.Start();

            Console.WriteLine("调用CountdownEvent的Wait()方法");
            cde.Wait();                                                 // Wait()阻止当前线程，直到CurrentCount计数为0
            Console.WriteLine("CountdownEvent计数为0，完成等待");


        }
    }
}
