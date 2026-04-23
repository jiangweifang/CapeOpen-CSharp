## CapeOpen-CSharp

基于 [US EPA CapeOpen](https://github.com/wbarret1/CapeOpen) 的 .NET Framework 4.8 实现，提供 CAPE-OPEN 化工过程模拟互操作标准的 .NET/COM 翻译层。

> 这是科学家朋友拜托我研究的，我不太能理解计算结果，但是功能我还是可以修改的。
> 借鉴了 [laugh0608](https://github.com/laugh0608/CapeOpen-CSharp/tree/master) 的知乎中一些提示来理解这个插件的工作方式。

---

### 主要功能

| 模块 | 说明 |
|---|---|
| **CapeOpen** | 核心类库：CAPE-OPEN IDL 接口的 .NET 映射、单元操作基类 (`CapeUnitBase`)、参数系统、端口与物料对象封装 |
| **Test (MixerExample)** | 示例混合器单元操作，演示参数和端口的使用 |

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

### 使用方式

本项目不包含安装程序。调试完成后直接注册 COM 即可使用：

```bat
regasm /codebase CapeOpen.dll
```

整个程序集 `ComVisible`，可注册为 COM 组件供 Aspen Plus、COFE 等化工模拟器调用。

### 构建

- 目标框架：.NET Framework 4.8
- 使用 Visual Studio 打开 `CapeOpen.sln` 直接构建
