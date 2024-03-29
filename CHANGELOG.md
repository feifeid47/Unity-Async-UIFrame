# Changelog
## [1.1.8] - 2024-03-24
### Added
- 添加Button按钮点击事件自动绑定的属性，并删除Demo2（与Demo2的使用稍有不同）
```C#
// 实现一
class TestUI : UIBase
{
    private Button btnTest;

    protected override void OnBind()
    {
        btnTest.onClick.AddListener(OnClickBtnTest);
    }

    protected override void OnUnbind()
    {
        btnTest.onClick.RemoveListener(OnClickBtnTest);
    }

    private void OnClickBtnTest()
    {
    }
}
// 上面的代码可以使用UGUIButtonEvent属性，简化成下面的
// @BtnTest为按钮名称。如果子UI也有同样的名称，并不会引用子UI的按钮
// 实现二
class TestUI: UIBase
{
    [UGUIButtonEvent(name: "@BtnTest")]
    private void OnClickBtnTest()
    {
    }
}
```
### Fixed
- 修复自动引用会引用子UI中的节点问题，再也不用担心父UI和子UI中因为节点名称一样而导致自动引用错误  

## [1.1.7] - 2024-03-17
### Added
- 新增Timer定时器属性
```C#
// 实现一
class TestUI : UIBase
{
    private UITimer timer = null;

    protected override void OnBind()
    {
        timer = UIFrame.CreateTimer(delay: 1, UpdateEverySecond, isLoop: true);
    }

    protected override void OnUnbind()
    {
        timer.Cancel();
    }

    private void UpdateEverySecond()
    {
        // 每秒更新
    }
}
// 上面的代码可以使用UITimer属性，简化成下面的
// 实现二
class TestUI: UIBase
{
    [UITimer(delay: 1f, isLoop: true)]
    private void UpdateEverySecond()
    {
        // 每秒更新
    }
}

```

## [1.1.6] - 2024-03-10
### Added
- 新增Timer定时器
```C#
// 使用UIFrame创建定时器
UITimer timer = UIFrame.CreateTimer(0.3f, () => { }, isLoop = true);
// 取消定时器
timer.Cancel();

// UIBase中的创建定时器
class TestUI: UIBase
{
    protected override Task OnCreate()
    {
        # 创建定时器，gameObject被销毁时会自动Cancel
        var timer = this.CreateTimer(0.3f, () => { }, isLoop: true);
        return Task.CompletedTask;
    }
}

```

## [1.1.5] - 2024-01-27
### Fixed
- 修复UIFrame.Instantiate实例化的UI没有设置父物节点的问题

## [1.1.4] - 2024-01-24
### Fixed
- 修复Release中的错误

## [1.1.3] - 2023-09-24
### Fixed
- 修复Auto Destroy为false时，数据不更新的BUG

## [1.1.2] - 2023-08-13
### Changed
- UIWindow,UIPanel属性改为WindowLayer,PanelLayer
- UIFrame.IsPanel和UIFrame.IsWindow被删除，使用UIFrame.GetLayer来代替。例如：UIFrame.GetLayer(type) is PanelLayer
- UIFrame预制体结构发生变化，请参考Demo

### Added
- 新增UIFrame.GetLayer, UIFrame.GetLayerTransform
- 新增多层UI控制，可以继承自UILayer来实现多层控制，例如：BattleLayer用来显示战斗UI，NewbieLayer用来显示新手引导UI等等，不同层之间相互不影响。

## [1.1.1] - 2023-08-05

### Changed
- UIFrame.Show带返回值，返回显示的UI
- UIFrame.Hide新增forceDestroy参数，隐藏UI时并强制销毁(即使AutoDestroy为false)

### Added
- 新增UIFrame.Get和UIFrame.TryGet，获得已经实例化的UI
- 新增UIFrame.GetAll，获得所有已经实例化的UI
- 新增UIFrame.RefreshAll，刷新所有UI
```
UIFrame.GetAll
可以通过predicate方法来筛选
例如：
（1）所有已经实例化的UI，UIGrame.GetAll();
（2）所有已经实例化的Panel，UIFrame.GetAll(type => UIFrame.IsPanel(type));
（3）所有已经实例化的Window，UIFrame.GetAll(type => UIFrame.IsWindow(type));
（4）所有已经实例化的可见的UI，UIFrame.GetAll(type => UIFrame.Get(type).gameObject.activeInHierarchy);

UIFrame.RefreshAll
使用场景：
（1）切换多语言时刷新所有UI，更新界面显示的语言
（2）其他需要刷新所有UI的情况
```
### Fixed
- 修复timeout可能存在未Cancel的情况

## [1.1.0] - 2023-04-24

### Changed
- UIFrame.Refresh新增默认参数
- UIFrame.TrySetData改为私有

## [1.0.9] - 2023-04-13

### Added
- 支持Unity2019.4版本

### Changed
- 修改demo导入方式

## [1.0.8] - 2023-04-11

### Fixed
- 修复UIFrame.Instantiate创建物体时还未初始化完成就显示出来的BUG
- Demo添加到UIFrame.Demo程序集中

## [1.0.7] - 2023-04-10

### Added
- 新增Demo

### Fixed
- 修复UIFrame.Instantiate没有正确设置Parent和Children
- 修复OnCreate中访问UIDate时为空的BUG

## [1.0.6] - 2023-04-06

### Fixed
- 修复BUG

## [1.0.5] - 2023-03-16

### Changed
- 修改UIFrame.OnAssetRequest
- ShowAsync，HideAsync修改为私有

### Fixed
- 修复遗漏的异常捕获
- 修复UIFrame.HideAsync空引用异常

## [1.0.4] - 2023-03-15

### Fixed

- 修复UIFrame.Show显示UI时使用默认参数时导致的错误
- 修复UIFrame.Hide中遗漏的timeout.Cancel()

## [1.0.3] - 2023-03-15

### Added

- 创建UIFrameSetting时自动填入默认模板文件
- 自动引用功能新增目录扫描，推荐在打包前调用一次，保证自动引用的权威
```C#
var path = "Assets/Prefabs";
UIAutoReference.SetReference(path);
```

## [1.0.2] - 2023-03-12

### Changed

- 重构UIFrame

## [1.0.1] - 2023-03-03

### Changed

- 修改UIBase的生命周期

## [1.0.0] - 2023-01-08

第一次发布 *UIFrame*。