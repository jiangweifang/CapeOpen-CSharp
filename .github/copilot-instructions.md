# Copilot Instructions

## 项目指南
- CapeOpen 项目 (D:\GitHub\CapeOpen) 是一个 .NET Framework 4.8 类库（由 US EPA 开发），实现 CAPE-OPEN 化工过程模拟互操作标准的 .NET/COM 翻译。核心内容包括：CAPE-OPEN IDL 接口的 .NET 映射、单元操作基类 (CapeUnitBase)、参数系统（Real/Integer/Boolean/Option/Array Parameter）、端口与物料对象封装、MixerExample 示例单元操作、WAR 废物减量算法插件。整个程序集 ComVisible，可注册为 COM 组件供 Aspen Plus、COFE 等化工模拟器调用。配套 CapeOpenSetup (WiX v3) 安装程序项目。