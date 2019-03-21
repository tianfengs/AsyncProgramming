using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Async_Await_AsAwMethod
{
    using System.IO;
    using System.Net;
    using System.Threading;

    public partial class FrmAsyncAwait : Form
    {
        public FrmAsyncAwait()
        {
            InitializeComponent();
        }

        private async void btnClick_Click(object sender, EventArgs e)
        {
            long length = await AccessWebAsync();

            // 这里可以做一些不依赖回复的操作
            OtherWork();

            this.richTextBox1.Text += String.Format("\n 回复的字节长度为:  {0}.\r\n", length);
            txbMainThreadID.Text = Thread.CurrentThread.ManagedThreadId.ToString();
        }

        /// <summary>
        /// 使用C# 5.0（.NET 4.5）中提供的async和await关键字来定义异步方法
        /// 使用async和await定义异步方法不会创建新线程，它运行在现有线程上执行多个任务
        /// 
        /// ？？此时不知道大家有没有一个疑问的？在现有线程上(即UI线程上)运行一个耗时的操作时，为什么不会堵塞UI线程的呢？答：它使方法可被分割成多个片段，其中一些片段可能异步运行，这样这个方法可能异步完成。
        ///
        /// ”async”关键字指示编译器在方法内部可能会使用”await”关键字，这样该方法就可以在await处挂起并且在await标记的任务完成后异步唤醒。
        /// 只有返回类型为void、Task或Task<TResult>的方法才能用”async”标记。并且，并不是所有返回类型满足上面条件的方法都能用”async”标记。
        /// 
        /// 使用async和await关键字实现的异步方法，此时的异步方法被分成了多个代码片段去执行的，而不是像之前的异步编程模型(APM)和EAP那样，使用线程池线程去执行一整个方法。
        /// 
        /// 使用async标识的异步方法的运行在GUI线程上，所以就不用像APM中那样考虑跨线程访问的问题了。
        /// </summary>
        /// <returns></returns>
        private async Task<long> AccessWebAsync()
        {
            MemoryStream content = new MemoryStream();

            // 对MSDN发起一个Web请求
            HttpWebRequest webRequest = WebRequest.Create("http://msdn.microsoft.com/zh-cn/") as HttpWebRequest;
            if (webRequest != null)
            {
                // 返回回复结果
                using (WebResponse response = await webRequest.GetResponseAsync())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        await responseStream.CopyToAsync(content);
                    }
                }
            }

            Thread.Sleep(2000);

            txbAsynMethodID.Text = Thread.CurrentThread.ManagedThreadId.ToString();
            return content.Length;
        }

        private void OtherWork()
        {
            //Console.WriteLine("other work。。。。finish");
            this.richTextBox1.Text += String.Format("\n other work。。。。finish.\r\n");
        }
    }
}
