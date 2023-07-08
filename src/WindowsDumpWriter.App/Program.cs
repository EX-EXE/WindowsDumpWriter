using System;

namespace WindowsDumpWriter.App;

internal class Program
{
    static void Main(string[] args)
    {
        var cmdDict = GetCommandLine();
        var output = cmdDict["Output"];
        var dumpType = uint.Parse(cmdDict["DumpType"]);
        var processId = int.Parse(cmdDict["ProcessId"]);
        var threadId = (int)-1;
        var address = (long)-1;

        if (cmdDict.ContainsKey("ThreadId"))
        {
            int.TryParse(cmdDict["ThreadId"], out threadId);
        }
        if (cmdDict.ContainsKey("ExceptionAddress"))
        {
            address = HexToLong(cmdDict["ExceptionAddress"]);
        }

        var result = WindowsDumpWriter.Write(
            output,
            dumpType,
            processId,
            threadId,
            address);
        Environment.ExitCode = result ? 0 : 1;
    }

    static long HexToLong(ReadOnlySpan<char> hex)
    {
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            hex = hex.Slice(2);
        }
        return Convert.ToInt64(hex.ToString(), 16);
    }

    private static readonly string Prefix = @"--";
    private static readonly string[] CommandKeys = { "Output", "DumpType", "ProcessId", "ThreadId", "ExceptionAddress" };
    private static IOrderedEnumerable<string> SortCommandKeys => CommandKeys.OrderByDescending(x => x.Length);
    public static Dictionary<string, string> GetCommandLine()
    {
        (int keyIndex, int valueIndex) FindIndex(ReadOnlySpan<char> cmd)
        {
            var searchStartIndex = 0;
            while (true)
            {
                // Search Prefix
                var index = cmd.IndexOf(Prefix, searchStartIndex, StringComparison.Ordinal);
                if (0 <= index)
                {
                    // Search Keys
                    foreach (var key in SortCommandKeys)
                    {
                        if (cmd.StartsWith(key.AsSpan(), index + Prefix.Length, StringComparison.OrdinalIgnoreCase))
                        {
                            return (index, index + Prefix.Length + key.Length);
                        }
                    }
                    // Next
                    searchStartIndex = index + 1;
                }
                else
                {
                    return (-1, -1);
                }
            }
        }

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var cmd = Environment.CommandLine.AsSpan();
        while (true)
        {
            (int keyIndex, int valueIndex) = FindIndex(cmd);
            if (0 <= keyIndex)
            {
                var key = cmd.Slice(keyIndex + Prefix.Length, valueIndex - keyIndex - Prefix.Length).ToString();
                (int keyNextIndex, _) = FindIndex(cmd.Slice(valueIndex));
                if (0 <= keyNextIndex)
                {
                    result.TryAdd(key, cmd.Slice(valueIndex, keyNextIndex).Trim().ToString());
                    cmd = cmd.Slice(valueIndex + keyNextIndex);
                    continue;
                }
                else
                {
                    result.Add(key, cmd.Slice(valueIndex).Trim().ToString());
                    // End
                    break;
                }
            }
            else
            {
                // End
                break;
            }
        }
        return result;
    }
}

public static class ReadOnlySpanCharExtensions
{
    public static int IndexOf(
        this ReadOnlySpan<char> span,
        ReadOnlySpan<char> value,
        int start,
        StringComparison comparisonType)
    {
        var spanSlice = span.Slice(start);
        return spanSlice.IndexOf(value, comparisonType);
    }

    public static bool StartsWith(
        this ReadOnlySpan<char> span,
        ReadOnlySpan<char> value,
        int start,
        StringComparison comparisonType)
    {
        var spanSlice = span.Slice(start);
        return spanSlice.StartsWith(value, comparisonType);
    }
}