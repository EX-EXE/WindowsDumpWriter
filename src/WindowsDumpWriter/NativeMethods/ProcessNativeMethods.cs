
using System.Runtime.InteropServices;

namespace WindowsDumpWriter;

internal partial class NativeMethods
{
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(
        ProcessAccessFlags dwDesiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
        int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenThread(
        ThreadAccessFlags dwDesiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
        uint dwThreadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr CreateToolhelp32Snapshot(
        CreateToolhelp32SnapshotFlags dwFlags,
        uint th32ProcessID);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool Thread32First(
        IntPtr hSnapshot,
        ref THREADENTRY32 lpte);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool Thread32Next(
        IntPtr hSnapshot,
        ref THREADENTRY32 lpte);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern int GetThreadId(IntPtr hThread);

    [Flags]
    public enum ProcessAccessFlags : uint
    {
        PROCESS_TERMINATE = 0x0001,
        PROCESS_CREATE_THREAD = 0x0002,
        PROCESS_SET_SESSIONID = 0x0004,
        PROCESS_VM_OPERATION = 0x0008,
        PROCESS_VM_READ = 0x0010,
        PROCESS_VM_WRITE = 0x0020,
        PROCESS_DUP_HANDLE = 0x0040,
        PROCESS_CREATE_PROCESS = 0x0080,
        PROCESS_SET_QUOTA = 0x0100,
        PROCESS_SET_INFORMATION = 0x0200,
        PROCESS_QUERY_INFORMATION = 0x0400,
        PROCESS_SUSPEND_RESUME = 0x0800,
        PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
        PROCESS_ALL_ACCESS = 0x1F0FFF
    }

    [Flags]
    public enum ThreadAccessFlags : uint
    {
        SYNCHRONIZE = 0x00100000,
        STANDARD_RIGHTS_REQUIRED = 0x000F0000,
        THREAD_ALL_ACCESS = SYNCHRONIZE | STANDARD_RIGHTS_REQUIRED | 0xFFFF,
        THREAD_ALL_ACCESS_WINXP = SYNCHRONIZE | STANDARD_RIGHTS_REQUIRED | 0x3FF,

        THREAD_TERMINATE = 0x0001,
        THREAD_SUSPEND_RESUME = 0x0002,
        THREAD_GET_CONTEXT = 0x0008,
        THREAD_SET_CONTEXT = 0x0010,
        THREAD_QUERY_INFORMATION = 0x0040,
        THREAD_SET_INFORMATION = 0x0020,
        THREAD_SET_THREAD_TOKEN = 0x0080,
        THREAD_IMPERSONATE = 0x0100,
        THREAD_DIRECT_IMPERSONATION = 0x0200,
        THREAD_SET_LIMITED_INFORMATION = 0x0400,
        THREAD_QUERY_LIMITED_INFORMATION = 0x0800,
        THREAD_RESUME = 0x1000,
    }

    [Flags]
    public enum CreateToolhelp32SnapshotFlags : uint
    {
        TH32CS_SNAPHEAPLIST = 0x00000001,
        TH32CS_SNAPPROCESS = 0x00000002,
        TH32CS_SNAPTHREAD = 0x00000004,
        TH32CS_SNAPMODULE = 0x00000008,
        TH32CS_SNAPMODULE32 = 0x00000010,
        TH32CS_SNAPALL = (TH32CS_SNAPHEAPLIST | TH32CS_SNAPPROCESS | TH32CS_SNAPTHREAD | TH32CS_SNAPMODULE),
        TH32CS_INHERIT = 0x80000000,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct THREADENTRY32
    {
        public uint dwSize;
        public uint cntUsage;
        public uint th32ThreadID;
        public uint th32OwnerProcessID;
        public int tpBasePri;
        public int tpDeltaPri;
        public uint dwFlags;
    }

}
