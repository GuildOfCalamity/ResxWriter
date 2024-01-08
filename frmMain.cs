using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskBand;

namespace ResxWriter
{
    public partial class frmMain : Form
    {
        #region [Props]
        readonly List<string> _commonDelimiters = new List<string> { ",", ";", "~", "TAB" };
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
            tbFilePath.Text = $"{Environment.CurrentDirectory}\\";
            SwitchButton(btnGenerateResx, ResxWriter.Properties.Resources.Button02);

            foreach (var delimiter in _commonDelimiters)
            {
                cbDelimiters.Items.Add(delimiter);
                _userDelimiter = delimiter;
            }

            if (cbDelimiters.Items.Count > 0)
                cbDelimiters.SelectedIndex = 0;

            Logger.Instance.OnDebug += (msg) => { System.Diagnostics.Debug.WriteLine($"{msg}"); };
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

                    AddValuesToTextBox(_userValues, tbContents);

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

        void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #region [Helper Methods]
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
        void AddValuesToTextBox(Dictionary<string, string> dictionary, TextBox textBox)
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
                //btn.Invalidate();
                btn.Refresh();
            }
        }

        /// <summary>
        /// Thread-safe method
        /// </summary>
        /// <param name="ctrl"><see cref="System.Windows.Forms.CheckBox"/></param>
        /// <param name="state">true=checked, false=unchecked</param>
        public void ToggleCheckBox(System.Windows.Forms.CheckBox ctrl, bool state)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => ToggleCheckBox(ctrl, state)));
            else
                ctrl.Checked = state;
        }

        /// <summary>
        /// Thread-safe method
        /// </summary>
        /// <param name="ctrl"><see cref="Control"/></param>
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
        /// <param name="state"><see cref="FormWindowState"/></param>
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
        /// Returns the declaring type's namespace.
        /// </summary>
        public static string GetCurrentNamespace()
        {
            return System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Namespace;
        }

        /// <summary>
        /// Returns the declaring type's assembly name.
        /// </summary>
        public static string GetCurrentAssemblyName()
        {
            //return System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly.FullName;
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        }

        /// <summary>
        /// Returns the AssemblyVersion, not the FileVersion.
        /// </summary>
        public static Version GetCurrentAssemblyVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? new Version();
        }
        #endregion

    }
}
