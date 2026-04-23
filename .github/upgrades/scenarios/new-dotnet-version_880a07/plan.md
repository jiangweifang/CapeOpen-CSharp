# .NET Framework 4.8 → .NET 8 Upgrade Plan

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Migration Strategy](#2-migration-strategy)
3. [Detailed Dependency Analysis](#3-detailed-dependency-analysis)
4. [Project-by-Project Plans](#4-project-by-project-plans)
   - [4.1 CapeOpen.csproj](#41-capeopencsproj)
   - [4.2 Test.csproj](#42-testcsproj)
5. [Package Update Reference](#5-package-update-reference)
6. [Breaking Changes Catalog](#6-breaking-changes-catalog)
7. [Risk Management](#7-risk-management)
8. [Testing & Validation Strategy](#8-testing--validation-strategy)
9. [Complexity & Effort Assessment](#9-complexity--effort-assessment)
10. [Source Control Strategy](#10-source-control-strategy)
11. [Success Criteria](#11-success-criteria)

---

## 1. Executive Summary

### 1.1 Scenario
Upgrade the **CapeOpen** solution from **.NET Framework 4.8** to **.NET 8.0**. The solution implements the CAPE-OPEN chemical process simulation interoperability standard. Both projects in the solution are non-SDK-style (Classic) and must be converted to SDK-style.

### 1.2 Scope

| Project | Current TFM | Proposed TFM | Project Kind | Files | LOC |
|---------|-------------|--------------|--------------|-------|-----|
| `CapeOpen\CapeOpen.csproj` | `net48` | `net8.0-windows` | ClassicWinForms (ComVisible) | 54 | 43,716 |
| `Test\Test.csproj` | `net48` | `net8.0` | ClassicClassLibrary | 2 | 405 |

- **Total NuGet packages**: 0 (no package update planning required)
- **Total API issues**: 2,409 (96.7% are Windows Forms — resolved by `net8.0-windows` TFM)
- **Security vulnerabilities**: None
- **COM Interop issues flagged**: 2 (requires manual review — see §6.4)

### 1.3 Selected Strategy
**All-At-Once Strategy** — both projects upgraded simultaneously in a single coordinated atomic operation.

**Rationale**:
- Only 2 projects (well below the 5-project threshold for incremental approach)
- Linear dependency: `Test` → `CapeOpen` (depth = 2, no cycles)
- 0 NuGet packages — no package coordination concerns
- No `.NET Framework`-only packages blocking the migration
- The vast majority of issues (Windows Forms APIs) are auto-resolved by switching TFM to `net8.0-windows`
- Splitting into phases would create artificial checkpoints with no value

### 1.4 Critical Issues / Risks
1. ⚠️ **COM Hosting model change** — `RegisterForComInterop` is replaced by `EnableComHosting` and a `comhost.dll` shim. Existing consumers (Aspen Plus, COFE, etc.) call into the assembly via COM; the registration and loading mechanism changes significantly.
2. ⚠️ **`tlbexp.exe` no longer ships with .NET 8** — the project currently produces a TLB via `RegisterForComInterop`. In .NET 8, type libraries are not auto-generated; an existing `.tlb` must be supplied via `<ComHostTypeLibrary>`.
3. ⚠️ **GAC references** — `EnvDTE`, `Accessibility`, `System.Configuration.Install`, `System.Design`, `System.Management` are .NET Framework GAC references that need replacement (NuGet equivalents or removal).
4. ⚠️ **`System.Configuration.Install`** has no .NET 8 equivalent.
5. ⚠️ **Strong-name signing** — `CapeOpenKey.snk` and `SignAssembly=true` must carry over to the SDK-style project.
6. ⚠️ **WiX Setup project (`CapeOpenSetup`)** — not in the solution but mentioned in copilot instructions; will need updates to use the new COM registration model. **Out of scope** for this plan but flagged as downstream impact.

### 1.5 Iteration Strategy
Classification: **Simple Solution**. Plan generated in a small number of coordinated iterations (skeleton → discovery → all sections in one pass), reflecting the All-At-Once execution model.

---

## 2. Migration Strategy

### 2.1 Approach: All-At-Once
All upgrade operations are executed as **one atomic batch**:
1. Convert both `.csproj` files to SDK-style and set their target frameworks simultaneously.
2. Restore and build the entire solution.
3. Fix all compilation errors caused by framework changes in the same pass.
4. Verify the solution builds with 0 errors.

There are **no intermediate states** — at no point does the solution sit half-migrated.

### 2.2 Why Not Incremental
- Only 2 projects; incremental adds overhead without benefit.
- `Test` directly depends on `CapeOpen` — they must move together because mixing `net48` and `net8.0-windows` references is not supported.
- No tier/phase grouping makes sense.

### 2.3 Execution Order Within the Atomic Operation
1. **Prerequisite check** — verify .NET 8 SDK is installed.
2. **Convert project files** — both `.csproj` files to SDK-style with new TFMs.
3. **Update assembly references** — replace GAC references with NuGet equivalents.
4. **Configure COM hosting** — add `EnableComHosting` and supply a type library.
5. **Restore dependencies** (`dotnet restore`).
6. **Build solution and fix all compilation errors** (single bounded pass).
7. **Verify** — solution builds with 0 errors and 0 warnings.

Testing (running unit tests in `Test.csproj`) follows as a **separate task** after the atomic upgrade succeeds.

### 2.4 Parallel vs Sequential
All operations within the atomic upgrade are performed as a **coordinated batch**. There is no parallelism to consider at the project level (only 2 projects, executed together).

---

## 3. Detailed Dependency Analysis

### 3.1 Dependency Graph

```
Test.csproj (net48 → net8.0)
    │
    └──> CapeOpen.csproj (net48 → net8.0-windows)
              │
              └──> [GAC References — require replacement]
                   • Accessibility
                   • EnvDTE (EmbedInteropTypes=true)
                   • System
                   • System.Configuration.Install
                   • System.Core
                   • System.Design
                   • System.Drawing
                   • System.Management
                   • System.Windows.Forms
                   • System.Xml
```

### 3.2 Migration Order Justification
- `CapeOpen` is a **leaf** (zero project dependencies).
- `Test` depends on `CapeOpen`.
- Under All-At-Once both move simultaneously; there is no separate "phase 1 / phase 2".
- After conversion: `CapeOpen` (`net8.0-windows`) is referenceable by `Test` (`net8.0`) because `net8.0` consumers can reference `net8.0-windows` libraries on Windows.

### 3.3 Cycles
**None detected.**

### 3.4 Critical Path
The critical path is `CapeOpen` itself — its size (43,716 LOC, 21 files with incidents) and COM/WinForms surface dominate the upgrade effort. `Test` is trivial (405 LOC, 0 incidents).

---

## 4. Project-by-Project Plans

### 4.1 CapeOpen.csproj

#### 4.1.1 Current State
- **Target framework**: `net48`
- **Project kind**: ClassicWinForms (non-SDK-style)
- **Output type**: Library (DLL)
- **ComVisible**: Yes — entire assembly with `RegisterForComInterop=true`
- **Strong name**: Yes — `CapeOpenKey.snk`
- **Application icon**: `epa_seal_medium.ico`
- **Files**: 54 (.cs and resources)
- **Files with incidents**: 21
- **LOC**: 43,716
- **Estimated LOC to modify**: 2,405+ (~5.5%)
- **NuGet packages**: 0
- **GAC assembly references**: 11

#### 4.1.2 Target State
- **Target framework**: `net8.0-windows`
- **Project kind**: SDK-style WinForms library
- **SDK**: `Microsoft.NET.Sdk`
- **Properties**:
  - `<TargetFramework>net8.0-windows</TargetFramework>`
  - `<UseWindowsForms>true</UseWindowsForms>`
  - `<EnableComHosting>true</EnableComHosting>` (replaces `RegisterForComInterop`)
  - `<SignAssembly>true</SignAssembly>` + `<AssemblyOriginatorKeyFile>CapeOpenKey.snk</AssemblyOriginatorKeyFile>`
  - `<ApplicationIcon>epa_seal_medium.ico</ApplicationIcon>`
  - `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
  - `<RootNamespace>CapeOpen</RootNamespace>`
  - `<AssemblyName>CapeOpen</AssemblyName>`

#### 4.1.3 Migration Steps

1. **Pre-conversion backup** — git commit captures pre-state (already done — branch `upgrade-to-NET8`).
2. **Convert to SDK-style project format** — Use `upgrade_convert_project_to_sdk_style`.
3. **Set target framework** to `net8.0-windows`.
4. **Enable WinForms support** — add `<UseWindowsForms>true</UseWindowsForms>`.
5. **Configure COM hosting**:
   - Remove `<RegisterForComInterop>true</RegisterForComInterop>`.
   - Add `<EnableComHosting>true</EnableComHosting>`.
   - If a stable `.tlb` is required by COFE/Aspen Plus, generate or reuse the existing `.tlb` and reference it via `<ComHostTypeLibrary Include="CapeOpen.tlb" />`. **Note**: `.NET 8` does not auto-generate type libraries.
6. **Migrate assembly references**:

   | Original GAC Reference | .NET 8 Replacement | Notes |
   |---|---|---|
   | `System` / `System.Core` / `System.Xml` | (implicit) | Auto-included in SDK |
   | `System.Drawing` | NuGet `System.Drawing.Common` (latest 8.x) | Required for `System.Drawing.Icon` etc. |
   | `System.Windows.Forms` | (implicit via `UseWindowsForms`) | — |
   | `System.Design` | Manual review | Most types removed; refactor or remove usages |
   | `Accessibility` | (implicit via WinForms) | Verify auto-included |
   | `System.Configuration.Install` | **No replacement** — review & remove | Refactor any `Installer`/`RunInstaller` usage |
   | `System.Management` | NuGet `System.Management` (latest 8.x) | Windows-only |
   | `EnvDTE` (EmbedInteropTypes) | NuGet `EnvDTE` | Verify still needed at runtime; may be removable if design-time only |

7. **Verify resource files** — `.resx` files and `epa_seal_medium.ico` carry over automatically under SDK-style.
8. **Verify `Properties\AssemblyInfo.cs`** — SDK-style auto-generates assembly attributes. Either delete redundant attributes (keep COM-specific ones like `[Guid]`, `[ComVisible]`), OR set `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>`.
9. **Restore + build + fix all compilation errors** in a single pass.
10. **Verify** — solution builds with 0 errors.

#### 4.1.4 Validation Checklist
- [ ] Project loads in Visual Studio without errors
- [ ] `dotnet restore` succeeds
- [ ] `dotnet build -c Release` produces 0 errors and 0 warnings
- [ ] Output assembly `CapeOpen.dll` is strong-name signed
- [ ] `CapeOpen.comhost.dll` is generated alongside `CapeOpen.dll`
- [ ] All `[ComVisible]`/`[Guid]` attributes preserved
- [ ] XML documentation file generated

---

### 4.2 Test.csproj

#### 4.2.1 Current State
- **Target framework**: `net48`
- **Project kind**: ClassicClassLibrary (non-SDK-style)
- **Files**: 2
- **LOC**: 405
- **API issues**: 0
- **Dependencies**: project reference to `CapeOpen.csproj`
- **NuGet packages**: 0

#### 4.2.2 Target State
- **Target framework**: `net8.0` (no Windows-specific APIs flagged)
- **Project kind**: SDK-style class library
- **SDK**: `Microsoft.NET.Sdk`

#### 4.2.3 Migration Steps
1. Convert to SDK-style using `upgrade_convert_project_to_sdk_style`.
2. Set `<TargetFramework>net8.0</TargetFramework>`.
3. Preserve project reference to `..\CapeOpen\CapeOpen.csproj`.
4. **⚠️ Requires validation**: Inspect the actual contents of `Test.csproj`/sources during execution to determine whether this is a unit test project. If yes, add appropriate test SDK packages:
   - `Microsoft.NET.Test.Sdk` (latest 17.x)
   - Test framework + adapter (xUnit / MSTest / NUnit) compatible with `net8.0`
   - `<IsPackable>false</IsPackable>`
5. Restore + build.

#### 4.2.4 Validation Checklist
- [ ] Project loads without errors
- [ ] Builds with 0 errors / 0 warnings
- [ ] If a test project: `dotnet test` discovers and runs all tests
- [ ] All previously passing tests still pass

---

## 5. Package Update Reference

The assessment found **0 NuGet packages** in either project. Both projects use only GAC assembly references today.

### 5.1 New Packages Introduced by Upgrade

These packages must be **added** to `CapeOpen.csproj` to replace GAC references that no longer exist in .NET 8:

| Package | Target Version | Project | Reason |
|---------|----------------|---------|--------|
| `System.Drawing.Common` | latest `8.0.x` | CapeOpen | Replaces `System.Drawing` GAC ref; required for `System.Drawing.Icon` |
| `System.Management` | latest `8.0.x` | CapeOpen | Replaces `System.Management` GAC ref (Windows-only) |
| `EnvDTE` | latest stable | CapeOpen | Only if `EnvDTE` references are required at runtime |

### 5.2 Test Project Packages (Conditional)

If `Test.csproj` is a unit test project (to be confirmed during execution):

| Package | Target Version | Reason |
|---------|----------------|--------|
| `Microsoft.NET.Test.Sdk` | latest `17.x` | Test runner |
| `<test framework>` (e.g. `MSTest.TestFramework`, `xunit`, `NUnit`) | latest `net8.0`-compatible | Test framework |
| `<test adapter>` | latest | VS Test Explorer integration |

### 5.3 Packages to Remove
None — there are no NuGet packages currently referenced.

### 5.4 Security Vulnerabilities
**None found** in the assessment.

---

## 6. Breaking Changes Catalog

### 6.1 Project File Format (Mandatory — `Project.0001`)
Both `.csproj` files must be converted from non-SDK Classic format to SDK-style.

### 6.2 Target Framework (Mandatory — `Project.0002`)
- `CapeOpen`: `net48` → `net8.0-windows`
- `Test`: `net48` → `net8.0`

### 6.3 Windows Forms (Binary Incompatible — 2,325 issues, 96.7%)
- **Resolution**: setting `<UseWindowsForms>true</UseWindowsForms>` on `net8.0-windows` makes WinForms APIs available again.
- **Code changes required**: typically **none** — the assessment counts API references that simply need the WinForms runtime, not API removals.
- **Watch list** — legacy controls removed in .NET Core/5+ (207 occurrences flagged):
  - `StatusBar` → `StatusStrip`
  - `DataGrid` → `DataGridView`
  - `ContextMenu` → `ContextMenuStrip`
  - `MainMenu` / `MenuItem` → `MenuStrip` / `ToolStripMenuItem`
  - `ToolBar` → `ToolStrip`
  - **Action**: during build, fix any `CS0234`/`CS0246` errors for these types.

### 6.4 COM Interop Changes (2 issues)
- `RegisterForComInterop` is **not supported** in SDK-style/.NET 8 → use `<EnableComHosting>true</EnableComHosting>`.
- `tlbexp.exe` does not ship with .NET 8 → type libraries must be supplied manually via `<ComHostTypeLibrary>`.
- COM activation now uses `<assembly>.comhost.dll` registered with `regsvr32` instead of `RegAsm`.
- **Impact on consumers (Aspen Plus, COFE)**: registration script changes; the COM CLSIDs and interface IIDs remain unchanged if `[Guid]` attributes are preserved.
- **Action**: review the 2 specific COM-interop API issues in the assessment for the exact APIs flagged.

### 6.5 GDI+ / System.Drawing (66 source-incompatible issues)
- `System.Drawing.Icon` and related types moved to NuGet package `System.Drawing.Common`.
- **Resolution**: add `System.Drawing.Common` package; no source-code changes required.

### 6.6 GAC References Without .NET 8 Replacements
- **`System.Configuration.Install`** — `Installer`, `RunInstaller`, custom installers no longer supported in .NET 8.
  - **Action**: search for `using System.Configuration.Install;` and `[RunInstaller]` attributes; remove or move to a separate Framework-targeted setup tool.
- **`System.Design`** — most types removed; `System.ComponentModel.Design` types are mostly available.
  - **Action**: identify specific `System.Design.*` usages during build; refactor or remove.

### 6.7 Strong-Name Signing
Format/behavior unchanged; SDK-style supports `<SignAssembly>` and `<AssemblyOriginatorKeyFile>` natively.

### 6.8 AssemblyInfo Generation
- SDK-style auto-generates `AssemblyInfo` attributes by default.
- **Conflict risk**: existing `Properties\AssemblyInfo.cs` will produce duplicate-attribute errors.
- **Resolution options**:
  1. Delete redundant attributes from `AssemblyInfo.cs`, keep only COM-specific ones (`[ComVisible]`, `[Guid]`).
  2. OR set `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` and keep `AssemblyInfo.cs` as-is.

---

## 7. Risk Management

### 7.1 High-Risk Items

| # | Risk | Severity | Likelihood | Mitigation |
|---|------|----------|------------|------------|
| 1 | COM consumers (Aspen Plus, COFE) cannot load the .NET 8 COM server | **High** | Medium | Test registration via `regsvr32 CapeOpen.comhost.dll`; verify CLSID/IID stability via preserved `[Guid]` attributes; smoke-test loading in target simulator |
| 2 | Type library (`.tlb`) regression — consumers depend on a specific TLB layout | High | Medium | Reuse existing `.tlb` via `<ComHostTypeLibrary>`; alternatively generate from IDL using `midl.exe` |
| 3 | `System.Configuration.Install` usage cannot be removed cleanly | Medium | Low | Inspect codebase first; if used, isolate installer logic into a separate Framework-targeted helper |
| 4 | `EnvDTE` runtime dependency breaks in non-VS environments | Medium | Low | Verify whether EnvDTE is actually used at runtime; if design-time only, remove it |
| 5 | Architecture mismatch — host process is x86 but `comhost.dll` is AnyCPU | Medium | Medium | Decide on platform: produce explicit `x64` (and optionally `x86`) builds |
| 6 | Hidden `System.Design` API usage causes compile errors | Low | Medium | Address during the bounded "fix all compilation errors" pass |
| 7 | Strong-name verification fails on .NET 8 | Low | Low | `.snk` format unchanged; verify post-build with `sn -v CapeOpen.dll` |

### 7.2 Security
**No security vulnerabilities found** in the assessment. No remediation needed.

### 7.3 Platform / Architecture Notes
- Current project: `AnyCPU` with `Prefer32Bit=false`.
- Recommendation for COM scenarios: produce **explicit `x64`** (and optionally `x86`) builds. .NET 8 `comhost.dll` is platform-specific.
- **Action**: confirm with downstream simulators whether they load 32-bit or 64-bit COM servers.

### 7.4 Rollback Plan
- All work occurs on branch `upgrade-to-NET8`.
- If upgrade fails: discard the branch; `master` remains untouched.

### 7.5 Contingency
- If COM hosting on .NET 8 is incompatible with target chemical simulators: keep the assembly as **dual-targeted** `net48;net8.0-windows` to provide a Framework fallback for COM consumers. **Fallback only** — initial plan is single-target `net8.0-windows`.

---

## 8. Testing & Validation Strategy

### 8.1 Build Validation (after atomic upgrade)
- [ ] `dotnet restore CapeOpen.sln` — succeeds
- [ ] `dotnet build CapeOpen.sln -c Debug` — 0 errors
- [ ] `dotnet build CapeOpen.sln -c Release` — 0 errors, 0 warnings
- [ ] `CapeOpen.dll`, `CapeOpen.comhost.dll`, `CapeOpen.xml` produced
- [ ] Strong name verified

### 8.2 Test Execution
- [ ] Determine if `Test.csproj` is an automated test project (inspect for `[TestMethod]`/`[Fact]`/`[Test]`).
- [ ] If yes: `dotnet test Test\Test.csproj` — all tests pass.
- [ ] If no: build-only validation suffices.

### 8.3 COM Registration Validation (manual / out of scope for automated tasks)
The following are **manual checks** to be performed by the developer after the automated upgrade tasks complete:
- Run `regsvr32 CapeOpen.comhost.dll` — succeeds.
- Run `regsvr32 /u CapeOpen.comhost.dll` — unregisters cleanly.
- Use `oleview` or PowerShell `New-Object -ComObject CapeOpen.MixerExample` (or appropriate ProgID) — instantiates without error.
- Load the COM server in Aspen Plus / COFE — operates correctly.

### 8.4 Per-Project Validation
Per §4.1.4 and §4.2.4 checklists.

---

## 9. Complexity & Effort Assessment

### 9.1 Per-Project Complexity

| Project | Complexity | Drivers |
|---------|------------|---------|
| `CapeOpen` | **High** | 43k LOC, ComVisible, WinForms-heavy, 11 GAC references, COM hosting model change |
| `Test` | **Low** | 405 LOC, 0 API issues, simple class library |

### 9.2 Phase Complexity
Single phase (atomic) — overall complexity dominated by `CapeOpen`. Rated **Medium-High** overall, primarily because of the COM hosting model change rather than code-level refactoring (most WinForms issues self-resolve via TFM).

### 9.3 Resource Requirements
- **Skills required**:
  - .NET 8 / SDK-style project format — Intermediate
  - WinForms on .NET 8 — Intermediate
  - COM Interop and `EnableComHosting` — **Advanced** (highest-skill requirement)
  - Strong naming — Basic
- **Parallel capacity**: Not applicable (atomic, single developer end-to-end).

---

## 10. Source Control Strategy

### 10.1 Branching
- **Source branch**: `master`
- **Upgrade branch**: `upgrade-to-NET8` (already created and checked out)
- **Pending changes**: committed before upgrade started (commit `d46cd9e`)
- All upgrade work occurs on `upgrade-to-NET8`.

### 10.2 Commit Strategy
Per All-At-Once strategy: **prefer a single atomic commit** for the entire upgrade. Recommended commit message:

```
Upgrade solution from .NET Framework 4.8 to .NET 8

- Convert CapeOpen.csproj and Test.csproj to SDK-style
- Target net8.0-windows (CapeOpen) and net8.0 (Test)
- Replace RegisterForComInterop with EnableComHosting
- Add System.Drawing.Common, System.Management package references
- Remove obsolete GAC references; preserve strong-name signing
- Resolve all compilation errors
```

If the work naturally splits into independently-meaningful units (e.g., project conversion vs compile-fix), separate commits are acceptable but stay on the same branch.

### 10.3 Merge Strategy
- After all validation passes, open a Pull Request `upgrade-to-NET8` → `master`.
- PR checklist (manual reviewer responsibilities):
  - [ ] Build succeeds
  - [ ] All tests pass
  - [ ] COM registration validated locally
  - [ ] (If applicable) `CapeOpenSetup` WiX project updated separately

---

## 11. Success Criteria

### 11.1 Technical Criteria
- [ ] `CapeOpen.csproj` targets `net8.0-windows` and is SDK-style.
- [ ] `Test.csproj` targets `net8.0` and is SDK-style.
- [ ] Both projects build with **0 errors** and **0 warnings** in `Debug` and `Release`.
- [ ] `CapeOpen.dll` is strong-name signed using `CapeOpenKey.snk`.
- [ ] `CapeOpen.comhost.dll` is produced.
- [ ] All `[ComVisible]`, `[Guid]`, `[ClassInterface]` attributes preserved (CLSIDs/IIDs unchanged).
- [ ] `Test.csproj` (if a test project) — all tests pass.
- [ ] No remaining references to removed APIs (`System.Configuration.Install`, legacy WinForms controls).

### 11.2 Quality Criteria
- [ ] No new compiler warnings introduced.
- [ ] Code style/formatting in non-project files unchanged (only project files and minimum compile-fix code modifications).
- [ ] XML documentation generation preserved.

### 11.3 Process Criteria
- [ ] All-At-Once strategy followed — single atomic upgrade pass.
- [ ] Work performed on `upgrade-to-NET8` branch.
- [ ] `master` branch remains untouched until PR review.

### 11.4 Deferred / Out-of-Scope
The following are **not** automated success criteria and require separate manual follow-up:
- Functional validation in Aspen Plus / COFE / other CAPE-OPEN consumers.
- Updating the `CapeOpenSetup` WiX installer project to use the new COM registration model.
- Cross-architecture (x86 vs x64) validation.

---

**End of Plan**
