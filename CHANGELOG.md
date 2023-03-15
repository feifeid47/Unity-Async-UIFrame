# Changelog

这是更新日志

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