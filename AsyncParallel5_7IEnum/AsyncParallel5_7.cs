using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsyncParallel5_7IEnum
{
    class AsyncParallel5_7
    {
        static void Main(string[] args)
        {
            // 1. Yield方法实现迭代器
            ConstArrayListYield constArrayList = new ConstArrayListYield();

            //// 2. IEnumerator接口实现迭代器
            //ConstArrayList constArrayList = new ConstArrayList();

            foreach (int item in constArrayList)
            {
                Console.WriteLine(item);
            }

            Console.ReadKey();
        }
    }

    // 1. yield方法实现迭代器
    class ConstArrayListYield : IEnumerable
    {
        public List<int> constItems = new List<int> { 1, 2, 3, 4, 5 };
        public IEnumerator GetEnumerator()
        {
            for (int i = 0; i < constItems.Count; ++i)
            {
                yield return constItems[i];
            }

        }
    }

    // 2. IEnumerator接口方法实现迭代器
    //  2.1 一个常量的数组，用于foreach遍历
    class ConstArrayList : IEnumerable
    {
        public int[] constItems = new int[] { 1, 2, 3, 4, 5 };
        public IEnumerator GetEnumerator()
        {
            return new ConstArrayListEnumeratorSimple(this);
        }
    }

    //  2.2 这个常量数组的迭代器
    class ConstArrayListEnumeratorSimple : IEnumerator
    {
        private ConstArrayList constArrayList;
        int index;
        int currentElement;

        public ConstArrayListEnumeratorSimple(ConstArrayList constArrayList)
        {
            this.constArrayList = constArrayList;
            index = -1;
        }

        public object Current
        {
            get
            {
                return currentElement;
            }
        }

        public bool MoveNext()
        {
            if (index < constArrayList.constItems.Length - 1)
            {
                currentElement = constArrayList.constItems[++index];
                return true;
            }
            else
            {
                currentElement = -1;
                return false;
            }
        }

        public void Reset()
        {
            index = -1;
        }
    }
}
