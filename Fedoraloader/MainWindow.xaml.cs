using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using Fedoraloader.Injection;
using Microsoft.Win32;
using System.Net.Http;
using System.IO.Compression;
using System.Net;

namespace Fedoraloader
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string ACTION_URL = "https://nightly.link/tf2cheater2013/Fedoraware/workflows/msbuild/main/Fedoraware.zip";
        public static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        private readonly string _workDir;
        private readonly string _fileDir;

        public MainWindow()
        {
            InitializeComponent();

            // Check for administrator rights
            if (!IsElevated)
            {
                MessageBox.Show("This program will not work without administrator rights!\nPlease run it as administrator.", "", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Check windows version
            if (Environment.OSVersion.Version.Major < 10)
            {
                MessageBox.Show("This program was designed for Windows 10 and higher and might not work properly on your system.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Prepare working directory
            _workDir = Path.GetTempPath() + "Fedloader";
            _fileDir = _workDir + @"\Data\";
            Console.WriteLine(_workDir);

            BypassCheckbox.IsChecked = Properties.Settings.Default.UseBypass;
            Title = Utils.RandomString(8);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        private void MenuBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadButton.IsEnabled = false;
            new Thread(() =>
            {
                RunLoader();
                UpdateStatus("", false);
                LoadButton.Dispatcher.BeginInvoke((Action)(() =>
                {
                    LoadButton.IsEnabled = true;
                }));
            }).Start();
        }

        private void RunLoader()
        {
            // Cleanup and preperation
            UpdateStatus("Preparing files...");
            try
            {
                if (Directory.Exists(_fileDir))
                {
                    Directory.Delete(_fileDir, true);
                }

                Directory.CreateDirectory(_fileDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create working directory:\n" + ex.Message + "\nIs the loader or Fedoraware already running?", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Thread.Sleep(250);

            // Wait for TF2
            UpdateStatus("Searching for TF2...");
            Process tfProcess = GetGameProcess();
            if (tfProcess == null)
            {
                MessageBox.Show("Team Fortress 2 could not be found!\nMake sure that you open it before loading.", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string dllFile = _fileDir + @"Fware(Release).dll";

            // Download latest build
            try
            {
                string dlPath = _fileDir + @"\" + Utils.RandomString(8) + ".zip";

                Thread.Sleep(250);

                // Download latest build
                UpdateStatus("Downloading...");
                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile(ACTION_URL, dlPath);
                }

                Thread.Sleep(250);

                // Extract build
                UpdateStatus("Extracting files...");
                ZipFile.ExtractToDirectory(dlPath, _fileDir);
                File.Delete(dlPath);

                Thread.Sleep(250);

                dllFile = Directory.GetFiles(_fileDir, "*.dll").First();
            }
            catch (Exception ex)
            {
                if (MessageBox.Show("Failed to download the latest build:\n" + ex.Message, "", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            // Inject the dll if it exists
            if (File.Exists(dllFile))
            {
                UpdateStatus("Injecting...");
                Inject(tfProcess, dllFile);
            }
            else
            {
                MessageBoxResult dlgResult = MessageBox.Show("The loader file could not be found!\nDo you want to manually select a .dll file?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dlgResult != MessageBoxResult.Yes) { return; }

                OpenFileDialog dllDialog = new OpenFileDialog()
                {
                    Multiselect = false,
                    Title = "Select .dll file",
                    Filter = "DLL File (*.dll)|*.dll"
                };

                if (dllDialog.ShowDialog() == true)
                {
                    UpdateStatus("Injecting custom dll...");
                    Inject(tfProcess, dllDialog.FileName);
                }
            }
        }

        private void Inject(Process pProcess, string pFileName)
        {
            Injector tfjector = new Injector();
            InjectionResult injectionResult = tfjector.Inject(pProcess.Handle, pFileName);
            switch (injectionResult)
            {
                case InjectionResult.AllocationError:
                    MessageBox.Show("Allocation error!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;

                case InjectionResult.CallLoadLibraryError:
                    MessageBox.Show("Call LoadLibrary error!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;

                case InjectionResult.HookFunctionsFail:
                    MessageBox.Show("Failed to hook functions!\nMake sure to run the loader as administrator.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;

                case InjectionResult.LoadLibraryAddressNotFound:
                    MessageBox.Show("LoadLibrary address not found!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;

                case InjectionResult.RestoreHooksFail:
                    MessageBox.Show("Failed to restore hooks!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;

                case InjectionResult.SetLoadLibraryPathError:
                    MessageBox.Show("Failed to set LoadLibrary path!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;

                case InjectionResult.Success:
                    UpdateStatus("Injection successful!");
                    if (Properties.Settings.Default.ShowMessages)
                    {
                        MessageBox.Show("Fedoraloader was successful!\nUse the INSERT key to open the menu.", "", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    Thread.Sleep(1000);
                    Application.Current.Shutdown();
                    break;

                default:
                    MessageBox.Show("Unknown error!", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }

        private Process GetGameProcess()
        {
            if (BypassCheckbox.IsChecked == true)
            {
                if (!VacBypass()
                    && MessageBox.Show("VAC bypass failed!", "", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.Cancel)
                {
                    return null;
                }
            }

            Process[] procList = Process.GetProcessesByName("hl2");
            return procList.Length == 0 ? null : procList.First();
        }

        private bool VacBypass()
        {
            IEnumerable<Process> killProcs = Process.GetProcesses()
                .Where(p => p.ProcessName == "hl2" || p.ProcessName == "steam" || p.ProcessName == "steamwebhelper");

            // Kill all steam processes
            UpdateStatus("Closing Steam...");
            foreach (Process proc in killProcs)
            {
                proc.Kill();
            }

            do
            {
                Thread.Sleep(250);
            } while (Process.GetProcesses().Any(p => p.ProcessName == "hl2" || p.ProcessName == "steam" || p.ProcessName == "steamwebhelper"));

            // Extract VAC Bypass
            Directory.CreateDirectory(_workDir + @"\Bypass\");
            string vbFile = _workDir + @"\Bypass\vb.dll";
            File.WriteAllBytes(vbFile, Properties.Resources.VAC_Bypass);

            Thread.Sleep(250);

            // Start new steam process
            UpdateStatus("Starting Steam...");
            string steamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamExe", "") as string;
            if (steamPath == null || !File.Exists(steamPath))
            {
                return false;
            }

            // Wait for process
            Process steamProcess = new Process();
            steamProcess.StartInfo.FileName = steamPath;
            steamProcess.StartInfo.UseShellExecute = true;
            steamProcess.StartInfo.Arguments = "-applaunch 440 " + Properties.Settings.Default.SteamArgs;
            steamProcess.StartInfo.Verb = "runas";
            steamProcess.Start();

            bool moduleFound = false;
            do
            {
                foreach (ProcessModule pm in steamProcess.Modules)
                {
                    if (pm.ModuleName == "steam.exe")
                    {
                        moduleFound = true;
                    }
                }
                Thread.Sleep(250);
            } while (!moduleFound);

            UpdateStatus("Loading VAC bypass...");
            Injector steamjector = new Injector();
            InjectionResult injectionResult = steamjector.Inject(steamProcess.Handle, vbFile);
            if (injectionResult != InjectionResult.Success)
            {
                MessageBox.Show("VAC bypass failed! Could not inject into Steam.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            UpdateStatus("Waiting for TF2...");

            do
            {
                Thread.Sleep(1500);
            } while (!Process.GetProcesses().Any(p => p.ProcessName is "hl2"));
            Thread.Sleep(500);

            return true;
        }

        private void UpdateStatus(string pStatus, bool pVisible = true)
        {
            StatusText.Dispatcher.BeginInvoke((Action)(() =>
            {
                StatusText.Text = pStatus;
                StatusText.Visibility = pVisible ? Visibility.Visible : Visibility.Collapsed;
            }));

            Console.WriteLine("Status: " + pStatus);
        }

    }
}
