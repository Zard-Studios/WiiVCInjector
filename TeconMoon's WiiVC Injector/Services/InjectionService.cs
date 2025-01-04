using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace TeconMoon_s_WiiVC_Injector.Services
{
    public class InjectionService
    {
        private readonly ToolManager toolManager;
        private string tempPath = string.Empty;
        private string tempSourcePath = string.Empty;
        private string tempBuildPath = string.Empty;

        public InjectionService(ToolManager toolManager)
        {
            this.toolManager = toolManager;
            InitializePaths();
        }

        private void InitializePaths()
        {
            tempPath = GetTempPath();
            tempSourcePath = Path.Combine(tempPath, "SOURCETEMP");
            tempBuildPath = Path.Combine(tempPath, "BUILDDIR");
            
            Directory.CreateDirectory(tempPath);
            Directory.CreateDirectory(tempSourcePath);
            Directory.CreateDirectory(tempBuildPath);
        }

        public async Task<bool> InjectGame(InjectionOptions options, IProgress<(int progress, string status)> progress)
        {
            try
            {
                // Verifica requisiti
                await CheckSystemRequirements();
                
                // Prepara l'ambiente
                progress.Report((10, "Preparing environment..."));
                PrepareEnvironment();

                // Processa il ROM
                progress.Report((30, "Processing ROM file..."));
                await ProcessRom(options.RomPath);

                // Converti in formato NFS
                progress.Report((50, "Converting to NFS format..."));
                await ConvertToNfs(options);

                // Cripta i contenuti
                progress.Report((70, "Encrypting contents..."));
                await EncryptContents(options);

                // Pulisci
                progress.Report((90, "Cleaning up..."));
                Cleanup();

                progress.Report((100, "Conversion complete!"));
                return true;
            }
            catch (Exception ex)
            {
                progress.Report((0, $"Error: {ex.Message}"));
                return false;
            }
        }

        private string GetTempPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Path.Combine(Path.GetTempPath(), "WiiVCInjector");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    "Library/Caches/WiiVCInjector");
            else
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                    ".cache/WiiVCInjector");
        }

        private async Task CheckSystemRequirements()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var wineResult = await ExecuteCommandAsync("wine", "--version");
                if (!wineResult)
                    throw new PlatformNotSupportedException("Wine is required but not found");
            }

            if (!toolManager.IsToolAvailable("wit"))
                throw new FileNotFoundException("Required tools are missing");
        }

        private async Task<bool> ExecuteCommandAsync(string command, string arguments)
        {
            try
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        private async Task ProcessRom(string romPath)
        {
            // Copia il ROM nella directory temporanea
            var tempRomPath = Path.Combine(tempSourcePath, "game.iso");
            File.Copy(romPath, tempRomPath, true);

            // Estrai TMD e ticket
            var witPath = toolManager.GetToolPath("wit");
            var result = await ExecuteCommandAsync(witPath, 
                $"extract \"{tempRomPath}\" --psel data --psel -update --files +tmd.bin --files +ticket.bin --dest \"{Path.Combine(tempSourcePath, "TIKTEMP")}\" -vv1");
            
            if (!result)
                throw new Exception("Failed to extract TMD and ticket");

            // Copia i file necessari
            var tikTempPath = Path.Combine(tempSourcePath, "TIKTEMP");
            File.Copy(
                Path.Combine(tikTempPath, "tmd.bin"), 
                Path.Combine(tempBuildPath, "code", "rvlt.tmd"), 
                true);
            File.Copy(
                Path.Combine(tikTempPath, "ticket.bin"), 
                Path.Combine(tempBuildPath, "code", "rvlt.tik"), 
                true);
            
            Directory.Delete(tikTempPath, true);
        }

        private async Task ConvertToNfs(InjectionOptions options)
        {
            Directory.SetCurrentDirectory(Path.Combine(tempBuildPath, "content"));
            
            var nfs2iso2nfsPath = toolManager.GetToolPath("nfs2iso2nfs");
            var lrpatchflag = options.ForceWidescreen ? " -lrpatch" : "";
            var nfspatchflag = options.ForcePal ? " -pal" : "";
            
            string arguments = "";
            switch (GetConsoleType(options))
            {
                case "wii":
                    arguments = $"-enc{nfspatchflag}{lrpatchflag} -iso \"{Path.Combine(tempSourcePath, "game.iso")}\"";
                    break;
                case "gcn":
                    arguments = $"-enc -homebrew -passthrough -iso \"{Path.Combine(tempSourcePath, "game.iso")}\"";
                    break;
                default:
                    throw new ArgumentException("Invalid console type");
            }

            var result = await ExecuteCommandAsync(nfs2iso2nfsPath, arguments);
            if (!result)
                throw new Exception("Failed to convert to NFS format");
        }

        private async Task EncryptContents(InjectionOptions options)
        {
            Directory.SetCurrentDirectory(tempPath);
            
            var nusPackerPath = toolManager.GetToolPath("NUSPacker");
            var sanitizedGameName = SanitizeFilename(options.GameName);
            var outputPath = Path.Combine(
                options.OutputPath, 
                $"{sanitizedGameName} WUP-N-{options.TitleId}");
            
            var result = await ExecuteCommandAsync(
                nusPackerPath, 
                $"-in BUILDDIR -out \"{outputPath}\" -encryptKeyWith common.key");
            
            if (!result)
                throw new Exception("Failed to encrypt contents");
        }

        private void PrepareEnvironment()
        {
            // Pulisci le directory temporanee
            if (Directory.Exists(tempSourcePath))
                Directory.Delete(tempSourcePath, true);
            if (Directory.Exists(tempBuildPath))
                Directory.Delete(tempBuildPath, true);
            
            // Ricrea le directory
            Directory.CreateDirectory(tempSourcePath);
            Directory.CreateDirectory(tempBuildPath);
            Directory.CreateDirectory(Path.Combine(tempBuildPath, "code"));
            Directory.CreateDirectory(Path.Combine(tempBuildPath, "content"));
        }

        private void Cleanup()
        {
            try
            {
                if (Directory.Exists(tempSourcePath))
                    Directory.Delete(tempSourcePath, true);
                if (Directory.Exists(Path.Combine(tempPath, "output")))
                    Directory.Delete(Path.Combine(tempPath, "output"), true);
                if (Directory.Exists(Path.Combine(tempPath, "tmp")))
                    Directory.Delete(Path.Combine(tempPath, "tmp"), true);
            }
            catch (Exception ex)
            {
                // Log error but don't throw
                Console.WriteLine($"Cleanup error: {ex.Message}");
            }
        }

        private string GetConsoleType(InjectionOptions options)
        {
            // Implementa la logica per determinare il tipo di console
            // basandoti sui dati del ROM e sulle opzioni
            return "wii"; // o "gcn" per GameCube
        }

        private string SanitizeFilename(string filename)
        {
            var invalids = Path.GetInvalidFileNameChars();
            return string.Join("_", filename.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }

        // Altri metodi di supporto...
    }

    public class InjectionOptions
    {
        public string RomPath { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public string TitleId { get; set; } = string.Empty;
        public bool ForceWidescreen { get; set; }
        public bool ForcePal { get; set; }
        public bool NoGamePad { get; set; }
    }
} 