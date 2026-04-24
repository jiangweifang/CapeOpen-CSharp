# Copilot Instructions

## 项目指南
- CapeOpen-CSharp 项目 (D:\GitHub\CapeOpen-CSharp，本仓库) 是 US EPA CapeOpen 的 .NET 8 (net8.0-windows) 移植版，实现 CAPE-OPEN 化工过程模拟互操作标准的 .NET/COM 翻译。核心内容包括：CAPE-OPEN IDL 接口的 .NET 映射、单元操作基类 (CapeUnitBase)、参数系统（Real/Integer/Boolean/Option/Array Parameter）、端口与物料对象封装、MixerExample 示例单元操作、WAR 废物减量算法插件。整个程序集 ComVisible，通过 .NET 8 EnableComHosting 生成 *.comhost.dll 注册为 COM 组件供 Aspen Plus、COFE、PRO/II 等化工模拟器调用。模拟器通过 TLB 中注册的 CLSID + CAPE-OPEN 标准接口（ICapeUnit、ICapeUnitPort、ICapeUtilities 等）访问单元操作。原上游配套的 CapeOpenSetup (WiX v3) 安装程序项目已弃用，改用 tools/CapeOpenRegistrar 直接写注册表完成 CLSID + CAPE-OPEN CATID + CapeDescription + TLB 的注册。

## ComVisible 规则
- 原 .NET Framework CapeOpen.dll 通过 tlbexp 生成的 CLSID 已记录在 `docs/CapeOpen-TLB-CLSIDs.md`，**任何修改 ComVisible 类时必须查此文档取真实 GUID，绝不可编造**。
- **需要 ComVisible(true) + `[Guid]` + `[ClassInterface(ClassInterfaceType.None)]` 的类**：模拟器通过 COM/TLB 直接访问（`CoCreateInstance` 实例化）的类，即 TLB 中注册了 CLSID 的类。GUID 必须与 `docs/CapeOpen-TLB-CLSIDs.md` 一致。
- **应设为 ComVisible(false) 的类**：内部辅助类（如 CapeCalculator、CapeReportBase、CapeMaterialPort、CapeMaterialObject 等仅被 .NET 代码使用、模拟器不会通过 CLSID 创建的类），不需要 `[Guid]` 和 `[ClassInterface]`。

## 文件编码要求
- 所有创建/编辑文件必须保证 UTF-8 编码（无 BOM 或带 BOM 视情况），尤其在 PowerShell 写文件时使用 -Encoding utf8 或 utf8NoBOM。

## CAPE-OPEN API 文档查找
- 当用户询问 CAPE-OPEN API（类、接口、枚举、委托、异常等）相关问题时，优先从 `docs/api/` 目录下的 md 文档中查找对应 API 的说明。该目录包含从 `CAPE-OPEN.Net.chm` 自动抽取生成的文档：README.md（总览）、types-index.md（所有类型一览）、classes.md、interfaces.md、enums.md、exceptions.md、delegates-events.md、toc.md（完整目录树）。