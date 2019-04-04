using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// MyDynamicOrderablePartitioner.cs
/// </summary>
namespace AsyncParallel5_4
{
    class AsyncParallel5_4
    {
        static void Main(string[] args)
        {
            OrderableListPartitioner_Test.Test();

            Console.ReadKey();
        }
    }

    /// <summary>
    /// 实现动态数量的分区程序
    /// 这是按区块分区的示例，其中每个区块都由一个元素组成。  通过一次提供多个元素，
    /// 您可以减少锁争用，并在理论上实现更快的性能。  但是，有时较大的区块可能需要
    /// 额外的负载平衡逻辑才能使所有线程在工作完成之前保持忙碌。 
    /// </summary>
    public class OrderableListPartitioner<TSource> : OrderablePartitioner<TSource>
    {
        private readonly IList<TSource> m_input;
        public OrderableListPartitioner(IList<TSource> input)
            : base(true, false, true)
        {
            m_input = input;
        }

        public override bool SupportsDynamicPartitions
        {
            get
            {
                return true;
            }
        }

        public override IList<IEnumerator<KeyValuePair<long, TSource>>> GetOrderablePartitions(int partitionCount)
        {
            var dynamicPartitions = GetOrderableDynamicPartitions();
            var partitions = new IEnumerator<KeyValuePair<long, TSource>>[partitionCount];
            for (int i = 0; i < partitionCount; i++)
            {
                partitions[i] = dynamicPartitions.GetEnumerator();
            }
            return partitions;
        }

        public override IEnumerable<KeyValuePair<long, TSource>> GetOrderableDynamicPartitions()
        {
            return new ListDynamicPartitions(m_input);
        }

        private class ListDynamicPartitions : IEnumerable<KeyValuePair<long, TSource>>
        {
            private IList<TSource> m_input;
            private int m_pos = 0;

            internal ListDynamicPartitions(IList<TSource> input)
            {
                m_input = input;
            }

            public IEnumerator<KeyValuePair<long, TSource>> GetEnumerator()
            {
                while (true)
                {
                    // 由于使用到公共资源只有m_pos值类型索引，所以只需要保证m_pos访问的原子性
                    int elemIndex = Interlocked.Increment(ref m_pos) - 1;
                    if (elemIndex >= m_input.Count)
                    {
                        yield break;
                    }
                    yield return new KeyValuePair<long, TSource>(elemIndex, m_input[elemIndex]);
                }
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<KeyValuePair<long, TSource>>)this).GetEnumerator();
            }
        }
    }

    /// <summary>
    /// 动态分区测试
    /// </summary>
    public class OrderableListPartitioner_Test
    {
        public static void Test()
        {
            var nums = Enumerable.Range(0, 10000).ToArray();
            OrderableListPartitioner<int> partitioner = new OrderableListPartitioner<int>(nums);

            // Use with Parallel.ForEach

            Parallel.ForEach(partitioner, (i) => Console.WriteLine(i));

            //Use with PLINQ
            var query = from num in partitioner.AsParallel()
                        where num % 2 == 0
                        select num;
            foreach (var v in query)
                Console.WriteLine(v);

        }
    }

}
