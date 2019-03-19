using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DownloadFileAPM
{
    using System.IO;
    using System.Net;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Before");

            DownloadFileAsync("https://www.baidu.com");

            Console.WriteLine("After");

            Console.ReadKey();
        }


        #region use APM Asynchronous Programming Model to download file asynchronously

        private static void DownloadFileAsync(string url)
        {
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);

            RequestState requestState = new RequestState();
            requestState.request = myHttpWebRequest;
            myHttpWebRequest.BeginGetResponse(new AsyncCallback(ResponseCallback), requestState);
        }

        private static void ResponseCallback(IAsyncResult ar)
        {
            RequestState myRequestState = (RequestState)ar.AsyncState;
            HttpWebRequest myHttpRequest = myRequestState.request;

            myRequestState.response = (HttpWebResponse)myHttpRequest.EndGetResponse(ar);

            Stream responseStream = myRequestState.response.GetResponseStream();
            myRequestState.streamResponse = responseStream;
            IAsyncResult asyncResult = responseStream.BeginRead(myRequestState.BufferRead, 0, myRequestState.BufferRead.Length, ReadCallBack, myRequestState);
        }

        private static void ReadCallBack(IAsyncResult ar)
        {
            try
            {
                //
                RequestState myRequestState = (RequestState)ar.AsyncState;
                //
                Stream responseStream = myRequestState.streamResponse;
                //
                int readSize = responseStream.EndRead(ar);
                if (readSize > 0)
                {
                    myRequestState.requestData.Append(Encoding.ASCII.GetString(myRequestState.BufferRead, 0, readSize));
                    IAsyncResult asyncResult = responseStream.BeginRead(myRequestState.BufferRead, 0, myRequestState.BufferRead.Length, new AsyncCallback(ReadCallBack), myRequestState);
                    return;
                }
                else
                {
                    Console.WriteLine("\nThe contents of the Html page are : ");
                    if (myRequestState.requestData.Length > 1)
                    {
                        string stringContent;
                        stringContent = myRequestState.requestData.ToString();
                        Console.WriteLine(stringContent);
                    }
                    Console.WriteLine("Press any key to continue..........");
                    Console.ReadLine();

                    responseStream.Close();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Error Message is {0}", e.Message);
            }
        }

        public class RequestState
        {
            // This class stores the State of the request.
            const int BUFFER_SIZE = 1024;
            public StringBuilder requestData;
            public byte[] BufferRead;
            public HttpWebRequest request;
            public HttpWebResponse response;
            public Stream streamResponse;
            public RequestState()
            {
                BufferRead = new byte[BUFFER_SIZE];
                requestData = new StringBuilder("");
                request = null;
                streamResponse = null;
            }
        }
        #endregion
    }
}
