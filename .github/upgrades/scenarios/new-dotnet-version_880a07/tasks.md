# CapeOpen .NET Framework 4.8 → .NET 8.0 Upgrade Tasks

## Overview

This document tracks the execution of the CapeOpen solution upgrade from .NET Framework 4.8 to .NET 8.0. Both projects will be upgraded simultaneously in a single atomic operation, followed by testing and validation.

**Progress**: 3/4 tasks complete (75%) ![0%](https://progress-bar.xyz/75)

---

## Tasks

### [✓] TASK-001: Verify prerequisites *(Completed: 2026-04-23 03:53)*
**References**: Plan §9.2

- [✓] (1) Verify .NET 8 SDK is installed
- [✓] (2) .NET 8 SDK meets minimum requirements (**Verify**)

---

### [✓] TASK-002: Atomic framework and dependency upgrade with COM hosting *(Completed: 2026-04-23 04:02)*
**References**: Plan §4.1.3, Plan §4.2.3, Plan §5.1, Plan §6

- [✓] (1) Build CapeOpen.csproj (net48 Classic) using MSBuild to generate `CapeOpen\bin\Debug\CapeOpen.tlb`
- [✓] (2) Copy `CapeOpen\bin\Debug\CapeOpen.tlb` to `CapeOpen\Tlb\CapeOpen.tlb` and git add the file
- [✓] (3) Convert both CapeOpen.csproj and Test.csproj to SDK-style using `upgrade_convert_project_to_sdk_style`
- [✓] (4) Update CapeOpen.csproj: set `<TargetFramework>net8.0-windows</TargetFramework>`, add `<UseWindowsForms>true</UseWindowsForms>`, configure COM hosting per Plan §4.1.3 step 5.4
- [✓] (5) Update Test.csproj: set `<TargetFramework>net8.0</TargetFramework>`
- [✓] (6) Add NuGet package references to CapeOpen.csproj per Plan §5.1 (System.Drawing.Common, System.Management, EnvDTE if needed)
- [✓] (7) Restore dependencies
- [✓] (8) All dependencies restored successfully (**Verify**)
- [✓] (9) Build solution to identify compilation errors
- [✓] (10) Fix all compilation errors found (reference Plan §6 Breaking Changes Catalog for GAC reference replacements, System.Configuration.Install removal, legacy WinForms controls)
- [✓] (11) Rebuild solution to verify fixes
- [✓] (12) Solution builds with 0 errors and CapeOpen.comhost.dll is generated (**Verify**)

---

### [✓] TASK-003: Run full test suite and validate upgrade *(Completed: 2026-04-23 04:02)*
**References**: Plan §4.2.3, Plan §8.2

- [✓] (1) Inspect Test.csproj source files to determine if unit test project (look for [TestMethod], [Fact], [Test] attributes)
- [✓] (2) If test project: add test SDK packages per Plan §5.2 (Microsoft.NET.Test.Sdk, test framework, adapter), set `<IsPackable>false</IsPackable>`, restore dependencies, run `dotnet test Test\Test.csproj`, fix any failures per Plan §6 Breaking Changes, re-run tests; if not test project: verify it builds successfully
- [✓] (3) All tests pass with 0 failures (if test project) OR Test.csproj confirmed as buildable non-test library (**Verify**)

---

### [▶] TASK-004: Final commit
**References**: Plan §10.2

- [▶] (1) Commit all changes with message: "Upgrade CapeOpen solution from .NET Framework 4.8 to .NET 8.0"

---





