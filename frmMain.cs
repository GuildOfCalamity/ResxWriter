using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ResxWriter
{
    /// <summary>
    /// TODO: Superclass a <see cref="ListView"/> for transparency.
    /// </summary>
    public partial class frmMain : Form
    {
        #region [Props]
        readonly List<string> _commonDelimiters = new List<string> { ",", ";", "~", "|", "TAB" };
        Dictionary<string, string> _userValues = new Dictionary<string, string>();
        string _userDelimiter = string.Empty;
        string _passedArg = string.Empty;
        string _genericError = "An error was detected.";
        bool _useMeta = false;
        bool _closing = false;
        System.Drawing.Font _font = new System.Drawing.Font("Calibri", 13);
        System.Drawing.Pen _pen = new System.Drawing.Pen(System.Drawing.Color.Red, 2.0F);
        System.Windows.Forms.ErrorProvider _pathErrorProvider;
        DateTime _lastChange = DateTime.MinValue;
        ValueStopwatch _vsw = ValueStopwatch.StartNew();
        #endregion

        #region [Animation]
        Image _backgroundImage;
        int _moveX = 2;
        int _moveY = 2;
        int _marginX = -1;
        int _marginY = -1;
        System.Windows.Forms.Timer _timer;
        Point _imagePosition;
        #endregion

        public frmMain()
        {
            InitializeComponent();
        }

        public frmMain(string[] args)
        {
            InitializeComponent();
            foreach (var a in args)
            { 
                _passedArg = $"{a}"; 
            }

            // Test for adding explorer right-click context:
            if (_passedArg.Length > 0 && _passedArg.Contains("shell-extension-add"))
                AddOpenWithOptionToExplorerShell(true);
            else if (_passedArg.Length > 0 && _passedArg.Contains("shell-extension-sub"))
                AddOpenWithOptionToExplorerShell(false);
        }

        #region [Event Methods]
        void frmMain_Shown(object sender, EventArgs e)
        {
            #region [Setup Tool Strip Menu Items]

            // The background color will not apply correctly, so we'll use this trick as a workaround.
            openLogToolStripMenuItem.BackgroundImage = ResxWriter.Properties.Resources.SB_Background;
            openLogToolStripMenuItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            openSettingsToolStripMenuItem.BackgroundImage = ResxWriter.Properties.Resources.SB_Background;
            openSettingsToolStripMenuItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;

            // Configure left side image for log.
            openLogToolStripMenuItem.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            openLogToolStripMenuItem.Image = ResxWriter.Properties.Resources.SB_DotOff;
            openLogToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // Configure left side image for settings.
            openSettingsToolStripMenuItem.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
            openSettingsToolStripMenuItem.Image = ResxWriter.Properties.Resources.SB_DotOff;
            openSettingsToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // Configure click events.
            toolStripSplitButton1.Click += new System.EventHandler(toolStripSplitButton_Click);
            openLogToolStripMenuItem.Click += new System.EventHandler(openLogToolStripMenuItem_Click);
            openSettingsToolStripMenuItem.Click += new System.EventHandler(openSettingsToolStripMenuItem_Click);
            #endregion

            #region [Setup ErrorProvider for FilePath]
            _pathErrorProvider = new System.Windows.Forms.ErrorProvider();
            _pathErrorProvider.SetIconAlignment(this.tbFilePath, ErrorIconAlignment.MiddleLeft);
            _pathErrorProvider.SetIconPadding(this.tbFilePath, 2);
            _pathErrorProvider.BlinkRate = 450;
            _pathErrorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            tbFilePath.Validating += new System.ComponentModel.CancelEventHandler(FilePathOnValidating);
            tbFilePath.TextChanged += new System.EventHandler(FilePathOnTextChanged);
            #endregion

            // Enable double buffering for the main form.
            this.DoubleBuffered = true;

            UpdateStatusBar("Click the folder icon to select a file and then click import.");
            SwitchButton(btnGenerateResx, ResxWriter.Properties.Resources.Button02);

            #region [Load settings]
            var lastPath = SettingsManager.LastPath;
            if (!string.IsNullOrEmpty(_passedArg))
                tbFilePath.Text = _passedArg;
            else if (!string.IsNullOrEmpty(lastPath))
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

            // Restore user's desired location.
            if (SettingsManager.WindowWidth != -1)
            {
                this.Top = SettingsManager.WindowTop;
                this.Left = SettingsManager.WindowLeft;
                this.Width = SettingsManager.WindowWidth;
                this.Height = SettingsManager.WindowHeight;
            }

            if (SettingsManager.RunAnimation)
            {
                // Create and configure the timer for our background animation.
                _backgroundImage = ResxWriter.Properties.Resources.App_Icon_png;
                _marginX = (int)(_backgroundImage.Width * 0.11) * -1;
                _marginY = (int)(_backgroundImage.Height * 0.09) * -1;
                _timer = new System.Windows.Forms.Timer();
                _timer.Interval = 40; // milliseconds
                _timer.Tick += TimerOnTick;
                _timer.Start();
            }

            // Have we saved a location that is not possible?
            CanWeFit(this, new Rectangle(30, 20, 900, 575));

            if (SettingsManager.MakeShortcut)
            {
                // ** Desktop Shortcut **
                if (!Utils.DoesShortcutExist(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name))
                    Utils.CreateApplicationShortcut(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, true);

                // ** StartMenu Shortcut **
                //if (!Utils.DoesShortcutExist(Environment.GetFolderPath(Environment.SpecialFolder.Programs), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name))
                //    Utils.CreateApplicationShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Programs), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, true);
            }

            //this.BackgroundImageLayout = ImageLayout.Stretch;
            //this.BackgroundImage = Image.FromFile(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Background.png"));

            //bool overlap = tbFilePath.CollidesWith(btnFileSelect);

            // Example of drawing using the the form's GDI drawing surface.
            //this.Paint += new System.Windows.Forms.PaintEventHandler(MainFormOnPaint);

            //UpdateStatusBar("[INFO] Some super long text to test margins and justification settings in the application so we can see where any issues might be with regards to visuals.");
        }

        /// <summary>
        /// If we can't find a screen to fit us, reset to the primary work area.
        /// </summary>
        void CanWeFit(Form form, Rectangle fallback)
        {
            if (!Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(form.Bounds)))
            {
                Debug.WriteLine($"We don't fit, defaulting {nameof(form)} bounds to {fallback}.");
                form.Bounds = fallback;
            }
        }

        #region [Simple Background Animation]
        /// <summary>
        /// Background floating image with bounce effect.
        /// </summary>
        void TimerOnTick(object sender, EventArgs e)
        {
            if (_backgroundImage == null || _closing)
                return;

            // Update the image position.
            _imagePosition.X += _moveX;
            _imagePosition.Y += _moveY;

            // Check X boundary.
            if (_imagePosition.X < _marginX || _imagePosition.X + _backgroundImage.Width > (this.ClientSize.Width + Math.Abs(_marginX)))
                _moveX = -_moveX;

            // Check Y boundary.
            if (_imagePosition.Y < _marginY || _imagePosition.Y + _backgroundImage.Height > (this.ClientSize.Height + Math.Abs(_marginY)))
                _moveY = -_moveY;

            // Force the form to redraw.
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_backgroundImage != null && !_closing)
            {
                //e.Graphics.DrawImage(_backgroundImage, _imagePosition.X, _imagePosition.Y, _backgroundImage.Width, _backgroundImage.Height);
                e.Graphics.DrawImage(_backgroundImage, _imagePosition);
            }
        }
        #endregion

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
                        UpdateStatusBar(_genericError);
                        Logger.Instance.Write($"Could not read the file: {ex.Message}", LogLevel.Error);
                        ShowMsgBoxError($"Could not read the file.\r\n{ex.Message}", "Error");
                    }
                }
                else
                {
                    SwitchButton(btnGenerateResx, ResxWriter.Properties.Resources.Button02);
                    UpdateStatusBar("File selection was canceled.");
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
                    //    UpdateStatusBar($"File does not exist, please check you path and try again.");
                    //    return;
                    //}

                    _userValues = ReadDelimitedFile(tbFilePath.Text, _userDelimiter[0]);


                    if (_userValues.Count > 0)
                    {
                        UpdateStatusBar($"File data has been loaded.  {_userValues.Count} items total.");
                        
                        if (_userValues.Count > 10)
                            tbContents.ScrollBars = ScrollBars.Vertical;

                        Task.Run(() => FlashButton(btnGenerateResx));
                    }
                    else
                    {
                        UpdateStatusBar($"Check your input file and try again.");
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
                    UpdateStatusBar(_genericError);
                    SwitchButton(btnGenerateResx, ResxWriter.Properties.Resources.Button02);
                    Logger.Instance.Write($"Could not read the file: {ex.Message}", LogLevel.Error);
                    ShowMsgBoxError($"Could not read the file.\r\n{ex.Message}", "Error");
                }
            }
            else
            {
                SwitchButton(btnGenerateResx, ResxWriter.Properties.Resources.Button02);
                UpdateStatusBar("Import was canceled.");
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
                    UpdateStatusBar("Check that you have imported some valid data.");
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

                        UpdateStatusBar("Resx file generated successfully.");
                        Logger.Instance.Write($"Resx file generated successfully: \"{filePath}\" ({_userValues.Count} items).", LogLevel.Success);
                        //ShowMsgBoxSuccess("Resx file generated successfully.", "Success");
                    }
                    catch (XmlException ex) // Handle XML-related exceptions.
                    {
                        UpdateStatusBar(_genericError);
                        Logger.Instance.Write($"Could not generate the resx file: {ex.Message}", LogLevel.Error);
                        ShowMsgBoxError($"Could not generate the resx file.\r\n{ex.Message}", "XML Process Error");
                    }
                    catch (Exception ex)
                    {
                        UpdateStatusBar(_genericError);
                        Logger.Instance.Write($"Could not generate the resx file: {ex.Message}", LogLevel.Error);
                        ShowMsgBoxError($"Could not generate the resx file.\r\n{ex.Message}", "Process Error");
                    }
                }
                else
                {
                    UpdateStatusBar("Export was canceled.");
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
            {
                _useMeta = cb.Checked;
                if (_useMeta)
                    cb.Text = "Items will be added as metadata";
                else
                    cb.Text = "Items will be added as resources";
            }
        }

        void cbDelimiters_TextUpdate(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cbDelimiters.Text))
            {
                _userDelimiter = cbDelimiters.Text;
                UpdateStatusBar($"Custom delimiter set to \"{_userDelimiter}\"");
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
                _closing = true;
                _timer?.Stop();

                if (this.WindowState == FormWindowState.Normal)
                {
                    SettingsManager.WindowLeft = this.Left;
                    SettingsManager.WindowTop = this.Top;
                    SettingsManager.WindowHeight = this.Height;
                    SettingsManager.WindowWidth = this.Width;
                }
                /* 
                  https://www.w3.org/TR/xml/#charsets
                  Special chars do not store well in XML, so we have a logic check here for 0x09/HTAB.
                  We could also use the escaped versions...
                      NL = &#x0A;
                      CR = &#x0D;
                     TAB = &#x09;
                   SPACE = &#x20;

                 If viewing in a web browser, then sometimes it's helpful to use CDATA...
                   <element><![CDATA[&#x09;]]></element>
                */
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

        /// <summary>
        /// When the text changes run our validation routine.
        /// </summary>
        void FilePathOnTextChanged(object sender, EventArgs e)
        {
            // There's some logic here to prevent hammering since
            // the File.Exists() call may take a few milliseconds.
            var diff = DateTime.Now - _lastChange;
            if (diff.TotalMilliseconds >= 20)
            {
                _vsw = ValueStopwatch.StartNew();
                if (!File.Exists(tbFilePath.Text))
                {
                    // Set the error with the text to display.
                    _pathErrorProvider.SetError(tbFilePath, "Current file path does not exist.");
                }
                else
                {
                    // Clear the error.
                    _pathErrorProvider.SetError(tbFilePath, String.Empty);
                }
                Debug.WriteLine($"FileCheck_ElapsedTime: {_vsw.GetElapsedTime().ToReadableString()}");
            }
            else // If a change happens too quickly we'll clear the error.
            {
                _pathErrorProvider.SetError(tbFilePath, String.Empty);
            }
            _lastChange = DateTime.Now;
        }

        /// <summary>
        /// When you change the focus by using the mouse or by calling the 
        /// Focus method, focus events occur in the following order:
        ///  1.) Enter
        ///  2.) GotFocus
        ///  3.) LostFocus
        ///  4.) Leave
        ///  5.) Validating
        ///  6.) Validated
        /// https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.control.validating?view=windowsdesktop-8.0#remarks
        /// </summary>
        void FilePathOnValidating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var tb = sender as TextBox;
            if (!File.Exists(tb.Text))
            {
                // Cancel the event.
                //e.Cancel = true; // This will prevent further actions until the issue is corrected.
                
                // Select the text to be corrected.
                tb.Select(0, tb.Text.Length);

                // Set the error with the text to display. 
                _pathErrorProvider.SetError(tb, "Current file path does not exist.");
            }
            else
            {
                // Clear the error.
                _pathErrorProvider.SetError(tb, String.Empty);
            }
        }

        /// <summary>
        /// No need to update animation if we're minimized.
        /// </summary>
        /// <remarks>
        /// This event will fire twice upon startup.
        /// </remarks>
        void frmMain_SizeChanged(object sender, EventArgs e)
        {
            if (_timer != null && _timer.Enabled && this.WindowState == FormWindowState.Minimized)
                _timer?.Stop();
            else if (_timer != null && !_timer.Enabled && SettingsManager.RunAnimation)
                _timer?.Start();

            // The justification offered from the designer does not work the way we want, so
            // we'll use this to keep our ToolStripSplitButton right-justified on the status bar.
            if (this.WindowState != FormWindowState.Minimized)
            {
                int sideBuffer = 80;
                var frm = sender as Form;
                var newWidth = frm.Width - sideBuffer;
                if (newWidth > sideBuffer)
                    sbStatusPanel.Width = newWidth;
            }
        }

        /// <summary>
        /// <see cref="ToolStripMenuItem"/> for opening the log file.
        /// </summary>
        void openLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tsmi = sender as ToolStripMenuItem;
            try
            {
                // Don't re-enter if already active.
                if (tsmi.Image.BytewiseCompare(ResxWriter.Properties.Resources.SB_DotOn))
                    return;

                // Update the image to indicate the process is in use.
                UpdateMenuItemImage(tsmi, ResxWriter.Properties.Resources.SB_DotOn);

                var p = Process.Start(Logger.Instance.LogPath);

                // This was not reliable.
                //p.Exited += (po, pe) => { tsmi.Image = ResxWriter.Properties.Resources.SB_DotOff; };

                Task.Run(() =>
                {
                    while (!p.HasExited && !_closing) { Thread.Sleep(400); }
                    UpdateMenuItemImage(tsmi, ResxWriter.Properties.Resources.SB_DotOff);
                });
            }
            catch (Exception) { tsmi.Image = ResxWriter.Properties.Resources.SB_DotOff; }
        }

        /// <summary>
        /// <see cref="ToolStripMenuItem"/> for opening the settings file.
        /// </summary>
        void openSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tsmi = sender as ToolStripMenuItem;
            try
            {
                // Don't re-enter if already active.
                if (tsmi.Image.BytewiseCompare(ResxWriter.Properties.Resources.SB_DotOn))
                    return;

                // Update the image to indicate the process is in use.
                UpdateMenuItemImage(tsmi, ResxWriter.Properties.Resources.SB_DotOn);

                var p = Process.Start(SettingsManager.Location);

                // This was not reliable.
                //p.Exited += (po, pe) => { tsmi.Image = ResxWriter.Properties.Resources.SB_DotOff; };

                Task.Run(() =>
                {
                    while (!p.HasExited && !_closing) { Thread.Sleep(400); }
                    UpdateMenuItemImage(tsmi, ResxWriter.Properties.Resources.SB_DotOff);
                });
            }
            catch (Exception) { tsmi.Image = ResxWriter.Properties.Resources.SB_DotOff; }
        }


        /// <summary>
        /// This is the event for the tool strip button when clicked.
        /// This happens when the dropdown arrow is clicked or the main button is clicked.
        /// We will ignore this event since we want the user to select an item from the menu.
        /// </summary>
        void toolStripSplitButton_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("[INFO] ToolStripSplitButton Clicked");
        }

        /// <summary>
        /// https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.control.paint?view=windowsdesktop-8.0
        /// The Paint event is raised when the control is redrawn. It passes an instance of PaintEventArgs to the method(s)
        /// that handles the Paint event. When creating a new custom control or an inherited control with a different visual
        /// appearance, you must provide code to render the control by overriding the OnPaint method.
        /// </summary>
        void MainFormOnPaint(object sender, PaintEventArgs e)
        {
            //var frm = sender as Form;
            Graphics g = e.Graphics;

            // Place the text to the right of the ComboBox control.
            var x = cbDelimiters.Left + cbDelimiters.Width + 10;
            var y = cbDelimiters.Top + 1;

            // Draw message text.
            g.DrawString("Rendering text using the form's GDI drawing surface.", _font, System.Drawing.Brushes.Wheat, new Point(x, y));
            g.DrawEllipse(_pen, btnImport.Left + 2, btnImport.Top + 2, btnImport.Width - 4, btnImport.Height - 4);
            g.DrawLine(_pen, btnImport.Left + 2, btnImport.Top + 2, btnImport.Right - 2, btnImport.Bottom - 2);
            g.DrawLine(_pen, btnImport.Right - 2, btnImport.Top + 2, btnImport.Left + 2, btnImport.Bottom - 2);
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
                UpdateStatusBar(_genericError);
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
                UpdateStatusBar(_genericError);
                ShowMsgBoxError($"Error reading the resx file.\r\n{ex.Message}", "XML Process Error");
            }
            catch (SystemException ex)
            {
                UpdateStatusBar(_genericError);
                ShowMsgBoxError($"Error reading the resx file.\r\n{ex.Message}", "System Process Error");
            }
            catch (Exception ex)
            {
                UpdateStatusBar(_genericError);
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

        /// <summary>
        /// Adds the provided <see cref="Dictionary{TKey, TValue}"/> to the <see cref="ListView"/>.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="lv"></param>
        void AddImportToListView(Dictionary<string, string> dictionary, ListView lv)
        {
            int idx = 0;
            ClearListView(lv);

            lv.BeginUpdate();
            foreach (var kvp in dictionary)
                AddToListView($"{kvp.Key}", $"{kvp.Value}", ++idx, lv);

            ResizeHeaders(lv);

            lv.EndUpdate();
        }

        /// <summary>
        /// Update the status label of the application.
        /// </summary>
        /// <remarks>Thread safe method</remarks>
        void UpdateStatusBar(string message)
        {
            stbStatus.InvokeIfRequired(() =>
            {
                sbStatusPanel.Text = message;
            });
        }

        /// <summary>
        /// Clear the text contents of a <see cref="TextBox"/> and sets the scrollbars property to none.
        /// </summary>
        /// <remarks>Thread safe method</remarks>
        public void ClearTextbox(TextBox tb)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => ClearTextbox(tb)));
            else
            {
                tb.Text = string.Empty;
                tb.ScrollBars = ScrollBars.None;
            }
        }

        /// <summary>
        /// Clears the items in a <see cref="ListView"/>.
        /// </summary>
        /// <remarks>Thread safe method</remarks>
        public void ClearListView(ListView lv)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => ClearListView(lv)));
            else
            {
                lv.Items.Clear();
            }
        }

        /// <summary>
        /// Changes the Enabled state of a <see cref="Control"/>.
        /// </summary>
        /// <param name="ctrl"><see cref="Control"/></param>
        /// <param name="state">true=enabled, false=disabled</param>
        /// <remarks>Thread safe method</remarks>
        public void ToggleControl(Control ctrl, bool state)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => ToggleControl(ctrl, state)));
            else
                ctrl.Enabled = state;
        }

        /// <summary>
        /// Sets the <see cref="Button"/>'s Image property.
        /// </summary>
        /// <param name="btn"><see cref="Button"/></param>
        /// <param name="img"><see cref="Bitmap"></param>
        /// <remarks>Thread safe method</remarks>
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
        /// Sets the <see cref="ToolStripMenuItem"/>'s Image property.
        /// </summary>
        /// <param name="mi"><see cref="ToolStripMenuItem"/></param>
        /// <param name="img"><see cref="Bitmap"></param>
        /// <remarks>Thread safe method</remarks>
        public void UpdateMenuItemImage(ToolStripMenuItem mi, Bitmap img)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => UpdateMenuItemImage(mi, img)));
            else
            {
                mi.Image = img;
                //this.Refresh();
                //mi.Invalidate();
            }
        }

        /// <summary>
        /// Changes the Checked state of a <see cref="CheckBox"/> control.
        /// </summary>
        /// <param name="ctrl"><see cref="System.Windows.Forms.CheckBox"/></param>
        /// <param name="state">true=checked, false=unchecked</param>
        /// <remarks>Thread safe method</remarks>
        public void ToggleCheckBox(CheckBox ctrl, bool state)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => ToggleCheckBox(ctrl, state)));
            else
                ctrl.Checked = state;
        }

        /// <summary>
        /// Updates a <see cref="Control"/>'s text property.
        /// </summary>
        /// <param name="ctrl"><see cref="System.Windows.Forms.Control"/></param>
        /// <param name="data">text for control</param>
        /// <remarks>Thread safe method</remarks>
        public void UpdateControl(Control ctrl, string data)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => UpdateControl(ctrl, data)));
            else
                ctrl.Text = data;
        }

        /// <summary>
        /// Updates the main window state.
        /// </summary>
        /// <param name="state"><see cref="System.Windows.Forms.FormWindowState"/></param>
        /// <remarks>Thread safe method</remarks>
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
        /// <remarks>Thread safe method</remarks>
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
        /// To adjust the width of the longest item in the column, set the Width property to -1.
        /// To autosize to the width of the column heading, set the Width property to -2.
        /// </summary>
        /// <param name="listView"><see cref="ListView"/></param>
        /// <remarks>Thread safe method</remarks>
        void ResizeHeaders(ListView listView)
        {
            try
            {
                if (InvokeRequired)
                    BeginInvoke(new Action(() => ResizeHeaders(listView)));
                else
                {
                    var cols = listView.Columns;
                    foreach (var col in cols)
                    {
                        var ch = col as ColumnHeader;
                        ch.Width = -2;
                    }
                }
            }
            catch (Exception) { }
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

            //SetListViewBackground(listView);
            //listView.BackgroundImage = ResxWriter.Properties.Resources.App_Icon_png;
        }

        /// <summary>
        /// Attach the current form background as an image to 
        /// <paramref name="lv"/> to fake transparency effect.
        /// </summary>
        /// <param name="lv"><see cref="ListView"/></param>
        /// <remarks>This is not a true transparent effect.</remarks>
        public static void SetListViewBackground(ListView lv, int x_offset = 8, int y_offset = 32)
        {
            try
            {
                int alpha = 32;
                Point p1 = lv.Parent.PointToScreen(lv.Location);
                Point p2 = lv.PointToScreen(Point.Empty);
                p2.Offset(-p1.X, -p1.Y);
                if (lv.BackgroundImage != null)
                    lv.BackgroundImage.Dispose(); // remove previous image
                lv.Hide();
                Bitmap bmp = new Bitmap(lv.Parent.Width, lv.Parent.Height);
                lv.Parent.DrawToBitmap(bmp, lv.Parent.ClientRectangle);
                Rectangle r = lv.Bounds;
                r.Offset(p2.X + x_offset, p2.Y + y_offset);
                bmp = bmp.Clone(r, PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    using (SolidBrush br = new SolidBrush(Color.FromArgb(alpha, lv.BackColor)))
                    {
                        g.FillRectangle(br, lv.ClientRectangle);
                    }
                }
                lv.BackgroundImage = bmp;
                lv.Show();
            }
            catch (OutOfMemoryException) 
            {
                Debug.WriteLine($"[WARNING] Check your x_offset or y_offset values.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] SetListViewBackground: {ex.Message}");
            }
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
        /// Self-reference for extracting the application icon.
        /// </summary>
        /// <returns><see cref="Icon"/></returns>
        Icon GetApplicationIcon() => Utils.GetFileIcon(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".exe", true);

        /// <summary>
        /// Test method for adding a right-click option to the explorer shell.
        /// </summary>
        void AddOpenWithOptionToExplorerShell(bool addToShell)
        {
            string path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), $"AddToShell.exe");
            try
            {
                if (File.Exists(path))
                {
                    string argPath = string.Format("\"{0}\"", Application.ExecutablePath);
                    string explorerText = $"\"Open using {Assembly.GetExecutingAssembly().GetName().Name}...\"";
                    string args = string.Format("\"{0}\" {1} {2}", addToShell, argPath, explorerText);
                    Utils.AttemptPrivilegeEscalation(path, args, false);
                }
                else
                {
                    Logger.Instance.Write($"An error occurred trying to find the utility.", LogLevel.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Write($"An error occurred trying to call the privilege escaltion to set the right click search option: {ex.Message}", LogLevel.Error);
            }
        }
        #endregion
    }
}
