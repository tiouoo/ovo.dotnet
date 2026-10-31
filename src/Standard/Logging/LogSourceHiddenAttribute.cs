using System;

namespace Standard.Logging;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, Inherited = false)]
public sealed class LogSourceHiddenAttribute : Attribute;
