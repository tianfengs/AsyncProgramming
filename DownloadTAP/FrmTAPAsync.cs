using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DownloadTAP
{
    using System.IO;
    using System.Net;
    using System.Threading;

    public partial class FrmTAPAsync : Form
    {
        int DownloadSize = 0;                   // 当前下载量，进度 IProgress
        string downloadPath = null;             // 下载到的本地路径和文件名
        long totalSize = 0;                     // 通过 HttpWebResponse 的对象response获取 response.ContentLength;
        FileStream filestream;                  // 文件流

        CancellationTokenSource cts = null;     // 取消Task线程，设置线程状态
        Task task = null;                       // 建立一个task对象

        SynchronizationContext sc;          // 同步上下文。SynchronizationContext.Current对象不是一个AppDomain一个实例的，而是每个线程一个实例。
                                            // SynchronizationContext可以使一个线程与另一个线程进行通信。
                                            //假设你又两个线程，Thead1和Thread2。Thread1做某些事情，然后它想在Thread2里面执行一些代码。
                                            //一个可以实现的方式是请求Thread得到SynchronizationContext这个对象，把它给Thread1，
                                            //然后Thread1可以调用SynchronizationContext的send方法在Thread2里面执行代码。
                                            //不是每一个线程都有一个SynchronizationContext对象。一个总是有SynchronizationContext对象的是UI线程。所以可以用来跨线程修改控件。

        public FrmTAPAsync()
        {
            InitializeComponent();

            string url = "http://download.microsoft.com/download/7/0/3/703455ee-a747-4cc8-bd3e-98a615c3aedb/dotNetFx35setup.exe";
            txbUrl.Text = url;
            this.btnPause.Enabled = false;

            // Get Total Size of the download file
            GetTotalSize();
            downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + Path.GetFileName(this.txbUrl.Text.Trim());

            // 判断指定路径是否有相同文件，如果有，1.删除，准备下载；2.不删除，灰显下载按钮，并设置进度条为100.
            if (File.Exists(downloadPath))
            {
                FileInfo fileInfo = new FileInfo(downloadPath);
                DownloadSize = (int)fileInfo.Length;
                if (DownloadSize == totalSize)
                {
                    string message = "There is already a file with the same name, "
                           + "do you want to delete it? "
                           + "If not, please change the local path. ";
                    var result = MessageBox.Show(
                        message,
                        "File name conflict: " + downloadPath,
                        MessageBoxButtons.OKCancel);

                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        File.Delete(downloadPath);
                    }
                    else
                    {
                        progressBar1.Value = (int)((float)DownloadSize / (float)totalSize * 100);
                        this.btnStart.Enabled = false;
                        this.btnPause.Enabled = false;
                    }
                }
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            filestream = new FileStream(downloadPath, FileMode.OpenOrCreate);   // 创建文件流，用来准备向本地传送文件
            this.btnStart.Enabled = false;
            this.btnPause.Enabled = true;

            filestream.Seek(DownloadSize, SeekOrigin.Begin);                    // 将文件指针移动到 从文件开始计算的当前下载字节数的位置

            // 捕捉调用线程的同步上下文派生对象
            sc = SynchronizationContext.Current;            // SynchronizationContext.Current可以使我们得到一个当前线程的SynchronizationContext的对象。
                                                            //我们必须清楚如下问题：SynchronizationContext.Current对象不是一个AppDomain一个实例的，而是每个线程一个实例。
                                                            //这就意味着两个线程在调用Synchronization.Current时将会拥有他们自己的SynchronizationContext对象实例。
                                                            //这个context上下文对象存储在线程data store（就像我之前说的，不是在appDomain的全局内存空间）。

            // 创建CancellationTokenSource对象用于取消Task
            cts = new CancellationTokenSource();

            // 使用指定的操作初始化新的 Task。创建一个新的异步线程，所创建的线程默认为后台线程
            task = new Task(() => Actionmethod(cts.Token), cts.Token);

            // 启动 Task，并将它安排到当前的 TaskScheduler 中执行。 
            task.Start();

            //await DownLoadFileAsync(txbUrl.Text.Trim(), cts.Token,new Progress<int>(p => progressBar1.Value = p));
        }

        // 任务中执行的方法
        private void Actionmethod(CancellationToken token)
        {
            // 使用同步上文文的Post方法把更新UI的方法让主线程执行
            DownLoadFile(txbUrl.Text.Trim(), token, new Progress<int>(p =>
            {
                sc.Post(new SendOrPostCallback((result) => progressBar1.Value = (int)result), p);       // 通过 SynchronizationContext 实例 sc 异步 跨线程更新UI界面progressBar1控件
            }));
        }

        //// 更新UI
        //private void progressChange(object result)
        //{
        //    this.progressBar1.Value = (int)result;
        //}

        private void btnPause_Click(object sender, EventArgs e)
        {
            // 发出一个取消请求
            cts.Cancel();
        }

        //  Download File
        // CancellationToken 参数赋值获得一个取消请求
        // progress参数负责进度报告
        private void DownLoadFile(string url, CancellationToken ct, IProgress<int> progress)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            Stream responseStream = null;
            int bufferSize = 2048;
            byte[] bufferBytes = new byte[bufferSize];
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                if (DownloadSize != 0)
                {
                    request.AddRange(DownloadSize);         // 向请求添加从请求数据的开始处或结束处的特定范围的字节范围标头。
                                                            // 如果range是正数，range参数指定的范围的起始点。 服务器应开始发送数据从range参数指定到末尾的 HTTP 实体中的数据。
                                                            // 如果range为负，range参数指定的范围的结束点。 服务器应该开始将数据发送到 HTTP 实体中的数据开始从range指定的参数。
                }

                response = (HttpWebResponse)request.GetResponse();
                responseStream = response.GetResponseStream();
                int readSize = 0;
                while (true)
                {
                    // 收到取消请求则退出异步操作
                    if (ct.IsCancellationRequested == true)
                    {
                        MessageBox.Show(String.Format("下载暂停，下载的文件地址为：{0}\n 已经下载的字节数为: {1}字节", downloadPath, DownloadSize));

                        response.Close();
                        filestream.Close();
                        sc.Post((state) =>                  // 通过 SynchronizationContext 实例 sc 异步 跨线程更新UI界面btnStart、btnPause控件
                        {
                            this.btnStart.Enabled = true;
                            this.btnPause.Enabled = false;
                        }, null);

                        // 退出异步操作
                        break;
                    }

                    readSize = responseStream.Read(bufferBytes, 0, bufferBytes.Length);
                    if (readSize > 0)
                    {
                        DownloadSize += readSize;
                        int percentComplete = (int)((float)DownloadSize / (float)totalSize * 100);
                        filestream.Write(bufferBytes, 0, readSize);

                        // 报告进度
                        progress.Report(percentComplete);
                    }
                    else
                    {
                        MessageBox.Show(String.Format("下载已完成，下载的文件地址为：{0}，文件的总字节数为: {1}字节", downloadPath, totalSize));

                        sc.Post((state) =>
                        {
                            this.btnStart.Enabled = false;
                            this.btnPause.Enabled = false;
                        }, null);

                        response.Close();
                        filestream.Close();
                        break;
                    }
                }
            }
            catch (AggregateException ex)
            {
                // 因为调用Cancel方法会抛出OperationCanceledException异常
                // 将任何OperationCanceledException对象都视为以处理
                ex.Handle(e => e is OperationCanceledException);
            }
        }

        // 
        /// <summary>
        /// 获取文件的全部尺寸. HttpWebResponse response; totalSize = response.ContentLength;
        /// </summary>
        private void GetTotalSize()
        {
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(txbUrl.Text.Trim());
            HttpWebResponse response = (HttpWebResponse)myHttpWebRequest.GetResponse();
            totalSize = response.ContentLength;
            response.Close();
        }

        #region 使用C#中async和await关键字
        // async Download File
        private async Task DownLoadFileAsync(string url, CancellationToken ct, IProgress<int> progress)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            Stream responseStream = null;
            int bufferSize = 2048;
            byte[] bufferBytes = new byte[bufferSize];
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                if (DownloadSize != 0)
                {
                    request.AddRange(DownloadSize);
                }

                response = (HttpWebResponse)await request.GetResponseAsync();
                responseStream = response.GetResponseStream();
                int readSize = 0;
                while (true)
                {
                    if (ct.IsCancellationRequested == true)
                    {
                        MessageBox.Show(String.Format("下载暂停，下载的文件地址为：{0}\n 已经下载的字节数为: {1}字节", downloadPath, DownloadSize));
                        response.Close();
                        filestream.Close();

                        this.btnStart.Enabled = true;
                        this.btnPause.Enabled = false;
                        break;
                    }

                    readSize = await responseStream.ReadAsync(bufferBytes, 0, bufferBytes.Length);
                    if (readSize > 0)
                    {
                        DownloadSize += readSize;
                        int percentComplete = (int)((float)DownloadSize / (float)totalSize * 100);
                        await filestream.WriteAsync(bufferBytes, 0, readSize);

                        // 报告进度
                        progress.Report(percentComplete);
                    }
                    else
                    {
                        MessageBox.Show(String.Format("下载已完成，下载的文件地址为：{0}，文件的总字节数为: {1}字节", downloadPath, totalSize));

                        this.btnStart.Enabled = false;
                        this.btnPause.Enabled = false;
                        response.Close();
                        filestream.Close();
                        break;
                    }
                }
            }
            catch (AggregateException ex)
            {
                ex.Handle(e => e is OperationCanceledException);
            }
        }

        #endregion
    }
}
