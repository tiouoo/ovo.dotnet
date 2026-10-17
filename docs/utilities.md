# 工具方法

## 相对时间

```csharp
date.ToRelativeTime();
date.ToRelativeTime("en");
date.ToRelativeTime("en-US");
date.ToRelativeTime("zh-CN");
date.ToRelativeTime("en", referenceTime: now, useRelativeDates: true);
```

DateTime、DateTimeOffset 均支持。省略语言默认中文，支持中文和英文的地区语言标识，其他语言会抛出异常。referenceTime 默认当前时间；DateTime 的 UTC、Local、Unspecified 按 DateTimeOffset 的标准转换规则处理，Unspecified 视为本地时间。

支持过去和未来、英文单复数、昨天／明天。默认满 30 天显示 yyyy-MM-dd；useRelativeDates 为 true 时继续显示月／年，按 30 天／365 天近似计算，不代表日历月差。分钟、小时、天的分界分别为 60 秒、60 分钟、24 小时。

## 其他扩展

```csharp
date.ToUnixTimestamp();
date.ToUnixMilliseconds();
"中文".ToBase64().FromBase64();
"text".ToBase64(Encoding.Unicode).FromBase64(Encoding.Unicode);
value.AsJson(indented: false);
json.FromJson<MySettings>();
json.FromJson<MySettings>(serializerOptions);
text.ToInt(defaultValue: 0, provider: CultureInfo.InvariantCulture);
```

Base64 默认 UTF-8，无效输入抛出 FormatException。AsJson 支持 null，FromJson 使用 System.Text.Json 的默认规则，也可传入自定义选项。

AddRange 会先保存输入快照，支持集合自身及其 LINQ 查询作为输入，不会在枚举过程中修改原集合。
