using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MillisecondTimer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            init();
        }

        MillisecondTimer timer1;

        System.Threading.Timer timer2;

        private void ChangeText2(object state)
        {
            string min = string.Empty;  // 分
            string sec = string.Empty;  // 秒
            string mse = string.Empty;  // 毫秒

            i2++;

            // 分
            if (i2 / 100 / 60 < 10)
            {
                min = "0" + (i2 / 100 / 60).ToString();
            }
            else
            {
                min = (i2 / 100 / 60).ToString();
            }

            // 秒
            if (i2 / 100 % 60 < 10)
            {
                sec = "0" + (i2 / 100 % 60).ToString();
            }
            else
            {
                sec = (i2 / 100 % 60).ToString();
            }

            // 毫秒
            if (i2 % 100 < 10)
            {
                mse = "0" + (i2 % 100 ).ToString();
            }
            else
            {
                mse = (i2 % 100 ).ToString();
            }
            string str = min + ":" + sec + ":" + mse;
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<String>(obj =>
                {
                    textBox2.Text = obj;
                }), str);
                
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            timer1 = new MillisecondTimer();
            timer1.Interval = 1;
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Start();

            timer2= new System.Threading.Timer(ChangeText2, null, 0, 1);
        }

        int i,i2;

        void timer1_Tick(object sender, EventArgs e)
        {
            Console.WriteLine(i++);

            if (this.InvokeRequired)
            {
                this.Invoke(new Action<String>(ChangeText), "InvokeRequired = true.改变控件Text值\n" +
                    "");
                //this.textBox1.Invoke(new Action<int>(InvokeCount), (int)mainThreadId);
            }
            
        }

        private void ChangeText(string obj)
        {
            string min = string.Empty;  // 分
            string sec = string.Empty;  // 秒
            string mse = string.Empty;  // 毫秒

            // 分
            if (i / 1000 / 60 < 10)
            {
                min = "0" + (i / 1000 / 60).ToString();
            }
            else
            {
                min = (i / 1000 / 60).ToString();
            }

            // 秒
            if (i/1000%60<10)
            {
                sec = "0" + (i / 1000 % 60).ToString();
            }
            else
            {
                sec= (i / 1000 % 60).ToString();
            }

            // 毫秒
            if (i % 1000/10 < 10)
            {
                mse = "0" + (i % 1000/10).ToString();
            }
            else
            {
                mse = (i % 1000/10).ToString();
            }
            textBox1.Text = min + ":" + sec + ":" + mse;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            timer1.Dispose();

            timer2.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            i = 0;
            i2 = 0;
            init();
        }

        private void init()
        {
            textBox1.Text = "00:00:00";
            textBox2.Text = "00:00:00";
        }
    }
}
