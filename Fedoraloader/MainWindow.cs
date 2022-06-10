using System.Diagnostics;
using System.IO.Compression;
using System.Management.Automation;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Fedoraloader.Injection;
using Microsoft.Win32;

namespace Fedoraloader
{
    public partial class MainWindow : Form
    {
        public const string ACTION_URL = "https://nightly.link/tf2cheater2013/Fedoraware/workflows/msbuild/main/Fedoraware.zip";
        public static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        private Point _mouseStartPos;
        private readonly string _workDir;
        private readonly string _fileDir;

        public MainWindow()
        {
            InitializeComponent();

            // Check for administrator rights
            if (!IsElevated)
            {
                MessageBox.Show("This program will not work without administrator rights!\nPlease run it as administrator.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Check windows version
            if (Environment.OSVersion.Version.Major < 10)
            {
                MessageBox.Show("This program was designed for Windows 10 and higher and might not work properly on your system.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Prepare working directory
            _workDir = Path.GetTempPath() + "Fedloader";
            _fileDir = _workDir + @"\Data\";
            Debug.WriteLine(_workDir);

            chkBypass.Checked = Properties.Settings.Default.bypass;

            Text = Utils.RandomString(8);

            // Initialize tooltips
            mainToolTip.SetToolTip(chkBypass, "Starts Steam with a VAC Bypass.\nThis will restart Steam and TF2.");
        }

        private async void btnLoad_Click(object sender, EventArgs e)
        {
            btnLoad.Enabled = false;

            await Task.Run(() =>
            {
                RunLoader();
                UpdateStatus("", false);
                btnLoad.Invoke(() => { btnLoad.Enabled = true; });
            });
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
                MessageBox.Show("Failed to create working directory:\n" + ex.Message + "\nIs the loader or Fedoraware already running?", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Thread.Sleep(250);

            // Wait for TF2
            UpdateStatus("Searching for TF2...");
            Process? tfProcess = GetGameProcess();
            if (tfProcess == null)
            {
                MessageBox.Show("Team Fortress 2 could not be found!\nMake sure that you open it before loading.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                HttpClientHandler clientHandler = new();
                clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

                using (var httpClient = new HttpClient())
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, ACTION_URL);
                    using var contentStream = httpClient.Send(request).Content.ReadAsStream();
                    using var fileStream = new FileStream(dlPath, FileMode.Create, FileAccess.Write);
                    contentStream.CopyTo(fileStream);
                }

                Thread.Sleep(250);

                // Extract build
                UpdateStatus("Extracting files...");
                ZipFile.ExtractToDirectory(dlPath, _fileDir, true);
                File.Delete(dlPath);

                Thread.Sleep(250);

                dllFile = Directory.GetFiles(_fileDir, "*.dll").First();
            }
            catch (Exception ex)
            {
                if (MessageBox.Show("Failed to download the latest build:\n" + ex.Message, "", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
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
                DialogResult dlgResult = MessageBox.Show("The loader file could not be found!\nDo you want to manually select a .dll file?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dlgResult != DialogResult.Yes) { return; }

                OpenFileDialog dllDialog = new()
                {
                    Multiselect = false,
                    Title = "Select .dll file",
                    Filter = "DLL File (*.dll)|*.dll"
                };

                if (dllDialog.ShowDialog() == DialogResult.OK)
                {
                    UpdateStatus("Injecting custom dll...");
                    Inject(tfProcess, dllDialog.FileName);
                }
            }
        }

        private void Inject(Process pProcess, string pFileName)
        {
            Injector tfInjector = new();
            InjectionResult injectionResult = tfInjector.Inject(pProcess.Handle, pFileName);
            switch (injectionResult)
            {
                case InjectionResult.AllocationError:
                    MessageBox.Show("Allocation error!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;

                case InjectionResult.CallLoadLibraryError:
                    MessageBox.Show("Call LoadLibrary error!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;

                case InjectionResult.HookFunctionsFail:
                    MessageBox.Show("Failed to hook functions!\nMake sure to run the loader as administrator.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;

                case InjectionResult.LoadLibraryAddressNotFound:
                    MessageBox.Show("LoadLibrary address not found!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;

                case InjectionResult.RestoreHooksFail:
                    MessageBox.Show("Failed to restore hooks!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;

                case InjectionResult.SetLoadLibraryPathError:
                    MessageBox.Show("Failed to set LoadLibrary path!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;

                case InjectionResult.Success:
                    UpdateStatus("Injection successful!");
                    if (Properties.Settings.Default.successMsg)
                    {
                        MessageBox.Show("Fedoraloader was successful!\nUse the INSERT key to open the menu.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    Thread.Sleep(1000);
                    Application.Exit();
                    break;

                default:
                    MessageBox.Show("Unknown error!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private Process? GetGameProcess()
        {
            if (chkBypass.Checked)
            {
                if (!VacBypass())
                {
                    if (MessageBox.Show("VAC bypass failed!", "", MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Error) == DialogResult.Cancel)
                    {
                        return null;
                    }
                }
            }

            Process[] procList = Process.GetProcessesByName("hl2");
            return procList.Length == 0 ? null : procList.First();
        }

        private bool VacBypass()
        {
            IEnumerable<Process> killProcs = Process.GetProcesses()
                .Where(p => p.ProcessName is "hl2" or "steam" or "steamwebhelper");

            // Kill all steam processes
            UpdateStatus("Closing Steam...");
            foreach (Process proc in killProcs)
            {
                proc.Kill();
            }

            do
            {
                Thread.Sleep(250);
            } while (Process.GetProcesses().Any(p => p.ProcessName is "hl2" or "steam" or "steamwebhelper"));

            // Extract VAC Bypass
            Directory.CreateDirectory(_workDir + @"\Bypass\");
            string vbFile = _workDir + @"\Bypass\vb.dll";
            File.WriteAllBytes(vbFile, Properties.Resources.VAC_Bypass);

            Thread.Sleep(250);

            // Start new steam process
            UpdateStatus("Starting Steam...");
            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamExe", "") is not string steamPath || !File.Exists(steamPath))
            {
                return false;
            }

            // Wait for process
            Process steamProcess = new();
            steamProcess.StartInfo.FileName = steamPath;
            steamProcess.StartInfo.UseShellExecute = true;
            steamProcess.StartInfo.Arguments = "-applaunch 440 " + Properties.Settings.Default.steamArgs;
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
            Injector steamInjector = new();
            InjectionResult injectionResult = steamInjector.Inject(steamProcess.Handle, vbFile);
            if (injectionResult != InjectionResult.Success)
            {
                MessageBox.Show("VAC bypass failed! Could not inject into Steam.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            lblStatus.Invoke(() =>
            {
                lblStatus.Visible = pVisible;
                lblStatus.Text = pStatus;
            });
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        #region Draggable Form
        private void lblTitle_MouseDown(object sender, MouseEventArgs e)
        {
            pnlHeader_MouseDown(sender, e);
        }

        private void pnlHeader_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseStartPos = new Point(-e.X, -e.Y);
        }

        private void lblTitle_MouseMove(object sender, MouseEventArgs e)
        {
            pnlHeader_MouseMove(sender, e);
        }

        private void pnlHeader_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(_mouseStartPos.X, _mouseStartPos.Y);
                Location = mousePos;
            }
        }
        #endregion

        #region Settings
        private void chkBypass_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.bypass = chkBypass.Checked;
            Properties.Settings.Default.Save();
        }
        #endregion

        private void pnlLaunchArgs_Click(object sender, EventArgs e)
        {
            new Settings().ShowDialog();
        }
    }
}
