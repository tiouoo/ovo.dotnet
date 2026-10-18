# Standard 构建值注入

## 构建时注入

使用 `ProjectReference` 引用 `Standard.csproj` 后，在使用方项目中显式导入 targets：

```xml
<ItemGroup>
    <StandardBuildValue Include="MyApp.ClientId" EnvironmentVariable="MYAPP_CLIENT_ID" />
    <StandardBuildValue Include="MyApp.Channel" Value="$(BuildChannel)" EnvironmentVariable="MYAPP_CHANNEL" />
</ItemGroup>
<Import Project="path/to/Standard/Standard.targets" />
```

`Include` 是程序集元数据键。非空 `Value` 优先，否则在构建时读取 `EnvironmentVariable`，缺失时不生成该项。CI 将变量放在构建进程的环境中即可；经 PupNet 等工具启动的子进程也会继承环境。不需要生成源码或临时配置文件。保留 SDK 默认的 `GenerateAssemblyInfo=true`。

## 运行时读取

```csharp
using Standard.Build;

// 明确指定注入值所在的程序集，不能使用 Standard 自己的程序集。
var values = new AssemblyBuildValues(typeof(MyAppService).Assembly);
var channel = values.GetValue("MyApp.Channel") ?? "development";
var clientId = values.GetValue("MyApp.ClientId", "MYAPP_CLIENT_ID");
```

读取顺序：优先返回非空白的嵌入值，再读取可选的运行时环境变量并 Trim，最后返回 null。嵌入值保留原文；元数据在实例创建时缓存，环境变量在每次读取时获取。版本信息通常不指定环境回退，确保显示的是实际构建信息。

程序集元数据可以被读取，此机制不对注入的凭据加密。
