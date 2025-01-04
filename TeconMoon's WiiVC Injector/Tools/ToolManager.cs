using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public class ToolManager
{
    private static readonly Dictionary<string, NativeTool> tools = new()
    {
        { "wit", new NativeTool 
            {
                Windows = "WIT/wit.exe",
                MacIntel = "wit/wit-mac",
                MacArm = "wit/wit-arm64-mac",
                Linux = "wit/wit-linux"
            }
        },
        { "nfs2iso2nfs", new NativeTool 
            {
                Windows = "EXE/nfs2iso2nfs.exe",
                MacIntel = "bin/nfs2iso2nfs-mac",
                MacArm = "bin/nfs2iso2nfs-arm64-mac",
                Linux = "bin/nfs2iso2nfs-linux"
            }
        },
        { "NUSPacker", new NativeTool 
            {
                Windows = "JAR/NUSPacker.exe",
                MacIntel = "jar/NUSPacker-mac",
                MacArm = "jar/NUSPacker-arm64-mac",
                Linux = "jar/NUSPacker-linux"
            }
        }
    };

    public string GetToolPath(string toolName)
    {
        if (!tools.TryGetValue(toolName, out var tool))
            throw new FileNotFoundException($"Tool {toolName} not found");

        string path;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            path = tool.Windows;
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            path = RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? tool.MacArm : tool.MacIntel;
        else
            path = tool.Linux;

        return Path.Combine(GetToolsBasePath(), path);
    }

    public bool IsToolAvailable(string toolName)
    {
        return tools.ContainsKey(toolName) && 
               File.Exists(GetToolPath(toolName));
    }

    private string GetToolsBasePath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Path.Combine(Path.GetTempPath(), "WiiVCInjector", "tools");
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Library/Application Support/WiiVCInjector/tools/");
        else
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                ".local/share/WiiVCInjector/tools/");
    }
}

public class Tool
{
    public string WindowsPath { get; set; } = string.Empty;
    public string MacPath { get; set; } = string.Empty;
    public string LinuxPath { get; set; } = string.Empty;
    public bool RequiresExecution { get; set; }
} 