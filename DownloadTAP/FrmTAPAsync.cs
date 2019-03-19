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
        int DownloadSize = 0;
        string downloadPath = null;
        long totalSize = 0;
        FileStream filestream;

        CancellationTokenSource cts = null;
        Task task = null;

        SynchronizationContext sc;

        public FrmTAPAsync()
        {
            InitializeComponent();

            string url = "http://download.microsoft.com/download/7/0/3/703455ee-a747-4cc8-bd3e-98a615c3aedb/dotNetFx35setup.exe";
            txbUrl.Text = url;
            this.btnPause.Enabled = false;

            // Get Total Size of the download file
            GetTotalSize();
            downloadPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + Path.GetFileName(this.txbUrl.Text.Trim());
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
            filestream = new FileStream(downloadPath, FileMode.OpenOrCreate);
            this.btnStart.Enabled = false;
            this.btnPause.Enabled = true;

            filestream.Seek(DownloadSize, SeekOrigin.Begin);

            // 捕捉调用线程的同步上下文派生对象
            sc = SynchronizationContext.Current;
            cts = new CancellationTokenSource();
            // 使用指定的操作初始化新的 Task。

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
                sc.Post(new SendOrPostCallback((result) => progressBar1.Value = (int)result), p);
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
                    request.AddRange(DownloadSize);
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
                        sc.Post((state) =>
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

        // Get Total Size of File
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
