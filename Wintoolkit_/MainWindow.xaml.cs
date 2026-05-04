using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WinToolkitWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // --- GESTIONE DELLA NAVIGAZIONE DEL MENÙ ---
        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            // Nascondi tutti i pannelli
            PanelSistema.Visibility = Visibility.Collapsed;
            PanelSoftware.Visibility = Visibility.Collapsed;
            PanelPortable.Visibility = Visibility.Collapsed;
            PanelStato.Visibility = Visibility.Collapsed;

            // Mostra il pannello corrispondente al Tag del bottone cliccato
            string targetPanel = (sender as Button)?.Tag.ToString();

            if (targetPanel == "PanelSistema") PanelSistema.Visibility = Visibility.Visible;
            else if (targetPanel == "PanelSoftware") PanelSoftware.Visibility = Visibility.Visible;
            else if (targetPanel == "PanelPortable") PanelPortable.Visibility = Visibility.Visible;
            else if (targetPanel == "PanelStato") PanelStato.Visibility = Visibility.Visible;
        }

        // --- GESTIONE SCRIPT E INSTALLAZIONI ESTERNE ---
        private void BtnScript_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string action)
            {
                try
                {
                    if (action.StartsWith("winget:"))
                    {
                        string appId = action.Replace("winget:", "");
                        RunProcessExternal("cmd.exe", $"/c color 0B & winget install --id {appId} -e --accept-source-agreements & pause");
                    }
                    else if (action.StartsWith("choco:"))
                    {
                        string appId = action.Replace("choco:", "");
                        RunProcessExternal("cmd.exe", $"/c color 0D & choco install {appId} -y & pause");
                    }
                    else if (action == "scoop_init")
                    {
                        string scoopCmd = "-NoProfile -ExecutionPolicy Bypass -Command \"Set-ExecutionPolicy RemoteSigned -Scope CurrentUser; Invoke-RestMethod -Uri https://get.scoop.sh | Invoke-Expression; scoop bucket add extras; pause\"";
                        RunProcessExternal("powershell.exe", scoopCmd);
                    }
                    else if (action.StartsWith("scoop:"))
                    {
                        string appId = action.Replace("scoop:", "");
                        RunProcessExternal("cmd.exe", $"/c color 0A & scoop install {appId} & pause");
                    }
                    else // I tuoi file .bat originari
                    {
                        string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", action);
                        if (!File.Exists(scriptPath))
                        {
                            MessageBox.Show("File non trovato: " + scriptPath);
                            return;
                        }

                        bool isPs = action.EndsWith(".ps1");
                        string args = isPs ? $"-ExecutionPolicy Bypass -File \"{scriptPath}\"" : $"/c \"{scriptPath}\"";
                        RunProcessExternal(isPs ? "powershell.exe" : "cmd.exe", args);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Errore: " + ex.Message);
                }
            }
        }

        private void RunProcessExternal(string fileName, string args)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                UseShellExecute = true,
                Verb = "runas" // Richiede UAC
            });
        }

        // --- GESTIONE OUTPUT INTEGRATO A SCHERMO (STATO SISTEMA) ---
        private async void BtnSystem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string action)
            {
                if (action == "clear")
                {
                    TxtOutput.Text = "Pronto. Clicca un pulsante per interrogare il sistema...";
                    return;
                }

                // Disabilita il bottone temporaneamente e avvisa l'utente
                btn.IsEnabled = false;
                TxtOutput.Text = "Analisi in corso... Attendere prego (il caricamento di System Info può richiedere fino a un minuto).\n\n";

                string fileName = "cmd.exe";
                string args = "";

                if (action == "sysinfo")
                {
                    args = "/c systeminfo";
                }
                else if (action == "ipconfig")
                {
                    args = "/c ipconfig /all";
                }
                else if (action == "hotfix")
                {
                    fileName = "powershell.exe";
                    args = "-NoProfile -Command \"Get-HotFix | Sort-Object InstalledOn -Descending | Format-Table -AutoSize | Out-String -Width 200\"";
                }

                try
                {
                    // Lancia il comando in un Task asincrono per non congelare la grafica
                    string result = await Task.Run(() =>
                    {
                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = fileName,
                            Arguments = args,
                            UseShellExecute = false, // Obbligatorio per reindirizzare l'output
                            RedirectStandardOutput = true,
                            RedirectStandardError = true, // Catturiamo anche gli errori
                            CreateNoWindow = true,
                            StandardOutputEncoding = System.Text.Encoding.GetEncoding(850)
                        };

                        using (Process process = Process.Start(psi))
                        {
                            // Legge sia l'output che eventuali errori di sistema
                            string output = process.StandardOutput.ReadToEnd();
                            string err = process.StandardError.ReadToEnd();
                            process.WaitForExit();

                            return string.IsNullOrWhiteSpace(output) ? err : output;
                        }
                    });

                    TxtOutput.Text = string.IsNullOrWhiteSpace(result) ? "Nessun output ricevuto." : result;
                }
                catch (Exception ex)
                {
                    TxtOutput.Text = $"Si è verificato un errore:\n{ex.Message}";
                }
                finally
                {
                    // Riabilita il bottone una volta finito
                    btn.IsEnabled = true;
                }
            }
        }
    }
}