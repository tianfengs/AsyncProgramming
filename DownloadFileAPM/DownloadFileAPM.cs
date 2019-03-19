using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 本文摘自 “[你必须知道的异步编程]——异步编程模型(APM)”
/// https://www.cnblogs.com/zhili/archive/2013/05/10/APM.html
/// 
/// 在 C# 1.x版本引入：委托、事件、APM
/// C# 2.0 引入：泛型、匿名方法、迭代器、可空类型
/// C# 3.0 引入：隐式类型的变量、对象类型初始化、自动实现属性、匿名类型、扩展方法、查询表达式、Lamda表达式、表达式树、分部类和方法、Linq
/// C# 4.0 引入：动态绑定、命名和可选参数、泛型的协变和逆变、互操作性
/// C# 5.0 （.NET 4.5):异步和等待（async和await）、调用方信息（Caller Information）
/// </summary>
namespace DownloadFileAPM
{
    using System.IO;
    using System.Net;

    /// <summary>
    /// 当需要读取文件中的内容时，我们通常会采用FileStream的"同步方法"Read来读取，该同步方法的定义为：
    /// 从文件流中读取字节块并将该数据写入给定的字节数组中
    /// array代表把读取的字节块写入的缓存区
    /// offset代表array的字节偏量，将在此处读取字节
    /// count 代表最多读取的字节数
    /// public override int Read(byte[] array, int offset, int count)
    /// 
    /// BeginRead方法代表异步执行Read操作，并返回实现IAsyncResult接口的对象，该对象存储着异步操作的信息，下面就看下BeginRead方法的定义，看看与同步Read的方法区别在哪里的.
    ///
    /// 开始"异步读操作"
    /// 前面的3个参数和同步方法代表的意思一样，这里就不说了，可以看到这里多出了2个参数
    /// userCallback代表当异步IO操作完成时，你希望由一个线程池线程执行的方法,该方法必须匹配AsyncCallback委托
    /// stateObject代表你希望转发给回调方法的一个对象的引用，在回调方法中，可以查询IAsyncResult接口的AsyncState属性来访问该对象
    /// public override IAsyncResult BeginRead(byte[] array, int offset, int numBytes, AsyncCallback userCallback, Object stateObject)
    /// 
    /// 对于访问异步操作的结果，APM提供了四种方式供开发人员选择：
    /// 在调用BeginXxx方法的线程上调用EndXxx方法来得到异步操作的结果，但是这种方式会阻塞调用线程，知道操作完成之后调用线程才继续运行
    /// 查询IAsyncResult的AsyncWaitHandle属性，从而得到WaitHandle，然后再调用它的WaitOne方法来使一个线程阻塞并等待操作完成再调用EndXxx方法来获得操作的结果。
    /// 循环查询IAsyncResult的IsComplete属性，操作完成后再调用EndXxx方法来获得操作返回的结果。
    /// 使用 AsyncCallback委托来指定操作完成时要调用的方法，在操作完成后调用的方法中调用EndXxx操作来获得异步操作的结果。
    /// 在上面的4种方式中，第4种方式是APM的首选方式，因为此时不会阻塞执行BeginXxx方法的线程，然而其他三种都会阻塞调用线程，相当于效果和使用同步方法是一样，个人感觉根本失去了异步编程的特点，所以其他三种方式可以简单了解下，在实际异步编程中都是使用委托的方式。
    /// 
    /// 本质：
    /// 其实异步编程模型这个模式，就是微软利用委托和线程池帮助我们实现的一个模式:
    /// 该模式利用一个线程池线程去执行一个操作，在FileStream类BeginRead方法中就是执行一个读取文件操作，该线程池线程会立即将控制权返回给调用线程，此时线程池线程在后台进行这个异步操作；
    /// 异步操作完成之后，通过回调函数来获取异步操作返回的结果。此时就是利用委托的机制。
    /// 所以说异步编程模式时利用委托和线程池线程搞出来的模式，包括后面的基于事件的异步编程和基于任务的异步编程，还有C# 5中的async和await关键字，都是利用这委托和线程池搞出来的。
    /// 他们的本质其实都是一样的，只是后面提出来的使异步编程更加简单罢了。
    /// 
    /// 在使用FileStream对象时的注意事项：
    /// 需要先决定是同步执行还是异步执行。并显示地指定 FileOptions.Asynchronous 参数或 useAsync 参数为true。
    /// </summary>
    class DownloadFileAPM
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Before");

            DownloadFileAsync("https://www.baidu.com"); // 异步调用，显示在屏幕
            //DownLoadFileSync("https://www.baidu.com");    // 同步调用，输出到文件中

            Console.WriteLine("After");

            Console.ReadKey();
        }


        #region use APM Asynchronous Programming Model to download file asynchronously

        private static void DownloadFileAsync(string url)
        {
            // 初始化 HttpWebRequest 对象
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            // 创建 RequestState 实例，把 HttpWebRequest 实例赋值给它的request字段
            RequestState requestState = new RequestState();
            requestState.request = myHttpWebRequest;

            // 异步发送http get请求，设置异步完成后的回调函数 ResponseCallback，并把 RequestState 的 实例传递给回调函数
            // 立刻返回，让主线程继续执行
            myHttpWebRequest.BeginGetResponse(new AsyncCallback(ResponseCallback), requestState);
        }

        // 当每一个异步操作完成后，调用以下回调方法
        private static void ResponseCallback(IAsyncResult ar)
        {
            // 获得传递过来的 RequestState 对象
            RequestState myRequestState = (RequestState)ar.AsyncState;
 
            HttpWebRequest myHttpRequest = myRequestState.request;

            // 结束异步get请求，并返回HttpWebResponse 对象，并赋值给 RequestState 的对象的response字段
            myRequestState.response = (HttpWebResponse)myHttpRequest.EndGetResponse(ar);

            // 从Server获得 返回数据流
            Stream responseStream = myRequestState.response.GetResponseStream();
            // 并赋值给 RequestState 的对象的 streamResponse 字段
            myRequestState.streamResponse = responseStream;

            // 异步读取 流中的数据，设置异步操作完成后的回调函数 ReadCallBack，并把 RequestState 的 实例传递给回调函数
            IAsyncResult asyncResult = responseStream.BeginRead(myRequestState.BufferRead, 0, myRequestState.BufferRead.Length, ReadCallBack, myRequestState);
        }

        // 在异步读取完成后，把内容写到FileStream中
        private static void ReadCallBack(IAsyncResult ar)
        {
            try
            {
                // 获得传递过来的 RequestState 对象
                RequestState myRequestState = (RequestState)ar.AsyncState;

                // 把 RequestState 对象的流信息字段streamResponse内容，赋值给 Stream 实例
                Stream responseStream = myRequestState.streamResponse;

                // 结束异步读取，并返回读取的字数
                int readSize = responseStream.EndRead(ar);

                // 递归读取，直到返回的字数为0，即没有可读取的内容
                if (readSize > 0)
                {
                    myRequestState.requestData.Append(Encoding.ASCII.GetString(myRequestState.BufferRead, 0, readSize));
                    IAsyncResult asyncResult = responseStream.BeginRead(myRequestState.BufferRead, 0, myRequestState.BufferRead.Length, new AsyncCallback(ReadCallBack), myRequestState);
                    return;
                }
                else
                {
                    // 在屏幕上输出读取结果
                    Console.WriteLine("\nThe contents of the Html page are : ");
                    if (myRequestState.requestData.Length > 1)
                    {
                        string stringContent;
                        stringContent = myRequestState.requestData.ToString();
                        Console.WriteLine(stringContent);
                    }
                    Console.WriteLine("Press any key to continue..........");
                    //Console.ReadLine();

                    responseStream.Close();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Error Message is {0}", e.Message);
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

                savepath = AppContext.BaseDirectory + "APM.txt";
                filestream = new FileStream(savepath,FileMode.OpenOrCreate);
            }
            public FileStream filestream;
            public string savepath;
        }
        #endregion

        #region Download File Synchrously
        private static void DownLoadFileSync(string url)
        {
            // Create an instance of the RequestState 
            RequestState requestState = new RequestState();
            try
            {
                // Initialize an HttpWebRequest object
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);

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

                Console.WriteLine("\nThe Length of the File is: {0}", requestState.filestream.Length);
                Console.WriteLine("DownLoad Completely, Download path is: {0}", requestState.savepath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error Message is:{0}", e.Message);
            }
            finally
            {
                requestState.response.Close();
                requestState.filestream.Close();
            }
        }
        #endregion 
    }
}
