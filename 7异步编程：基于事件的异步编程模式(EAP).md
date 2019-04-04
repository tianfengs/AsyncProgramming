---
typora-copy-images-to: MarkDownPics
---

�첽���ϵ������7�����첽��̣������¼����첽���ģʽ(EAP)��
http://www.cnblogs.com/heyuquan/archive/2013/04/01/2993085.html

��**.NET1.0 IAsyncResult�첽���ģ��(APM)**����ͨ��Begin*** ��������������IAsyncResult����ʹ�� End*** ����������������ͨ���ص����������첽�������������

**����������û���ṩ"����֪ͨ"�ȹ��ܼ�"���̼߳�ؼ��ķ���"**��

.NET2.0 �������ˣ������¼����첽���ģʽ(EAP��Event-based Asynchronous Pattern)��

ͨ���¼���AsyncOperationManager���AsyncOperation��������������ʵ�����¹��ܣ�

> 1. �첽ִ�к�ʱ������
> 2. ��ý��ȱ�����������
> 3. ֧�ֺ�ʱ�����ȡ��
> 4. �������Ľ��ֵ���쳣��Ϣ
> 5.  �����ӣ�֧��ͬʱִ�ж���첽���������ȱ��桢���������ȡ�����������ؽ��ֵ���쳣��Ϣ

������Լ򵥵Ķ��߳�Ӧ�ó���BackgroundWorker����ṩ��һ���򵥵Ľ�����������ڸ����ӵ��첽Ӧ�ó��򣬿��Կ���ʵ��һ�����ϻ����¼����첽ģʽ���ࡣ

### һ��EAP�첽���ģ�͵��ŵ�

EAP��**ΪWindows���忪����Ա**�����ģ�����Ҫ�ŵ����ڣ�

1.  EAP��Microsoft Visual Studio UI����������˺ܺõļ��ɡ�

   Ҳ����˵���ɽ������ʵ����EAP�����Ϸŵ�һ��Visual Studio�����ƽ���ϣ�Ȼ��˫���¼�������Visual Studio�Զ������¼��ص���������������ͬ�¼�����������

2. EAP�����ڲ�ͨ��SynchronizationContext�࣬��Ӧ�ó���ģ��ӳ�䵽�����̴߳���ģ�ͣ��Է�����̲߳����ؼ���

### ����������������

Ϊ��ʵ�ֻ����¼����첽ģʽ�����Ǳ��������������Ҫ�İ������ࣺ

- **AsyncOperationManager**
- **AsyncOperation**

AsyncOperationManager���AsyncOperation����**System.ComponentModel**�����ռ�Ϊ�����ṩ��������Ҫ�������ࡣ

�ڻ����¼����첽ģʽ��װ��׼�����첽�����У���ȷ������첽����֧����**����Ӧ�ó���ģ�ͣ����� ASP.NET������̨Ӧ�ó���� Windows ����Ӧ�ó���**���ʵ����̻߳������ġ����ÿͻ����¼��������

AsyncOperationManager���AsyncOperation���API���£�

```
// Ϊ֧���첽�������õ����ṩ���������������಻�ܱ��̳С�
public static class AsyncOperationManager
{
    // ��ȡ�����������첽������ͬ�������ġ�
    public static SynchronizationContext SynchronizationContext { get; set; }
 
    // ���ؿ����ڶ��ض��첽�����ĳ���ʱ����и��ٵ�AsyncOperation����
    // ����:userSuppliedState:
    //     һ����������ʹһ���ͻ���״̬�������� ID����һ���ض��첽�����������
    public static AsyncOperation CreateOperation(object userSuppliedState)
    {
        return AsyncOperation.CreateOperation(userSuppliedState,SynchronizationContext);
    }
}
 
// �����첽�����������ڡ�
public sealed class AsyncOperation
{
    // ���캯��
    private AsyncOperation(object userSuppliedState, SynchronizationContext syncContext);
    internal static AsyncOperation CreateOperation(object userSuppliedState
                                            , SynchronizationContext syncContext);
 
    // ��ȡ���ݸ����캯����SynchronizationContext����
    public SynchronizationContext SynchronizationContext { get; }
    // ��ȡ����������Ψһ��ʶ�첽�����Ķ���
    public object UserSuppliedState { get; }
 
    // �ڸ���Ӧ�ó���ģ���ʺϵ��̻߳��������е���ί�С�
    public void Post(SendOrPostCallback d, object arg);
    // �����첽�����������ڡ�
    public void OperationCompleted();
    // Ч��ͬ���� Post() + OperationCompleted() �������
    public void PostOperationCompleted(SendOrPostCallback d, object arg);
}
```

- **AsyncOperationManager��̬��**����̬�����ܷ�ģ���˲��ɱ��̳С������Ӿ�̬��̳лᱨ����̬������ Object ����������С��ʶ����ǰ��Ϊ�ܷ������ sealed �ؼ��֣�

- AsyncOperationManagerΪ֧���첽�������õ����ṩ**��������**����������������� .NET Framework ֧�ֵ�����Ӧ�ó���ģʽ�¡�

- **AsyncOperationʵ��**�ṩ���ض��첽�����**�����ڽ��и���**��������**�����������֪ͨ**�����������ڲ���ֹ�첽�����������**�������ȱ���**��**�������**�����ֲ���ֹ�첽�����Ĵ�����ͨ��AsyncOperation�� **Post() ����**ʵ�֣���

- AsyncOperation����һ��**˽�еĹ��캯��**��һ���ڲ�**CreateOperation() ��̬����**����AsyncOperationManager�����AsyncOperation.CreateOperation() ��̬����������AsyncOperationʵ������**����ģʽ������**��

- AsyncOperation����**ͨ��SynchronizationContext��**��ʵ���ڸ���Ӧ�ó�����ʵ����̻߳������ġ�**���ÿͻ����¼��������**�����£�

  ```
  // �ṩ�ڸ���ͬ��ģ���д���ͬ�������ĵĻ������ܡ�
  public class SynchronizationContext
  {
      // ��ȡ��ǰ�̵߳�ͬ�������ġ�
      public static SynchronizationContext Current { get; }
   
      // ��������������дʱ����Ӧ�����ѿ�ʼ��֪ͨ��
      public virtual void OperationStarted();
      // ��������������дʱ�����첽��Ϣ���ȵ�һ��ͬ�������ġ�
      public virtual void Post(SendOrPostCallback d, object state);
      // ��������������дʱ����Ӧ��������ɵ�֪ͨ��
      public virtual void OperationCompleted();
      ����
  }
  ```

  - ��AsyncOperation���캯���е���SynchronizationContext��OperationStarted() ;
  - ��AsyncOperation�� Post() �����е���SynchronizationContext��Post() 
  - ��AsyncOperation��OperationCompleted()�����е���SynchronizationContext��OperationCompleted()

- SendOrPostCallbackί��ǩ����

  ```
  // ��ʾ����Ϣ���������ȵ�ͬ��������ʱҪ���õķ�����
  public delegate void SendOrPostCallback(object state);
  ```

### ���������¼����첽ģʽ������

1. �����¼����첽ģʽ���Բ��ö�����ʽ������ȡ����ĳ���ض���֧�ֲ����ĸ��ӳ̶ȣ�

   - ��򵥵������ֻ��һ�� **...Async����**��һ����Ӧ�� **...Completed** **�¼�**���Լ���Щ������ͬ���汾

   - ���ӵ�����������ɸ� **...Async**������ÿ�ַ�������һ����Ӧ�� **...Completed** �¼����Լ���Щ������ͬ���汾��

   - �����ӵ��໹����Ϊÿ���첽����֧��ȡ����CancelAsync()�����������ȱ�������������ReportProgress() ����+ProgressChanged�¼���

   - �����֧�ֶ���첽������ÿ���첽�������ز�ͬ���͵����ݣ�Ӧ�ã�

     - ��**�����������**��**���ȱ���**�ֿ�
     - ʹ���ʵ���EventArgsΪÿ���첽��������һ�������� **...ProgressChanged**�¼��Դ���÷����������������

   - ����಻֧�ֶ���������ã��뿼�ǹ���IsBusy����

   - ��Ҫ�첽������ͬ���汾���� Out �� Ref ����������Ӧ��Ϊ��Ӧ **...CompletedEventArgs**��һ���֣��磺

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

2.  ���������Ҫ֧�ֶ���첽��ʱ��������ִ�С���ô��

   - Ϊ **...Async**���������һ��userState����������˲���Ӧ��ʼ���� **...Async**����ǩ���е����һ�������������ڸ��ٸ���������������
   - ע��Ҫ���㹹�����첽����ά��һ��**userState����ļ���**��ʹ�� **lock ���򱣻�**�˼��ϣ���Ϊ���ֵ��ö����ڴ˼�������Ӻ��Ƴ�userState����
   - �� **...Async**������ʼʱ����AsyncOperationManager.CreateOperation������userState����Ϊÿ���첽���񴴽�AsyncOperation����userState�洢��AsyncOperation��UserSuppliedState�����С��ڹ������첽����ʹ�ø����Ա�ʶȡ�������������ݸ�CompletedEventArgs��ProgressChangedEventArgs������UserState��������ʶ��ǰ�������Ȼ�����¼����ض��첽����
   - ����Ӧ�ڴ�userState�����������������¼�ʱ���㹹�����첽��Ӧ��AsyncCompletedEventArgs.UserState����Ӽ�����ɾ��

3. �쳣����

   EAP�Ĵ������ϵͳ�����ಿ�ֲ�һ�¡�

   - ���ȣ��쳣�����׳���
   - ������¼��������У������ѯAsyncCompletedEventArgs��Exception���ԣ������ǲ���null��
   - �������null���ͱ���ʹ��if����ж�Exception������������ͣ�������ʹ��catch�顣

4. ע��

   - ȷ�� **...EventArgs��**�ض���**...����**������ʹ�� **...EventArgs��**ʱ������Ҫ�󿪷���Աǿ��ת������ֵ

   - ȷ��ʼ��������������**Completed �¼�**���ɹ���ɡ��쳣����ȡ��ʱӦ�������¼����κ�����£�Ӧ�ó��򶼲�Ӧ���������������Ӧ�ó��򱣳ֿ���״̬��������ȴһֱ�������

   - ȷ�����Բ����첽�����з������κ��쳣����������쳣ָ�ɸ� Error ����

   - ȷ�� **...CompletedEventArgs ��**�����Ա����Ϊֻ�����Զ������ֶΣ���Ϊ�ֶλ���ֹ���ݰ󶨡�����:

     ```
     public MyReturnType Result { get; }
     ```

   - �ڹ��� **...CompletedEventArgs ��**����ʱ��ͨ��this.RaiseExceptionIfNecessary() ����ȷ������ֵ����ȷʹ�á�Eg��

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

     ���ԣ��� **...Completed�¼�**��������У�Ӧ�������ȼ�� **...CompletedEventArgs.Error** �� **...CompletedEventArgs.Cancelled** ���ԣ�Ȼ���ٷ���RunWorkerCompletedEventArgs.Result���ԡ�





























