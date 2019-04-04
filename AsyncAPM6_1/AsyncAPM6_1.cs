using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncAPM6_1
{
    class AsyncAPM6_1
    {
        static void Main(string[] args)
        {
            //// 1. 阻塞方式，调用异步操作
            //Test_EndInvoke();
            //// 2. 非阻塞方式，调用异步操作
            //Test_NoStop();

            // APM 阻塞式异步响应
            Calculate_For_Break.Test();

            // APM 回调式异步响应
            Calculate_For_Callback.Test();

            // 使用委托进行异步调用
            Calculate_For_Delegate.Test();

            Console.ReadKey();
        }

        #region 使用CalculateAsyncResult
        /// <summary>
        /// 1. 阻塞方式，调用异步操作
        /// </summary>
        public static void Test_EndInvoke()
        {
            CalculateLib cal = new CalculateLib();

            IAsyncResult calculateResult = cal.BeginCalculate(123, 456, null, null);

            string resultStr = string.Empty;
            int result = cal.EndCalculate(ref resultStr, calculateResult);

            Console.WriteLine($"123*456={result}");
        }

        /// <summary>
        /// 2. 非阻塞方式，调用异步操作
        /// </summary>
        public static void Test_NoStop()
        {
            CalculateLib cal = new CalculateLib();

            IAsyncResult calculateResult = cal.BeginCalculate(123, 456, new RefAsyncCallback(AfterCallback), cal);
        }

        private static void AfterCallback(ref string resultStr, IAsyncResult ar)
        {
            CalculateLib cal = (CalculateLib)ar.AsyncState;     

            cal.EndCalculate(ref resultStr, ar);

            Console.WriteLine($"结果是：{resultStr}");
        }

        #endregion
    }

    #region APM 阻塞式异步响应、APM 回调式异步响应、使用委托进行异步调用
    public delegate int AsyncInvokeDel(int num1, int num2, ref string resultStr);

    /// <summary>
    /// APM 阻塞式异步响应
    /// </summary>
    public class Calculate_For_Break
    {
        public static void Test()
        {
            CalculateLib cal = new CalculateLib();

            // 基于IAsyncResult构造一个异步API   （回调参数和状态对象都传递null）
            IAsyncResult calculateResult = cal.BeginCalculate(123, 456, null, null);
            // 执行异步调用后，若我们需要控制后续执行代码在异步操作执行完之后执行，可通过下面三种方式阻止其他工作：
            // 1、IAsyncResult 的 AsyncWaitHandle 属性，带异步操作完成时获得信号。
            // 2、通过 IAsyncResult 的 IsCompleted 属性进行轮询。通过轮询还可实现进度条功能。
            // 3、调用异步操作的 End*** 方法。
            // ***********************************************************
            // 1、calculateResult.AsyncWaitHandle.WaitOne();
            // 2、while (calculateResult.IsCompleted) { Thread.Sleep(1000); }
            // 3、
            string resultStr = string.Empty;
            int result = cal.EndCalculate(ref resultStr, calculateResult);

            Console.WriteLine($"123*456={result}");
        }
    }

    /// <summary>
    /// APM 回调式异步响应
    /// </summary>
    public class Calculate_For_Callback
    {
        public static void Test()
        {
            CalculateLib cal = new CalculateLib();

            // 基于IAsyncResult构造一个异步API
            IAsyncResult calculateResult = cal.BeginCalculate(123, 456, AfterCallback, cal);
        }

        /// <summary>
        /// 异步操作完成后做出响应
        /// </summary>
        private static void AfterCallback(ref string resultStr, IAsyncResult ar)
        {
            // 执行异步调用后，若我们不需要阻止后续代码的执行，那么我们可以把异步执行操作后的响应放到回调中进行。
            CalculateLib cal = ar.AsyncState as CalculateLib;
            cal.EndCalculate(ref resultStr, ar);
            // 再根据resultStr值做逻辑。

            Console.WriteLine($"结果是：{resultStr}");
        }
    }

    /// <summary>
    /// 使用委托进行异步调用
    /// </summary>
    public class Calculate_For_Delegate
    {
        public static void Test()
        {
            CalculateLib cal = new CalculateLib();

            // 使用委托进行同步或异步调用
            AsyncInvokeDel calculateAction = cal.Calculate;
            string resultStrAction = string.Empty;
            // int result1 = calculateAction.Invoke(123, 456);
            IAsyncResult calculateResult1 = calculateAction.BeginInvoke(123, 456, ref resultStrAction, null, null);
            int result1 = calculateAction.EndInvoke(ref resultStrAction, calculateResult1);

            Console.WriteLine($"123*456={result1}");
        }
    }
    #endregion


    #region 扩展IAsyncResult接口
    // 带ref参数的自定义委托
    public delegate void RefAsyncCallback(ref string resultStr, IAsyncResult ar);

    public class CalculateAsyncResult : IAsyncResult
    {
        private int _calcNum1;
        private int _calcNum2;
        private RefAsyncCallback _userCallback;

        public CalculateAsyncResult(int num1, int num2, RefAsyncCallback userCallback, object asyncState)
        {
            this._calcNum1 = num1;
            this._calcNum2 = num2;
            this._userCallback = userCallback;
            this.AsyncState = asyncState;
            // 异步执行操作
            ThreadPool.QueueUserWorkItem((obj) =>
            { AsyncCalculate(obj); }, this);
        }

        #region IAsyncResult接口
        public bool IsCompleted { get; set; }

        private ManualResetEvent _asyncWaitHandle;
        public WaitHandle AsyncWaitHandle
        {
            get
            {
                if (_asyncWaitHandle == null)
                {
                    ManualResetEvent event2 = new ManualResetEvent(false);
                    Interlocked.CompareExchange(ref _asyncWaitHandle, event2, null);
                }
                return _asyncWaitHandle;
            }
        }

        public object AsyncState { get; }

        public bool CompletedSynchronously { get; set; }
        #endregion

        /// <summary>
        /// 存储最后结果值
        /// </summary>
        public int FinnalyResult { get; set; }
        /// &lt;summary&gt;
        /// End方法只应调用一次，超过一次报错
        /// &lt;/summary&gt;
        public int EndCallCount = 0;
        /// <summary>
        /// ref参数
        /// </summary>
        public string ResultStr;

        /// <summary>
        /// 异步进行耗时计算
        /// </summary>
        /// <param name="obj">CalculateAsyncResult实例本身</param>
        private static void AsyncCalculate(object obj)
        {
            CalculateAsyncResult asyncResult = obj as CalculateAsyncResult;
            Thread.SpinWait(1000);
            asyncResult.FinnalyResult = asyncResult._calcNum1 * asyncResult._calcNum2;
            asyncResult.ResultStr = asyncResult.FinnalyResult.ToString();

            // 是否同步完成
            asyncResult.CompletedSynchronously = false;
            asyncResult.IsCompleted = true;
            ((ManualResetEvent)asyncResult.AsyncWaitHandle).Set();
            asyncResult._userCallback?.Invoke(ref asyncResult.ResultStr, asyncResult);
        }
    }

    public class CalculateLib
    {
        public IAsyncResult BeginCalculate(int num1,int num2,RefAsyncCallback userCallback,object asyncState)
        {
            CalculateAsyncResult result = new CalculateAsyncResult(num1, num2, userCallback, asyncState);
            return result;
        }

        public int EndCalculate(ref string resultStr, IAsyncResult ar)
        {
            CalculateAsyncResult result = ar as CalculateAsyncResult;
            if (Interlocked.CompareExchange(ref result.EndCallCount, 1, 0) == 1)
            {
                throw new Exception("End方法只能调用一次。");
            }
            result.AsyncWaitHandle.WaitOne();

            resultStr = result.ResultStr;

            return result.FinnalyResult;
        }

        public int Calculate(int num1, int num2, ref string resultStr)
        {
            resultStr = (num1 * num2).ToString();
            return num1 * num2;
        }
    }
    #endregion
}
