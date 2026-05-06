using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wintoolkit_
{
    public partial class MainWindow : Window
    {
        private readonly string CustomScriptsPath;

        public MainWindow()
        {
            InitializeComponent();

            CustomScriptsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "custom");
            LoadCustomScripts();
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            PanelSistema.Visibility = Visibility.Collapsed;
            PanelSoftware.Visibility = Visibility.Collapsed;
            PanelPortable.Visibility = Visibility.Collapsed;
            PanelStato.Visibility = Visibility.Collapsed;
            PanelImpostazioni.Visibility = Visibility.Collapsed;

            if (sender is Button btn && btn.Tag != null)
            {
                string target = btn.Tag.ToString() ?? "";
                if (target == "PanelSistema") PanelSistema.Visibility = Visibility.Visible;
                else if (target == "PanelSoftware") PanelSoftware.Visibility = Visibility.Visible;
                else if (target == "PanelPortable") PanelPortable.Visibility = Visibility.Visible;
                else if (target == "PanelStato") PanelStato.Visibility = Visibility.Visible;
                else if (target == "PanelImpostazioni") PanelImpostazioni.Visibility = Visibility.Visible;
            }
        }

        private void LoadCustomScripts()
        {
            CustomScriptsPanel.Children.Clear();
            try
            {
                if (!Directory.Exists(CustomScriptsPath)) Directory.CreateDirectory(CustomScriptsPath);

                string[] files = Directory.GetFiles(CustomScriptsPath);
                foreach (string file in files)
                {
                    if (file.EndsWith(".bat", StringComparison.OrdinalIgnoreCase) || file.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                    {
                        Button btn = new Button
                        {
                            Content = Path.GetFileNameWithoutExtension(file),
                            Tag = file,
                            Style = (Style)FindResource("ActionBtn"),
                            Background = new SolidColorBrush(Color.FromRgb(85, 85, 85))
                        };
                        btn.Click += (s, ev) => RunScriptInWindow(file);
                        CustomScriptsPanel.Children.Add(btn);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore durante il caricamento degli script: " + ex.Message);
            }
        }

        private void BtnScript_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string action)
            {
                string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", action);
                if (File.Exists(scriptPath)) RunScriptInWindow(scriptPath);
                else MessageBox.Show("Impossibile trovare il file: " + scriptPath);
            }
        }

        private void RunScriptInWindow(string path)
        {
            try
            {
                bool isPs = path.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase);
                Process.Start(new ProcessStartInfo
                {
                    FileName = isPs ? "powershell.exe" : "cmd.exe",
                    Arguments = isPs ? $"-NoProfile -ExecutionPolicy Bypass -File \"{path}\"" : $"/c \"{path}\"",
                    UseShellExecute = true,
                    Verb = "runas"
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Esecuzione annullata: " + ex.Message);
            }
        }

        private void OpenCustomScripts_Click(object s, RoutedEventArgs e) => Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = CustomScriptsPath });
        private void ReloadCustomScripts_Click(object s, RoutedEventArgs e) => LoadCustomScripts();

        private async void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string action)
            {
                if (action == "restore_point")
                {
                    btn.IsEnabled = false;
                    ProgBar.Visibility = Visibility.Visible;
                    ProgBar.IsIndeterminate = true;
                    TxtStatus.Text = "Creazione punto di ripristino in corso (in background)...";

                    LogContainer.Visibility = Visibility.Visible;
                    TxtLog.AppendText($"\n\n--- [{DateTime.Now.ToShortTimeString()}] Creazione Punto di Ripristino avviata ---\n");

                    await Task.Run(() =>
                    {
                        try
                        {
                            // Avviamo PowerShell nascosto ma con permessi admin senza intercettare l'output (che causava crash sui sistemi blindati)
                            ProcessStartInfo psi = new ProcessStartInfo
                            {
                                FileName = "powershell.exe",
                                Arguments = "-NoProfile -ExecutionPolicy Bypass -Command \"Enable-ComputerRestore -Drive 'C:\'; Checkpoint-Computer -Description 'WinToolkit Manual Backup' -RestorePointType 'MODIFY_SETTINGS'\"",
                                UseShellExecute = true,
                                WindowStyle = ProcessWindowStyle.Hidden,
                                Verb = "runas"
                            };

                            using (Process process = Process.Start(psi))
                            {
                                process?.WaitForExit();
                            }
                            Dispatcher.Invoke(() => TxtLog.AppendText("Operazione conclusa da Windows.\n"));
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(() => TxtLog.AppendText($"[ERRORE] {ex.Message}\n"));
                        }
                    });

                    TxtStatus.Text = "Comando punto di ripristino inviato al sistema.";
                    ProgBar.Visibility = Visibility.Hidden;
                    btn.IsEnabled = true;
                }
                else if (action == "system_image")
                {
                    TxtStatus.Text = "Apertura strumento Backup e Ripristino di Windows...";
                    try
                    {
                        Process.Start(new ProcessStartInfo { FileName = "sdclt.exe", UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Impossibile avviare lo strumento: " + ex.Message);
                    }
                }
            }
        }

        private async void BtnSilentInstall_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string action)
            {
                string appName = btn.Content?.ToString() ?? "Software";
                btn.IsEnabled = false;
                ProgBar.Visibility = Visibility.Visible;
                ProgBar.IsIndeterminate = true;
                TxtStatus.Text = $"Installazione di {appName} in corso...";

                TxtLog.AppendText($"\n\n--- [{DateTime.Now.ToShortTimeString()}] Avvio operazione: {appName} ---\n");
                LogContainer.Visibility = Visibility.Visible;

                await Task.Run(() =>
                {
                    string file = "powershell.exe";
                    string args = "";

                    if (action == "scoop_init")
                    {
                        args = "-NoProfile -ExecutionPolicy Bypass -Command \"[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; iex (new-object net.webclient).downloadstring('https://get.scoop.sh'); scoop bucket add extras\"";
                    }
                    else
                    {
                        string[] parts = action.Split('|');
                        if (parts.Length < 2) return;

                        string manager = parts[0];
                        string id = parts[1];

                        if (manager == "winget") args = $"-NoProfile -Command \"winget install --id {id} -e --silent --accept-package-agreements --accept-source-agreements\"";
                        else if (manager == "choco") args = $"-NoProfile -Command \"choco install {id} -y\"";
                        else if (manager == "scoop") args = $"-NoProfile -Command \"scoop install {id}\"";
                    }

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = file,
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process process = new Process { StartInfo = psi })
                    {
                        process.OutputDataReceived += (s, ev) => Dispatcher.Invoke(() => {
                            if (!string.IsNullOrEmpty(ev.Data)) { TxtLog.AppendText(ev.Data + "\n"); TxtLog.ScrollToEnd(); }
                        });
                        process.ErrorDataReceived += (s, ev) => Dispatcher.Invoke(() => {
                            if (!string.IsNullOrEmpty(ev.Data)) { TxtLog.AppendText("[ERRORE] " + ev.Data + "\n"); TxtLog.ScrollToEnd(); }
                        });

                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                    }
                });

                if (action.Contains("scoop"))
                {
                    string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "scoop", "apps");
                    TxtLog.AppendText($"\n[INFO] I software portable Scoop si trovano in: {path}\n");
                }

                TxtStatus.Text = $"{appName} installato correttamente!";
                ProgBar.Visibility = Visibility.Hidden;
                btn.IsEnabled = true;
            }
        }

        private async void BtnSystem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string action)
            {
                if (action == "clear") { TxtOutput.Text = "Pronto. Clicca un pulsante per interrogare il sistema..."; return; }

                btn.IsEnabled = false;
                TxtOutput.Text = "Analisi in corso... attendere prego.\n(La raccolta dei dati può impiegare alcuni secondi).";

                string fileName = "cmd.exe";
                string args = "";

                if (action == "sysinfo") args = "/c systeminfo";
                else if (action == "ipconfig") args = "/c ipconfig /all";
                else if (action == "hotfix")
                {
                    fileName = "powershell.exe";
                    args = "-NoProfile -Command \"Get-HotFix | Sort-Object InstalledOn -Descending | Format-Table -AutoSize | Out-String -Width 200\"";
                }

                try
                {
                    string result = await Task.Run(async () =>
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = fileName,
                            Arguments = args,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        };

                        using var process = new Process { StartInfo = psi };
                        process.Start();
                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();
                        process.WaitForExit();

                        return string.IsNullOrWhiteSpace(output) ? error : output;
                    });
                    TxtOutput.Text = result;
                }
                catch (Exception ex)
                {
                    TxtOutput.Text = "Si è verificato un errore critico: " + ex.Message;
                }
                finally
                {
                    btn.IsEnabled = true;
                }
            }
        }

        private void Status_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            LogContainer.Visibility = LogContainer.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void CloseLog_Click(object sender, RoutedEventArgs e)
        {
            LogContainer.Visibility = Visibility.Collapsed;
        }

        private void BtnTheme_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                bool dark = btn.Tag.ToString() == "Dark";
                Resources["BgColor"] = new SolidColorBrush(dark ? Color.FromRgb(30, 30, 30) : Color.FromRgb(240, 240, 240));
                Resources["SidebarColor"] = new SolidColorBrush(dark ? Color.FromRgb(37, 37, 38) : Color.FromRgb(220, 220, 220));
                Resources["CardColor"] = new SolidColorBrush(dark ? Color.FromRgb(45, 45, 48) : Color.FromRgb(255, 255, 255));
                Resources["TextColor"] = new SolidColorBrush(dark ? Colors.White : Colors.Black);
            }
        }

        private void OpenWebsite_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://angolodiwindows.com",
                UseShellExecute = true
            });
        }
    }
}