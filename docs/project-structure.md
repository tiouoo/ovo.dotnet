# 项目结构与命名

目录与命名空间一致，不再使用统一的 `Modules` 外层或含义模糊的 `Helper` 分类。

## 项目与依赖

解决方案包含三个项目，均以 .NET 10 为目标：

| 项目 | 职责 | 依赖 |
| --- | --- | --- |
| Standard | 通用工具、日志、平台识别、通用生命周期事件和后台任务扩展 | 无包依赖、无项目依赖 |
| Standard.Mvvm | 可观察任务树、状态、进度与命令 | Standard、CommunityToolkit.Mvvm |
| Standard.Avalonia | Avalonia 值转换器与拖放事件 | Standard、Avalonia |

`Standard.Mvvm` 与 `Standard.Avalonia` 相互独立。使用任务管理和 Avalonia UI 的应用应同时引用这两个项目；核心项目不依赖任何 UI 或 MVVM 框架。

本次拆分需要调用方更新以下引用：

| 原 API | 新 API |
| --- | --- |
| Standard.Tasks 中的任务管理类型 | Standard.Mvvm.Tasks（TaskExtensions 除外） |
| Standard.UI.Converters.CharacterWrapConverter | Standard.Avalonia.UI.Converters.CharacterWrapConverter |
| Standard.Events.ApplicationEvents.DragDrop / RaiseDragDrop | Standard.Avalonia.Events.DragDropEvents.DragDrop / RaiseDragDrop |

`ApplicationEvents.SaveSettings` 和 `Exiting` 保留在核心项目中。XAML 中引用转换器时，CLR 命名空间与程序集也需更新为 `clr-namespace:Standard.Avalonia.UI.Converters;assembly=Standard.Avalonia`。

以下目录表以 `Standard` 核心项目为基准；MVVM 与 Avalonia 目录分别位于对应项目中。

| 目录 | 职责 |
| --- | --- |
| Application | 应用启动配置 |
| Events | 设置、退出与 UI 生命周期事件 |
| Extensions | 按目标类型划分的扩展方法 |
| IO | 文件系统操作 |
| Logging | 文件日志与日志级别 |
| Net | URL 处理 |
| Platform | 桌面平台与运行环境识别 |
| Tasks | 通用后台任务扩展；可观察任务管理位于 Standard.Mvvm/Tasks |
| Threading | 防抖与可取消的周期任务 |
| Standard.Avalonia/UI/Converters | Avalonia 值转换器 |

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

删除依赖旧 TioUi 的 ITioWindow；窗口操作由应用自身负责。转换器和拖放事件由 Standard.Avalonia 项目提供。

DirectoryUtilities 的排除路径支持文件与目录；保留被排除项的祖先目录，不递归进入符号链接。TryClearDirectory 返回成功与否，仅捕获文件系统异常。

防抖回调在计时器线程执行。周期 GC 需要显式调用并传入取消令牌，常规应用建议使用运行时自动回收。

## 验证

```powershell
dotnet build Standard.slnx
```
