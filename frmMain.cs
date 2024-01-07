using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using System.Xml;

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
            textContentTextBox.Text = $"{Environment.CurrentDirectory}\\";
            SwitchButton(btnGenerateResx, global::ResxWriter.Properties.Resources.Button02);

            foreach (var delimiter in _commonDelimiters)
            {
                cbDelimiters.Items.Add(delimiter);
                _userDelimiter = delimiter;
            }

            if (cbDelimiters.Items.Count > 0)
                cbDelimiters.SelectedIndex = 0;
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
                        textContentTextBox.Text = filePath;

                        // Read the UTF-8 text file content.
                        //string fileContent = File.ReadAllText(filePath, Encoding.UTF8);

                    }
                    catch (Exception ex)
                    {
                        UpdateStatus(_genericError);
                        MessageBox.Show($"Could not read the file.\r\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    SwitchButton(btnGenerateResx, global::ResxWriter.Properties.Resources.Button02);
                    UpdateStatus("File selection was canceled.");
                }
            }
        }


        /// <summary>
        /// Import the file contents.
        /// </summary>
        void btnImport_Click(object sender, EventArgs e)
        {
            if (textContentTextBox.Text.Length > 0)
            {
                try
                {
                    // Read the UTF-8 text file content.
                    //string fileContent = File.ReadAllText(filePath, Encoding.UTF8);

                    if (_userDelimiter.Equals("TAB"))
                        _userDelimiter = "\t";

                    _userValues = ReadDelimitedFile(textContentTextBox.Text, _userDelimiter[0]);

                    UpdateStatus($"File data has been loaded.  {_userValues.Count} items total.");
                    
                    if (_userValues.Count > 0)
                        SwitchButton(btnGenerateResx, global::ResxWriter.Properties.Resources.Button01);
                    else
                        SwitchButton(btnGenerateResx, global::ResxWriter.Properties.Resources.Button02);

                    AddValuesToTextBox(_userValues, tbContents);
                }
                catch (Exception ex)
                {
                    UpdateStatus(_genericError);
                    SwitchButton(btnGenerateResx, global::ResxWriter.Properties.Resources.Button02);
                    MessageBox.Show($"Could not read the file.\r\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                SwitchButton(btnGenerateResx, global::ResxWriter.Properties.Resources.Button02);
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
                saveFileDialog.FileName = $"{Path.GetFileNameWithoutExtension(textContentTextBox.Text)}.resx";

                if (_userValues.Count == 0)
                {
                    UpdateStatus("Check that you have imported some valid data.");
                    SwitchButton(btnGenerateResx, global::ResxWriter.Properties.Resources.Button02);
                    MessageBox.Show("No valid delimited values to work with from the provided file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    }
                    catch (XmlException ex) // Handle XML-related exceptions.
                    {
                        UpdateStatus(_genericError);
                        MessageBox.Show($"Could not generate the resx file.\r\n{ex.Message}", "XML Process Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch (Exception ex)
                    {
                        UpdateStatus(_genericError);
                        MessageBox.Show($"Could not generate the resx file.\r\n{ex.Message}", "Process Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show($"Error reading the file.\r\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show($"Error reading the resx file.\r\n{ex.Message}", "XML Process Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (SystemException ex)
            {
                UpdateStatus(_genericError);
                MessageBox.Show($"Error reading the resx file.\r\n{ex.Message}", "System Process Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                UpdateStatus(_genericError);
                MessageBox.Show($"Error reading the resx file.\r\n{ex.Message}", "Process Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        public void ClearTextbox()
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => ClearTextbox()));
            else
                tbContents.Text = string.Empty;
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
                btn.Image = img;
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
        #endregion

    }
}
