# Copilot Instructions

## 项目指南
- CapeOpen-CSharp 项目 (D:\GitHub\CapeOpen-CSharp，本仓库) 是 US EPA CapeOpen 的 .NET 8 (net8.0-windows) 移植版，实现 CAPE-OPEN 化工过程模拟互操作标准的 .NET/COM 翻译。核心内容包括：CAPE-OPEN IDL 接口的 .NET 映射、单元操作基类 (CapeUnitBase)、参数系统（Real/Integer/Boolean/Option/Array Parameter）、端口与物料对象封装、MixerExample 示例单元操作、WAR 废物减量算法插件。整个程序集 ComVisible，通过 .NET 8 EnableComHosting 生成 *.comhost.dll 注册为 COM 组件供 Aspen Plus、COFE、PRO/II 等化工模拟器调用。模拟器通过 TLB 中注册的 CLSID + CAPE-OPEN 标准接口（ICapeUnit、ICapeUnitPort、ICapeUtilities 等）访问单元操作。原上游配套的 CapeOpenSetup (WiX v3) 安装程序项目已弃用，改用 tools/CapeOpenRegistrar 直接写注册表完成 CLSID + CAPE-OPEN CATID + CapeDescription + TLB 的注册。
- CapeOpen-CSharp 项目的关键经验：原 .NET Framework CapeOpen.dll 通过 tlbexp 自动生成了大量类的 CLSID 写入 CapeOpen.tlb。在 .NET 8 ComHost 下，必须给 TLB 中所有 ComVisible(true) 的类显式标 [Guid] 且与 TLB 一致，否则 clsidmap 与 TLB 不一致会导致问题。所有 CLSID 真实值已保存到 docs/CapeOpen-TLB-CLSIDs.md。任何修改需要 ComVisible 的类都必须查这份文档拿真实 GUID，绝不可随便编造。
- 在 CapeOpen-CSharp 项目中，任何实现或使用 CAPE-OPEN 接口的类必须具有 [System.Runtime.InteropServices.ComVisible(true)]、[Guid] 属性，以及 [System.Runtime.InteropServices.ClassInterface(System.Runtime.InteropServices.ClassInterfaceType.None)] 属性。纯 .NET 辅助类（不实现 CAPE-OPEN 接口）不需要  ComVisible(true) + [Guid] + [ClassInterface(System.Runtime.InteropServices.ClassInterfaceType.None)] 。

## 文件编码要求
- 所有创建/编辑文件必须保证 UTF-8 编码（无 BOM 或带 BOM 视情况），尤其在 PowerShell 写文件时使用 -Encoding utf8 或 utf8NoBOM。