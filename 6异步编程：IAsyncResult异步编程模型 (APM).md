---
typora-copy-images-to: MarkDownPics
---

�첽���ϵ������6�����첽��̣�IAsyncResult�첽���ģ�� (APM)��
http://www.cnblogs.com/heyuquan/archive/2013/03/22/2976420.html



�ڿ������߳�Ӧ�ó���ʱ���ܶ��˶���ʹ��ThreadPool��QueueUserWorkItem����������һ�μ򵥵��첽������Ȼ���������������������ơ�����������û��һ���ڽ��Ļ�������֪��������ʲôʱ����ɣ�Ҳû��һ�������ڲ������ʱ���һ������ֵ��

> ���̳߳ش����߳������ַ�����
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
> �ڶ��ַ����ǿ��Զ��߳̽��о�ȷ���Ƶģ��������ƽ�������ʱ�������ص������ȡ�

Microsoft���������첽���ģʽ��

- .NET 1.0 �첽���ģʽ����APM������IAsyncResult�ӿ�ʵ�֡������ĵ���
- .NET 2.0 �����¼����첽���ģʽ����EAP�������¼�ʵ�֡�
- .NET 4.x ����������첽���ģʽ����TAP�����ͱ��ģʽ���Ƽ���

### 0��IAsyncResult���ģʽ�����淶

ʹ��IAsyncResult���ģʽ���첽������ͨ����Ϊ Begin*** �� End*** ������������ʵ�ֵģ������������ֱ�ָ����ʼ�ͽ����첽����������:

> FileStream���ṩ**BeginRead**��**EndRead**���������ļ��첽��ȡ�ֽڡ�
>
> ����������ʵ���� **Read** �������첽�汾��

�ڵ��� Begin...��Ӧ�ó�����Լ����ڵ����߳���ִ��ָ�ͬʱ�첽��������һ���߳���ִ�С�������з���ֵ��Ӧ���� End... ����ȡ�����Ľ������

1. Begin...()
   - Begin...()�������и÷�����ͬ���汾ǩ�����������κβ�����

   - Begin...()����ǩ���в������κ����������ǩ���� **�����������** �Ĺ淶�ǣ�

     - ��һ����������һ��AsyncCallbackί�У�������������**�첽�������ʱ**���õķ�����
     - �ڶ���������һ���û�����Ķ�������AsyncCallbackί�з�����ͳ״̬��Ϣ����ͨ���˶�����ί���з���End...()������
     - ���������������Դ���**null**��

   - ����IAsyncResult����

     ```
     // ��ʾ�첽������״̬��
     [ComVisible(true)]
     public interface IAsyncResult
     {
         // ��ȡ�û�����Ķ������޶�����������첽��������Ϣ��
         object AsyncState { get; }
         // ��ȡ���ڵȴ��첽������ɵ�System.Threading.WaitHandle�����첽�������ʱ����źš�
         WaitHandle AsyncWaitHandle { get; }
         // ��ȡһ��ֵ����ֵָʾ�첽�����Ƿ�ͬ����ɡ�
         bool CompletedSynchronously { get; }
         // ��ȡһ��ֵ����ֵָʾ�첽�����Ƿ�����ɡ�
         bool IsCompleted { get; }
     }
      
     // ����ί���������Һ���ʾ����ʹ�����Զ���Ĵ�ref������ί�У�
     public delegate void AsyncCallback(IAsyncResult ar)
     ```

2. End...()

   - End...()�����ɽ����첽�������������End...()ʱ��IAsyncResult�����ʾ���첽������δ��ɣ���End...()���������첽�������ǰ���������̡߳�
   - End...()�����ķ���ֵ����ͬ�������ķ���ֵ������ͬ��End...()�������и÷���ͬ���汾��ǩ��������������out��ref�������Լ���BeginInvoke���ص�IAsyncResult���淶��IAsyncResult�����ŵ����
     - Ҫ���÷���ֵ��������÷�����
     - ������out��ref������ʵ����ί��ҲҪ����out��ref�������Ա��ڻص��л�ö�Ӧ���ô���ֵ����Ӧ�߼���

3. ���ǵ���End...()����������ֻ����һ��

   > **�첽����Ӧ�����صĹ���**��
   >
   > I/O���Ƶ��첽�������������FileOptions.Asynchronous��ʶ��FileStream����BeginRead()������Windows����һ��I/O�������I/O Request Packet��IRP���󷽷�**���������̶߳�����������**��**��Windows��IRP���͸��ʵ����豸��������**��IRP�а�����ΪBeginRead()��������Ļص�������**��Ӳ���豸�����IRP�󣬻ὫIRP��ί���Ŷӵ�CLR���̳߳ض�����**��
   �������End...()����������������Դй©��

   - ���첽����ʱ������I/O���Ʋ�����CLR�����һЩ�ڲ���Դ���������ʱ��CLR����������Щ��Դֱ��End...()���������á����һֱ������End...()����Щ��Դ��ֱ��������ֹʱ�Żᱻ���ա���End...()��������г���������Դ�ͷţ�
   - ����һ���첽����ʱ��ʵ���ϲ���֪���ò��������ǳɹ�����ʧ�ܣ���Ϊ������Ӳ����ִ�У���Ҫ֪����һ�㣬ֻ��ͨ������End***������������ķ���ֵ���߿����Ƿ��׳��쳣��
   - ��Ҫע�����I/O���Ƶ��첽������ȫ��֧��ȡ������Ϊ������Ӳ��ִ�У�������������һ����ʶ�������ʱ���������ģ��ȡ����Ϊ��



����ͨ��IAsyncResult�첽���ģʽ���������䳡�����������

### һ������IAsyncResult����һ���첽API

1. ����һ����չIAsyncResult�ӿڵ��࣬����ʵ���첽���á���**��ʾ������AsyncAPM6_1��**��

2. ʹ��ί�н����첽��̣�**��ʾ������AsyncAPM6_2��**��

   /// ʹ����������ʽ�첽������1.2.3)��һ�ַ�����ʽ�첽����(4)��
   /// 1. �� EndInvoke ����
   /// 2. �� WaitHandle ����
   /// 3. �� IsCompleted��ѯ ����
   /// 4. ʹ�ûص�������������ʽ��Ӧ�첽���÷���

3. ���̲߳ٿ�

   ���� Windows ����ؼ������ϲ����̰߳�ȫ�ġ�ȷ�����̰߳�ȫ��ʽ���ʿؼ��ǳ���Ҫ������Щ����£���������Ҫ���̵߳��ÿؼ��ķ�����.NET Framework �ṩ�˴��κ��̲߳����ؼ��ķ�ʽ��

   - �ǰ�ȫ��ʽ���ʿؼ���**�˷�ʽ����Զ��Ҫ��ʹ��**��

     ```
     // ��ȡ������һ��ֵ����ֵָʾ�Ƿ񲶻�Դ����̵߳ĵ��ã�
     // ��Щ�����ڵ���Ӧ�ó���ʱ���ʿؼ���System.Windows.Forms.Control.Handle���ԡ�
     // ��������˶Դ����̵߳ĵ��ã���Ϊ true������Ϊ false��
     public static bool CheckForIllegalCrossThreadCalls { get; set; }
     ```

   -   ��ȫ��ʽ���ʿؼ�

     ԭ����һ���̷߳��͵��ò����̱߽߳罫�䷢�͵���һ���̣߳��������ò��뵽�����ؼ��̵߳���Ϣ������,���ؼ������̴߳��������Ϣʱ,�ͻ����Լ�����������ִ�д���ķ��������˹���ֻ�е����̺߳ʹ����ؼ����̣߳���û�д������̣߳�

     ע�⣺��һ���̷߳��͵��ò����̱߽߳罫�䷢�͵���һ���̻߳�**�ķѴ�����ϵͳ��Դ**������Ӧ�����ظ����������߳��ϵĿؼ���

     - ʹ��BackgroundWork��̨�����߳̿ؼ���ʽ������������¼����첽���ģʽ(EAP)��
     - ���TaskScheduler.FromCurrentSynchronizationContext()��Taskʵ��
     - �����߳�������ExecuteContext��������ExeceteContext.Run()��̬������ָ�����߳���������ִ�С��������2.�첽��̣�ʹ���̳߳ع�����ִ�������ģ�
     - ʹ��Control�����ṩ��Invoke��BeginInvoke����
     - ��WPFӦ�ó����п���ͨ��WPF�ṩ��Dispatcher�����ṩ��Invoke��BeginInvoke��������ɿ��̹߳�����

     > ������Ҫ��˵IAsyncResult�첽���ģʽ������ֻ��ϸ����Invoke ��BeginInvoke���̷߳��ʿؼ���ʽ��   
     >
     > Control��ʵ����**ISynchronizeInvoke**�ӿڣ��ṩ��Invoke��BeginInvoke������֧�������̸߳���GUI����ؼ��Ļ��ơ�

     ```
     public interface ISynchronizeInvoke
     {
         // ��ȡһ��ֵ����ֵָʾ�����߳��Ƿ���ؼ��Ĵ����߳���ͬ��
         bool InvokeRequired { get; }
         // �ڿؼ��������߳����첽ִ��ָ��ί�С�
         AsyncResult BeginInvoke(Delegate method, params object[] args);
         object EndInvoke(IAsyncResult asyncResult);
         // �ڿؼ��������߳���ͬ��ִ��ָ��ί�С�
         object Invoke(Delegate method, params object[] args);
     }
     ```

     1. Control���Invoke��BeginInvoke

        - Invoke����ͬ�����ã����жϿؼ������߳��뵱ǰ�߳��Ƿ���ͬ����ͬ��ֱ�ӵ���ί�з���������ʹ��Win32API��PostMessage �첽ִ��,���� Invoke �ڲ������IAsyncResult.AsyncWaitHandle�ȴ�ִ�����
        - BeginInvoke�����첽���ã�ʹ��Win32API��PostMessage �첽ִ�У����ҷ��� IAsyncResult ���� 

        ```
        UnsafeNativeMethods.PostMessage(new HandleRef(this, this.Handle)
                          , threadCallbackMessage, IntPtr.Zero, IntPtr.Zero);
                          
        [DllImport("user32.dll", CharSet=CharSet.Auto)]
        public static extern bool PostMessage(HandleRefhwnd, intmsg, IntPtrwparam, IntPtrlparam);
        ```

        **PostMessage** ��windows api��������һ����Ϣ���͵�һ�����ڵ���Ϣ���С����**����**��**�첽**�ģ�Ҳ���Ǹ÷���������Ϻ����Ϸ��أ�����ȴ�ί�з�����ִ�н������������߳̽����ᱻ������

        ����Ӧͬ��������windows api�ǣ�**SendMessage**()����Ϣ���������Ϣͨ������GetMessage��PeekMessageȡ�ã�

     2. InvokeRequired

        ��ȡһ��ֵ����ֵָʾ�����߳��Ƿ���ؼ��Ĵ����߳���ͬ���ڲ��ؼ����£�

        ```
        Int windowThreadProcessId = SafeNativeMethods.GetWindowThreadProcessId(ref2, out num);
        Int currentThreadId = SafeNativeMethods.GetCurrentThreadId();
        return (windowThreadProcessId != currentThreadId);
        ```

     3. ʾ��:��**��ʾ������AsyncAPM6_3���̸߳�ֵ��**��

        ע�⣬��InvokeControl������ʹ�� this.Invoke(Delegate del) ��ʹ�� this.textBox1.Invoke(Delegate del) Ч����һ���ġ�

     4. �쳣��Ϣ��

        "�ڴ���**���ھ��**֮ǰ,�����ڿؼ��ϵ��� Invoke �� BeginInvoke"

        - �������ڴ��廹δ�������ʱ���ڹ��캯�����첽ȥ������Invoke ��BeginInvoke��

        -  ������ʹ�ø����̴߳���һ�����ڲ���Application.Run()ȥ����������ھ��δ������֮ǰ������Invoke ��BeginInvoke������ʱ�½��Ĵ����൱�ڿ�����һ�����̣�����Ϊ�´��ڹ����ĸ����߳̿�������Ϣѭ�����ƣ�������������룺

          ```
          new Thread((ThreadStart)delegate
              {
                  WaitBeforeLogin = new Form2();
                  Application.Run(WaitBeforeLogin);
              }).Start();
          ```

        ����������ڵ���Invoke �� BeginInvoke֮ǰ��ѯ��鴰�ڵ�IsHandleCreated���ԡ�

        ```
        // ��ȡһ��ֵ����ֵָʾ�ؼ��Ƿ������������ľ����
        public bool IsHandleCreated { get; }
        while (!this.IsHandleCreated) { ���� }
        ```



������Ҫ�����첽���ģʽ֮һ���첽���ģ�ͣ�APM�������ǻ���IAsyncResult���ģʽʵ�ֵ��첽��̷�ʽ�����ҹ�����һ���̳���IAsyncResult�ӿڵ�ʾ������չʾ������ģʽ��ί�м����̷߳��ʿؼ��ϵľ���Ӧ�á�

��һ�ڣ����ܻ����¼��ı��ģ�͡��� 