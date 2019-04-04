using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

/// <summary>
/// 目前，Windows软件一般使用Timer定时器进行定时。Timer定时器是由应用程序响应定时消息WM_TIMER实现定时。
/// Timer定时器是IBM PC硬件和ROM BIOS构造的定时器的简单扩充。PC的ROM初始化8253定时器来产生硬件中断08H，而08H中断的频率为18.2Hz，即至少每隔54.925 ms中断一次。
/// 此外，这个定时消息的优先权太低，只有在除WM_PAINT外的所有消息被处理完后，才能得到处理。
/// 
/// 多媒体定时器也是利用系统定时器工作的，但它的工作机理和普通定时器有所不同。
/// 首先，多媒体定时器可以按精度要求设置8253的T/C0通道的计数初值，使定时器不存在54.945ms的限制；
/// 其次，多媒体定时器不依赖于消息机制，而是用函数产生一个独立的线程，在一定的中断次数到达后，直接调用预先设置好的回调函数进行处理，
/// 不必等到应用程序的消息队列为空，从而切实保障了定时中断得到实时响应，使其定时精度可达1ms。
/// </summary>
namespace MillisecondTimer
{
    using System;

    using System.Runtime.InteropServices;

    using System.ComponentModel;


    public sealed class MillisecondTimer : IComponent, IDisposable
    {

        //*****************************************************  字 段  *******************************************************************
        private static TimerCaps caps;
        private int interval;
        private bool isRunning;
        private int resolution;
        private TimerCallback timerCallback;
        private int timerID;


        //*****************************************************  属 性  *******************************************************************
        /// <summary>
        /// 
        /// </summary>
        public int Interval
        {
            get
            {
                return this.interval;
            }
            set
            {
                if ((value < caps.periodMin) || (value > caps.periodMax))
                {
                    throw new Exception("超出计时范围！");
                }
                this.interval = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return this.isRunning;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public ISite Site
        {
            set;
            get;
        }





        //*****************************************************  事 件  *******************************************************************

        public event EventHandler Disposed;  // 这个事件实现了IComponet接口
        public event EventHandler Tick;




        //***************************************************  构造函数和释构函数  ******************************************************************

        static MillisecondTimer()
        {
            timeGetDevCaps(ref caps, Marshal.SizeOf(caps));
        }


        public MillisecondTimer()
        {
            this.interval = caps.periodMin;    // 
            this.resolution = caps.periodMin;  //

            this.isRunning = false;
            this.timerCallback = new TimerCallback(this.TimerEventCallback);

        }


        public MillisecondTimer(IContainer container)
            : this()
        {
            container.Add(this);
        }



        ~MillisecondTimer()
        {
            timeKillEvent(this.timerID);
        }



        //*****************************************************  方 法  *******************************************************************



        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            if (!this.isRunning)
            {
                this.timerID = timeSetEvent(this.interval, this.resolution, this.timerCallback, 0, 1); // 间隔性地运行

                if (this.timerID == 0)
                {
                    throw new Exception("无法启动计时器");
                }
                this.isRunning = true;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            if (this.isRunning)
            {
                timeKillEvent(this.timerID);
                this.isRunning = false;
            }
        }




        /// <summary>
        /// 实现IDisposable接口
        /// </summary>
        public void Dispose()
        {
            timeKillEvent(this.timerID);
            GC.SuppressFinalize(this);
            EventHandler disposed = this.Disposed;
            if (disposed != null)
            {
                disposed(this, EventArgs.Empty);
            }
        }



        //***************************************************  内部函数  ******************************************************************
        [DllImport("winmm.dll")]
        private static extern int timeSetEvent(int delay, int resolution, TimerCallback callback, int user, int mode);


        [DllImport("winmm.dll")]
        private static extern int timeKillEvent(int id);


        [DllImport("winmm.dll")]
        private static extern int timeGetDevCaps(ref TimerCaps caps, int sizeOfTimerCaps);
        //  The timeGetDevCaps function queries the timer device to determine its resolution. 



        private void TimerEventCallback(int id, int msg, int user, int param1, int param2)
        {
            if (this.Tick != null)
            {
                this.Tick(this, null);  // 引发事件
            }
        }






        //***************************************************  内部类型  ******************************************************************

        private delegate void TimerCallback(int id, int msg, int user, int param1, int param2); // timeSetEvent所对应的回调函数的签名


        /// <summary>
        /// 定时器的分辨率（resolution）。单位是ms，毫秒？
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct TimerCaps
        {
            public int periodMin;
            public int periodMax;
        }

    }
}
