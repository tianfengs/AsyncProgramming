/// <summary>
/// CustomerOrderablePartitioner.cs
/// </summary>
namespace AsyncParallel5_3
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    class AsyncParallel5_3
    {
        static void Main(string[] args)
        {
            SingleElementOrderablePartitioner_Test.Test();

            Console.ReadKey();
        }
    }

    class SingleElementOrderablePartitioner<T> : OrderablePartitioner<T>
    {
        IEnumerable<T> m_referenceEnumerable;
        private class Shared<U>
        {
            internal U Value;
            public Shared(U item)
            {
                Value = item;
            }
        }

        public SingleElementOrderablePartitioner(IEnumerable<T> enumerable)
            : base(true, true, true)
        {
            if (enumerable == null)
                throw new ArgumentException("enumerable");
            m_referenceEnumerable = enumerable;
        }

        public override IList<IEnumerator<KeyValuePair<long, T>>> GetOrderablePartitions(int partitionCount)
        {
            if (partitionCount < 1)
                throw new ArgumentOutOfRangeException("partitionCount");
            List<IEnumerator<KeyValuePair<long, T>>> list = new List<IEnumerator<KeyValuePair<long, T>>>();
            var dynamicPartitions = new InternalEnumerable(m_referenceEnumerable.GetEnumerator(), true);
            for (int i = 0; i < partitionCount; i++)
                list.Add(dynamicPartitions.GetEnumerator());
            return list;
        }

        public override IEnumerable<KeyValuePair<long, T>> GetOrderableDynamicPartitions()
        {
            return new InternalEnumerable(m_referenceEnumerable.GetEnumerator(), false);
        }

        public override bool SupportsDynamicPartitions
        {
            get { return true; }
        }

        private class InternalEnumerable : IEnumerable<KeyValuePair<long, T>>, IDisposable
        {
            IEnumerator<T> m_reader;
            bool m_disposed = false;
            Shared<long> m_index = null;

            int m_activeEnumerators;
            bool m_downcountEnumerator;

            /// <param name="downcountEnumerators">
            /// true：指代动态分区，
            /// false：指代静态分区，每次获取一个静态分区时递增一个分区激活数
            /// ，在资源释放的时候只有分区激活数为0时才释放集合，否则递减分区激活数
            /// </param>
            public InternalEnumerable(IEnumerator<T> reader, bool downcountEnumerators)
            {
                m_reader = reader;
                m_index = new Shared<long>(0);
                m_activeEnumerators = 0;
                m_downcountEnumerator = downcountEnumerators;
            }

            public IEnumerator<KeyValuePair<long, T>> GetEnumerator()
            {
                if (m_disposed)
                    throw new ObjectDisposedException("InternalEnumerable: Can't call GetEnumerator() after disposing");

                if (m_downcountEnumerator)
                    Interlocked.Increment(ref m_activeEnumerators);
                return new InternalEnumerator(m_reader, this, m_index);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<KeyValuePair<long, T>>)this).GetEnumerator();
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
                if (m_downcountEnumerator)
                {
                    if (Interlocked.Decrement(ref m_activeEnumerators) == 0)
                    {
                        m_reader.Dispose();
                    }
                }
            }
        }

        private class InternalEnumerator : IEnumerator<KeyValuePair<long, T>>
        {
            KeyValuePair<long, T> m_current;
            IEnumerator<T> m_source;
            InternalEnumerable m_controllingEnumerable;
            Shared<long> m_index = null;
            bool m_disposed = false;

            public InternalEnumerator(IEnumerator<T> source
                , InternalEnumerable controllingEnumerable, Shared<long> index)
            {
                m_source = source;
                m_current = default(KeyValuePair<long, T>);
                m_controllingEnumerable = controllingEnumerable;
                m_index = index;
            }

            object IEnumerator.Current
            {
                get { return m_current; }
            }

            KeyValuePair<long, T> IEnumerator<KeyValuePair<long, T>>.Current
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
                    if (rval)
                    {
                        m_current = new KeyValuePair<long, T>(m_index.Value, m_source.Current);
                        m_index.Value = m_index.Value + 1;
                    }
                    else
                        m_current = default(KeyValuePair<long, T>);
                }
                return rval;
            }

            void IDisposable.Dispose()
            {
                if (!m_disposed)
                {
                    m_controllingEnumerable.DisposeEnumerator();
                    m_disposed = true;
                }
            }
        }

    }

    public class SingleElementOrderablePartitioner_Test
    {
        public static void Test()
        {
            var someCollection = new string[] { "four", "score", "and", "twenty", "years", "ago" };
            var someOrderablePartitioner = new SingleElementOrderablePartitioner<string>(someCollection);

            Console.WriteLine("使用Parallel.ForEach");
            Parallel.ForEach(someOrderablePartitioner, (item, state, index) =>
            {
                Console.WriteLine("ForEach: item = {0}, index = {1}, thread id = {2}", item, index, Thread.CurrentThread.ManagedThreadId);
            });

            Console.WriteLine("使用Parallel.Invoke");
            var staticPartitioner = someOrderablePartitioner.GetOrderablePartitions(2);
            int partitionerListIndex = 0;
            Action staticAction = () =>
            {
                int myIndex = Interlocked.Increment(ref partitionerListIndex) - 1;
                var enumerator = staticPartitioner[myIndex];
                while (enumerator.MoveNext())
                    Console.WriteLine("Static partitioning: item = {0}, index = {1}, thread id = {2}",
                        enumerator.Current.Value, enumerator.Current.Key, Thread.CurrentThread.ManagedThreadId);
                enumerator.Dispose();
            };
            Parallel.Invoke(staticAction, staticAction);
        }
    }
}
