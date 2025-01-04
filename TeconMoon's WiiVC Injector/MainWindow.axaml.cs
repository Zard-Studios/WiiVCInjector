using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TeconMoon_s_WiiVC_Injector.Services;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Linq;

namespace TeconMoon_s_WiiVC_Injector
{
    public partial class MainWindow : Window
    {
        private readonly InjectionService injectionService;
        private readonly ToolManager toolManager;
        private string? romPath;
        private string? outputPath;

        public MainWindow()
        {
            InitializeComponent();
            toolManager = new ToolManager();
            injectionService = new InjectionService(toolManager);
            
            var selectRomButton = this.FindControl<Button>("SelectRomButton");
            var selectOutputButton = this.FindControl<Button>("SelectOutputButton");
            var startButton = this.FindControl<Button>("StartButton");

            if (selectRomButton != null) selectRomButton.Click += SelectRomButton_Click;
            if (selectOutputButton != null) selectOutputButton.Click += SelectOutputButton_Click;
            if (startButton != null) startButton.Click += StartButton_Click;
        }

        private async Task<string?> SelectFile(string title, string[] fileTypes)
        {
            var options = new FilePickerOpenOptions
            {
                Title = title,
                FileTypeFilter = fileTypes.Select(ext => new FilePickerFileType(ext) 
                { 
                    Patterns = new[] { ext } 
                }).ToArray()
            };

            var result = await StorageProvider.OpenFilePickerAsync(options);
            return result.Count > 0 ? result[0].Path.LocalPath : null;
        }

        private async void SelectRomButton_Click(object? sender, RoutedEventArgs e)
        {
            romPath = await SelectFile("Select ROM file", new[] { "*.iso", "*.wbfs", "*.gcm", "*.dol" });
            if (romPath != null)
            {
                var romPathTextBox = this.FindControl<TextBox>("RomPathTextBox");
                if (romPathTextBox != null)
                {
                    romPathTextBox.Text = romPath;
                }
            }
        }

        private async void SelectOutputButton_Click(object? sender, RoutedEventArgs e)
        {
            var result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Select Output Folder"
            });

            if (result.Count > 0)
            {
                outputPath = result[0].Path.LocalPath;
                var outputPathTextBox = this.FindControl<TextBox>("OutputPathTextBox");
                if (outputPathTextBox != null)
                {
                    outputPathTextBox.Text = outputPath;
                }
            }
        }

        private async void StartButton_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(romPath) || string.IsNullOrEmpty(outputPath))
            {
                await ShowError("Please select both ROM and output folder.");
                return;
            }

            try
            {
                var options = new InjectionOptions
                {
                    RomPath = romPath,
                    OutputPath = outputPath,
                    GameName = this.FindControl<TextBox>("GameNameTextBox")?.Text ?? "Default Game Name",
                    TitleId = this.FindControl<TextBox>("TitleIdTextBox")?.Text ?? string.Empty,
                    ForceWidescreen = this.FindControl<CheckBox>("ForceWidescreenCheckBox")?.IsChecked ?? false,
                    ForcePal = this.FindControl<CheckBox>("ForcePalCheckBox")?.IsChecked ?? false,
                    NoGamePad = this.FindControl<CheckBox>("NoGamePadCheckBox")?.IsChecked ?? false
                };

                var progress = new Progress<(int progress, string status)>(report =>
                {
                    var progressBar = this.FindControl<ProgressBar>("ProgressBar");
                    var statusText = this.FindControl<TextBlock>("StatusText");
                    
                    if (progressBar != null) progressBar.Value = report.progress;
                    if (statusText != null) statusText.Text = report.status;
                });

                await injectionService.InjectGame(options, progress);
                
                await MessageBoxManager.GetMessageBoxStandard(
                    "Success",
                    "Game injection completed successfully!",
                    ButtonEnum.Ok).ShowAsync();
            }
            catch (Exception ex)
            {
                await ShowError($"Error during injection: {ex.Message}");
            }
        }

        private async Task ShowError(string message)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                "Error",
                message,
                ButtonEnum.Ok,
                MsBox.Avalonia.Enums.Icon.Error).ShowAsync();
        }
    }
} 