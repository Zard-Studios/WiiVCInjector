using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class NativeTools
{
    private static readonly Dictionary<string, NativeTool> tools = new()
    {
        { "wit", new NativeTool 
            {
                Windows = "WIT/wit.exe",
                MacIntel = "wit/wit-x64-mac",
                MacArm = "wit/wit-arm64-mac",
                Linux = "wit/wit-linux"
            }
        },
        { "wiivc", new NativeTool
            {
                Windows = "EXE/wiivc.exe",
                MacIntel = "bin/wiivc-x64-mac",
                MacArm = "bin/wiivc-arm64-mac",
                Linux = "bin/wiivc-linux"
            }
        },
        { "GetExtTypePatcher", new NativeTool 
            {
                Windows = "EXE/GetExtTypePatcher.exe",
                MacIntel = "bin/get-ext-type-x64-mac",
                MacArm = "bin/get-ext-type-arm64-mac",
                Linux = "bin/get-ext-type-linux"
            }
        },
        { "wiivc_chan_booter", new NativeTool 
            {
                Windows = "DOL/FIX94_wiivc_chan_booter.dol",
                MacIntel = "dol/FIX94_wiivc_chan_booter.dol",
                MacArm = "dol/FIX94_wiivc_chan_booter.dol",
                Linux = "dol/FIX94_wiivc_chan_booter.dol"
            }
        }
        // Aggiungere altri tool...
    };

    public static string GetToolPath(string toolName)
    {
        if (!tools.TryGetValue(toolName, out var tool))
        {
            throw new KeyNotFoundException($"Tool '{toolName}' not found");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return tool.Windows;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 
                ? tool.MacArm 
                : tool.MacIntel;
        else
            return tool.Linux;
    }
}

public class NativeTool
{
    public string Windows { get; set; } = string.Empty;
    public string MacIntel { get; set; } = string.Empty;
    public string MacArm { get; set; } = string.Empty;
    public string Linux { get; set; } = string.Empty;
} 