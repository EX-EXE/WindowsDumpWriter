[![NuGet version](https://badge.fury.io/nu/WindowsDumpWriter.svg)](https://badge.fury.io/nu/WindowsDumpWriter)
# WindowsDumpWriter
Library function that output dump on Windows.

# Library Usage
PM> Install-Package [WindowsDumpWriter](https://www.nuget.org/packages/WindowsDumpWriter/)
```
var result = WindowsDumpWriter.Write(
    outputFilePath,
    dumpType,
    processId,
    threadId,
    address);
```

# App Usage
## CommandLine Example
```
WindowsDumpWriter.exe --Output "./MiniDump.dmp" --DumpType 6182 --ProcessId 39520
```
```
WindowsDumpWriter.exe --Output "./MiniDump.dmp" --DumpType 6182 --ProcessId 39520 --ThreadId 46800 --ExceptionAddress 0x000000FD0E4FEBE0
```
| Param | Description |
|---|---|
| --Output | Output dump file path. |
| --DumpType | DumpType |
| --ProcessId | Dump process id. |
| --ThreadId | ThreadId in exception info. |
| --ExceptionAddress | ExceptionAddress in exception info. |

## VC++ Example
```cpp
#include <windows.h>
#include <dbghelp.h>
#include <thread>

int GenerateDump(EXCEPTION_POINTERS* exceptionPtr)
{
	const DWORD processId = GetCurrentProcessId();
	const DWORD threadId = GetCurrentThreadId();

	// Create ProcessInfo
	STARTUPINFO startupInfo;
	PROCESS_INFORMATION processInfo;
	ZeroMemory(&startupInfo, sizeof(startupInfo));
	ZeroMemory(&processInfo, sizeof(processInfo));
	startupInfo.cb = sizeof(startupInfo);

	// Cmd
	static constexpr int bufferSize = 4096;
	WCHAR cmd[bufferSize];
	const int dumpType = MiniDumpWithFullMemory | MiniDumpWithHandleData | MiniDumpWithUnloadedModules | MiniDumpWithFullMemoryInfo | MiniDumpWithThreadInfo;
	swprintf(cmd, 
		bufferSize,
		L"\"%s\" --Output \"%s\" --DumpType %d --ProcessId %d --ThreadId %d --ExceptionAddress 0x%p",
		L"./WindowsDumpWriter.exe",
		L"./MiniDump.dmp",
		dumpType,
		processId,
		threadId,
		exceptionPtr);

	// Process
	if (CreateProcess(
		nullptr,
		cmd,
		nullptr,
		nullptr,
		false,
		0,
		nullptr,
		nullptr,
		&startupInfo,
		&processInfo))
	{
		// Wait Process
		WaitForSingleObject(processInfo.hProcess, INFINITE);

		// Exit Code
		unsigned long exitCode;
		GetExitCodeProcess(processInfo.hProcess, &exitCode);

		// Close Handle
		CloseHandle(processInfo.hProcess);
		CloseHandle(processInfo.hThread);
	}
	return EXCEPTION_EXECUTE_HANDLER;
}

int main()
{
	std::thread thread([&]
		{
			__try
			{
				// Exception Code
				int* ptr = nullptr;
				*ptr = 0;
			}
			__except (GenerateDump(GetExceptionInformation()))
			{
			}
		});
	thread.join();
}
```
