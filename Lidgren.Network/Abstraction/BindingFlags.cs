namespace System.Reflection
{
    using System;

    [Flags]
    public enum BindingFlags
    {
        Default = 0x00,
        IgnoreCase = 0x01,
        DeclaredOnly = 0x02,
        Instance = 0x04,
        Static = 0x08,
        Public = 0x10,
        NonPublic = 0x20,
        FlattenHierarchy = 0x40,
        InvokeMethod = 0x0100,
        CreateInstance = 0x0200,
        GetField = 0x0400,
        SetField = 0x0800,
        GetProperty = 0x1000,
        SetProperty = 0x2000,
        PutDispProperty = 0x4000,
        PutRefDispProperty = 0x8000,
        ExactBinding = 0x010000,
        SuppressChangeType = 0x020000,
        OptionalParamBinding = 0x040000,
        IgnoreReturn = 0x01000000,
    }
}