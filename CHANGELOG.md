# Changelog

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