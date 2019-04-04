---
typora-copy-images-to: MarkDownPics
---

异步编程系列文章7：《异步编程：基于事件的异步编程模式(EAP)》
http://www.cnblogs.com/heyuquan/archive/2013/04/01/2993085.html

“**.NET1.0 IAsyncResult异步编程模型(APM)**”，通过Begin*** 开启操作并返回IAsyncResult对象，使用 End*** 方法来结束操作，通过回调方法来做异步操作后其它事项。

**最大的问题是没有提供"进度通知"等功能及"多线程间控件的访问"**。

.NET2.0 中引入了：基于事件的异步编程模式(EAP，Event-based Asynchronous Pattern)。

通过事件、AsyncOperationManager类和AsyncOperation类两个帮助器类实现如下功能：

> 1. 异步执行耗时的任务
> 2. 获得进度报告和增量结果
> 3. 支持耗时任务的取消
> 4. 获得任务的结果值或异常信息
> 5.  更复杂：支持同时执行多个异步操作、进度报告、增量结果、取消操作、返回结果值或异常信息

对于相对简单的多线程应用程序，BackgroundWorker组件提供了一个简单的解决方案。对于更复杂的异步应用程序，可以考虑实现一个符合基于事件的异步模式的类。

### 一、EAP异步编程模型的优点

EAP是**为Windows窗体开发人员**创建的，其主要优点在于：

1.  EAP与Microsoft Visual Studio UI设计器进行了很好的集成。

   也就是说，可将大多数实现了EAP的类拖放到一个Visual Studio设计器平面上，然后双击事件名，让Visual Studio自动生成事件回调方法，并将方法同事件关联起来。

2. EAP类在内部通过SynchronizationContext类，将应用程序模型映射到合适线程处理模型，以方便跨线程操作控件。

### 二、两个帮助器类

为了实现基于事件的异步模式，我们必须先理解两个重要的帮助器类：

- **AsyncOperationManager**
- **AsyncOperation**

AsyncOperationManager类和AsyncOperation类是**System.ComponentModel**命名空间为我们提供了两个重要帮助器类。

在基于事件的异步模式封装标准化的异步功能中，它确保你的异步操作支持在**各种应用程序模型（包括 ASP.NET、控制台应用程序和 Windows 窗体应用程序）**的适当“线程或上下文”调用客户端事件处理程序。

AsyncOperationManager类和AsyncOperation类的API如下：

```
// 为支持异步方法调用的类提供‘并发管理’。此类不能被继承。
public static class AsyncOperationManager
{
    // 获取或设置用于异步操作的同步上下文。
    public static SynchronizationContext SynchronizationContext { get; set; }
 
    // 返回可用于对特定异步操作的持续时间进行跟踪的AsyncOperation对象。
    // 参数:userSuppliedState:
    //     一个对象，用于使一个客户端状态（如任务 ID）与一个特定异步操作相关联。
    public static AsyncOperation CreateOperation(object userSuppliedState)
    {
        return AsyncOperation.CreateOperation(userSuppliedState,SynchronizationContext);
    }
}
 
// 跟踪异步操作的生存期。
public sealed class AsyncOperation
{
    // 构造函数
    private AsyncOperation(object userSuppliedState, SynchronizationContext syncContext);
    internal static AsyncOperation CreateOperation(object userSuppliedState
                                            , SynchronizationContext syncContext);
 
    // 获取传递给构造函数的SynchronizationContext对象。
    public SynchronizationContext SynchronizationContext { get; }
    // 获取或设置用于唯一标识异步操作的对象。
    public object UserSuppliedState { get; }
 
    // 在各种应用程序模型适合的线程或上下文中调用委托。
    public void Post(SendOrPostCallback d, object arg);
    // 结束异步操作的生存期。
    public void OperationCompleted();
    // 效果同调用 Post() + OperationCompleted() 方法组合
    public void PostOperationCompleted(SendOrPostCallback d, object arg);
}
```

- **AsyncOperationManager静态类**。静态类是密封的，因此不可被继承。倘若从静态类继承会报错“静态类必须从 Object 派生”。（小常识，以前以为密封类就是 sealed 关键字）

- AsyncOperationManager为支持异步方法调用的类提供**并发管理**，该类可正常运行于 .NET Framework 支持的所有应用程序模式下。

- **AsyncOperation实例**提供对特定异步任务的**生存期进行跟踪**。可用来**处理任务完成通知**，还可用于在不终止异步操作的情况下**发布进度报告**和**增量结果**（这种不终止异步操作的处理是通过AsyncOperation的 **Post() 方法**实现）。

- AsyncOperation类有一个**私有的构造函数**和一个内部**CreateOperation() 静态方法**。由AsyncOperationManager类调用AsyncOperation.CreateOperation() 静态方法来创建AsyncOperation实例。（**单例模式？？？**）

- AsyncOperation类是**通过SynchronizationContext类**来实现在各种应用程序的适当“线程或上下文”**调用客户端事件处理程序**。如下：

  ```
  // 提供在各种同步模型中传播同步上下文的基本功能。
  public class SynchronizationContext
  {
      // 获取当前线程的同步上下文。
      public static SynchronizationContext Current { get; }
   
      // 当在派生类中重写时，响应操作已开始的通知。
      public virtual void OperationStarted();
      // 当在派生类中重写时，将异步消息调度到一个同步上下文。
      public virtual void Post(SendOrPostCallback d, object state);
      // 当在派生类中重写时，响应操作已完成的通知。
      public virtual void OperationCompleted();
      ……
  }
  ```

  - 在AsyncOperation构造函数中调用SynchronizationContext的OperationStarted() ;
  - 在AsyncOperation的 Post() 方法中调用SynchronizationContext的Post() 
  - 在AsyncOperation的OperationCompleted()方法中调用SynchronizationContext的OperationCompleted()

- SendOrPostCallback委托签名：

  ```
  // 表示在消息即将被调度到同步上下文时要调用的方法。
  public delegate void SendOrPostCallback(object state);
  ```

### 三、基于事件的异步模式的特征

1. 基于事件的异步模式可以采用多种形式，具体取决于某个特定类支持操作的复杂程度：

   - 最简单的类可能只有一个 **...Async方法**和一个对应的 **...Completed** **事件**，以及这些方法的同步版本

   - 复杂的类可能有若干个 **...Async**方法，每种方法都有一个对应的 **...Completed** 事件，以及这些方法的同步版本。

   - 更复杂的类还可能为每个异步方法支持取消（CancelAsync()方法）、进度报告和增量结果（ReportProgress() 方法+ProgressChanged事件）

   - 如果类支持多个异步方法，每个异步方法返回不同类型的数据，应该：

     - 将**增量结果报告**与**进度报告**分开
     - 使用适当的EventArgs为每个异步方法定义一个单独的 **...ProgressChanged**事件以处理该方法的增量结果数据

   - 如果类不支持多个并发调用，请考虑公开IsBusy属性

   - 如要异步操作的同步版本中有 Out 和 Ref 参数，它们应做为对应 **...CompletedEventArgs**的一部分，如：

     ```
     public int MethodName(string arg1, ref string arg2, out string arg3);
      
     public void MethodNameAsync(string arg1, string arg2);
     public class MethodNameCompletedEventArgs : AsyncCompletedEventArgs
     {
         public int Result { get; };
         public string Arg2 { get; };
         public string Arg3 { get; };
     }
     ```

2.  如果你的组件要支持多个异步耗时的任务并行执行。那么：

   - 为 **...Async**方法多添加一个userState对象参数（此参数应当始终是 **...Async**方法签名中的最后一个参数），用于跟踪各个操作的生存期
   - 注意要在你构建的异步类中维护一个**userState对象的集合**。使用 **lock 区域保护**此集合，因为各种调用都会在此集合中添加和移除userState对象
   - 在 **...Async**方法开始时调用AsyncOperationManager.CreateOperation并传入userState对象，为每个异步任务创建AsyncOperation对象，userState存储在AsyncOperation的UserSuppliedState属性中。在构建的异步类中使用该属性标识取消操作，并传递给CompletedEventArgs和ProgressChangedEventArgs参数的UserState属性来标识当前引发进度或完成事件的特定异步任务
   - 当对应于此userState对象的任务引发完成事件时，你构建的异步类应将AsyncCompletedEventArgs.UserState对象从集合中删除

3. 异常处理

   EAP的错误处理和系统的其余部分不一致。

   - 首先，异常不会抛出。
   - 在你的事件处理方法中，必须查询AsyncCompletedEventArgs的Exception属性，看它是不是null。
   - 如果不是null，就必须使用if语句判断Exception派生对象的类型，而不是使用catch块。

4. 注意

   - 确保 **...EventArgs类**特定于**...方法**。即当使用 **...EventArgs类**时，切勿要求开发人员强制转换类型值

   - 确保始终引发方法名称**Completed 事件**。成功完成、异常或者取消时应引发此事件。任何情况下，应用程序都不应遇到这样的情况：应用程序保持空闲状态，而操作却一直不能完成

   - 确保可以捕获异步操作中发生的任何异常并将捕获的异常指派给 Error 属性

   - 确保 **...CompletedEventArgs 类**将其成员公开为只读属性而不是字段，因为字段会阻止数据绑定。例如:

     ```
     public MyReturnType Result { get; }
     ```

   - 在构建 **...CompletedEventArgs 类**属性时，通过this.RaiseExceptionIfNecessary() 方法确保属性值被正确使用。Eg：

     ```
     private bool isPrimeValue;
     public bool IsPrime
     {
         get
         {
             RaiseExceptionIfNecessary();
             return isPrimeValue;
         }
     }
     ```

     所以，在 **...Completed事件**处理程序中，应当总是先检查 **...CompletedEventArgs.Error** 和 **...CompletedEventArgs.Cancelled** 属性，然后再访问RunWorkerCompletedEventArgs.Result属性。





























