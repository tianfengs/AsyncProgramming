using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Async_Await_APM
{
    using System.Runtime.Remoting.Messaging;
    using System.Threading;

    delegate string AsyncMethodCaller();

    public partial class Form1 : Form
    {
        SynchronizationContext sc;

        public Form1()
        {
            InitializeComponent();

        }

        private void btnClick_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Clear();
            btnClick.Enabled = false;
            AsyncMethodCaller caller = new AsyncMethodCaller(TestMethod);
            IAsyncResult result = caller.BeginInvoke(GetResult, null);

            // 捕捉调用线程的同步上下文派生对象
            sc = SynchronizationContext.Current;
        }

        // 回调方法
        private void GetResult(IAsyncResult ar)
        {
            AsyncMethodCaller caller = (AsyncMethodCaller)((AsyncResult)ar).AsyncDelegate;
            // 调用EndInvoke去等待异步调用完成并且获得返回值
            // 如果异步调用尚未完成，则 EndInvoke 会一直阻止调用线程，直到异步调用完成
            string resultvalue = caller.EndInvoke(ar);
            //sc.Post(ShowState,resultvalue);
            //richTextBox1.Invoke(showStateCallback, resultvalue);  // 没有实现用方法showStateCallback
            richTextBox1.Invoke(new Action<string>((state)=>
            {
                richTextBox1.Text = state.ToString();
                btnClick.Enabled = true;
            }),resultvalue);
        }

        // 显示结果到richTextBox
        private void ShowState(object state)
        {
            richTextBox1.Text = state.ToString();
            btnClick.Enabled = true;
        }

        // 同步方法
        private string TestMethod()
        {
            // 模拟做一些耗时的操作
            // 实际项目中可能是读取一个大文件或者从远程服务器中获取数据等。
            for (int i = 0; i < 10; i++)
            {
                Thread.Sleep(200);
            }

            return "点击我按钮事件完成";
        }
    }
}
