using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DesktopPet
{
        /// <summary>
        /// Window form for the installer/Repair/Uninstaller. Will add shortcuts to windows.
        /// </summary>
        /// <preliminary/>
    public partial class Install : Form
    {
            /// <summary>
            /// Path where the application is located.
            /// </summary>
        readonly string appPath;
            /// <summary>
            /// Path where the application can be installed.
            /// </summary>
        readonly string installPath;
            /// <summary>
            /// Path to the user desktop location.
            /// </summary>
        readonly string desktopPath;
            /// <summary>
            /// Path to the user start menu location.
            /// </summary>
        readonly string startMenuPath;
            /// <summary>
            /// Path to the start-up location.
            /// </summary>
        readonly string autostartPath;
            /// <summary>
            /// Webpage for this application.
            /// </summary>
        readonly string webpage = "http://esheep.petrucci.ch";
            /// <summary>
            /// Application Name.
            /// </summary>
        readonly string appName = "DesktopPet";
            /// <summary>
            /// Name of the uninstall batch file.
            /// </summary>
        readonly string uninstallBatch = "uninstall.cmd";

            /// <summary>
            /// Constructor. Set the path of the different locations and start the update-check process.
            /// </summary>
        public Install()
        {
            InitializeComponent();

            appPath = Application.StartupPath;
            installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appName);
            desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
            autostartPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                // Check updates only if the application is installed (so downloads from other page will not notify an update after download)
            if(Program.IsApplicationInstalled())
                CheckUpdates();
        }

            /// <summary>
            /// Form was loaded.
            /// </summary>
            /// <param name="sender">Caller as object.</param>
            /// <param name="e">Arguments values.</param>
        private void Install_Load(object sender, EventArgs e)
        {
            Text = "Install " + Animations.Xml.AnimationXML.Header.Title;
            label1.Text = "Install " + Animations.Xml.AnimationXML.Header.Petname;
        }
        
            /// <summary>
            /// Open the form window. Allows the user to install or uninstall this application.
            /// </summary>
        public void ShowInstallation()
        {
            if (!Program.IsApplicationInstalled())
            {
                string msg = "THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS \"AS IS\" ";
                msg += "AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE ";
                msg += "IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE ";
                msg += "DISCLAIMED.IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE ";
                msg += "FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL ";
                msg += "DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR ";
                msg += "SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER ";
                msg += "CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, ";
                msg += "OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE ";
                msg += "OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. ";

                textBox1.Text = msg;
                textBox1.Visible = true;
                button1.Text = "Accept";
                button2.Visible = false;
                button3.Visible = false;
            }
            else
            {
                button1.Visible = false;
                button2.Visible = true;
                button3.Visible = true;
                textBox1.Visible = false;
                checkBox1.Visible = true;
                checkBox2.Visible = true;
                checkBox3.Visible = true;
                button2.Enabled = false;

                checkBox1.Checked = File.Exists(Path.Combine(startMenuPath, $"{appName}.url"));
                checkBox2.Checked = File.Exists(Path.Combine(desktopPath, $"{appName}.url"));
                checkBox3.Checked = File.Exists(Path.Combine(autostartPath, $"{appName}.url"));
            }

            Show();
        }

            /// <summary>
            /// Install was pressed. The application will be installed on the AppData folder.
            /// </summary>
            /// <param name="sender">Caller as object.</param>
            /// <param name="e">Mouse event values.</param>
        private void Button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Accept")
            {
                button1.Text = "Install";
                button2.Visible = false;
                textBox1.Visible = false;
                checkBox1.Visible = true;
                checkBox2.Visible = true;
                checkBox3.Visible = true;
                progressBar1.Visible = true;
            }
            else if(button1.Text == "Install")
            {
                checkBox1.Enabled = false;
                checkBox2.Enabled = false;
                checkBox3.Enabled = false;
                button1.Enabled = false;
                Application.DoEvents();
                progressBar1.Value = 100;
                InstallApplication();
            }
            else
            {
                Application.Exit();
            }
        }

            /// <summary>
            /// Application will be installed. Exe will be copied to the install path, icons will be created and registry will be updated.<br />
            /// A uninstall.cmd will also be created for the uninstall process.
            /// </summary>
        private void InstallApplication()
        {
            string sDestExe = installPath + "\\" + appName + ".exe";
            bool bPersonalIcon = true;
            if (!Directory.Exists(installPath))
            {
                Directory.CreateDirectory(installPath);
            }
            try
            {
                File.Copy(Application.ExecutablePath, sDestExe, true);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
                progressBar1.Value = 0;
                return;
            }

            try
            {
                FileStream fs = File.OpenWrite(installPath + "\\installpet.xml");
                fs.Write(System.Text.Encoding.UTF8.GetBytes(Animations.Xml.AnimationXMLString), 0, Animations.Xml.AnimationXMLString.Length);
                fs.Close();

                fs = File.OpenWrite(installPath + "\\icon.ico");
                byte[] sIco = Animations.Xml.bitmapIcon.ToArray();
                fs.Write(sIco, 0, sIco.Length);
                fs.Close();
            }
            catch(Exception)
            {
                bPersonalIcon = false;
            }

            string contents;

            if (bPersonalIcon)
            {
                contents = $@"
[InternetShortcut] 
URL=file:///{sDestExe.Replace('\\', '/')}
IconFile={installPath+"\\icon.ico"}
IconIndex=0 
            ";
            }
            else
            {
                contents = $@"
[InternetShortcut] 
URL=file:///{sDestExe.Replace('\\', '/')}
IconFile={sDestExe.Replace('\\', '/')}
IconIndex=0 
            ";
            }

                // add to start menu
            if (checkBox1.Checked)
            {   
                string file = Path.Combine(startMenuPath, $"{appName}.url");
                File.WriteAllText(file, contents);
            }

                // add to desktop
            if (checkBox2.Checked)
            {
                string file = Path.Combine(desktopPath, $"{appName}.url");
                File.WriteAllText(file, contents);
            }

                // add to auto start
            if (checkBox3.Checked)
            {
                string file = Path.Combine(autostartPath, $"{appName}.url");
                File.WriteAllText(file, contents);
            }

            // Create an uninstall.cmd script 
            string UninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
            string keyName = $@"{UninstallKey}\{appName}";

            // Generate the commands to remove the shortcuts
            var deletions = $@"del ""{Path.Combine(startMenuPath, $"{appName}.url")}""{Environment.NewLine}";
            deletions += $@"del ""{Path.Combine(desktopPath, $"{appName}.url")}""{Environment.NewLine}";
            deletions += $@"del ""{Path.Combine(autostartPath, $"{appName}.url")}""{Environment.NewLine}";
            string script = $@"
@echo on 
setlocal 
cd %~dp0\.. 
:: Remove running application
taskkill /IM {appName}.exe /f
:: Delete the shortcuts 
{deletions} 
:: Use start to finish before we're deleted 
start /min """" cmd /c
%= Use reg to delete the app key =%
reg delete ""HKCU\{keyName}"" /f
%= Remove the app's root folder =%
rd /s /q ""{appName}""
            ";

            string scriptPath = Path.Combine(installPath, uninstallBatch);
            File.WriteAllText(scriptPath, script);

            using (var key = Registry.CurrentUser.CreateSubKey(keyName))
            {
                // strings 
                string path = installPath + "\\" + appName + ".exe";
                key.SetValue("DisplayIcon", path);
                key.SetValue("DisplayName", appName);
                key.SetValue("DisplayVersion", Application.ProductVersion);
                key.SetValue("InstallDate", DateTime.Now.ToString("yyyymmdd"));
                key.SetValue("InstallLocation", installPath);
                key.SetValue("Publisher", Application.CompanyName);
                key.SetValue("QuietUninstallString", scriptPath);
                key.SetValue("UninstallString", scriptPath); // TODO: Show GIF of some sort? 
                key.SetValue("URLInfoAbout", webpage);
                key.SetValue("URLUpdateInfo", webpage);
                // DWords 
                key.SetValue("EstimatedSize", 1500);
                key.SetValue("NoModify", 1);
                key.SetValue("NoRepair", 1);
                key.SetValue("Language", 0x0409);   // English locale
            }

            button1.Text = "Installed";

            using (var petProcess = new Process())
            {
                petProcess.StartInfo.FileName = installPath + "\\" + appName + ".exe";
                petProcess.Start();
            }

            Hide();
            Application.DoEvents();
            Application.Exit();
        }

            /// <summary>
            /// Update was pressed. Application icons will be created or removed.
            /// </summary>
            /// <param name="sender">Caller as object.</param>
            /// <param name="e">Mouse event values.</param>
        private void Button2_Click(object sender, EventArgs e)
        {
            string sDestExe = installPath + "\\" + appName + ".exe";

            string contents = $@"
[InternetShortcut] 
URL=file:///{sDestExe.Replace('\\', '/')}
IconFile={sDestExe.Replace('\\', '/')}
IconIndex=0
            ";
            
            if (checkBox1.Checked != File.Exists(Path.Combine(startMenuPath, $"{appName}.url")))
            {
                string file = Path.Combine(startMenuPath, $"{appName}.url");
                    
                if (checkBox1.Checked)  // add to start menu
                    File.WriteAllText(file, contents);
                else                    // remove from start menu
                    File.Delete(file);
            }
            if(checkBox2.Checked != File.Exists(Path.Combine(desktopPath, $"{appName}.url")))
            {
                string file = Path.Combine(desktopPath, $"{appName}.url");
                if (checkBox2.Checked)  // add to desktop
                    File.WriteAllText(file, contents);
                else                    // remove from desktop
                    File.Delete(file);
            }
            if(checkBox3.Checked != File.Exists(Path.Combine(autostartPath, $"{appName}.url")))
            {
                string file = Path.Combine(autostartPath, $"{appName}.url");
                if (checkBox3.Checked)  // add to auto start
                    File.WriteAllText(file, contents);
                else                    // remove from auto start
                    File.Delete(file);
            }

            button2.Enabled = false;
        }

            /// <summary>
            /// Uninstall was pressed. The uninstall batch file will be executed.
            /// </summary>
            /// <param name="sender">Caller as object.</param>
            /// <param name="e">Mouse event values.</param>
        private void Button3_Click(object sender, EventArgs e)
        {
            using (var uninstallProcess = new Process())
            {
                uninstallProcess.StartInfo.FileName = installPath + "\\" + uninstallBatch;
                uninstallProcess.Start();
            }
        }

            /// <summary>
            /// Closing form. If it is visible, hide the window so that functions are still available.
            /// </summary>
            /// <param name="sender">Caller as sender.</param>
            /// <param name="e">Closing event values.</param>
        private void Install_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Visible)
            {
                Hide();
                e.Cancel = true;
            }
        }

            /// <summary>
            /// If checkbox was changed and it is in update mode, enable the update button.
            /// </summary>
            /// <param name="sender">Caller as object.</param>
            /// <param name="e">Check event values.</param>
        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (button2.Visible) button2.Enabled = true;
        }

            /// <summary>
            /// If checkbox was changed and it is in update mode, enable the update button.
            /// </summary>
            /// <param name="sender">Caller as object.</param>
            /// <param name="e">Check event values.</param>
        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (button2.Visible) button2.Enabled = true;
        }

            /// <summary>
            /// If checkbox was changed and it is in update mode, enable the update button.
            /// </summary>
            /// <param name="sender">Caller as object.</param>
            /// <param name="e">Check event values.</param>
        private void CheckBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (button2.Visible) button2.Enabled = true;
        }

            /// <summary>
            /// Check for updates. If an update is present, will replace this application with the new one.
            /// </summary>
        private void CheckUpdates()
        {
            string sZipFile = appPath + "\\" + appName + ".gz";
            string sOldFile = appPath + "\\" + appName + "_old.exe";
            try
            {
                if (File.Exists(sOldFile))
                {
                    File.Delete(sOldFile);
                }
                if (File.Exists(sZipFile))
                {
                    File.Delete(sZipFile);
                }
            }
            catch(Exception)
            {
            }
            WebRequest request = WebRequest.Create("https://github.com/Adrianotiger/desktopPet/releases/latest");
            WebResponse response = request.GetResponse();
            var version = response.ResponseUri.Segments.GetValue(response.ResponseUri.Segments.Length - 1).ToString();
            version = Regex.Replace(version, "[^0-9.]", "");
            Stream data = response.GetResponseStream();
            string versionWeb = version;
            string versionApp = Application.ProductVersion.Substring(0, Application.ProductVersion.LastIndexOf("."));

            if(versionApp != versionWeb)
            {
                Stream dataChangelog = response.GetResponseStream();
                string changeLog = Application.ProductVersion;
                using (StreamReader sr = new StreamReader(dataChangelog))
                {
                    var changelogPage = sr.ReadToEnd();
                    var pattern = @"<ul>([\S\s]*?)<\/ul>";
                    changeLog = Regex.Match(changelogPage, pattern).Groups[1].ToString();
                    changeLog = changeLog.Replace("<li>", " - ").Replace("</li>", "");
                }

                if (MessageBox.Show("A newer version was found on the web: " + versionWeb + "\n==========================\nCHANGELOG:\n" + changeLog  + "\n========================== \nDo you want install it now?", appName + " version: " + versionApp, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
                        webClient.DownloadFileAsync(
                                new Uri(response.ResponseUri.ToString().Replace("/tag/", "/download/") + "/DesktopPet.gz"),
                                $@"{sZipFile}");
                    }
                }
            }
        }

            /// <summary>
            /// Exe was downloaded successfully. Application can be closed.
            /// </summary>
            /// <param name="sender">Caller as object.</param>
            /// <param name="e">Download event values.</param>
        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            string sExeFile = appPath + "\\" + appName + ".exe";
            string sZipFile = appPath + "\\" + appName + ".gz";
            string sOldFile = appPath + "\\" + appName + "_old.exe";
            if(e.Error != null)
            {
                MessageBox.Show("Unable to update: " + e.Error.Message);
                return;
            }

            try
            {
                File.Move(sExeFile, sOldFile);

                using (var fileStream = System.IO.File.OpenRead(sZipFile))
                {
                    using (var zipStream = new System.IO.Compression.GZipStream(fileStream, System.IO.Compression.CompressionMode.Decompress))
                    {
                        using (var destFile = System.IO.File.Create(sExeFile))
                        {
                            zipStream.CopyTo(destFile);
                        }
                    }
                }

                using (var petProcess = new Process())
                {
                    petProcess.StartInfo.FileName = sExeFile;
                    petProcess.Start();
                }

                Hide();
                MessageBox.Show("Application was updated.");
                Application.Exit();
            }
            catch (Exception ex)
            {
                File.Move(sOldFile, sExeFile);
                MessageBox.Show("Unable to update: " + ex.Message);
            }
        }
    }
}
