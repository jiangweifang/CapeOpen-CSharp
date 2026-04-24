# Copilot Instructions

## 项目指南
- CapeOpen 项目 (D:\GitHub\CapeOpen) 是一个 .NET Framework 4.8 类库（由 US EPA 开发），实现 CAPE-OPEN 化工过程模拟互操作标准的 .NET/COM 翻译。核心内容包括：CAPE-OPEN IDL 接口的 .NET 映射、单元操作基类 (CapeUnitBase)、参数系统（Real/Integer/Boolean/Option/Array Parameter）、端口与物料对象封装、MixerExample 示例单元操作、WAR 废物减量算法插件。整个程序集 ComVisible，可注册为 COM 组件供 Aspen Plus、COFE 等化工模拟器调用。配套 CapeOpenSetup (WiX v3) 安装程序项目。

## 调试记录
- CapeOpen-CSharp 项目调试记录：Test\MixerExample.cs 中类级别的 COM 互操作特性组合（[Serializable]、[ComVisible(true)]、[Guid("883D46FE-5713-424C-BF10-7ED34263CD6D")]、[Description]、[CapeName]、[CapeDescription]、[CapeVersion]、[CapeVendorURL]、[CapeHelpURL]、[CapeAbout]、[CapeConsumesThermo(true)]、[CapeUnitOperation(true)]、[CapeSupportsThermodynamics10(true)]、[ClassInterface(ClassInterfaceType.None)]）在 .NET Framework 4.8 下有效，能正常被 Aspen Plus/COFE 等 COM 宿主加载并执行到 MixerExample 构造函数/Calculate 方法；但迁移到 .NET 8 后无法执行到此处（COM 注册/激活路径不同，.NET 8 需要使用 ComHosting/RegFree COM、在 csproj 中启用 EnableComHosting，并通过 dotnet.host 生成 .comhost.dll 进行注册，传统 regasm 流程不再适用）。后续排查 .NET 8 COM 互操作问题时参考此记录。