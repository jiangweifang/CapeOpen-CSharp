// Registers .NET 8 ComHost-hosted CAPE-OPEN assemblies fully:
//   - Base CLSID / InprocServer32 / ProgID (replacing what regsvr32 should do)
//   - CAPE-OPEN Implemented Categories (CATIDs)
//   - CapeDescription metadata
// Workaround for: .NET 8 EnableComHosting + <ComHostTypeLibrary> together cause
// DllRegisterServer in *.comhost.dll to silently fail without writing any
// CLSID keys. We bypass it entirely by writing the registry directly, since
// the comhost.dll's exported DllGetClassObject will still work — it dispatches
// based on its embedded .clsidmap regardless of how the registry got populated.
//
// Usage:
//   CapeOpenRegistrar.exe register   <managed.dll> [<managed.dll> ...]
//   CapeOpenRegistrar.exe unregister <managed.dll> [<managed.dll> ...]
//
// For each managed assembly path, the corresponding *.comhost.dll alongside it
// is used as the InprocServer32 target.

using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

if (args.Length < 2 || (args[0] != "register" && args[0] != "unregister" && args[0] != "dump-tlb"))
{
    Console.Error.WriteLine("Usage: CapeOpenRegistrar (register|unregister|dump-tlb) <path> [<path> ...]");
    return 2;
}

if (args[0] == "dump-tlb")
{
    foreach (var tlbPath in args.Skip(1).Select(Path.GetFullPath))
    {
        Console.WriteLine($"=== {tlbPath} ===");
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
            Console.WriteLine($"  {kind,-20} {{{attr.guid}}}  {name}");
        }
        Marshal.ReleaseComObject(tlb);
    }
    return 0;
}

bool unregister = args[0] == "unregister";
var assemblyPaths = args.Skip(1).Select(Path.GetFullPath).ToArray();

// CAPE-OPEN CATIDs (from CapeOpen/Interfaces/COGuids1.cs)
const string CapeOpenComponent_CATID = "{678c09a1-7d66-11d2-a67d-00105a42887f}";
const string CapeUnitOperation_CATID = "{678c09a5-7d66-11d2-a67d-00105a42887f}";
const string CATID_MONITORING_OBJECT = "{7BA1AF89-B2E4-493d-BD80-2970BF4CBE99}";
const string Consumes_Thermo_CATID = "{4150C28A-EE06-403f-A871-87AFEC38A249}";
const string SupportsThermodynamics10_CATID = "{0D562DC8-EA8E-4210-AB39-B66513C0CD09}";
const string SupportsThermodynamics11_CATID = "{4667023A-5A8E-4cca-AB6D-9D78C5112FED}";

foreach (var asmPath in assemblyPaths)
{
    Console.WriteLine();
    Console.WriteLine($"=== {(unregister ? "Unregister" : "Register")} {asmPath} ===");
    ProcessAssembly(asmPath, unregister);
    RegisterTypeLibrary(asmPath, unregister);
}
return 0;

static void RegisterTypeLibrary(string asmPath, bool unreg)
{
    // Look for a .tlb file in a few conventional locations.
    var asmDir = Path.GetDirectoryName(asmPath)!;
    var asmName = Path.GetFileNameWithoutExtension(asmPath);
    var projDir = Path.GetFullPath(Path.Combine(asmDir, "..", "..", ".."));
    var candidates = new[]
    {
        Path.Combine(asmDir, asmName + ".tlb"),
        Path.Combine(projDir, "Tlb", asmName + ".tlb"),
    };
    var tlbPath = candidates.FirstOrDefault(File.Exists);
    if (tlbPath is null)
    {
        Console.WriteLine($"  (No .tlb found alongside or in ../Tlb; skipping TypeLib registration.)");
        return;
    }
    try
    {
        if (unreg)
        {
            // To unregister we need IID/version; load it first to get them.
            var tlb = (System.Runtime.InteropServices.ComTypes.ITypeLib)LoadTypeLibEx(tlbPath, RegKind.None);
            tlb.GetLibAttr(out var attrPtr);
            var attr = Marshal.PtrToStructure<System.Runtime.InteropServices.ComTypes.TYPELIBATTR>(attrPtr);
            tlb.ReleaseTLibAttr(attrPtr);
            UnRegisterTypeLib(ref attr.guid, attr.wMajorVerNum, attr.wMinorVerNum, attr.lcid, (SYSKIND)attr.syskind);
            Marshal.ReleaseComObject(tlb);
            Console.WriteLine($"  TypeLib unregistered: {tlbPath}");
        }
        else
        {
            var tlb = LoadTypeLibEx(tlbPath, RegKind.Register);
            Marshal.ReleaseComObject(tlb);
            Console.WriteLine($"  TypeLib registered:   {tlbPath}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  WARN: TypeLib registration failed: {ex.Message}");
    }
}

[DllImport("oleaut32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
[return: MarshalAs(UnmanagedType.Interface)]
static extern object LoadTypeLibEx(string szFile, RegKind regKind);

[DllImport("oleaut32.dll", PreserveSig = false)]
static extern void UnRegisterTypeLib(ref Guid libID, short wVerMajor, short wVerMinor, int lcid, SYSKIND syskind);

void ProcessAssembly(string asmPath, bool unreg)
{
    var asmDir = Path.GetDirectoryName(asmPath)!;
    var asmFileName = Path.GetFileNameWithoutExtension(asmPath);
    var comHostPath = Path.Combine(asmDir, asmFileName + ".comhost.dll");
    if (!File.Exists(comHostPath))
    {
        Console.Error.WriteLine($"  ERROR: {comHostPath} not found alongside the managed dll.");
        return;
    }
    var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
    // Also include Microsoft.WindowsDesktop.App runtime so we can resolve System.Windows.Forms
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

    // Effective assembly-level ComVisible default
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

        // Effective ComVisible
        bool? explicitCV = null;
        foreach (var a in attrs)
        {
            if (IsAttr(a, "ComVisible") && a.ConstructorArguments.Count > 0)
            { explicitCV = (bool)a.ConstructorArguments[0].Value!; break; }
        }
        bool comVisible = explicitCV ?? asmComVisible;
        if (!comVisible) continue;

        // Need explicit Guid
        string? guid = null;
        foreach (var a in attrs)
        {
            if (IsAttr(a, "Guid") && a.ConstructorArguments.Count > 0)
            { guid = (string?)a.ConstructorArguments[0].Value; break; }
        }
        if (string.IsNullOrEmpty(guid)) continue;

        // Filter to actual Cape components: derives from CapeUnitBase/CapeObjectBase OR has CapeName / CapeUnitOperation attribute
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
        Console.WriteLine($"  {t.FullName}  CLSID={clsid}");

        // We always work in the 64-bit registry view (HKLM\SOFTWARE\Classes\CLSID).
        // For 32-bit COM hosts on a 64-bit OS, the system maps to Wow6432Node automatically.
        var clsidKeyPath = $@"CLSID\{clsid}";

        if (unreg)
        {
            try { Registry.ClassesRoot.DeleteSubKeyTree(clsidKeyPath, throwOnMissingSubKey: false); }
            catch (Exception ex) { Console.WriteLine($"      WARN: delete failed: {ex.Message}"); }
            // Also remove ProgID
            var progIdKey = t.FullName!;
            try { Registry.ClassesRoot.DeleteSubKeyTree(progIdKey, throwOnMissingSubKey: false); } catch { }
            Console.WriteLine($"      [unregistered]");
            continue;
        }

        // 0) Base CLSID + InprocServer32 (since regsvr32 on the comhost.dll
        //    silently fails when ComHostTypeLibrary is configured with multiple CLSIDs)
        using var clsidKey = Registry.ClassesRoot.CreateSubKey(clsidKeyPath, writable: true)
            ?? throw new InvalidOperationException($"Could not create {clsidKeyPath}");
        clsidKey.SetValue("", t.FullName ?? "");
        using (var inproc = clsidKey.CreateSubKey("InprocServer32", writable: true)!)
        {
            inproc.SetValue("", comHostPath);
            inproc.SetValue("ThreadingModel", "Both");
            inproc.SetValue("CodeBase", codeBase);
        }
        // ProgID
        var progId = t.FullName!;
        using (var progIdKey = clsidKey.CreateSubKey("ProgId", writable: true)!)
        {
            progIdKey.SetValue("", progId);
        }
        using (var progIdRoot = Registry.ClassesRoot.CreateSubKey(progId, writable: true)!)
        {
            progIdRoot.SetValue("", t.FullName ?? "");
            using var clsidUnder = progIdRoot.CreateSubKey("CLSID", writable: true)!;
            clsidUnder.SetValue("", clsid);
        }

        // 1) Implemented Categories
        using (var impCat = clsidKey.CreateSubKey("Implemented Categories"))
        {
            impCat.CreateSubKey(CapeOpenComponent_CATID)?.Close();
            if (hasCapeUnitOp || derivesCape) impCat.CreateSubKey(CapeUnitOperation_CATID)?.Close();
            if (hasMonitor) impCat.CreateSubKey(CATID_MONITORING_OBJECT)?.Close();
            if (HasAttr(attrs, "CapeConsumesThermo")) impCat.CreateSubKey(Consumes_Thermo_CATID)?.Close();
            if (HasAttr(attrs, "CapeSupportsThermodynamics10")) impCat.CreateSubKey(SupportsThermodynamics10_CATID)?.Close();
            if (HasAttr(attrs, "CapeSupportsThermodynamics11")) impCat.CreateSubKey(SupportsThermodynamics11_CATID)?.Close();
        }

        // 2) (CodeBase already written above when creating InprocServer32)

        // 3) CapeDescription metadata
        var name = GetAttrArg(attrs, "CapeName") ?? t.FullName ?? "";
        var description = GetAttrArg(attrs, "CapeDescription") ?? "";
        var version = GetAttrArg(attrs, "CapeVersion") ?? "";
        var vendorUrl = GetAttrArg(attrs, "CapeVendorURL") ?? "";
        var helpUrl = GetAttrArg(attrs, "CapeHelpURL") ?? "";
        var about = GetAttrArg(attrs, "CapeAbout") ?? "";

        using (var desc = clsidKey.CreateSubKey("CapeDescription"))
        {
            desc.SetValue("Name", name);
            desc.SetValue("Description", description);
            desc.SetValue("CapeVersion", version);
            desc.SetValue("ComponentVersion", asmVersion);
            desc.SetValue("VendorURL", vendorUrl);
            desc.SetValue("HelpURL", helpUrl);
            desc.SetValue("About", about);
        }

        Console.WriteLine($"      Name=\"{name}\"  CapeUnitOp={(hasCapeUnitOp || derivesCape)}  Monitor={hasMonitor}");
    }
}

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
