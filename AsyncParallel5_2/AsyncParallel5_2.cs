using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// CustomerPartitioner.cs
/// 
/// 打算要在 ForEach 方法中使用分区程序
/// 必须支持动态数量的分区，即：
/// - 在Partitioner<TSource> 的派生类中重写 GetDynamicPartitions() 方法和 SupportsDynamicPartitions属性
/// - 在OrderablePartitioner<TSource> 派生类中重写GetOrderableDynamicPartitions() 方法和SupportsDynamicPartitions 属性
/// 分区程序能够在循环执行过程中随时按需为新分区提供枚举器。基本上，每当循环添加一个新并行任务时，它都会为该任务请求一个新分区。动态数量的分区程序在本质上也是负载平衡的。
/// Partitioner<TSource>
/// 
/// </summary>
namespace AsyncParallel5_2
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Threading;

    class AsyncParallel5_2
    {
        static void Main(string[] args)
        {
            SingleElementPartitioner_Test.Test();

            Console.ReadKey();
        }
    }

    /// <summary>
    /// Partitioner<TSource> 的派生类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingleElementPartitioner<T> : Partitioner<T>
    {
        /// <summary>
        /// 重载虚函数
        /// </summary>
        /// <param name="partitionCount"></param>
        /// <returns></returns>
        public override IList<IEnumerator<T>> GetPartitions(int partitionCount)
        {
            if (partitionCount < 1)
                throw new ArgumentOutOfRangeException("partitionCount");
            List<IEnumerator<T>> list = new List<IEnumerator<T>>(partitionCount);

            var dynamicPartitions = new InternalEnumerable(m_referenceEnumerable.GetEnumerator(), true);
            for (int i = 0; i < partitionCount; i++)
                list.Add(dynamicPartitions.GetEnumerator());
            return list;
        }

        IEnumerable<T> m_referenceEnumerable;

        public SingleElementPartitioner(IEnumerable<T> enumerable)
        {
            m_referenceEnumerable = enumerable ?? throw new ArgumentNullException("enumerable");
        }

        // 支持动态数量的分区，重载函数1
        public override bool SupportsDynamicPartitions
        {
            get { return true; }
        }

        // 支持动态数量的分区，重载函数2
        public override IEnumerable<T> GetDynamicPartitions()
        {
            return new InternalEnumerable(m_referenceEnumerable.GetEnumerator(), false);
        }

        private class InternalEnumerable : IEnumerable<T>, IDisposable
        {
            IEnumerator<T> m_reader;
            bool m_disposed = false;

            // 记录激活的Enumerator个数，用于释放操作
            int m_activeEnumerators;
            bool m_downcountEnumerators;
            
            /// <summary>
            /// 
            /// </summary>
            /// <param name="reader"></param>
            /// <param name="downcountEnumerators">
            ///     true：指代动态分区，false：指代静态分区，
            ///     每次获取一个静态分区时递增一个分区激活数，在资源释放的时候只有分区激活数为0时才释放集合，
            ///     否则递减分区激活数
            /// </param>
            public InternalEnumerable(IEnumerator<T> reader, bool downcountEnumerators)
            {
                m_reader = reader;
                m_activeEnumerators = 0;
                m_downcountEnumerators = downcountEnumerators;
            }

            public IEnumerator<T> GetEnumerator()
            {
                if (m_disposed)
                    throw new ObjectDisposedException("InternalEnumerable: Can't call GetEnumerator() after disposing");

                if (m_downcountEnumerators)
                    Interlocked.Increment(ref m_activeEnumerators);

                return new InternalEnumerator(m_reader, this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<T>)this).GetEnumerator();
            }

            public void Dispose()
            {
                if (!m_disposed)
                {
                    m_reader.Dispose();
                    m_disposed = true;
                }
            }

            // 由 InternalEnumerator 调用的释放
            public void DisposeEnumerator()
            {
                if (m_downcountEnumerators)
                {
                    if (Interlocked.Decrement(ref m_activeEnumerators) == 0)
                    {
                        m_reader.Dispose();
                    }
                }
            }
        }

        private class InternalEnumerator : IEnumerator<T>
        {
            T m_current;
            IEnumerator<T> m_source;
            InternalEnumerable m_conteollingEnumerable;
            bool m_disposed = false;

            public InternalEnumerator(IEnumerator<T> source, InternalEnumerable controllingEnumerable)
            {
                m_source = source;
                m_current = default(T);
                m_conteollingEnumerable = controllingEnumerable;
            }

            object IEnumerator.Current
            {
                get { return m_current; }
            }

            T IEnumerator<T>.Current
            {
                get { return m_current; }
            }

            void IEnumerator.Reset()
            {
                throw new NotSupportedException("Reset() not supported");
            }

            bool IEnumerator.MoveNext()
            {
                bool rval = false;
                // Parallel.ForEach内部在获取动态分区时，先取缓存中的enumerator，若没有才会获取动态分区
                // （即动态分区只会获取一次）所以集合源是同一个，访问时需要进行加锁
                lock (m_source)
                {
                    rval = m_source.MoveNext();
                    m_current = rval ? m_source.Current : default(T);
                }
                return rval;
            }

            void IDisposable.Dispose()
            {
                if (!m_disposed)
                {
                    m_conteollingEnumerable.DisposeEnumerator();
                    m_disposed = true;
                }
            }
        }

    }

    public class SingleElementPartitioner_Test
    {
        public static void Test()
        {
            String[] collection = new string[]{"red", "orange", "yellow", "green", "blue", "indigo",
                "violet", "black", "white", "grey"};
            SingleElementPartitioner<string> myPart = new SingleElementPartitioner<string>(collection);

            Console.WriteLine("示例：Parallel.ForEach");
            Parallel.ForEach(myPart, item =>
            {
                Console.WriteLine("  item = {0}, thread id = {1}"
                    , item, Thread.CurrentThread.ManagedThreadId);
            }
            );


            Console.WriteLine("静态数量的分区：2个分区，2个任务");
            var staticPartitions = myPart.GetPartitions(2);
            int index = 0;
            Action staticAction = () =>
            {
                int myIndex = Interlocked.Increment(ref index) - 1;
                var myItems = staticPartitions[myIndex];
                int id = Thread.CurrentThread.ManagedThreadId;

                while (myItems.MoveNext())
                {
                    // 保证多个线程有机会执行
                    Thread.Sleep(50);
                    Console.WriteLine("  item = {0}, thread id = {1}"
                        , myItems.Current, Thread.CurrentThread.ManagedThreadId);

                }
                myItems.Dispose();
            };
            Parallel.Invoke(staticAction, staticAction);


            Console.WriteLine("动态分区： 3个任务 ");
            var dynamicPartitions = myPart.GetDynamicPartitions();
            Action dynamicAction = () =>
            {
                var enumerator = dynamicPartitions.GetEnumerator();
                int id = Thread.CurrentThread.ManagedThreadId;

                while (enumerator.MoveNext())
                {
                    Thread.Sleep(50);
                    Console.WriteLine("  item = {0}, thread id = {1}", enumerator.Current, id);
                }
            };
            Parallel.Invoke(dynamicAction, dynamicAction, dynamicAction);
        }
    }
}
