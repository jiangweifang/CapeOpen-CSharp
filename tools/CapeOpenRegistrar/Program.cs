// Entry point.
//   - No args: launch the WinForms UI.
//   - With args: behave like the original CLI so existing scripts keep working.
//
// CLI usage:
//   CapeOpenRegistrar.exe register   [--target=auto|x86|x64|both] <managed.dll> [<managed.dll> ...]
//   CapeOpenRegistrar.exe unregister [--target=auto|x86|x64|both] <managed.dll> [<managed.dll> ...]
//   CapeOpenRegistrar.exe export     [--target=auto|x86|x64|both] <out.reg> <managed.dll> [<managed.dll> ...]
//   CapeOpenRegistrar.exe dump-tlb   <tlb> [<tlb> ...]

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CapeOpenRegistrar;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
            return 0;
        }

        if (args.Length < 2 ||
            (args[0] != "register" && args[0] != "unregister" && args[0] != "dump-tlb" && args[0] != "export"))
        {
            Console.Error.WriteLine("Usage: CapeOpenRegistrar (register|unregister|dump-tlb|export) [--target=auto|x86|x64|both] <args...>");
            return 2;
        }

        // Pull out optional --target=... flag (register/unregister/export only).
        var target = Registrar.BitnessTarget.Auto;
        var rest = new List<string>();
        foreach (var a in args.Skip(1))
        {
            if (a.StartsWith("--target=", StringComparison.OrdinalIgnoreCase))
            {
                var val = a.Substring("--target=".Length);
                if (!Enum.TryParse<Registrar.BitnessTarget>(val, ignoreCase: true, out target))
                {
                    Console.Error.WriteLine($"Invalid --target value '{val}'. Expected: auto|x86|x64|both.");
                    return 2;
                }
            }
            else rest.Add(a);
        }

        try
        {
            switch (args[0])
            {
                case "dump-tlb":
                    Registrar.DumpTlb(rest, Console.Out);
                    break;
                case "export":
                    if (rest.Count < 2)
                    {
                        Console.Error.WriteLine("Usage: CapeOpenRegistrar export [--target=...] <out.reg> <managed.dll> [<managed.dll> ...]");
                        return 2;
                    }
                    Registrar.Export(System.IO.Path.GetFullPath(rest[0]), rest.Skip(1), Console.Out, target);
                    break;
                case "register":
                case "unregister":
                    Registrar.Run(args[0], rest, Console.Out, target);
                    break;
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }
}
