## CapeOpen-CSharp

基于 [US EPA CapeOpen](https://github.com/wbarret1/CapeOpen) 的 **.NET 8 (net8.0-windows)** 实现，提供 CAPE-OPEN 化工过程模拟互操作标准的 .NET/COM 翻译层。

> 这是科学家朋友拜托我研究的，我不太能理解计算结果，但是功能我还是可以修改的。
> 借鉴了 [laugh0608](https://github.com/laugh0608/CapeOpen-CSharp/tree/master) 的知乎中一些提示来理解这个插件的工作方式。

---

### 主要功能

| 模块 | 说明 |
|---|---|
| **CapeOpen** | 核心类库：CAPE-OPEN IDL 接口的 .NET 映射、单元操作基类 (`CapeUnitBase`)、参数系统、端口与物料对象封装 |
| **Test (MixerExample)** | 示例混合器单元操作，演示参数和端口的使用 |
| **tools/CapeOpenRegistrar** | .NET 8 COM 注册工具（替代原 .NET Framework 下的 `regasm`） |

### 参数类型

支持 CAPE-OPEN 规范定义的五种参数类型：

- `RealParameter` — 实数参数（带 SI 单位转换）
- `IntegerParameter` — 整数参数
- `BooleanParameter` — 布尔参数
- `OptionParameter` — 选项参数（字符串枚举）
- `ArrayParameter` — 数组参数（支持嵌套）

### 界面编辑

- 通过 PropertyGrid 集合编辑器可视化添加/删除 Ports（端口）和 Parameters（参数）
- `ParameterCollectionEditor` 支持通过下拉框选择参数类型后添加

<img width="2848" height="1474" alt="添加端口" src="https://github.com/user-attachments/assets/25200ae2-d71e-4f66-9d6f-195f840520b2" />

<img width="2841" height="1482" alt="添加参数" src="https://github.com/user-attachments/assets/f5ebdeb2-1676-440a-8376-ba00c4128b59" />

### 持久化 (IPersistStream)

通过 `IPersistStream` / `IPersistStreamInit` 实现与 Aspen Plus、COFE 等 PME 的状态保存/恢复：

- **V2 格式**（当前）：保存完整参数元数据（名称、描述、模式、类型特定规格），支持动态添加参数的持久化与恢复
- **V1 格式**：向后兼容，仅按名称匹配恢复参数值
- 使用 GZip 压缩 + BinaryWriter 类型化序列化

---

## 构建

- 目标框架：**.NET 8 (net8.0-windows)**，启用 `EnableComHosting`
- 使用 Visual Studio 2022/2026 打开 `CapeOpen.sln` 直接构建，或命令行：

```powershell
dotnet build CapeOpen.sln -c Debug
```

构建产物（以 Debug 为例）：

| 文件 | 说明 |
|---|---|
| `CapeOpen\bin\Debug\net8.0-windows\CapeOpen.dll` | 托管程序集 |
| `CapeOpen\bin\Debug\net8.0-windows\CapeOpen.comhost.dll` | Native COM 入口（regsvr32 目标） |
| `CapeOpen\bin\Debug\net8.0-windows\CapeOpen.tlb` | 类型库（由 `<ComHostTypeLibrary>` 嵌入，同时输出一份 .tlb） |
| `Test\bin\Debug\net8.0-windows\Test.dll` / `Test.comhost.dll` / `Test.tlb` | 示例单元操作 |

---

## COM 注册（.NET 8 ComHost）

**.NET 8 下不能再使用 `regasm`**。.NET Core 以后不再支持 `[ComRegisterFunction]` 回调，`regsvr32` 对 `*.comhost.dll` 在同时启用 `<ComHostTypeLibrary>` 多 CLSID 场景下也会静默失败，不会写入 `HKCR\CLSID\{...}` 键。因此本仓库提供了 `tools/CapeOpenRegistrar` 直接操作注册表，它会：

1. 写入 `HKCR\CLSID\{clsid}`：默认值、`InprocServer32`（指向 `*.comhost.dll`）、`ThreadingModel=Both`、`CodeBase`；
2. 写入 `ProgId` 及其反向映射；
3. 写入 `Implemented Categories`（CAPE-OPEN Component / UnitOperation / Monitoring / Consumes Thermo / Supports Thermo 1.0/1.1 等 CATID）；
4. 写入 `CapeDescription` 元数据（来自 `[CapeName]` / `[CapeDescription]` / `[CapeVersion]` / `[CapeVendorURL]` / `[CapeHelpURL]` / `[CapeAbout]` 等特性）；
5. 调用 `LoadTypeLibEx` 注册 `*.tlb`（若存在）。

扫描规则：仅处理同时满足以下条件的类型：`ComVisible(true)` + 显式 `[Guid("...")]` + 派生自 `CapeObjectBase`/`CapeUnitBase` 或带 `[CapeUnitOperation]`/`[CapeName]` 特性。

> **重要提示**（见 `.github/copilot-instructions.md`）：所有 TLB 中的 `ComVisible` 类必须使用 `docs/CapeOpen-TLB-CLSIDs.md` 中记录的真实 CLSID，不可随意生成 GUID，否则 `clsidmap`（嵌在 `*.comhost.dll` 里）与 TLB、注册表会不一致，导致 PME 实例化失败。

### 步骤 1：构建注册器

```powershell
dotnet build tools\CapeOpenRegistrar\CapeOpenRegistrar.csproj -c Release
```

### 步骤 2：以管理员身份注册

PowerShell（**管理员**）中执行：

```powershell
$registrar = 'tools\CapeOpenRegistrar\bin\Release\net8.0\CapeOpenRegistrar.exe'

# 注册核心库
& $registrar register 'CapeOpen\bin\Debug\net8.0-windows\CapeOpen.dll'

# 注册示例 MixerExample
& $registrar register 'Test\bin\Debug\net8.0-windows\Test.dll'
```

命令行参数：

| 命令 | 说明 |
|---|---|
| `register <managed.dll> [...]` | 注册（同目录需存在 `*.comhost.dll`） |
| `unregister <managed.dll> [...]` | 卸载上面写入的所有键，并反注册 TLB |
| `dump-tlb <file.tlb> [...]` | 诊断用：打印 TLB 中每个类型的 `{Guid}`、Name、Kind |

注册成功后可用 `regedit` 查看 `HKEY_CLASSES_ROOT\CLSID\{clsid}`，应看到 `InprocServer32`、`Implemented Categories`、`CapeDescription` 三个子键。

### 步骤 3：卸载

```powershell
& $registrar unregister 'Test\bin\Debug\net8.0-windows\Test.dll'
& $registrar unregister 'CapeOpen\bin\Debug\net8.0-windows\CapeOpen.dll'
```

---

## 测试注册结果

### 方案 A：用 PME 加载（最终验证）

在 Aspen Plus / COFE / SuperPro 等模拟器里新建流程图 → 添加单元操作 → 应能在 CAPE-OPEN 单元列表中看到 `CapeOpen.MixerExample` 等注册的组件。放置后保存/重新打开工程，`IPersistStream` 会恢复参数状态。

### 方案 B：裸调用 `DllRegisterServer` 诊断（不依赖 PME）

`tools/TestRegisterServer.ps1` 会 `LoadLibrary` + `GetProcAddress("DllRegisterServer")` 直接调用 `*.comhost.dll` 的导出入口，返回 HRESULT，方便定位 `regsvr32` 静默失败的原因：

```powershell
pwsh tools\TestRegisterServer.ps1 -dll 'D:\GitHub\CapeOpen-CSharp\CapeOpen\bin\Debug\net8.0-windows\CapeOpen.comhost.dll'
# 输出: DllRegisterServer returned HRESULT = 0x00000000  (S_OK)
```

> 注意：HRESULT 为 0 并不代表 CLSID 键一定写入成功（.NET 8 的 comhost 在多 CLSID + TLB 嵌入场景下会返回 S_OK 却什么都没写）。以注册表里是否真的出现 `HKCR\CLSID\{clsid}\InprocServer32` 为准 —— 这正是我们改用 `CapeOpenRegistrar` 的原因。

### 方案 C：PowerShell 脚本（替代 CapeOpenRegistrar）

`tools/Register-CapeOpenComponents.ps1` 是同等能力的 PowerShell 版本，用 `MetadataLoadContext` 扫描特性并写注册表。适合不想再多编译一个工具的场景：

```powershell
# 管理员
pwsh .\tools\Register-CapeOpenComponents.ps1 `
    -AssemblyPaths @(
        'CapeOpen\bin\Debug\net8.0-windows\CapeOpen.dll',
        'Test\bin\Debug\net8.0-windows\Test.dll'
    )

# 卸载
pwsh .\tools\Register-CapeOpenComponents.ps1 `
    -AssemblyPaths @('Test\bin\Debug\net8.0-windows\Test.dll') -Unregister
```

该脚本**不**写入基础 CLSID / InprocServer32 键，它假设你已经通过其他方式（`regsvr32` 或 `CapeOpenRegistrar`）写入。如果你的组件只有一个 CLSID，`regsvr32 CapeOpen.comhost.dll` 能成功写入基础键，就可以用这个脚本补齐 CAPE-OPEN 专属键。多 CLSID 场景推荐直接用 `CapeOpenRegistrar`。

---

## 常见问题

- **`regsvr32 *.comhost.dll` 报 0x80040201 或静默成功但注册表为空** —— 这是 .NET 8 ComHost 与 `<ComHostTypeLibrary>` 多 CLSID 的已知问题，请改用 `CapeOpenRegistrar`。
- **PME 看不到单元操作** —— 检查 `HKCR\CLSID\{clsid}\Implemented Categories` 下是否有 `{678c09a1-...}`（CAPE-OPEN Component）和 `{678c09a5-...}`（Unit Operation）两个 CATID。
- **32-bit 模拟器找不到 64-bit 注册的组件** —— 你的 PME 若是 32 位进程，必须在 32 位下重新编译并注册（或同时注册到 `HKLM\SOFTWARE\Classes\Wow6432Node\CLSID`）。当前注册器统一写 `HKCR`（64 位视图）。
- **修改了 `[Guid]`** —— 请先 `unregister` 再 `register`；并同步更新 `docs/CapeOpen-TLB-CLSIDs.md` 与 TLB。
