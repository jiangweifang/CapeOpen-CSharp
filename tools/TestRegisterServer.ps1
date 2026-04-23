param([string]$dll = "D:\GitHub\CapeOpen-CSharp\CapeOpen\bin\Debug\net8.0-windows\CapeOpen.comhost.dll")

Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;
public static class DllHost {
    [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
    public static extern IntPtr LoadLibrary(string lpFileName);
    [DllImport("kernel32.dll", CharSet=CharSet.Ansi, ExactSpelling=true, SetLastError=true)]
    public static extern IntPtr GetProcAddress(IntPtr h, string proc);
    [DllImport("kernel32.dll", SetLastError=true)]
    public static extern bool FreeLibrary(IntPtr h);
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int DllRegisterServerDelegate();
    public static int CallDllRegisterServer(string path) {
        IntPtr h = LoadLibrary(path);
        if (h == IntPtr.Zero) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        try {
            IntPtr p = GetProcAddress(h, "DllRegisterServer");
            if (p == IntPtr.Zero) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            var fn = (DllRegisterServerDelegate)Marshal.GetDelegateForFunctionPointer(p, typeof(DllRegisterServerDelegate));
            return fn();
        } finally { FreeLibrary(h); }
    }
}
"@

$hr = [DllHost]::CallDllRegisterServer($dll)
Write-Host ("DllRegisterServer returned HRESULT = 0x{0:X8}" -f $hr)
if ($hr -ne 0) {
    Write-Host "Error: $([System.ComponentModel.Win32Exception]::new($hr).Message)"
}
