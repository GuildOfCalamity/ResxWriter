using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static System.Resources.ResXFileRef;

namespace ResxWriter
{
    /// <summary>
    /// TODO: Jazz up <see cref="tbContents"/> with a <see cref="ListView"/>.
    /// </summary>
    public partial class frmMain : Form
    {
        #region [Props]
        readonly List<string> _commonDelimiters = new List<string> { ",", ";", "~", "|", "TAB" };
        Dictionary<string, string> _userValues = new Dictionary<string, string>();
        string _userDelimiter = string.Empty;
        string _genericError = "An error was detected.";
        bool _useMeta = false;
        #endregion

        public frmMain()
        {
            InitializeComponent();
        }

        #region [Event Methods]
        void frmMain_Shown(object sender, EventArgs e)
        {
            UpdateStatus("Click the folder icon to select a file and then click import.");
            SwitchButton(btnGenerateResx, ResxWriter.Properties.Resources.Button02);

            #region [Load settings]
            var lastPath = SettingsManager.LastPath;
            if (!string.IsNullOrEmpty(lastPath))
                tbFilePath.Text = lastPath;
            else
                tbFilePath.Text = $"{Environment.CurrentDirectory}\\";

            var lastDelimiter = SettingsManager.LastDelimiter;
            #endregion

            #region [ComboBox Setup]
            foreach (var delimiter in _commonDelimiters)
            {
                cbDelimiters.Items.Add(delimiter);
                _userDelimiter = delimiter;
            }
            if (cbDelimiters.Items.Count > 0)
                cbDelimiters.SelectedIndex = 0;

            if (!string.IsNullOrEmpty(lastDelimiter))
                cbDelimiters.Text = _userDelimiter = lastDelimiter;

            #endregion

            DetermineWindowDPI();

            Logger.Instance.OnDebug += (msg) => { Debug.WriteLine($"{msg}"); };

            SetListTheme(lvContents);

            //Icon appIcon = Utils.GetFileIcon(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".exe", true);
        }

        /// <summary>
        /// Browse to the file location.
        /// </summary>
        void btnFileSelect_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select an import file name...";
                openFileDialog.InitialDirectory = Environment.CurrentDirectory;
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Get the selected file name.
                        string filePath = openFileDialog.FileName;

                        // Display the content in a TextBox.
                        tbFilePath.Text = filePath;

                        // Read the UTF-8 text file content.
                        //string fileContent = File.ReadAllText(filePath, Encoding.UTF8);
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus(_genericError);
                        Logger.Instance.Write($"Could not read the file: {ex.Message}", LogLevel.Error);
                        ShowMsgBoxError($"Could not read the file.\r\n{ex.Message}", "Error");
                    }
                }
                else
                {
                    SwitchButton(btnGenerateResx, ResxWriter.Properties.Resources.Button02);
                    UpdateStatus("File selection was canceled.");
                }
            }
        }

        /// <summary>
        /// Import the file contents.
        /// </summary>
        void btnImport_Click(object sender, EventArgs e)
        {
            if (tbFilePath.Text.Length > 0)
            {
                try
                {
                    if (_userDelimiter.Equals("TAB"))
                        _userDelimiter = "\t";

                    //if (!File.Exists(textContentTextBox.Text))
                    //{
                    //    UpdateStatus($"File does not exist, please check you path and try again.");
                    //    return;
                    //}

                    _userValues = ReadDelimitedFile(tbFilePath.Text, _userDelimiter[0]);


                    if (_userValues.Count > 0)
                    {
                        UpdateStatus($"File data has been loaded.  {_userValues.Count} items total.");
                        
                        if (_userValues.Count > 10)
                            tbContents.ScrollBars = ScrollBars.Vertical;

                        Task.Run(() => FlashButton(btnGenerateResx));
                    }
                    else
                    {
                        UpdateStatus($"Check your input file and try again.");
                        SwitchButton(btnGenerateResx, ResxWriter.Properties.Resources.Button02);
                    }

                    //AddImportToTextBox(_userValues, tbContents);
                    AddImportToListView(_userValues, lvContents);

                    // Test for reading resx files.
                    //var resxValues = ReadResxFile(textContentTextBox.Text);
                    //AddValuesToTextBox(resxValues, tbContents);
                }
                catch (Exception ex)
                {
                    UpdateStatus(_genericError);
                    SwitchButton(btnGenerateResx, ResxWriter.Properties.Resources.Button02);
                    Logger.Instance.Write($"Could not read the file: {ex.Message}", LogLevel.Error);
                    ShowMsgBoxError($"Could not read the file.\r\n{ex.Message}", "Error");
                }
            }
            else
            {
                SwitchButton(btnGenerateResx, ResxWriter.Properties.Resources.Button02);
                UpdateStatus("Import was canceled.");
            }
        }

        /// <summary>
        /// Create the resx file.
        /// </summary>
        void btnGenerateResx_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Select an output file name...";
                saveFileDialog.Filter = "Resx files (*.resx)|*.resx";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = $"{Path.GetFileNameWithoutExtension(tbFilePath.Text)}.resx";

                if (_userValues.Count == 0)
                {
                    UpdateStatus("Check that you have imported some valid data.");
                    SwitchButton(btnGenerateResx, ResxWriter.Properties.Resources.Button02);
                    ShowMsgBoxError("No valid delimited values to work with from the provided file.", "Validation Error");
                    return;
                }

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Get the selected file name.
                        string filePath = saveFileDialog.FileName;

                        // Create a ResXResourceWriter.
                        using (ResXResourceWriter resxWriter = new ResXResourceWriter(filePath))
                        {
                            foreach(var kvp in _userValues)
                            {
                                // Add the content to the resx file.
                                if (_useMeta)
                                    resxWriter.AddMetadata(kvp.Key, kvp.Value);
                                else
                                    resxWriter.AddResource(kvp.Key, kvp.Value);
                            }
                        }

                        UpdateStatus("Resx file generated successfully.");
                        Logger.Instance.Write($"Resx file generated successfully: \"{filePath}\" ({_userValues.Count} items).", LogLevel.Success);
                        //ShowMsgBoxSuccess("Resx file generated successfully.", "Success");
                    }
                    catch (XmlException ex) // Handle XML-related exceptions.
                    {
                        UpdateStatus(_genericError);
                        Logger.Instance.Write($"Could not generate the resx file: {ex.Message}", LogLevel.Error);
                        ShowMsgBoxError($"Could not generate the resx file.\r\n{ex.Message}", "XML Process Error");
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus(_genericError);
                        Logger.Instance.Write($"Could not generate the resx file: {ex.Message}", LogLevel.Error);
                        ShowMsgBoxError($"Could not generate the resx file.\r\n{ex.Message}", "Process Error");
                    }
                }
                else
                {
                    UpdateStatus("Export was canceled.");
                }
            }
        }

        void cbDelimiters_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb != null)
                _userDelimiter = cb.Items[cb.SelectedIndex] as string;
        }

        void cbMetadata_CheckedChanged(object sender, EventArgs e)
        {
            var cb = sender as CheckBox;
            if (cb != null)
                _useMeta = cb.Checked;
        }

        void cbDelimiters_TextUpdate(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cbDelimiters.Text))
            {
                _userDelimiter = cbDelimiters.Text;
                UpdateStatus($"Custom delimiter set to \"{_userDelimiter}\"");
            }
        }

        void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (this.WindowState == FormWindowState.Normal)
                {
                    SettingsManager.WindowLeft = this.Left;
                    SettingsManager.WindowTop = this.Top;
                    SettingsManager.WindowHeight = this.Height;
                    SettingsManager.WindowWidth = this.Width;
                }
                // Special chars do not store well in XML, so we have a logic check here for 0x09(HTAB).
                SettingsManager.LastDelimiter = _userDelimiter.Equals("\t") ? "TAB" : _userDelimiter;
                SettingsManager.WindowState = (int)this.WindowState;
                SettingsManager.LastPath = tbFilePath.Text;
                SettingsManager.Save(SettingsManager.AppSettings, SettingsManager.Location, SettingsManager.Version);
            }
            catch (Exception ex)
            {
                Logger.Instance.Write($"Error while closing form: {ex.Message}", LogLevel.Error);
            }
        }

        void lvContents_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Right-click shouldn't change the selection.
            if (Control.MouseButtons == System.Windows.Forms.MouseButtons.Right)
                return;

            var lv = sender as ListView;

            try
            {
                if (lv.SelectedItems.Count == 0)
                    return;

                var data = lv.SelectedItems[0].SubItems[1].Text;
                Debug.WriteLine($"SelectedItem => {data}");
            }
            catch (Exception ex ) 
            { 
                Debug.WriteLine($"[ERROR] {ex.Message}"); 
            }
        }
        #endregion

        #region [Helper Methods]
        /// <summary>
        /// Add a file hit to the listview (Thread safe).
        /// </summary>
        /// <param name="file">File to add</param>
        /// <param name="index">Position in GrepCollection</param>
        void AddToListView(string key, string value, int index, ListView lv)
        {
            lv.InvokeIfRequired(() =>
            {
                // Duplicate checking.
                //foreach (ListViewItem item in lstFileNames.Items) { }

                // Create the list item.
                var listItem = new ListViewItem(key)
                {
                    Name = index.ToString(), 
                    ForeColor = index % 2 == 0 ? Color.DeepSkyBlue : Color.DodgerBlue,
                    //ImageIndex = ListViewImageManager.GetImageIndex(file, ListViewImageList)
                };
                listItem.SubItems.Add(value);

                // Add explorer style of file size for display but store file size in bytes for comparison
                ListViewItem.ListViewSubItem subItem = new ListViewItem.ListViewSubItem(listItem, value)
                {
                    Tag = key, ForeColor = Color.SpringGreen,
                };
                listItem.SubItems.Add(subItem);
                //listItem.SubItems.Add("0");

                // must be last
                listItem.SubItems.Add(index.ToString());

                // Add list item to listview
                lvContents.Items.Add(listItem);

                // clear it out
                listItem = null;
            });
        }

        /// <summary>
        /// Reads a delimited file and returns its contents as a <see cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        Dictionary<string, string> ReadDelimitedFile(string filePath, char delimiter)
        {
            Dictionary<string, string> fieldDictionary = new Dictionary<string, string>();

            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            string[] fields = line.Split(delimiter);

                            // Ensure there are at least two fields
                            if (fields.Length >= 2)
                            {
                                // Add the first and second fields to the dictionary
                                fieldDictionary[fields[0]] = fields[1];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus(_genericError);
                ShowMsgBoxError($"Error reading the file.\r\n{ex.Message}", "Error");
            }

            return fieldDictionary;
        }

        /// <summary>
        /// Reads a resx file and returns its contents as a <see cref="Dictionary{TKey, TValue}"/>.
        /// </summary>
        Dictionary<string, string> ReadResxFile(string filePath)
        {
            Dictionary<string, string> resxValues = new Dictionary<string, string>();

            try
            {
                using (ResXResourceReader resxReader = new ResXResourceReader(filePath))
                {
                    // Iterate through the .resx file and load key-value pairs.
                    foreach (DictionaryEntry entry in resxReader)
                    {
                        if (entry.Value != null)
                        {
                            // Add key-value pair to the dictionary.
                            resxValues.Add(entry.Key.ToString(), entry.Value.ToString());
                        }
                    }
                }
            }
            catch (XmlException ex) // Handle XML-related exceptions.
            {
                UpdateStatus(_genericError);
                ShowMsgBoxError($"Error reading the resx file.\r\n{ex.Message}", "XML Process Error");
            }
            catch (SystemException ex)
            {
                UpdateStatus(_genericError);
                ShowMsgBoxError($"Error reading the resx file.\r\n{ex.Message}", "System Process Error");
            }
            catch (Exception ex)
            {
                UpdateStatus(_genericError);
                ShowMsgBoxError($"Error reading the resx file.\r\n{ex.Message}", "Process Error");
            }

            return resxValues;
        }

        /// <summary>
        /// Adds each KeyValuePair in the <paramref name="dictionary"/> to the <paramref name="textBox"/>.
        /// </summary>
        void AddImportToTextBox(Dictionary<string, string> dictionary, TextBox textBox)
        {
            if (dictionary.Count == 0)
            {
                textBox.Text = string.Empty;
                return;
            }

            StringBuilder sb = new StringBuilder();

            foreach (var kvp in dictionary)
                sb.AppendLine($"{kvp.Key}{_userDelimiter}{kvp.Value}");

            textBox.Text = sb.ToString();
        }

        void AddImportToListView(Dictionary<string, string> dictionary, ListView lv)
        {
            int idx = 0;
            ClearListView();

            lv.BeginUpdate();
            foreach (var kvp in dictionary)
                AddToListView($"{kvp.Key}", $"{kvp.Value}", ++idx, lv);
            lv.EndUpdate();
        }

        /// <summary>
        /// Thread-safe method
        /// </summary>
        void UpdateStatus(string message)
        {
            try
            {
                if (InvokeRequired)
                    BeginInvoke(new Action(() => UpdateStatus(message)));
                else
                {
                    statusTime.Text = $"{DateTime.Now.ToString("hh:mm:ss tt")}";
                    statusText.Text = $"{message}";
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Thread-safe method
        /// </summary>
        public void ClearTextbox()
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => ClearTextbox()));
            else
            {
                tbContents.Text = string.Empty;
                tbContents.ScrollBars = ScrollBars.None;
            }
        }

        /// <summary>
        /// Thread-safe method
        /// </summary>
        public void ClearListView()
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => ClearListView()));
            else
            {
                lvContents.Items.Clear();
            }
        }

        /// <summary>
        /// Thread-safe method
        /// </summary>
        /// <param name="ctrl"><see cref="Control"/></param>
        /// <param name="state">true=enabled, false=disabled</param>
        public void ToggleControl(Control ctrl, bool state)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => ToggleControl(ctrl, state)));
            else
                ctrl.Enabled = state;
        }

        /// <summary>
        /// Thread-safe method
        /// </summary>
        /// <param name="btn"><see cref="Button"/></param>
        /// <param name="img"><see cref="Bitmap"></param>
        public void SwitchButton(Button btn, Bitmap img)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => SwitchButton(btn, img)));
            else
            {
                btn.Image = img;
                btn.Refresh(); //btn.Invalidate();
            }
        }

        /// <summary>
        /// Thread-safe method
        /// </summary>
        /// <param name="ctrl"><see cref="System.Windows.Forms.CheckBox"/></param>
        /// <param name="state">true=checked, false=unchecked</param>
        public void ToggleCheckBox(CheckBox ctrl, bool state)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => ToggleCheckBox(ctrl, state)));
            else
                ctrl.Checked = state;
        }

        /// <summary>
        /// Thread-safe method
        /// </summary>
        /// <param name="ctrl"><see cref="System.Windows.Forms.Control"/></param>
        /// <param name="data">text for control</param>
        public void UpdateControl(Control ctrl, string data)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => UpdateControl(ctrl, data)));
            else
                ctrl.Text = data;
        }

        /// <summary>
        /// Thread-safe method
        /// </summary>
        /// <param name="state"><see cref="System.Windows.Forms.FormWindowState"/></param>
        void UpdateWindowState(FormWindowState state)
        {
            try
            {
                if (InvokeRequired)
                    BeginInvoke(new Action(() => UpdateWindowState(state)));
                else
                    this.WindowState = state;
            }
            catch (Exception) { }
        }

        /// <summary>
        /// A simple animation for the generate button.
        /// </summary>
        void FlashButton(Button btn, int blinkCount = 3, int blinkSpeed = 150)
        {
            for (int i = 0; i < blinkCount; i++) 
            {
                Thread.Sleep(blinkSpeed);
                //ToggleControl(btn, false);
                SwitchButton(btn, ResxWriter.Properties.Resources.Button02);
                Thread.Sleep(blinkSpeed);
                //ToggleControl(btn, true);
                SwitchButton(btn, ResxWriter.Properties.Resources.Button01);
            }
        }

        void ShowMsgBoxInfo(string msg, string title)
        {
            //MessageBox.Show($"{msg}", $"{title}", MessageBoxButtons.OK, MessageBoxIcon.Information);
            frmMessage.Show($"{msg}", $"{title}", MessageLevel.Info, true);
        }
        void ShowMsgBoxWarning(string msg, string title)
        {
            //MessageBox.Show($"{msg}", $"{title}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            frmMessage.Show($"{msg}", $"{title}", MessageLevel.Warning, true);
        }
        void ShowMsgBoxError(string msg, string title)
        {
            //MessageBox.Show($"{msg}", $"{title}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            frmMessage.Show($"{msg}", $"{title}", MessageLevel.Error, true);
        }
        void ShowMsgBoxSuccess(string msg, string title)
        {
            //MessageBox.Show($"{msg}", $"{title}", MessageBoxButtons.OK, MessageBoxIcon.Information);
            frmMessage.Show($"{msg}", $"{title}", MessageLevel.Success, true);
        }

        /// <summary>
        /// Get Windows DPI percent scale.
        /// </summary>
        void DetermineWindowDPI()
        {
            using (var graphics = CreateGraphics())
            {
                SettingsManager.WindowsDPI = Utils.GetCurrentDPI(graphics);
            }
        }

        /// <summary>
        /// Uses the current OS theme for the list view (row highlight, hover, columns, etc).
        /// </summary>
        /// <param name="listView"><see cref="ListView"/></param>
        public static void SetListTheme(ListView listView)
        {
            //lvContents.Font = Utils.ConvertStringToFont($"{SettingsManager.WindowFontName},11.9999,Regular");
            Utils.SetWindowTheme(listView.Handle, "Explorer", null);
            Utils.SendMessage(listView.Handle, Utils.LVM_SETEXTENDEDLISTVIEWSTYLE, new IntPtr(Utils.LVS_EX_DOUBLEBUFFER), new IntPtr(Utils.LVS_EX_DOUBLEBUFFER));
        }
        #endregion
    }
}
