# CapeOpen.tlb — 真实 CLSID 权威清单（**禁止编造**）

> ⚠️ **重要**：以下 CLSID 全部来自 `CapeOpen\Tlb\CapeOpen.tlb`（原 .NET Framework `tlbexp` 生成的类型库）。
> 在 .NET 8 ComHost 下，所有 `[ComVisible(true)]` 的类**必须**用与此清单一致的 `[Guid("...")]`，
> 否则 ComHost 生成的 `.clsidmap` 与 TLB 不一致，会导致 `DllRegisterServer` 失败、
> Aspen Plus / COFE 找不到组件、TypeLib 引用解析失败等一系列问题。
>
> **任何人添加/修改 ComVisible 类时，先查这份文档。**

提取方法：
```powershell
& "tools\CapeOpenRegistrar\bin\Release\net8.0-windows\CapeOpenRegistrar.exe" dump-tlb "CapeOpen\Tlb\CapeOpen.tlb"
```

## CoClasses（70 个）

### 抽象基类
| 类名 | CLSID |
|------|-------|
| `CapeIdentification` | `{bf54df05-924c-49a5-8ebb-733e37c38085}` |
| `CapeObjectBase` | `{2f8fdc51-b6c4-3df1-b286-c54ffc4b2b7f}` |
| `CapeUnitBase` | `{c8b5381b-ef11-38ea-8986-7fcefbc42a0e}` |
| `CapeParameter` | `{f027b4d1-a215-4107-aa75-34e929dd00a5}` |

### 参数类
| 类名 | CLSID |
|------|-------|
| `ArrayParameter` | `{3a5f7b2e-9c14-4d8a-b6e1-7f2a3d4c5e6b}` |
| `BooleanParameter` | `{8b8bc504-eeb5-4a13-b016-9614543e4536}` |
| `IntegerParameter` | `{2c57dc9f-1368-42eb-888f-5bc6ed7ddfa7}` |
| `OptionParameter` | `{8eb0f647-618c-4fcc-a16f-39a9d57ea72e}` |
| `RealParameter` | `{77e39c43-046b-4b1f-9ee0-aa9efc55d2ee}` |
| `ParameterCollection` | `{64a1b36c-106b-4d05-b585-d176cd4dd1db}` |

### 端口与单元操作
| 类名 | CLSID |
|------|-------|
| `UnitPort` | `{51066f52-c0f9-48d7-939e-3a229010e77c}` |
| `PortCollection` | `{1c5f7cc3-31b4-4d81-829f-3eb5d692f7bd}` |
| `UnitOperationSystem` | `{3a223dee-8414-4802-8391-d1b11b276a0f}` |
| `UnitOperationWrapper` | `{b41dece0-6c99-4ca4-b0eb-efadbdce23e9}` |
| `CapeThermoSystem` | `{b5483fd2-e8ab-4ba4-9ea6-53bbdb77ce81}` |

### 示例与编辑器
| 类名 | CLSID |
|------|-------|
| `MixerExample` | `{883d46fe-5713-424c-bf10-7ed34263cd6d}` |
| `MixerExample110` | `{56e8fdfd-2000-4264-9b47-745b26be0ec9}` |
| `WARAddIn` | `{0be9ccfd-29b4-4a42-b34e-76f5fe9b6bb4}` |
| `WARalgorithm` | `{86e05bad-db51-31dc-b4b1-8d17d28014ad}` |
| `BaseUnitEditor` | `{9e2d9215-1f76-33a5-a6d1-b6c196a22ce2}` |
| `UnitSelector` | `{c6e08496-4269-321a-986d-3b68ff2ffd81}` |

### 异常类
| 类名 | CLSID |
|------|-------|
| `CapeUserException` | `{28686562-77ad-448f-8a41-8cf9c3264a3e}` |
| `CapeUnknownException` | `{b550b2ca-6714-4e7f-813e-c93248142410}` |
| `CapeUnexpectedException` | `{16049506-e086-4baf-9905-9ed13d50d0e3}` |
| `CapeDataException` | `{53551e7c-ecb2-4894-b71a-ccd1e7d40995}` |
| `CapeBadCOParameter` | `{667d34e9-7ef7-4ca8-8d17-c7577f2c5b62}` |
| `CapeBadArgumentException` | `{d168e99f-c1ef-454c-8574-a8e26b62adb1}` |
| `CapeBoundariesException` | `{62b1ee2f-e488-4679-afa3-d490694d6b33}` |
| `CapeOutOfBoundsException` | `{4438458a-1659-48c2-9138-03ad8b4c38d8}` |
| `CapeComputationException` | `{9d416bf5-b9e3-429a-b13a-222ee85a92a7}` |
| `CapeFailedInitialisationException` | `{e407595c-6d1c-4b8c-a29d-db0be73efdda}` |
| `CapeImplementationException` | `{7828a87e-582d-4947-9e8f-4f56725b6d75}` |
| `CapeInvalidArgumentException` | `{b30127da-8e69-4d15-bab0-89132126bac9}` |
| `CapeInvalidOperationException` | `{c0b943fe-fb8f-46b6-a622-54d30027d18b}` |
| `CapeBadInvOrderException` | `{07ead8b4-4130-4ca6-94c1-e8ec4e9b23cb}` |
| `CapeLicenceErrorException` | `{cf4c55e9-6b0a-4248-9a33-b8134ea393f6}` |
| `CapeLimitedImplException` | `{5e6b74a2-d603-4e90-a92f-608e3f1cd39d}` |
| `CapeNoImplException` | `{1d2488a6-c428-4e38-afa6-04f2107172da}` |
| `CapeOutOfResourcesException` | `{42b785a7-2edd-4808-ac43-9e6e96373616}` |
| `CapeNoMemoryException` | `{1056a260-a996-4a1e-8bae-9476d643282b}` |
| `CapePersistenceException` | `{3237c6f8-3d46-47ee-b25f-52485a5253d8}` |
| `CapePersistenceNotFoundException` | `{271b9e29-637e-4eb0-9588-8a53203a3959}` |
| `CapePersistenceOverflowException` | `{a119de0b-c11e-4a14-ba5e-9a2d20b15578}` |
| `CapePersistenceSystemErrorException` | `{85cb2d40-48f6-4c33-bf0c-79cb00684440}` |
| `CapeIllegalAccessException` | `{45843244-ecc9-495d-adc3-bf9980a083eb}` |
| `CapeSolvingErrorException` | `{f617afea-0eee-4395-8c82-288bf8f2a136}` |
| `CapeHessianInfoNotAvailableException` | `{3044ea08-f054-4315-b67b-4e3cd2cf0b1e}` |
| `CapeTimeOutException` | `{0d5ca7d8-6574-4c7b-9b5f-320aa8375a3c}` |
| `COMCapeOpenExceptionWrapper` | `{31cd55de-aefd-44ff-8bab-f6252dd43f16}` |
| `CapeThrmPropertyNotAvailableException` | `{5ba36b8f-6187-4e5e-b263-0924365c9a81}` |

### 事件参数（EventArgs）
| 类名 | CLSID |
|------|-------|
| `ComponentNameChangedEventArgs` | `{d78014e7-fb1d-43ab-b807-b219fab97e8b}` |
| `ComponentDescriptionChangedEventArgs` | `{0c51c4f1-20e8-413d-93e1-4704b888354a}` |
| `ParameterValueChangedEventArgs` | `{c3592b59-92e8-4a24-a2eb-e23c38f88e7f}` |
| `ParameterDefaultValueChangedEventArgs` | `{355a5bdd-f6b5-4eee-97c7-f1533dd28889}` |
| `ParameterLowerBoundChangedEventArgs` | `{a982ad29-10b5-4c86-af74-3914dd902384}` |
| `ParameterUpperBoundChangedEventArgs` | `{92bf83fe-0855-4382-a15e-744890b5bbf2}` |
| `ParameterModeChangedEventArgs` | `{3c953f15-a1f3-47a9-829a-9f7590ceb5e9}` |
| `ParameterValidatedEventArgs` | `{5727414a-838d-49f8-afef-1cc8c578d756}` |
| `ParameterResetEventArgs` | `{01bf391b-415e-4f5e-905d-395a707dc125}` |
| `ParameterOptionListChangedEventArgs` | `{2aec279f-ebec-4806-aa00-cc215432db82}` |
| `ParameterRestrictedToListChangedEventArgs` | `{82e0e6c2-3103-4b5a-a5bc-ebab971b069a}` |
| `UnitOperationValidatedEventArgs` | `{9147e78b-29d6-4d91-956e-75d0fb90cea7}` |
| `UnitOperationCalculateEventArgs` | `{7831c38b-a1c6-40c5-b9fc-dac43426aad4}` |
| `UnitOperationBeginCalculationEventArgs` | `{763691e8-d792-4b97-a12a-d4ad7f66b5e4}` |
| `UnitOperationEndCalculationEventArgs` | `{172f4d6e-65d1-4d9e-a275-7880fa3a40a5}` |
| `PortConnectedEventArgs` | `{962b9fde-842e-43f8-9280-41c5bf80ddec}` |
| `PortDisconnectedEventArgs` | `{693f33aa-ee4a-4cdf-9ba1-8889086bc8ab}` |

### Delegate（委托）
| 类名 | CLSID |
|------|-------|
| `ComponentNameChangedHandler` | `{4a551682-8cdd-330e-8fa4-13737697f993}` |
| `ComponentDescriptionChangedHandler` | `{82de47ff-86bf-3ec9-8167-c7eddbed4acf}` |

### Dispatch 接口（tlbexp 自动生成，**勿手工添加**）
| 名称 | IID |
|------|-----|
| `_ComponentNameChangedHandler` | `{90a4c93f-da47-3b19-a908-1c38a0f2eaa2}` |
| `_ComponentDescriptionChangedHandler` | `{267cc71b-c33a-3eb2-b6e8-7306500c22bf}` |

---

## 不在 TLB 中的额外 GUID（migration 中新分配的）
这些 delegate 在原 TLB 中不存在，迁移到 .NET 8 时为 ComVisible(true) 需要新分配 GUID：

| 类名 | CLSID（新分配） | 文件 |
|------|----------------|------|
| `CollectionAddingNewHandler` | `{8ad1b20b-cb76-41de-a34f-c683d4ab5300}` | `commonIDL.cs` |
| `CollectionListChangedHandler` | `{6a1fe3c1-dd87-4b10-bca6-b6906523b338}` | `commonIDL.cs` |
> 以下 9 个类（CapeCalculator、CapeReportBase、StatusReport、LastRunReport、MixerCalculator、
> CapeMaterialPort、CapeEnergyPort、CapeInformationPort、CapeMaterialObject）为内部辅助类，
> 不被模拟器通过 COM 直接访问，已改为 `ComVisible(false)`，不再需要 GUID。

---

## TypeLib（库本身）
| 项 | GUID |
|----|------|
| `CapeOpen` TypeLib (LIBID) | `{d80fc39b-897e-4038-a89e-d89cec9a5fa1}` |

## 维护说明
- 添加新 ComVisible 类：分配新 GUID，更新本文档 + 类源码 + 重新生成 TLB
- 修改类签名：保持 GUID 不变（COM 二进制契约）
- 删除类：从本文档移到 "Deprecated" 区域并保留 GUID 永远不再复用
