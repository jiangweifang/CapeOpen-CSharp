using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CapeOpen
{
    // NOTE: IPersist (GUID 0000010c-...) is NOT declared separately because
    // the CLR's CCW already provides a built-in IPersist implementation for all
    // ComVisible managed objects. Declaring a custom [ComImport] IPersist with
    // the same GUID causes a vtable conflict in the CCW, leading to
    // FatalExecutionEngineError (0xc0000005).
    //
    // Instead, GetClassID is "flattened" into each derived interface so that
    // the COM vtable layout matches the native definition while avoiding the
    // duplicate-IPersist conflict.

    [System.Runtime.InteropServices.ComImport()]
    [System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
    [System.Runtime.InteropServices.Guid("00000109-0000-0000-C000-000000000046")]
    [System.Runtime.InteropServices.ComVisibleAttribute(false)]
    public interface IPersistStream
    {
        // vtable slot 3 – inherited from IPersist
        void GetClassID(out Guid pClassID);
        // vtable slot 4
        [System.Runtime.InteropServices.PreserveSig]
        int IsDirty();
        // vtable slot 5
        void Load(System.Runtime.InteropServices.ComTypes.IStream pStm);
        // vtable slot 6
        void Save(System.Runtime.InteropServices.ComTypes.IStream pStm,
            [System.Runtime.InteropServices.InAttribute, System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)] bool fClearDirty);
        // vtable slot 7
        void GetSizeMax(out long pcbSize);
    };

    [System.Runtime.InteropServices.ComImport()]
    [System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
    [System.Runtime.InteropServices.Guid("7FD52380-4E07-101B-AE2D-08002B2EC713")]
    [System.Runtime.InteropServices.ComVisibleAttribute(false)]
    public interface IPersistStreamInit
    {
        // vtable slot 3 – inherited from IPersist
        void GetClassID(out Guid pClassID);
        // vtable slot 4
        [System.Runtime.InteropServices.PreserveSig]
        int IsDirty();
        // vtable slot 5
        void Load(System.Runtime.InteropServices.ComTypes.IStream pStm);
        // vtable slot 6
        void Save(System.Runtime.InteropServices.ComTypes.IStream pStm,
            [System.Runtime.InteropServices.In, System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)] bool fClearDirty);
        // vtable slot 7
        void GetSizeMax(out long pcbSize);
        // vtable slot 8
        void InitNew();
    };
}

