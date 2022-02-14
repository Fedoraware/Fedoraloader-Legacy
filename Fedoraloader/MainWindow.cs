using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Fedoraloader.Injection;
using Fedoraloader.Injection.Native;

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

        public static bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
        private Point _mouseStartPos;

        public MainWindow()
        {
            InitializeComponent();
            Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));

            if (!IsElevated)
            {
                MessageBox.Show("This programm will not work without administrator rights!\nPlease run it as administrator.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Text = Process.GetProcesses().First(x => x.ProcessName != "hl2").ProcessName;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            Process[] procList = Process.GetProcessesByName("hl2");
            if (procList.Length == 0)
            {
                DialogResult dlgResult = MessageBox.Show("Team Fortress 2 could not be found!\nMake sure that you open it before loading.", "", MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
                if (dlgResult == DialogResult.Retry)
                {
                    btnLoad.PerformClick();
                }
                return;
            }

            Process tfProcess = procList.First();
            string dllPath = Path.GetTempPath() + @"\1x.dll";
            if (File.Exists(dllPath))
            {
                Inject(tfProcess, dllPath);
            }
            else
            {
                DialogResult dlgResult = MessageBox.Show("The loader file could not be found!\nDo you want to manually select a .dll file?", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dlgResult == DialogResult.Yes)
                {
                    OpenFileDialog dllDialog = new OpenFileDialog
                    {
                        Multiselect = false,
                        
                        Title = "Select .dll file",
                        Filter = "DLL File (*.dll)|*.dll"
                    };
                    if (dllDialog.ShowDialog() == DialogResult.OK)
                    {
                        Inject(tfProcess, dllDialog.FileName);
                    }
                }
            }
        }

        private void Inject(Process pProcess, string pFileName)
        {
            InjectionResult injectionResult = Injector.Inject(pProcess.Handle, pFileName + "x");
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
                    MessageBox.Show("Success!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Application.Exit();
                    break;

                default:
                    MessageBox.Show("Unknown error!", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
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
    }
}