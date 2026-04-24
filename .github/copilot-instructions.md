# Copilot Instructions

## 项目指南
- CapeOpen-CSharp 项目 (D:\GitHub\CapeOpen-CSharp，本仓库) 是 US EPA CapeOpen 的 .NET 8 (net8.0-windows) 移植版，实现 CAPE-OPEN 化工过程模拟互操作标准的 .NET/COM 翻译。核心内容包括：CAPE-OPEN IDL 接口的 .NET 映射、单元操作基类 (CapeUnitBase)、参数系统（Real/Integer/Boolean/Option/Array Parameter）、端口与物料对象封装、MixerExample 示例单元操作、WAR 废物减量算法插件。整个程序集 ComVisible，通过 .NET 8 EnableComHosting 生成 *.comhost.dll 注册为 COM 组件供 Aspen Plus、COFE、PRO/II 等化工模拟器调用。模拟器通过 TLB 中注册的 CLSID + CAPE-OPEN 标准接口（ICapeUnit、ICapeUnitPort、ICapeUtilities 等）访问单元操作。原上游配套的 CapeOpenSetup (WiX v3) 安装程序项目已弃用，改用 tools/CapeOpenRegistrar 直接写注册表完成 CLSID + CAPE-OPEN CATID + CapeDescription + TLB 的注册。

## ComVisible 规则
- 原 .NET Framework CapeOpen.dll 通过 tlbexp 生成的 CLSID 已记录在 `docs/CapeOpen-TLB-CLSIDs.md`，**任何修改 ComVisible 类时必须查此文档取真实 GUID，绝不可编造**。
- **需要 ComVisible(true) + `[Guid]` + `[ClassInterface(ClassInterfaceType.None)]` 的类**：模拟器通过 COM/TLB 直接访问（`CoCreateInstance` 实例化）的类，即 TLB 中注册了 CLSID 的类。GUID 必须与 `docs/CapeOpen-TLB-CLSIDs.md` 一致。
- **应设为 ComVisible(false) 的类**：内部辅助类（如 CapeCalculator、CapeReportBase、CapeMaterialPort、CapeMaterialObject 等仅被 .NET 代码使用、模拟器不会通过 CLSID 创建的类），不需要 `[Guid]` 和 `[ClassInterface]`。
- **COGuids.cs 和 CapeOpen-TLB-CLSIDs.md 是两个完全不同的东西**：COGuids.cs 包含 CAPE-OPEN 标准协议层的 IID（接口 GUID）和 CATID（组件类别 GUID），来自 CAPE-OPEN IDL 规范；CapeOpen-TLB-CLSIDs.md 包含本项目 .NET 实现类的 CLSID（来自 tlbexp 生成的 TLB），用于 [Guid("...")] 特性。两者互不重叠，不可混淆。

## 文件编码要求
- 所有创建/编辑文件必须保证 UTF-8 编码（无 BOM 或带 BOM 视情况），尤其在 PowerShell 写文件时使用 -Encoding utf8 或 utf8NoBOM。

## CAPE-OPEN API 文档查找
- 当用户询问 CAPE-OPEN API（类、接口、枚举、委托、异常等）相关问题时，优先从 `docs/api/` 目录下的 md 文档中查找对应 API 的说明。该目录包含从 `CAPE-OPEN.Net.chm` 自动抽取生成的文档：README.md（总览）、types-index.md（所有类型一览）、classes.md、interfaces.md、enums.md、exceptions.md、delegates-events.md、toc.md（完整目录树）。

## 调试记录
- CapeOpen-CSharp 项目调试记录：Test\MixerExample.cs 中类级别的 COM 互操作特性组合（[Serializable]、[ComVisible(true)]、[Guid("883D46FE-5713-424C-BF10-7ED34263CD6D")]、[Description]、[CapeName]、[CapeDescription]、[CapeVersion]、[CapeVendorURL]、[CapeHelpURL]、[CapeAbout]、[CapeConsumesThermo(true)]、[CapeUnitOperation(true)]、[CapeSupportsThermodynamics10(true)]、[ClassInterface(ClassInterfaceType.None)]）在 .NET Framework 4.8 下有效，能正常被 Aspen Plus/COFE 等 COM 宿主加载并执行到 MixerExample 构造函数/Calculate 方法；但迁移到 .NET 8 后无法执行到此处（COM 注册/激活路径不同，.NET 8 需要使用 ComHosting/RegFree COM、在 csproj 中启用 EnableComHosting，并通过 dotnet.host 生成 .comhost.dll 进行注册，传统 regasm 流程不再适用）。后续排查 .NET 8 COM 互操作问题时参考此记录。

### COM Hosting 调试经验
- 当新增 [ComVisible][Guid] 类（如 Test.TestMixerExample CLSID {AA7EF99E-...}）后，构造函数/Calculate 断点断不到的常见原因及排查顺序：
  1. *.comhost.dll 嵌入了一份 .clsidmap JSON 资源，CoreCLR 按此 map 把 CLSID 映射到托管 Type；若 map 里没有新 CLSID，CoCreate 直接返回 CLASS_E_CLASSNOTAVAILABLE (0x80040111)。验证方法：读取 Examples.comhost.dll 二进制，搜索 GUID 字符串，若不存在则 comhost 未重新生成。
  2. comhost 不重新生成的根因通常是 Examples.dll 被 CAPE-OPEN 宿主（COFE/PRO/II 等）锁住，MSB3026/MSB3027 "文件被 PRO/II 9.4 锁定" → build copy 失败 → comhost 保留旧版本。修复：关闭所有宿主进程 → 删除 Examples\bin 和 Examples\obj → dotnet build /p:Platform=x86。
  3. 验证 CoCreate 是否真的 OK：用 32-bit PowerShell + P/Invoke ole32!CoCreateInstance，返回 S_OK 即证明 comhost 激活链路通、托管构造函数已执行。若用 PowerShell 的 [Activator]::CreateInstance([type]::GetTypeFromCLSID(...)) 可能报 TYPE_E_ELEMENTNOTFOUND (0x8002802B)，这是 IDispatch/TypeLib 绑定失败，不是 CoCreate 本身失败。
  4. 断点仍断不到 = VS 附加方式错：.NET 8 托管代码在 comhost 启动的 CoreCLR 里运行，必须 Attach to Process 时勾选 "Managed (.NET Core, .NET 5+)" 而不是 "Managed (v4.x)"，且位数要匹配宿主进程。
  5. **注意**：Examples.comhost.dll 的文件时间戳不是"是否重新生成"的有效判据。SDK 的 _CreateComHost 任务从 SDK 自带的 comhost.dll 模板复制时会保留模板自身的原始 LastWriteTime（例如长期停留在 2026/3/19 21:19:54，158208 字节），然后把最新的 .clsidmap 作为资源嵌入进去。也就是说 **content 每次构建都刷新，但时间戳不会变**。因此诊断 "comhost 陈旧 / 没更新" 时严禁用时间戳判断，必须读二进制搜索新加入类的 GUID 字符串：例如 `$b=[IO.File]::ReadAllBytes($p); [Text.Encoding]::UTF8.GetString($b) -match 'AA7EF99E'`。
  6. 另外，.NET 8 COM 激活路径上 `[Serializable]` 对 CCW 激活没有任何影响（CoreCLR 的 built-in COM interop 不读此特性）；本项目的 IPersistStream/IPersistStreamInit 实现使用 BinaryWriter/BinaryReader 做类型化持久化，不走 BinaryFormatter，因此不受 .NET 8 默认禁用 BinaryFormatter 的影响。CoCreateInstance 返回 S_OK 即表示托管构造函数已执行完，"断点打不上" ≠ "构造函数没跑"，应先在 VS Modules 窗口确认 Examples.dll 的 Symbol Status 是否为 "Symbols loaded"，以及附加时是否勾选了 "Managed (.NET Core, .NET 5+)" 并匹配 PRO/II 的位数（32-bit）。
  7. **PRO/II 9.x 与 .NET 8 ComHost 架构级不兼容（2026/4 调查结论）**：PRO/II 9.x 激活托管 CAPE-OPEN 组件走的是 .NET Framework CLR 的 `Assembly.Load` / `AppDomain.CreateInstanceAndUnwrap` 路径（读 `InprocServer32\Assembly` + `Class` + `RuntimeVersion`），而**不是**标准 `CoCreateInstance` + comhost。.NET 8 comhost 不写这些 FW-only 键值（改用 comhost.dll 内嵌 `.clsidmap`）。手工 `reg add` 补这三个键**无效**，因为 PRO/II 真实尝试用 FW CLR 加载 assembly，.NET 8 assembly 因 PE runtime version 不匹配抛 `BadImageFormatException` 被静默吞掉。**判别方法**：(a) 在构造函数与静态 cctor 同时插文件日志探针（`File.AppendAllText(@"C:\temp\ctor-hit.log", ...)`，带 `Environment.ProcessId` + `RuntimeInformation.FrameworkDescription`）；(b) 32-bit PowerShell 跑 `tools\test-coclass-raw.ps1`（P/Invoke `ole32!CoCreateInstance` with `CLSCTX_INPROC_SERVER=1`）返回 HR=S_OK 且 log 有行 = comhost 链通；(c) PRO/II 拖入组件后 log 不生成 + 连静态 cctor 也不触发 = 宿主从未 CoCreate，是宿主层路径问题而非托管代码问题。对照实验（`TestMixerExample` `{AA7EF99E-6938-484F-98B9-4BE9AC34F85E}` 和 `MixerExample` `{883D46FE-5713-424C-BF10-7ED34263CD6D}` 在 PRO/II 同时失败）证明这不是某一类特有的属性问题。**修复方向**：(1) 推荐 —— CapeOpen SDK 层改 `<TargetFramework>netstandard2.0</TargetFramework>`，Examples 等消费者多目标 `<TargetFrameworks>net48;net8.0-windows</TargetFrameworks>`，net48 输出走 `regasm` 注册给 PRO/II，net8.0 输出走 `EnableComHosting` + `CapeOpenRegistrar` 注册给 COFE/新版 Aspen；(2) 完全回退纯 net48；(3) 写 net48 FW-side shim DLL 通过 OOP IPC 调 .NET 8 server。**COFE / 现代 Aspen Plus 等走标准 `CoCreateInstance` 的宿主不受此问题影响**。另外 `tools\CapeOpenRegistrar\Registrar.cs` 当前只写 `(Default)=comhost.dll` + `ThreadingModel` + `CodeBase`，**不写** `Assembly` / `Class` / `RuntimeVersion`——这对新宿主是正确的，PRO/II 不兼容属于架构级问题，registry 层无法修复。
