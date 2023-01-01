# 特点
```
(1) 一个简单易用的异步UI框架  
(2) 兼容多种资源管理系统（Addressable、YooAssets等）  
(3) 支持自动引用，暴露在inspector面板上的字段会自动从Hierarchy面板引用  
```
# 如何使用
初始化
```C#
private void Awake()
{
    // 注册资源请求释放事件
    UIFrame.OnAssetRequest += OnAssetRequest;
    UIFrame.OnAssetRelease += OnAssetRelease;
    // 注册UI卡住事件
    // 加载时间超过0.5s后触发UI卡住事件
    UIFrame.StuckTime = 0.5f;
    UIFrame.OnStuckStart += OnStuckStart;
    UIFrame.OnStuckEnd += OnStuckEnd;
}

// 资源请求事件，name为Prefab的名称。如：MainUI、TestUI
// 可以使用Addressables，YooAssets等第三方资源管理系统
private async void OnAssetRequest(string name, Action<GameObject> response)
{
    if (!handles.ContainsKey(name))
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(name);
        await handle.Task;
        handles[name] = handle;
    }
    response.Invoke(handles[name].Result);
}

// 资源释放事件
private void OnAssetRelease(string name)
{
    if(handles.ContainsKey(name))
    {
        handles[name].Release();
        handles.Remove(name);
    }
}

private void OnStuckStart(string name)
{
    // UI初始化加时间过长，卡住了,打开转圈面板
}

private void OnStuckEnd(string name)
{
    // 不卡了，关闭转圈面板
}
```
创建一个UI脚本，继承自UIComponent<T>
并挂到一脚本同名的Prefab中
```C#
public class TestUIProperties : UIProperties
{

}

public class TestUI : UIComponent<TestUIProperties>
{
    // 暴露在Inspector面板上的字段会自动引用
    // 只需让Hierarchy面板上的对象的名称与字段名称保持一致（不区分大小写），并以@开头
    // 如： 
    // @Content
    // @CloseBtn
    // @Img

    [SerializeField] private Text content;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Image img;

    // 初始化方法，生命周期内仅调用1次
    // 资源加载，网络请求，等耗时操作放在此处执行
    // 加载的资源或句柄请在OnDestroy方法内回收
    public async override Task Initialize()
    {
        await UnityWebRequest.Get("XXX").SendWebRequest();
        await Task.Delay(1000);
        var handle = Addressables.LoadAssetAsync<Sprite>("XXX");
        await handle;
        // ......... 
        // .........
    }

     // 注册事件，当UI显示时调用
    public override void AddListeners()
    {
        closeBtn.onClick.AddListener(OnClose);
    }

    // 注销事件，当UI隐藏时调用
    public override void RemoveListeners()
    {
        closeBtn.onClick.RemoveListener(OnClose);
    }

    // 刷新UI，UI数据更新放到此处执行
    public override void Refresh()
    {
        content.text = $"Sender = {this.Properties.Sender}";
    }

    private void OnClose()
    {
        // UIFrame.CloseWindow<TestUI>();
        // UIFrame.HidePanel();
    }

    private void OnDestroy()
    {
        // handle.Release();
    }
}
```
使用以下方法对UI进行控制  
Panel由栈进行控制，显示下一个Panel时会将当前Panel关闭，隐藏当前Panel时会显示上一个Panel  
Window一般用作弹窗，它显示在Panel之上，使用OpenWindow和CloseWindow进行控制  
一个UI不能既由充当Panel又充当Window，即不能使用ShowPanel，HidePanel方法的同时又使用OpenWindow，CloseWindow  
```C#
// 显示Panel
UIFrame.ShowPanel<TestUI>(new TestUIProperties());
// 隐藏Panel
UIFrame.HidePanel();
// 打开Window
UIFrame.OpenWindow<TestUI>(new TestUIProperties());
// 关闭Window
UIFrame.CloseWindow<TestUI>();
// 刷新UI
UIFrame.Refresh<TestUI>();
```