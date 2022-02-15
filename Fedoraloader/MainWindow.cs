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
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
        );

        public const string ACTION_URL = "https://nightly.link/tf2cheater2013/Fedoraware/workflows/msbuild/main/Fedoraware.zip";
        public static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        private Point _mouseStartPos;
        private string _workDir; // Fallback folder

        public MainWindow()
        {
            InitializeComponent();
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));

            // Check for administrator rights
            if (!IsElevated)
            {
                MessageBox.Show("This programm will not work without administrator rights!\nPlease run it as administrator.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Generate random work folder
            if (string.IsNullOrEmpty(Properties.Settings.Default.folderID))
            {
                do
                {
                    Properties.Settings.Default.folderID = Utils.RandomString(12);
                } while (Directory.Exists(Path.GetTempPath() + Properties.Settings.Default.folderID));
                Properties.Settings.Default.Save();
            }

            _workDir = Path.GetTempPath() + Properties.Settings.Default.folderID;
            Debug.WriteLine(_workDir);

            chkBypass.Checked = Properties.Settings.Default.bypass;
            chkDefender.Checked = Properties.Settings.Default.defender;

            Text = Utils.RandomString(8);
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
            UpdateStatus("Searching for TF2...");
            Process tfProcess = GetGameProcess();
            if (tfProcess == null) { return; }
            string dllFile = _workDir + @"Fware(Release).dll";

            // Download latest build
            try
            {
                string dlPath = _workDir + @"\" + Utils.RandomString(8) + ".zip";

                // Cleanup and preperation
                UpdateStatus("Preparing files...");
                if (Directory.Exists(_workDir))
                {
                    Directory.Delete(_workDir, true);
                }
                Directory.CreateDirectory(_workDir);

                // Add defender exception if enabled
                if (chkDefender.Checked)
                {
                    UpdateStatus("Adding Defender exception...");
                    if (!AddDefender(Directory.GetCurrentDirectory()) || !AddDefender(_workDir))
                    {
                        if (MessageBox.Show("Failed to add Defender exception!\nDo you want to continue?", "", MessageBoxButtons.YesNo) == DialogResult.No)
                        {
                            return;
                        }
                    }
                }

                // Download build
                UpdateStatus("Downloading...");
                WebClient wc = new();
                wc.DownloadFile(ACTION_URL, dlPath);

                // Extract build
                UpdateStatus("Extracting files...");
                ZipFile.ExtractToDirectory(dlPath, _workDir, true);
                File.Delete(dlPath);

                dllFile = Directory.GetFiles(_workDir, "*.dll").First();
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

                OpenFileDialog dllDialog = new();
                dllDialog.Multiselect = false;
                dllDialog.Title = "Select .dll file";
                dllDialog.Filter = "DLL File (*.dll)|*.dll";

                if (dllDialog.ShowDialog() == DialogResult.OK)
                {
                    UpdateStatus("Injecting custom dll...");
                    Inject(tfProcess, dllDialog.FileName);
                }
            }
        }

        private void Inject(Process pProcess, string pFileName)
        {
            InjectionResult injectionResult = Injector.Inject(pProcess.Handle, pFileName);
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
                    MessageBox.Show("Success!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                    break;

                default:
                    MessageBox.Show("Unknown error!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private Process? GetGameProcess()
        {
            /*if (chkBypass.Checked)
            {
                VacBypass();
            }*/

            Process[] procList = Process.GetProcessesByName("hl2");
            if (procList.Length == 0)
            {
                MessageBox.Show("Team Fortress 2 could not be found!\nMake sure that you open it before loading.", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            return procList.First();
        }

        private bool VacBypass()
        {
            /*IEnumerable<Process> killProcs = Process.GetProcesses()
                .Where(p => p.ProcessName is "hl2" or "Steam" or "Steam Client WebHelper");

            // Kill all steam processes
            foreach (Process proc in killProcs)
            {
                proc.Kill();
            }

            Thread.Sleep(1500);

            // Start new steam process
            string steamPath = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamExe", "");
            if (!File.Exists(steamPath))
            {
                return false;
            }

            // Wait for process
            Process steamProcess = Process.Start(steamPath);
            bool moduleFound = false;
            do
            {
                foreach (ProcessModule pm in steamProcess.Modules)
                {
                    if (pm.ModuleName == "Steam.exe")
                    {
                        Debug.WriteLine("Module.exe found!");
                        moduleFound = true;
                    }

                    if (pm.ModuleName == "Steam")
                    {
                        Debug.WriteLine("Module found!");
                        moduleFound = true;
                    }
                }
                Thread.Sleep(50);
            } while (!moduleFound);*/

            return true;
        }

        private bool AddDefender(string pDirectory)
        {
            try
            {
                _ = PowerShell.Create()
                    .AddScript(@"Add-MpPreference -ExclusionPath '" + pDirectory + "'")
                    .Invoke();

                Debug.WriteLine("Added folder to defender: " + pDirectory);
            }
            catch (Exception)
            {
                return false;
            }

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

        private void chkDefender_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.defender = chkDefender.Checked;
            Properties.Settings.Default.Save();
        }
        #endregion
    }
}