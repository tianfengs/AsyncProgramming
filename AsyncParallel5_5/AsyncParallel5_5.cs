using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// MyPartitioner.cs
/// </summary>
namespace AsyncParallel5_5
{
    class AsyncParallel5_5
    {
        static void Main(string[] args)
        {
            MyPartitioner_Test.Test();

            Console.ReadKey();
        }
    }

    /// <summary>
    /// 静态数量的分区的分区程序 
    /// </summary>
    public class MyPartitioner : Partitioner<int>
    {
        int[] source;
        double rateOfIncrease = 0;

        public MyPartitioner(int[] source, double rate)
        {
            this.source = source;
            rateOfIncrease = rate;
        }

        public override IList<IEnumerator<int>> GetPartitions(int partitionCount)
        {
            List<IEnumerator<int>> _list = new List<IEnumerator<int>>();
            int end = 0;
            int start = 0;
            int[] nums = CalculatePartitions(partitionCount, source.Length);
            for (int i = 0; i < nums.Length; i++)
            {
                start = nums[i];
                if (i < nums.Length - 1)
                    end = nums[i + 1];
                else
                    end = source.Length;
                _list.Add(GetItemsForPartition(start, end));
                Console.WriteLine("start = {0} ; end ={1} ", start.ToString(), end.ToString());
            }
            return (IList<IEnumerator<int>>)_list;
        }

        private int[] CalculatePartitions(int partitionCount, int sourceLength)
        {
            int[] partitionLimits = new int[partitionCount];
            partitionLimits[0] = 0;
            double totalWork = sourceLength * (sourceLength * rateOfIncrease);
            totalWork /= 2;
            double partitionArea = totalWork / partitionCount;
            for (int i = 1; i < partitionLimits.Length; i++)
            {
                double area = partitionArea * i;
                partitionLimits[i] = (int)Math.Floor(Math.Sqrt((2 * area) / rateOfIncrease));
            }

            return partitionLimits;
        }

        /// <summary>
        /// start <= 范围 < end
        /// </summary>
        private IEnumerator<int> GetItemsForPartition(int start, int end)
        {
            Console.WriteLine("called on thread {0}", Thread.CurrentThread.ManagedThreadId);
            for (int i = start; i < end; i++)
                yield return source[i];
        }
    }

    public class MyPartitioner_Test
    {
        public static void Test()
        {
            var source = Enumerable.Range(0, 10000).ToArray();
            Stopwatch sw = new Stopwatch();
            MyPartitioner partitioner = new MyPartitioner(source, .5);
            var query = from n in partitioner.AsParallel()
                        select ProcessData(n);
            foreach (var v in query) { }
            Console.WriteLine("Processing time with custom partitioner {0}", sw.ElapsedMilliseconds);

            var source2 = Enumerable.Range(0, 10000).ToArray();

            sw = Stopwatch.StartNew();


            var query2 = from n in source2.AsParallel()
                         select ProcessData(n);

            foreach (var v in query2) { }
            Console.WriteLine("Processing time with default partitioner {0}", sw.ElapsedMilliseconds);

        }

        private static int ProcessData(int i)
        {
            Thread.SpinWait(i * 1000);
            return i;
        }
    }

}
