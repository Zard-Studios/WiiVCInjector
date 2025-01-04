using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

public static class FileSystemHelper
{
    public static void SetExecutablePermissions(string filePath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "chmod",
                        Arguments = $"+x \"{filePath}\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                throw new PlatformNotSupportedException($"Failed to set executable permissions: {ex.Message}");
            }
        }
    }

    public static string NormalizePath(string path)
    {
        return Path.GetFullPath(path)
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
    }

    public static void CopyDirectory(string sourceDir, string destinationDir)
    {
        // Crea la directory di destinazione se non esiste
        Directory.CreateDirectory(destinationDir);

        // Copia tutti i file
        foreach (string file in Directory.GetFiles(sourceDir))
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(destinationDir, fileName);
            File.Copy(file, destFile, true);
            
            // Imposta i permessi su Unix
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SetExecutablePermissions(destFile);
            }
        }

        // Copia ricorsivamente tutte le sottodirectory
        foreach (string dir in Directory.GetDirectories(sourceDir))
        {
            string dirName = Path.GetFileName(dir);
            string destDir = Path.Combine(destinationDir, dirName);
            CopyDirectory(dir, destDir);
        }
    }

    public static void DeleteDirectoryRecursive(string path)
    {
        if (Directory.Exists(path))
        {
            // Rimuovi l'attributo readonly da tutti i file
            foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
            }
            Directory.Delete(path, true);
        }
    }
} 