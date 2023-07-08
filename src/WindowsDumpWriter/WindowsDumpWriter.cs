using System.Diagnostics;
using System.Runtime.InteropServices;
using static WindowsDumpWriter.NativeMethods;

namespace WindowsDumpWriter;

public class WindowsDumpWriter
{
    public static bool Write(string outputDumpPath, uint minidumpType, int processId)
    {
        return Write(outputDumpPath, minidumpType, processId, -1, -1);
    }

    public static bool Write(string outputDumpPath, uint minidumpType, int processId, int threadId, long address)
    {
        // Get Process
        var process = Process.GetProcessById(processId);
        if (process == null)
        {
            return false;
        }

        // Get Handle
        var processHandle = IntPtr.Zero;
        var threadHandles = Array.Empty<IntPtr>();
        try
        {
            processHandle = OpenProcess(processId);
            threadHandles = OpenAllThread(processId);

            // Dump
            return WriteDump(outputDumpPath, minidumpType, process, threadId, address);
        }
        finally
        {
            // Close Handles
            if (processHandle != IntPtr.Zero)
            {
                CloseHandle(processHandle);
                processHandle = IntPtr.Zero;
            }
            foreach (var threadHandle in threadHandles)
            {
                CloseHandle(threadHandle);
            }
            threadHandles = Array.Empty<IntPtr>();
        }
    }

    private static bool WriteDump(string outputDumpPath, uint minidumpType, Process process, int threadId, long address)
    {
        // Write
        var dumpFileHandle = NativeMethods.CreateFile(
            outputDumpPath,
            NativeMethods.EFileAccess.GenericWrite,
            NativeMethods.EFileShare.None,
            IntPtr.Zero,
            NativeMethods.ECreationDisposition.CreateAlways,
            NativeMethods.EFileAttributes.Normal,
            IntPtr.Zero);
        if (dumpFileHandle.ToInt32() == -1)
        {
            return false;
        }
        try
        {
            // Dump
            bool success = false;
            if (threadId < 0 || address < 0)
            {
                // No Exception
                success = NativeMethods.MiniDumpWriteDump(
                    process.Handle,
                    (uint)process.Id,
                    dumpFileHandle,
                    minidumpType,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero);
            }
            else
            {
                // Exception
                var exceptionInfo = new NativeMethods.MINIDUMP_EXCEPTION_INFORMATION()
                {
                    ExceptionPointers = new IntPtr(address),
                    ClientPointers = true,
                    ThreadId = (uint)threadId,
                };
                success = NativeMethods.MiniDumpWriteDump(
                    process.Handle,
                    (uint)process.Id,
                    dumpFileHandle,
                    minidumpType,
                    ref exceptionInfo,
                    IntPtr.Zero,
                    IntPtr.Zero);
            }
            return success;
        }
        finally
        {
            CloseHandle(dumpFileHandle);
        }
    }

    private static IntPtr OpenProcess(int processId)
    {
        var processHandle = NativeMethods.OpenProcess(
            NativeMethods.ProcessAccessFlags.PROCESS_QUERY_INFORMATION |
            NativeMethods.ProcessAccessFlags.PROCESS_VM_READ |
            NativeMethods.ProcessAccessFlags.PROCESS_DUP_HANDLE,
            false,
            processId);
        return processHandle;
    }

    private static IntPtr[] OpenAllThread(int processId)
    {
        List<IntPtr> threadHandles = new List<IntPtr>();

        var snapshotHandle = NativeMethods.CreateToolhelp32Snapshot(
            NativeMethods.CreateToolhelp32SnapshotFlags.TH32CS_SNAPTHREAD,
            0);
        if (snapshotHandle != IntPtr.Zero)
        {
            try
            {
                var threadEntry = new NativeMethods.THREADENTRY32();
                threadEntry.dwSize = (uint)Marshal.SizeOf(threadEntry);
                if (NativeMethods.Thread32First(snapshotHandle, ref threadEntry))
                {
                    do
                    {
                        if (threadEntry.th32OwnerProcessID == (uint)processId)
                        {
                            var threadHandle = NativeMethods.OpenThread(
                                NativeMethods.ThreadAccessFlags.THREAD_ALL_ACCESS,
                                false,
                                threadEntry.th32ThreadID);
                            if (threadHandle != IntPtr.Zero)
                            {
                                threadHandles.Add(threadHandle);
                            }
                        }
                    }
                    while (NativeMethods.Thread32Next(snapshotHandle, ref threadEntry));
                }
            }
            finally
            {
                NativeMethods.CloseHandle(snapshotHandle);
            }
        }
        return threadHandles.ToArray();
    }


}
