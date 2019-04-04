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

namespace AsyncAPM6_3跨线程赋值
{
    public partial class AsyncAPM6_3跨线程赋值 : Form
    {
        public AsyncAPM6_3跨线程赋值()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<String>(ChangeText), $"InvokeRequired = true.改变控件Text值,线程ID为{Thread.CurrentThread.ManagedThreadId}{Environment.NewLine}");
                //this.textBox1.Invoke(new Action<int>(InvokeCount), (int)mainThreadId);
            }
            else
            {
                ChangeText($"在创建控件的线程{Thread.CurrentThread.ManagedThreadId}上，改变控件Text值{Environment.NewLine}");
            }
        }

        private void ChangeText(String str)
        {
            this.textBox1.Text += str;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(() =>
             {
                 if (this.InvokeRequired)
                 {
                     this.Invoke(new Action<String>(ChangeText), $"InvokeRequired = true.改变控件Text值,线程ID为{Thread.CurrentThread.ManagedThreadId}{Environment.NewLine}");
                    //this.textBox1.Invoke(new Action<int>(InvokeCount), (int)mainThreadId);
                }
                 else
                 {
                     ChangeText($"在创建控件的线程{Thread.CurrentThread.ManagedThreadId}上，改变控件Text值{Environment.NewLine}");
                     //ChangeText("在创建控件的线程上，改变控件Text值\r\n ");
                 }
             });

            thread.Start();
        }

    }
}
