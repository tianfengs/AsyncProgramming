using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Linq;

using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DownloadAPMDelegate
{
    using System.Net;
    using System.IO;
    using System.Runtime.Remoting.Messaging;

    // 定义用来实现异步编程的委托
    delegate string AsyncMethodCaller(string fileurl);

    public partial class FrmAPMDelegate : Form
    {
        public FrmAPMDelegate()
        {
            InitializeComponent();

            txbUrl.Text = "http://download.microsoft.com/download/7/0/3/703455ee-a747-4cc8-bd3e-98a615c3aedb/dotNetFx35setup.exe";

            // 允许跨线程调用
            // 实际开发中不建议这样做的，违背了.NET 安全规范
            //CheckForIllegalCrossThreadCalls = false;
        }

        private void btnDownLoad_Click(object sender, EventArgs e)
        {
            rtbState.Text = "Download............";
            if (txbUrl.Text == string.Empty)
            {
                MessageBox.Show("Please input valid download file url");
                return;
            }

            AsyncMethodCaller methodCaller = new AsyncMethodCaller(DownLoadFileSync);
            methodCaller.BeginInvoke(txbUrl.Text.Trim(), GetResult, null);

        }

        // 异步操作完成时执行的方法
        private void GetResult(IAsyncResult ar)
        {
            AsyncMethodCaller caller = (AsyncMethodCaller)((AsyncResult)ar).AsyncDelegate;
            // 调用EndInvoke去等待异步调用完成并且获得返回值
            // 如果异步调用尚未完成，则 EndInvoke 会一直阻止调用线程，直到异步调用完成
            string returnstring = caller.EndInvoke(ar);
            //sc.Post(ShowState,resultvalue);

            //rtbState.Text = returnstring; // 不允许跨线程赋值
            rtbState.Invoke(new Action(() =>
            {
                rtbState.Text = returnstring;
            }));
        }

        private string DownLoadFileSync(string fileurl)
        {
            // Create an instance of the RequestState 
            RequestState requestState = new RequestState();
            try
            {
                // Initialize an HttpWebRequest object
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(fileurl);

                // assign HttpWebRequest instance to its request field.
                requestState.request = myHttpWebRequest;
                requestState.response = (HttpWebResponse)myHttpWebRequest.GetResponse();
                requestState.streamResponse = requestState.response.GetResponseStream();
                int readSize = requestState.streamResponse.Read(requestState.BufferRead, 0, requestState.BufferRead.Length);
                while (readSize > 0)
                {
                    requestState.filestream.Write(requestState.BufferRead, 0, readSize);
                    readSize = requestState.streamResponse.Read(requestState.BufferRead, 0, requestState.BufferRead.Length);
                }

                //Console.WriteLine("\nThe Length of the File is: {0}", requestState.filestream.Length);
                //Console.WriteLine("DownLoad Completely, Download path is: {0}", requestState.savepath);

                // 执行该方法的线程是线程池线程，该线程不是与创建richTextBox控件的线程不是一个线程
                // 如果不把 CheckForIllegalCrossThreadCalls 设置为false，该程序会出现“不能跨线程访问控件”的异常
                return string.Format("The Length of the File is: {0}", requestState.filestream.Length) + string.Format("\nDownLoad Completely, Download path is: {0}", requestState.savepath);
            }
            catch (Exception e)
            {
                //Console.WriteLine("Error Message is:{0}", e.Message);
                return string.Format("Exception occurs in DownLoadFileSync method, Error Message is:{0}", e.Message);
            }
            finally
            {
                requestState.response.Close();
                requestState.filestream.Close();
            }
        }

        /// <summary>
        /// 存储网络请求的状态（此处用于向回调函数传递变量）
        /// Microsoft技术文档介绍：HttpWebRequest类的方法BeginGetResponse()的举例文件中，涉及到这个方法
        /// https://docs.microsoft.com/zh-cn/dotnet/api/system.net.httpwebrequest.begingetresponse?redirectedfrom=MSDN&view=netframework-4.7.2#System_Net_HttpWebRequest_BeginGetResponse_System_AsyncCallback_System_Object_
        /// </summary>
        public class RequestState
        {
            // This class stores the State of the request.
            const int BUFFER_SIZE = 1024;
            public StringBuilder requestData;
            public byte[] BufferRead;
            public HttpWebRequest request;              // HttpWebRequest 是 WebRequest 的派生类，专门用于 Http, POST
            public HttpWebResponse response;            // HttpWebRequest 是 WebRequest 的派生类，专门用于 Http, GET
            public Stream streamResponse;
            public RequestState()
            {
                BufferRead = new byte[BUFFER_SIZE];
                requestData = new StringBuilder("");
                request = null;
                streamResponse = null;

                savepath = AppContext.BaseDirectory + "APM.exe";
                filestream = new FileStream(savepath, FileMode.OpenOrCreate);
            }
            public FileStream filestream;
            public string savepath;
        }
    }
}
