// Core registration logic extracted from Program.cs so it can be driven
// from either the CLI or the WinForms UI. All log output goes through a
// TextWriter so the UI can redirect it to a log box on a background thread.

using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace CapeOpenRegistrar;

internal static class Registrar
{
    // CAPE-OPEN CATIDs (from CapeOpen/Interfaces/COGuids1.cs)
    public const string CapeOpenComponent_CATID = "{678c09a1-7d66-11d2-a67d-00105a42887f}";
    public const string CapeUnitOperation_CATID = "{678c09a5-7d66-11d2-a67d-00105a42887f}";
    public const string CATID_MONITORING_OBJECT = "{7BA1AF89-B2E4-493d-BD80-2970BF4CBE99}";
    public const string Consumes_Thermo_CATID = "{4150C28A-EE06-403f-A871-87AFEC38A249}";
    public const string SupportsThermodynamics10_CATID = "{0D562DC8-EA8E-4210-AB39-B66513C0CD09}";
    public const string SupportsThermodynamics11_CATID = "{4667023A-5A8E-4cca-AB6D-9D78C5112FED}";

    public enum BitnessTarget { Auto, X86, X64, Both }

    public static void Run(string action, IEnumerable<string> assemblyPaths, TextWriter log,
        BitnessTarget target = BitnessTarget.Auto)
    {
        bool unregister = action == "unregister";
        foreach (var asmPath in assemblyPaths.Select(Path.GetFullPath))
        {
            log.WriteLine();
            log.WriteLine($"=== {(unregister ? "Unregister" : "Register")} {asmPath} ===");
            var views = ResolveViews(asmPath, target, log);
            foreach (var view in views)
            {
                log.WriteLine($"  [view: {(view == RegistryView.Registry32 ? "32-bit (Wow6432Node)" : "64-bit (native)")}]");
                ProcessAssembly(asmPath, unregister, log, view);
            }
            RegisterTypeLibrary(asmPath, unregister, log);
        }
    }

    public static void Export(string outReg, IEnumerable<string> assemblyPaths, TextWriter log,
        BitnessTarget target = BitnessTarget.Auto)
    {
        var outUnreg = Path.Combine(
            Path.GetDirectoryName(outReg)!,
            Path.GetFileNameWithoutExtension(outReg) + ".unregister" + Path.GetExtension(outReg));
        var regSb = new StringBuilder();
        var unregSb = new StringBuilder();
        regSb.AppendLine("Windows Registry Editor Version 5.00");
        regSb.AppendLine();
        unregSb.AppendLine("Windows Registry Editor Version 5.00");
        unregSb.AppendLine();
        foreach (var asmPath in assemblyPaths.Select(Path.GetFullPath))
        {
            log.WriteLine();
            log.WriteLine($"=== Export {asmPath} ===");
            var views = ResolveViews(asmPath, target, log);
            foreach (var view in views)
            {
                log.WriteLine($"  [view: {(view == RegistryView.Registry32 ? "32-bit (Wow6432Node)" : "64-bit (native)")}]");
                ExportAssembly(asmPath, regSb, unregSb, log, view);
            }
        }
        File.WriteAllText(outReg, regSb.ToString(), Encoding.Unicode);
        File.WriteAllText(outUnreg, unregSb.ToString(), Encoding.Unicode);
        log.WriteLine();
        log.WriteLine($"Wrote: {outReg}");
        log.WriteLine($"Wrote: {outUnreg}");
    }

    public static void DumpTlb(IEnumerable<string> tlbPaths, TextWriter log)
    {
        foreach (var tlbPath in tlbPaths.Select(Path.GetFullPath))
        {
            log.WriteLine($"=== {tlbPath} ===");
            var tlb = (System.Runtime.InteropServices.ComTypes.ITypeLib)LoadTypeLibEx(tlbPath, RegKind.None);
            int count = tlb.GetTypeInfoCount();
            for (int i = 0; i < count; i++)
            {
                tlb.GetTypeInfo(i, out var ti);
                tlb.GetDocumentation(i, out var name, out _, out _, out _);
                tlb.GetTypeInfoType(i, out var kind);
                ti.GetTypeAttr(out var attrPtr);
                var attr = Marshal.PtrToStructure<System.Runtime.InteropServices.ComTypes.TYPEATTR>(attrPtr);
                ti.ReleaseTypeAttr(attrPtr);
                log.WriteLine($"  {kind,-20} {{{attr.guid}}}  {name}");
            }
            Marshal.ReleaseComObject(tlb);
        }
    }

    static void RegisterTypeLibrary(string asmPath, bool unreg, TextWriter log)
    {
        var asmDir = Path.GetDirectoryName(asmPath)!;
        var asmName = Path.GetFileNameWithoutExtension(asmPath);
        // Climb up searching for a sibling "Tlb\<name>.tlb". Layouts differ:
        //   .NET Framework:  <proj>\bin\<Config>\<name>.dll            -> 3 levels up
        //   .NET 8 (SDK):    <proj>\bin\<Platform>\<Config>\<tfm>\...  -> 4 levels up
        var candidates = new List<string> { Path.Combine(asmDir, asmName + ".tlb") };
        var cur = asmDir;
        for (int i = 0; i < 6 && cur is not null; i++)
        {
            candidates.Add(Path.Combine(cur, "Tlb", asmName + ".tlb"));
            cur = Path.GetDirectoryName(cur);
        }
        var tlbPath = candidates.FirstOrDefault(File.Exists);
        if (tlbPath is null)
        {
            log.WriteLine($"  (No .tlb found alongside or in any ancestor Tlb\\ folder; skipping TypeLib registration.)");
            return;
        }
        log.WriteLine($"  TypeLib source: {tlbPath}");
        try
        {
            if (unreg)
            {
                var tlb = (System.Runtime.InteropServices.ComTypes.ITypeLib)LoadTypeLibEx(tlbPath, RegKind.None);
                tlb.GetLibAttr(out var attrPtr);
                var attr = Marshal.PtrToStructure<System.Runtime.InteropServices.ComTypes.TYPELIBATTR>(attrPtr);
                tlb.ReleaseTLibAttr(attrPtr);
                UnRegisterTypeLib(ref attr.guid, attr.wMajorVerNum, attr.wMinorVerNum, attr.lcid, (SYSKIND)attr.syskind);
                Marshal.ReleaseComObject(tlb);
                log.WriteLine($"  TypeLib unregistered: {tlbPath}");
            }
            else
            {
                var tlb = LoadTypeLibEx(tlbPath, RegKind.Register);
                Marshal.ReleaseComObject(tlb);
                log.WriteLine($"  TypeLib registered:   {tlbPath}");
            }
        }
        catch (Exception ex)
        {
            log.WriteLine($"  WARN: TypeLib registration failed: {ex.Message}");
        }
    }

    [DllImport("oleaut32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    [return: MarshalAs(UnmanagedType.Interface)]
    static extern object LoadTypeLibEx(string szFile, RegKind regKind);

    [DllImport("oleaut32.dll", PreserveSig = false)]
    static extern void UnRegisterTypeLib(ref Guid libID, short wVerMajor, short wVerMinor, int lcid, SYSKIND syskind);

    static void ProcessAssembly(string asmPath, bool unreg, TextWriter log, RegistryView view)
    {
        var asmDir = Path.GetDirectoryName(asmPath)!;
        var asmFileName = Path.GetFileNameWithoutExtension(asmPath);
        var comHostPath = Path.Combine(asmDir, asmFileName + ".comhost.dll");
        if (!File.Exists(comHostPath))
        {
            log.WriteLine($"  ERROR: {comHostPath} not found alongside the managed dll.");
            return;
        }
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var dotnetRoot = Path.GetFullPath(Path.Combine(runtimeDir, "..", "..", ".."));
        var desktopAppDir = Path.Combine(dotnetRoot, "shared", "Microsoft.WindowsDesktop.App");
        var desktopRuntime = Directory.Exists(desktopAppDir)
            ? Directory.GetDirectories(desktopAppDir).OrderByDescending(d => d).FirstOrDefault()
            : null;
        var paths = Directory.GetFiles(runtimeDir, "*.dll")
            .Concat(desktopRuntime is not null ? Directory.GetFiles(desktopRuntime, "*.dll") : Array.Empty<string>())
            .Concat(Directory.GetFiles(asmDir, "*.dll"))
            .Distinct()
            .ToArray();

        var resolver = new PathAssemblyResolver(paths);
        using var mlc = new MetadataLoadContext(resolver);
        var asm = mlc.LoadFromAssemblyPath(asmPath);
        var asmVersion = asm.GetName().Version!.ToString();
        var codeBase = "file:///" + asmPath.Replace('\\', '/');

        bool asmComVisible = false;
        foreach (var a in asm.GetCustomAttributesData())
        {
            if (IsAttr(a, "ComVisible") && a.ConstructorArguments.Count > 0)
            {
                asmComVisible = (bool)a.ConstructorArguments[0].Value!;
                break;
            }
        }

        foreach (var t in asm.GetExportedTypes())
        {
            if (t.IsAbstract || t.IsInterface) continue;
            var attrs = t.GetCustomAttributesData();

            bool? explicitCV = null;
            foreach (var a in attrs)
            {
                if (IsAttr(a, "ComVisible") && a.ConstructorArguments.Count > 0)
                { explicitCV = (bool)a.ConstructorArguments[0].Value!; break; }
            }
            bool comVisible = explicitCV ?? asmComVisible;
            if (!comVisible) continue;

            string? guid = null;
            foreach (var a in attrs)
            {
                if (IsAttr(a, "Guid") && a.ConstructorArguments.Count > 0)
                { guid = (string?)a.ConstructorArguments[0].Value; break; }
            }
            if (string.IsNullOrEmpty(guid)) continue;

            bool hasCapeUnitOp = HasAttr(attrs, "CapeUnitOperation");
            bool hasMonitor = HasAttr(attrs, "CapeFlowsheetMonitoring");
            bool hasCapeName = HasAttr(attrs, "CapeName");
            bool derivesCape = false;
            for (var bt = t.BaseType; bt is not null; bt = bt.BaseType)
            {
                if (bt.FullName is "CapeOpen.CapeUnitBase" or "CapeOpen.CapeObjectBase")
                { derivesCape = true; break; }
            }
            if (!(hasCapeUnitOp || hasCapeName || derivesCape)) continue;

            var clsid = "{" + guid + "}";
            log.WriteLine($"  {t.FullName}  CLSID={clsid}");

            var clsidKeyPath = $@"CLSID\{clsid}";

            using var classesRoot = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, view);

            if (unreg)
            {
                try { classesRoot.DeleteSubKeyTree(clsidKeyPath, throwOnMissingSubKey: false); }
                catch (Exception ex) { log.WriteLine($"      WARN: delete failed: {ex.Message}"); }
                var progIdKey = t.FullName!;
                try { classesRoot.DeleteSubKeyTree(progIdKey, throwOnMissingSubKey: false); } catch { }
                log.WriteLine($"      [unregistered]");
                continue;
            }

            using var clsidKey = classesRoot.CreateSubKey(clsidKeyPath, writable: true)
                ?? throw new InvalidOperationException($"Could not create {clsidKeyPath}");
            clsidKey.SetValue("", t.FullName ?? "");
            using (var inproc = clsidKey.CreateSubKey("InprocServer32", writable: true)!)
            {
                inproc.SetValue("", comHostPath);
                inproc.SetValue("ThreadingModel", "Both");
                inproc.SetValue("CodeBase", codeBase);
            }
            var progId = t.FullName!;
            using (var progIdKey = clsidKey.CreateSubKey("ProgId", writable: true)!)
            {
                progIdKey.SetValue("", progId);
            }
            using (var progIdRoot = classesRoot.CreateSubKey(progId, writable: true)!)
            {
                progIdRoot.SetValue("", t.FullName ?? "");
                using var clsidUnder = progIdRoot.CreateSubKey("CLSID", writable: true)!;
                clsidUnder.SetValue("", clsid);
            }

            using (var impCat = clsidKey.CreateSubKey("Implemented Categories"))
            {
                impCat!.CreateSubKey(CapeOpenComponent_CATID)?.Close();
                if (hasCapeUnitOp || derivesCape) impCat.CreateSubKey(CapeUnitOperation_CATID)?.Close();
                if (hasMonitor) impCat.CreateSubKey(CATID_MONITORING_OBJECT)?.Close();
                if (HasAttr(attrs, "CapeConsumesThermo")) impCat.CreateSubKey(Consumes_Thermo_CATID)?.Close();
                if (HasAttr(attrs, "CapeSupportsThermodynamics10")) impCat.CreateSubKey(SupportsThermodynamics10_CATID)?.Close();
                if (HasAttr(attrs, "CapeSupportsThermodynamics11")) impCat.CreateSubKey(SupportsThermodynamics11_CATID)?.Close();
            }

            var name = GetAttrArg(attrs, "CapeName") ?? t.FullName ?? "";
            var description = GetAttrArg(attrs, "CapeDescription") ?? "";
            var version = GetAttrArg(attrs, "CapeVersion") ?? "";
            var vendorUrl = GetAttrArg(attrs, "CapeVendorURL") ?? "";
            var helpUrl = GetAttrArg(attrs, "CapeHelpURL") ?? "";
            var about = GetAttrArg(attrs, "CapeAbout") ?? "";

            using (var desc = clsidKey.CreateSubKey("CapeDescription"))
            {
                desc!.SetValue("Name", name);
                desc.SetValue("Description", description);
                desc.SetValue("CapeVersion", version);
                desc.SetValue("ComponentVersion", asmVersion);
                desc.SetValue("VendorURL", vendorUrl);
                desc.SetValue("HelpURL", helpUrl);
                desc.SetValue("About", about);
            }

            log.WriteLine($"      Name=\"{name}\"  CapeUnitOp={(hasCapeUnitOp || derivesCape)}  Monitor={hasMonitor}");
        }
    }

    static void ExportAssembly(string asmPath, StringBuilder reg, StringBuilder unreg, TextWriter log, RegistryView view)
    {
        var asmDir = Path.GetDirectoryName(asmPath)!;
        var asmFileName = Path.GetFileNameWithoutExtension(asmPath);
        var comHostPath = Path.Combine(asmDir, asmFileName + ".comhost.dll");
        if (!File.Exists(comHostPath))
        {
            log.WriteLine($"  ERROR: {comHostPath} not found alongside the managed dll.");
            return;
        }
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var dotnetRoot = Path.GetFullPath(Path.Combine(runtimeDir, "..", "..", ".."));
        var desktopAppDir = Path.Combine(dotnetRoot, "shared", "Microsoft.WindowsDesktop.App");
        var desktopRuntime = Directory.Exists(desktopAppDir)
            ? Directory.GetDirectories(desktopAppDir).OrderByDescending(d => d).FirstOrDefault()
            : null;
        var paths = Directory.GetFiles(runtimeDir, "*.dll")
            .Concat(desktopRuntime is not null ? Directory.GetFiles(desktopRuntime, "*.dll") : Array.Empty<string>())
            .Concat(Directory.GetFiles(asmDir, "*.dll"))
            .Distinct()
            .ToArray();

        var resolver = new PathAssemblyResolver(paths);
        using var mlc = new MetadataLoadContext(resolver);
        var asm = mlc.LoadFromAssemblyPath(asmPath);
        var asmVersion = asm.GetName().Version!.ToString();
        var codeBase = "file:///" + asmPath.Replace('\\', '/');

        bool asmComVisible = false;
        foreach (var a in asm.GetCustomAttributesData())
        {
            if (IsAttr(a, "ComVisible") && a.ConstructorArguments.Count > 0)
            { asmComVisible = (bool)a.ConstructorArguments[0].Value!; break; }
        }

        foreach (var t in asm.GetExportedTypes())
        {
            if (t.IsAbstract || t.IsInterface) continue;
            var attrs = t.GetCustomAttributesData();

            bool? explicitCV = null;
            foreach (var a in attrs)
            {
                if (IsAttr(a, "ComVisible") && a.ConstructorArguments.Count > 0)
                { explicitCV = (bool)a.ConstructorArguments[0].Value!; break; }
            }
            bool comVisible = explicitCV ?? asmComVisible;
            if (!comVisible) continue;

            string? guid = null;
            foreach (var a in attrs)
            {
                if (IsAttr(a, "Guid") && a.ConstructorArguments.Count > 0)
                { guid = (string?)a.ConstructorArguments[0].Value; break; }
            }
            if (string.IsNullOrEmpty(guid)) continue;

            bool hasCapeUnitOp = HasAttr(attrs, "CapeUnitOperation");
            bool hasMonitor = HasAttr(attrs, "CapeFlowsheetMonitoring");
            bool hasCapeName = HasAttr(attrs, "CapeName");
            bool derivesCape = false;
            for (var bt = t.BaseType; bt is not null; bt = bt.BaseType)
            {
                if (bt.FullName is "CapeOpen.CapeUnitBase" or "CapeOpen.CapeObjectBase")
                { derivesCape = true; break; }
            }
            if (!(hasCapeUnitOp || hasCapeName || derivesCape)) continue;

            var clsid = "{" + guid + "}";
            var progId = t.FullName!;
            // For 32-bit view on 64-bit Windows, keys live under Wow6432Node. Be explicit
            // with HKLM\SOFTWARE\Classes[\Wow6432Node] so .reg import routes correctly.
            var classesRoot = view == RegistryView.Registry32
                ? @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes\Wow6432Node"
                : @"HKEY_LOCAL_MACHINE\SOFTWARE\Classes";
            var clsidRoot = $@"{classesRoot}\CLSID\{clsid}";
            var progIdRoot = $@"{classesRoot}\{RegEscape(progId)}";
            log.WriteLine($"  {t.FullName}  CLSID={clsid}");

            reg.AppendLine($"[{clsidRoot}]");
            reg.AppendLine($"@=\"{RegEscape(progId)}\"");
            reg.AppendLine();
            reg.AppendLine($"[{clsidRoot}\\InprocServer32]");
            reg.AppendLine($"@=\"{RegEscape(comHostPath)}\"");
            reg.AppendLine("\"ThreadingModel\"=\"Both\"");
            reg.AppendLine($"\"CodeBase\"=\"{RegEscape(codeBase)}\"");
            reg.AppendLine();
            reg.AppendLine($"[{clsidRoot}\\ProgId]");
            reg.AppendLine($"@=\"{RegEscape(progId)}\"");
            reg.AppendLine();
            reg.AppendLine($"[{progIdRoot}]");
            reg.AppendLine($"@=\"{RegEscape(progId)}\"");
            reg.AppendLine();
            reg.AppendLine($"[{progIdRoot}\\CLSID]");
            reg.AppendLine($"@=\"{RegEscape(clsid)}\"");
            reg.AppendLine();

            reg.AppendLine($"[{clsidRoot}\\Implemented Categories\\{CapeOpenComponent_CATID}]");
            reg.AppendLine();
            if (hasCapeUnitOp || derivesCape)
            {
                reg.AppendLine($"[{clsidRoot}\\Implemented Categories\\{CapeUnitOperation_CATID}]");
                reg.AppendLine();
            }
            if (hasMonitor)
            {
                reg.AppendLine($"[{clsidRoot}\\Implemented Categories\\{CATID_MONITORING_OBJECT}]");
                reg.AppendLine();
            }
            if (HasAttr(attrs, "CapeConsumesThermo"))
            {
                reg.AppendLine($"[{clsidRoot}\\Implemented Categories\\{Consumes_Thermo_CATID}]");
                reg.AppendLine();
            }
            if (HasAttr(attrs, "CapeSupportsThermodynamics10"))
            {
                reg.AppendLine($"[{clsidRoot}\\Implemented Categories\\{SupportsThermodynamics10_CATID}]");
                reg.AppendLine();
            }
            if (HasAttr(attrs, "CapeSupportsThermodynamics11"))
            {
                reg.AppendLine($"[{clsidRoot}\\Implemented Categories\\{SupportsThermodynamics11_CATID}]");
                reg.AppendLine();
            }

            var name = GetAttrArg(attrs, "CapeName") ?? t.FullName ?? "";
            var description = GetAttrArg(attrs, "CapeDescription") ?? "";
            var version = GetAttrArg(attrs, "CapeVersion") ?? "";
            var vendorUrl = GetAttrArg(attrs, "CapeVendorURL") ?? "";
            var helpUrl = GetAttrArg(attrs, "CapeHelpURL") ?? "";
            var about = GetAttrArg(attrs, "CapeAbout") ?? "";

            reg.AppendLine($"[{clsidRoot}\\CapeDescription]");
            reg.AppendLine($"\"Name\"=\"{RegEscape(name)}\"");
            reg.AppendLine($"\"Description\"=\"{RegEscape(description)}\"");
            reg.AppendLine($"\"CapeVersion\"=\"{RegEscape(version)}\"");
            reg.AppendLine($"\"ComponentVersion\"=\"{RegEscape(asmVersion)}\"");
            reg.AppendLine($"\"VendorURL\"=\"{RegEscape(vendorUrl)}\"");
            reg.AppendLine($"\"HelpURL\"=\"{RegEscape(helpUrl)}\"");
            reg.AppendLine($"\"About\"=\"{RegEscape(about)}\"");
            reg.AppendLine();

            unreg.AppendLine($"[-{clsidRoot}]");
            unreg.AppendLine($"[-{progIdRoot}]");
            unreg.AppendLine();

            log.WriteLine($"      Name=\"{name}\"  CapeUnitOp={(hasCapeUnitOp || derivesCape)}  Monitor={hasMonitor}");
        }
    }

    static string RegEscape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    static bool IsAttr(CustomAttributeData a, string shortName) =>
        a.AttributeType.Name == shortName || a.AttributeType.Name == shortName + "Attribute";

    static bool HasAttr(IList<CustomAttributeData> attrs, string shortName)
    {
        foreach (var a in attrs)
        {
            if (IsAttr(a, shortName))
            {
                if (a.ConstructorArguments.Count > 0 && a.ConstructorArguments[0].Value is bool b)
                    return b;
                return true;
            }
        }
        return false;
    }

    static string? GetAttrArg(IList<CustomAttributeData> attrs, string shortName)
    {
        foreach (var a in attrs)
        {
            if (IsAttr(a, shortName) && a.ConstructorArguments.Count > 0)
                return a.ConstructorArguments[0].Value as string;
        }
        return null;
    }

    enum RegKind { Default = 0, Register = 1, None = 2 }
    enum SYSKIND { Win16, Win32, Win64, Mac }

    // Resolve which registry view(s) to write based on user selection.
    // Auto  → read the *.comhost.dll PE header and pick the matching view.
    // X86   → Registry32 (Wow6432Node on 64-bit Windows).
    // X64   → Registry64 (native).
    // Both  → both views.
    static RegistryView[] ResolveViews(string asmPath, BitnessTarget target, TextWriter log)
    {
        switch (target)
        {
            case BitnessTarget.X86: return new[] { RegistryView.Registry32 };
            case BitnessTarget.X64: return new[] { RegistryView.Registry64 };
            case BitnessTarget.Both: return new[] { RegistryView.Registry32, RegistryView.Registry64 };
            default:
                var asmDir = Path.GetDirectoryName(asmPath)!;
                var asmName = Path.GetFileNameWithoutExtension(asmPath);
                var comHost = Path.Combine(asmDir, asmName + ".comhost.dll");
                var detected = File.Exists(comHost) ? DetectBitness(comHost) : (ushort)0;
                switch (detected)
                {
                    case 0x014c: // IMAGE_FILE_MACHINE_I386
                        log.WriteLine($"  (Auto: detected x86 comhost.)");
                        return new[] { RegistryView.Registry32 };
                    case 0x8664: // IMAGE_FILE_MACHINE_AMD64
                    case 0xAA64: // IMAGE_FILE_MACHINE_ARM64
                        log.WriteLine($"  (Auto: detected 64-bit comhost.)");
                        return new[] { RegistryView.Registry64 };
                    default:
                        log.WriteLine($"  (Auto: could not detect bitness, defaulting to 64-bit view.)");
                        return new[] { RegistryView.Registry64 };
                }
        }
    }

    // Read the PE "Machine" field from a Windows DLL/EXE without loading it.
    static ushort DetectBitness(string path)
    {
        try
        {
            using var fs = File.OpenRead(path);
            using var br = new BinaryReader(fs);
            fs.Seek(0x3C, SeekOrigin.Begin);
            int peOffset = br.ReadInt32();
            fs.Seek(peOffset, SeekOrigin.Begin);
            if (br.ReadUInt32() != 0x00004550) return 0; // "PE\0\0"
            return br.ReadUInt16();
        }
        catch { return 0; }
    }
}
