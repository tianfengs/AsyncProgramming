---
typora-copy-images-to: MarkDownPics
---

异步编程系列文章6：《异步编程：IAsyncResult异步编程模型 (APM)》
http://www.cnblogs.com/heyuquan/archive/2013/03/22/2976420.html



在开发多线程应用程序时，很多人都是使用ThreadPool的QueueUserWorkItem方法来发起一次简单的异步操作。然而，这个技术存在许多限制。最大的问题是没有一个内建的机制让你知道操作在什么时候完成，也没有一个机制在操作完成时获得一个返回值。

> 用线程池创建线程有两种方法：
>
> 1. public static bool QueueUserWorkItem(WaitCallback callBack, object state);
>
> 2. public static RegisteredWaitHandle RegisterWaitForSingleObject(		
>                WaitHandle waitObject				
>                , WaitOrTimerCallback callBack, object state
>
>    ?	    , Int millisecondsTimeOutInterval
>
>    ?	    , bool executeOnlyOnce);		
>
> 第二种方法是可以对线程进行精确控制的，包括控制结束、超时结束、回调方法等。

Microsoft引入三种异步编程模式：

- .NET 1.0 异步编程模式——APM，基于IAsyncResult接口实现。（此文档）
- .NET 2.0 基于事件的异步编程模式——EAP，基于事件实现。
- .NET 4.x 基于任务的异步编程模式——TAP，新型编程模式，推荐。

### 0、IAsyncResult设计模式——规范

使用IAsyncResult设计模式的异步操作是通过名为 Begin*** 和 End*** 的两个方法来实现的，这两个方法分别指代开始和结束异步操作。例如:

> FileStream类提供**BeginRead**和**EndRead**方法来从文件异步读取字节。
>
> 这两个方法实现了 **Read** 方法的异步版本。

在调用 Begin...后，应用程序可以继续在调用线程上执行指令，同时异步操作在另一个线程上执行。（如果有返回值还应调用 End... 来获取操作的结果）。

1. Begin...()
   - Begin...()方法带有该方法的同步版本签名中声明的任何参数。

   - Begin...()方法签名中不包含任何输出参数。签名里 **最后两个参数** 的规范是：

     - 第一个参数定义一个AsyncCallback委托，它的引用是在**异步操作完成时**调用的方法。
     - 第二个参数是一个用户定义的对象。它向AsyncCallback委托方法传统状态信息。可通过此对象在委托中访问End...()方法。
     - 这两个参数都可以传递**null**。

   - 返回IAsyncResult对象。

     ```
     // 表示异步操作的状态。
     [ComVisible(true)]
     public interface IAsyncResult
     {
         // 获取用户定义的对象，它限定或包含关于异步操作的信息。
         object AsyncState { get; }
         // 获取用于等待异步操作完成的System.Threading.WaitHandle，待异步操作完成时获得信号。
         WaitHandle AsyncWaitHandle { get; }
         // 获取一个值，该值指示异步操作是否同步完成。
         bool CompletedSynchronously { get; }
         // 获取一个值，该值指示异步操作是否已完成。
         bool IsCompleted { get; }
     }
      
     // 常用委托声明（我后面示例是使用了自定义的带ref参数的委托）
     public delegate void AsyncCallback(IAsyncResult ar)
     ```

2. End...()

   - End...()方法可结束异步操作，如果调用End...()时，IAsyncResult对象表示的异步操作还未完成，则End...()方法将在异步操作完成前阻塞调用线程。
   - End...()方法的返回值与其同步副本的返回值类型相同。End...()方法带有该方法同步版本的签名中声明的所有out和ref参数，以及由BeginInvoke返回的IAsyncResult，规范上IAsyncResult参数放到最后。
     - 要想获得返回值，必须调用方法；
     - 若带有out和ref参数，实现上委托也要带有out和ref参数，以便在回调中获得对应引用传参值做相应逻辑。

3. 总是调用End...()方法，而且只调用一次

   > **异步操作应该遵守的规则**：
   >
   > I/O限制的异步操作：比如像带FileOptions.Asynchronous标识的FileStream，其BeginRead()方法向Windows发送一个I/O请求包（I/O Request Packet，IRP）后方法**不会阻塞线程而是立即返回**，**由Windows将IRP传送给适当的设备驱动程序**，IRP中包含了为BeginRead()方法传入的回调函数，**待硬件设备处理好IRP后，会将IRP的委托排队到CLR的线程池队列中**。
   必须调用End...()方法，否则会造成资源泄漏：

   - 在异步操作时，对于I/O限制操作，CLR会分配一些内部资源，操作完成时，CLR继续保留这些资源直至End...()方法被调用。如果一直不调用End...()，这些资源会直到进程终止时才会被回收。（End...()方法设计中常常包含资源释放）
   - 发起一个异步操作时，实际上并不知道该操作最终是成功还是失败（因为操作由硬件在执行）。要知道这一点，只能通过调用End***方法，检查它的返回值或者看它是否抛出异常。
   - 需要注意的是I/O限制的异步操作完全不支持取消（因为操作由硬件执行），但可以设置一个标识，在完成时丢弃结果来模拟取消行为。



我们通过IAsyncResult异步编程模式的三个经典场合来加深理解

### 一、基于IAsyncResult构造一个异步API

1. 构建一个扩展IAsyncResult接口的类，并且实现异步调用。（**见示例：“AsyncAPM6_1”**）

2. 使用委托进行异步编程（**见示例：“AsyncAPM6_2”**）

   /// 使用三种阻塞式异步方法（1.2.3)，一种非阻塞式异步方法(4)：
   /// 1. 用 EndInvoke 阻塞
   /// 2. 用 WaitHandle 阻塞
   /// 3. 用 IsCompleted轮询 阻塞
   /// 4. 使用回调函数，非阻塞式响应异步调用方法

3. 多线程操控

   访问 Windows 窗体控件本质上不是线程安全的。确保以线程安全方式访问控件非常重要。在有些情况下，您可能需要多线程调用控件的方法。.NET Framework 提供了从任何线程操作控件的方式：

   - 非安全方式访问控件（**此方式请永远不要再使用**）

     ```
     // 获取或设置一个值，该值指示是否捕获对错误线程的调用，
     // 这些调用在调试应用程序时访问控件的System.Windows.Forms.Control.Handle属性。
     // 如果捕获了对错误线程的调用，则为 true；否则为 false。
     public static bool CheckForIllegalCrossThreadCalls { get; set; }
     ```

   -   安全方式访问控件

     原理：从一个线程封送调用并跨线程边界将其发送到另一个线程，并将调用插入到创建控件线程的消息队列中,当控件创建线程处理这个消息时,就会在自己的上下文中执行传入的方法。（此过程只有调用线程和创建控件的线程，并没有创建新线程）

     注意：从一个线程封送调用并跨线程边界将其发送到另一个线程会**耗费大量的系统资源**，所以应避免重复调用其他线程上的控件。

     - 使用BackgroundWork后台辅助线程控件方式（详见：基于事件的异步编程模式(EAP)）
     - 结合TaskScheduler.FromCurrentSynchronizationContext()和Task实现
     - 捕获线程上下文ExecuteContext，并调用ExeceteContext.Run()静态方法在指定的线程上下文中执行。（详见：2.异步编程：使用线程池管理——执行上下文）
     - 使用Control类上提供的Invoke和BeginInvoke方法
     - 在WPF应用程序中可以通过WPF提供的Dispatcher对象提供的Invoke和BeginInvoke方法来完成跨线程工作。

     > 因本文主要解说IAsyncResult异步编程模式，所以只详细分析Invoke 和BeginInvoke跨线程访问控件方式。   
     >
     > Control类实现了**ISynchronizeInvoke**接口，提供了Invoke和BeginInvoke方法来支持其它线程更新GUI界面控件的机制。

     ```
     public interface ISynchronizeInvoke
     {
         // 获取一个值，该值指示调用线程是否与控件的创建线程相同。
         bool InvokeRequired { get; }
         // 在控件创建的线程上异步执行指定委托。
         AsyncResult BeginInvoke(Delegate method, params object[] args);
         object EndInvoke(IAsyncResult asyncResult);
         // 在控件创建的线程上同步执行指定委托。
         object Invoke(Delegate method, params object[] args);
     }
     ```

     1. Control类的Invoke和BeginInvoke

        - Invoke：（同步调用）先判断控件创建线程与当前线程是否相同，相同则直接调用委托方法；否则使用Win32API的PostMessage 异步执行,但是 Invoke 内部会调用IAsyncResult.AsyncWaitHandle等待执行完成
        - BeginInvoke：（异步调用）使用Win32API的PostMessage 异步执行，并且返回 IAsyncResult 对象。 

        ```
        UnsafeNativeMethods.PostMessage(new HandleRef(this, this.Handle)
                          , threadCallbackMessage, IntPtr.Zero, IntPtr.Zero);
                          
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool PostMessage(HandleRefhwnd, intmsg, IntPtrwparam, IntPtrlparam);
        ```

        **PostMessage** 是windows api，用来把一个消息发送到一个窗口的消息队列。这个**方法**是**异步**的，也就是该方法封送完毕后马上返回，不会等待委托方法的执行结束，调用者线程将不会被阻塞。

        （对应同步方法的windows api是：**SendMessage**()；消息队列里的消息通过调用GetMessage和PeekMessage取得）

     2. InvokeRequired

        获取一个值，该值指示调用线程是否与控件的创建线程相同。内部关键如下：

        ```
        Int windowThreadProcessId = SafeNativeMethods.GetWindowThreadProcessId(ref2, out num);
        Int currentThreadId = SafeNativeMethods.GetCurrentThreadId();
        return (windowThreadProcessId != currentThreadId);
        ```

     3. 示例:（**见示例：“AsyncAPM6_3跨线程赋值”**）

        注意，在InvokeControl方法中使用 this.Invoke(Delegate del) 和使用 this.textBox1.Invoke(Delegate del) 效果是一样的。

     4. 异常信息：

        "在创建**窗口句柄**之前,不能在控件上调用 Invoke 或 BeginInvoke"

        - 可能是在窗体还未构造完成时，在构造函数中异步去调用了Invoke 或BeginInvoke；

        -  可能是使用辅助线程创建一个窗口并用Application.Run()去创建句柄，在句柄未创建好之前调用了Invoke 或BeginInvoke。（此时新建的窗口相当于开了另一个进程，并且为新窗口关联的辅助线程开启了消息循环机制），类似下面代码：

          ```
          new Thread((ThreadStart)delegate
              {
                  WaitBeforeLogin = new Form2();
                  Application.Run(WaitBeforeLogin);
              }).Start();
          ```

        解决方案：在调用Invoke 或 BeginInvoke之前轮询检查窗口的IsHandleCreated属性。

        ```
        // 获取一个值，该值指示控件是否有与它关联的句柄。
        public bool IsHandleCreated { get; }
        while (!this.IsHandleCreated) { …… }
        ```



本节主要讲了异步编程模式之一“异步编程模型（APM）”，是基于IAsyncResult设计模式实现的异步编程方式，并且构建了一个继承自IAsyncResult接口的示例，及展示了这种模式在委托及跨线程访问控件上的经典应用。

下一节，介绍基于事件的编程模型…… 