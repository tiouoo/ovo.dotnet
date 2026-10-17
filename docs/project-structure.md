# 项目结构与命名

目录与命名空间一致，不再使用统一的 `Modules` 外层或含义模糊的 `Helper` 分类。

| 目录 | 职责 |
| --- | --- |
| Application | 应用启动配置 |
| Events | 设置、退出与 UI 生命周期事件 |
| Extensions | 按目标类型划分的扩展方法 |
| IO | 文件系统操作 |
| Logging | 文件日志与日志级别 |
| Net | URL 处理 |
| Platform | 桌面平台与运行环境识别 |
| Tasks | 可观察任务树、状态、进度与操作 |
| Threading | 防抖与可取消的周期任务 |
| UI/Converters | Avalonia 值转换器 |

## 迁移

这是公开 API 的破坏性重构；调用方需要更新 using、类型名和下表列出的成员。

| 原名称 | 新名称 |
| --- | --- |
| Standard.Modules.* | Standard.*（按上述职责分类） |
| Initializer.Program | Application.ApplicationBootstrapper.Initialize |
| DiskIO.Helper.TryCreateFolder | IO.DirectoryUtilities.EnsureDirectoryExists |
| DiskIO.Helper.ClearFolder / TryClearFolder | IO.DirectoryUtilities.ClearDirectory / TryClearDirectory |
| DiskIO.Logger | Logging.Logger |
| Helper.Debouncer | Threading.Debouncer（实现 IDisposable） |
| Portal.Core.Helpers.UrlHelper | Standard.Net.UrlUtilities |
| RunnerType / RunnerTypeDetector | RuntimeKind / RuntimeEnvironment |
| CurrentRunner / CurrentPlatform | Current |
| DesktopType / DesktopTypeDetector | DesktopPlatform / DesktopPlatformDetector |
| LoopGc.BeginLoop | Threading.GarbageCollectionScheduler.RunAsync(interval, cancellationToken) |
| ByteSize.GetReadableRaw | ByteSizeExtensions.ToReadableSize |
| AppExiting / RaiseAppExiting | Exiting / RaiseExitingAsync |
| AppDragDrop / RaiseAppDragDrop | DragDrop / RaiseDragDrop |
| BeforeReadSettings / BeforeUiLoaded / AfterUiLoaded | BeforeSettingsLoad / BeforeUiLoad / AfterUiLoad（Raise 方法同改） |

扩展类统一使用 `Extensions` 后缀，并拆分为 String、Object、Collection、Json、DateTime、Number、ByteSize。除明确列出的成员外，扩展调用语法不变。

删除依赖旧 TioUi 的 ITioWindow；窗口操作由应用自身负责。转换器和拖放事件通过显式 Avalonia 包引用保留。

DirectoryUtilities 的排除路径支持文件与目录；保留被排除项的祖先目录，不递归进入符号链接。TryClearDirectory 返回成功与否，仅捕获文件系统异常。

防抖回调在计时器线程执行。周期 GC 需要显式调用并传入取消令牌，常规应用建议使用运行时自动回收。

## 验证

```powershell
dotnet build Standard.slnx
dotnet run --project tests/Standard.RegressionTests
```
