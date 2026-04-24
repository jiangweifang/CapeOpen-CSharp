# 数据审查问题记录

> 审查范围：`CapeOpen/unitCategories.json`、`CapeOpen/unitCategories.xml`、`CapeOpen/units.json`、`CapeOpen/units.xml`、`CapeOpen/WARdata.json`、`CapeOpen/WARdata.xml`  
> 审查日期：2025-07  
> 状态：**待科学家确认后决定是否修改**

---

## 一、unitCategories 疑似物理量纲错误

### 1.1 Voltage（电压）— 所有维度指数均为 1

- **文件**：unitCategories.json 第 367–378 行 / unitCategories.xml 第 421–432 行
- **当前值**：Mass:1, Time:1, Length:1, ElectricalCurrent:1, Temperature:1, AmountOfSubstance:1, Luminous:1, Currency:1
- **疑似正确值**：V = kg·m²·s⁻³·A⁻¹ → Mass:1, Length:2, Time:-3, ElectricalCurrent:-1, 其余 0
- **影响**：所有使用电压维度的参数，维度分析将完全错误

### 1.2 Heat Transfer Coefficient（传热系数）— SI_Unit 和 Length 指数

- **文件**：unitCategories.json / unitCategories.xml
- **当前值**：SI_Unit = "W"，Length = 2
- **疑似正确值**：SI_Unit = "W/(m²·K)"，W/(m²·K) = kg·s⁻³·K⁻¹ → Length 应为 0
- **影响**：传热系数的维度分析多出 m² 项

### 1.3 Accelerate — 名称与实际子单位不符

- **文件**：unitCategories.json / unitCategories.xml
- **当前值**：SI_Unit = "m/s2"，Length:1, Time:-2（加速度维度）
- **实际子单位**：cSt (centistokes)、St (stokes)、m²/s、mm²/s、cm²/s 等，均为**运动粘度**单位
- **疑似正确值**：SI_Unit = "m2/s"，m²/s → Length:2, Time:-1
- **影响**：该类别下所有运动粘度单位的维度表示错误

---

## 二、units 疑似转换系数错误

### 2.1 kg/cm2 → kg/m²

- **文件**：units.json 第 570–572 行 / units.xml 第 571–573 行
- **当前 ConversionTimes**：0.0001
- **疑似正确值**：10000（1 kg/cm² = 1 kg ÷ (0.01 m)² = 10000 kg/m²）

### 2.2 lb/ft2 → kg/m²

- **文件**：units.json 第 584–586 行 / units.xml 第 585–587 行
- **当前 ConversionTimes**：0.04214
- **疑似正确值**：≈ 4.8824（0.45359237 kg ÷ 0.09290304 m²）

### 2.3 in2/in3 → m⁻¹

- **文件**：units.json 第 1025–1027 行 / units.xml 第 1026–1028 行
- **当前 ConversionTimes**：0.39370079
- **疑似正确值**：39.370079（in²/in³ = 1/in = 1/0.0254 m ≈ 39.37 m⁻¹）

---

## 三、units Category 名称前导空格不一致

以下 units.json/xml 条目的 `Category` 值带有**前导空格**，与 unitCategories 中的定义不匹配：

| units 中的 Category | unitCategories 中的 Category | 涉及单位 |
|---------------------|------------------------------|---------|
| `" kg/m2 "` | `"kg/m2 "` | kg/m2, lb/ft2 |
| `" WeightFraction "` | `"WeightFraction "` | g/100g, g/g, g/kg, kg/kg, mg/kg, ppm, weight %, weight fraction |
| `" s-1 "` | `"s-1 "` | kHz, year-1 |

---

## 四、WARdata XML vs JSON

- 条目数均为 2117 条，数据值一致
- XML 中 CAS 字段有前导空格（如 `" 68855-24-3"`），JSON 中已 trim
- 未发现化学物质名称或毒理数据值的 XML↔JSON 不一致

---

## 五、确认无误的部分

- unitCategories XML 与 JSON 的 37 条记录完全一致
- units XML 与 JSON 的所有条目数值一致
- WARdata 2117 条化学物质记录 XML↔JSON 一致
- 基础 SI 单位（Mass/Time/Length/Temperature/ElectricalCurrent/AmountOfSubstance/Currency）的维度定义正确
- FlowRate、MassFlowRate、MolarFlowRate、Density、Pressure、Force、Energy、Power 等常用类别维度正确
