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
using System.Web;
using System.Web.Routing;
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
        // Used for testing the ImageComboBox control.
        readonly Dictionary<string,string> _delimiters = new Dictionary<string,string> 
        { 
            { "Comma ,", "," }, 
            { "Semicolon ;", ";" }, 
            { "Tilde ~", "~" }, 
            { "Pipe |", "|" }, 
            { "Tab \\t", "\t" } 
        };
        Dictionary<string, string> _userValues = new Dictionary<string, string>();
        string _userDelimiter = string.Empty;
        string _passedArg = string.Empty;
        string _genericError = "An error was detected.";
        bool _useMeta = false;
        bool _outputJS = false;
        bool _closing = false;
        bool _showWarning = false;
        Color _clrWarning = Color.FromArgb(240, 180, 0);
        System.Drawing.Font _warningFont = new System.Drawing.Font("Calibri", 16);
        System.Drawing.Font _standardFont = new System.Drawing.Font("Calibri", 13);
        System.Drawing.Pen _pen = new System.Drawing.Pen(System.Drawing.Color.Red, 2.0F);
        System.Windows.Forms.ErrorProvider _pathErrorProvider;
        DateTime _lastChange = DateTime.MinValue;
        ValueStopwatch _vsw = ValueStopwatch.StartNew();
        static Graphics _formPainter = null;
        static RegistryMonitor _regMon = null;
        static Process _logProcess = null;
        static Process _settingsProcess = null;
        int _codePage = 1252;
        Dictionary<string, string> _glyphs = new Dictionary<string, string>();
        #endregion

        #region [Animation]
        Image _backgroundImage;
        Point _imagePosition;
        System.Windows.Forms.Timer _timer;
        int _moveX = 2;
        int _moveY = 2;
        int _marginX = -1;
        int _marginY = -1;
        #endregion

        public frmMain()
        {
            InitializeComponent();
        }

        public frmMain(string[] args)
        {
            InitializeComponent();

            foreach (var a in args) { _passedArg = $"{a}"; }

            // Test for adding explorer right-click context:
            if (_passedArg.Length > 0 && _passedArg.Contains("shell-extension-add"))
                AddOpenWithOptionToExplorerShell(true);
            else if (_passedArg.Length > 0 && _passedArg.Contains("shell-extension-rem"))
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

            Logger.Instance.OnDebug += (msg) => { Debug.WriteLine($"{msg}"); };

            UpdateStatusBar("Click the folder icon to select a file and then click import.");
            SwitchButtonImage(btnGenerateResx, ResxWriter.Properties.Resources.Button02);

            #region [Load and apply settings]
            var lastPath = SettingsManager.LastPath;
            if (!string.IsNullOrEmpty(_passedArg))
                tbFilePath.Text = _passedArg;
            else if (!string.IsNullOrEmpty(lastPath))
                tbFilePath.Text = lastPath;
            else
                tbFilePath.Text = $"{Environment.CurrentDirectory}\\";

            tbCodePage.Text = $"{SettingsManager.CodePage}";

            var lastDelimiter = SettingsManager.LastDelimiter;

            // Restore user's desired location.
            if (SettingsManager.WindowWidth != -1)
            {
                this.Top = SettingsManager.WindowTop;
                this.Left = SettingsManager.WindowLeft;
                this.Width = SettingsManager.WindowWidth;
                this.Height = SettingsManager.WindowHeight;
            }

            // Create and configure timer for background animation.
            if (SettingsManager.RunAnimation)
            {
                _backgroundImage = ResxWriter.Properties.Resources.App_Icon_png;
                _marginX = (int)(_backgroundImage.Width * 0.11) * -1;
                _marginY = (int)(_backgroundImage.Height * 0.09) * -1;
                _timer = new System.Windows.Forms.Timer();
                _timer.Interval = 40; // milliseconds
                _timer.Tick += TimerOnTick;
                _timer.Start();
            }

            // Check for application shortcut.
            if (SettingsManager.MakeShortcut)
            {
                #region [Desktop Shortcut]
                if (!Utils.DoesShortcutExist(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name))
                    Utils.CreateApplicationShortcut(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, true);
                #endregion

                #region [StartMenu Shortcut]
                //if (!Utils.DoesShortcutExist(Environment.GetFolderPath(Environment.SpecialFolder.Programs), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name))
                //    Utils.CreateApplicationShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Programs), System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, true);
                #endregion
            }
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
            SetListTheme(lvContents);

            // Have we saved a location that is not possible?
            CanWeFit(this, new Rectangle(30, 20, 900, 575));

            //this.BackgroundImageLayout = ImageLayout.Stretch;
            //this.BackgroundImage = Image.FromFile(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "Background.png"));

            //bool overlap = tbFilePath.CollidesWith(btnFileSelect);

            // Example of drawing using the the form's GDI drawing surface.
            //this.Paint += new System.Windows.Forms.PaintEventHandler(MainFormOnPaint);

            //UpdateStatusBar("[INFO] Some super long text to test margins and justification settings in the application so we can see where any issues might be with regards to visuals.");

            #region [Log Application Dependencies]
            //var procDeps = Utils.GetProcessDependencies();
            var refAssems = Utils.GetReferencedAssemblies();
            Logger.Instance.Write($"Runtime assembly list:", LogLevel.Debug);
            foreach (KeyValuePair<string, Version> assem in refAssems)
                Logger.Instance.Write($"{assem.Key} v{assem.Value}", LogLevel.Debug);
            #endregion

            #region [Registry Monitoring]
            // Create and setup the registry monitor to detect the Windows Theme setting.
            //regMon = new RegistryMonitor(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize") 
            //{
            //    RegChangeNotifyFilter = RegChangeNotifyFilter.Value
            //};
            //regMon.RegChanged += (obj, rea) => { Debug.WriteLine("[INFO] System Theme Registry Value Changed."); };
            //regMon.Error += (obj, rea) => { Debug.WriteLine("[WARNING] System Theme Registry Value Error."); };
            //regMon.Start();
            #endregion

            #region [Test ImageComboBox control]
            //ImageComboBox imageComboBox = new ImageComboBox(
            //    Color.FromArgb(250, 250, 250), 
            //    Color.FromArgb(20, 20, 20), 
            //    new Size(170, 80), 
            //    new Size(24, 24), 
            //    new Font("Consolas", 12F, FontStyle.Regular, GraphicsUnit.Point, (byte)0));
            //imageComboBox.Left = lblDelims.Right + 80;
            //imageComboBox.Top = lblDelims.Top - 3;
            //foreach (var delimiter in _delimiters)
            //{
            //    imageComboBox.Items.Add(delimiter.Key);
            //    _userDelimiter = delimiter.Value;
            //    imageComboBox.ImageList.Images.Add(ResxWriter.Properties.Resources.CB_Delimiter);
            //    //imageComboBox.ImageList.Images.Add(Image.FromFile(".\\Assets\\CB_Delimiter.png"));
            //}
            //imageComboBox.SelectedIndex = 0;
            //this.Controls.Add(imageComboBox);
            //imageComboBox.ItemSelected += ImageComboBox_ItemSelected; // Subscribe to the custom event.
            #endregion

            #region [Build Glyph Table]
            // Look-up table. Some values referenced from https://www.toptal.com/designers/htmlarrows/letters/
            _glyphs.Add("À", @"\u00C0"); // Letter A with grave
            _glyphs.Add("Á", @"\u00C1"); // Letter A with acute
            _glyphs.Add("Â", @"\u00C2"); // Letter A with circumflex
            _glyphs.Add("Ã", @"\u00C3"); // Letter A with tilde
            _glyphs.Add("Ä", @"\u00C4"); // Letter A with diaeresis
            _glyphs.Add("Å", @"\u00C5"); // Letter A with ring above
            _glyphs.Add("Æ", @"\u00C6"); // Ligature AE
            _glyphs.Add("Ç", @"\u00C7"); // Letter C with cedilla
            _glyphs.Add("È", @"\u00C8"); // Letter E with grave
            _glyphs.Add("É", @"\u00C9"); // Letter E with acute
            _glyphs.Add("Ê", @"\u00CA"); // Letter E with circumflex
            _glyphs.Add("Ë", @"\u00CB"); // Letter E with diaeresis
            _glyphs.Add("Ì", @"\u00CC"); // Letter I with grave
            _glyphs.Add("Í", @"\u00CD"); // Letter I with acute
            _glyphs.Add("Î", @"\u00CE"); // Letter I with circumflex
            _glyphs.Add("Ï", @"\u00CF"); // Letter I with diaeresis
            _glyphs.Add("Ð", @"\u00D0"); // Letter ETH
            _glyphs.Add("Ñ", @"\u00D1"); // Letter N with tilde
            _glyphs.Add("Ò", @"\u00D2"); // Letter O with grave
            _glyphs.Add("Ó", @"\u00D3"); // Letter O with acute
            _glyphs.Add("Ô", @"\u00D4"); // Letter O with circumflex
            _glyphs.Add("Õ", @"\u00D5"); // Letter O with tilde
            _glyphs.Add("Ö", @"\u00D6"); // Letter O with diaeresis
            _glyphs.Add("Ø", @"\u00D8"); // Letter O with stroke
            _glyphs.Add("Ù", @"\u00D9"); // Letter U with grave
            _glyphs.Add("Ú", @"\u00DA"); // Letter U with acute
            _glyphs.Add("Û", @"\u00DB"); // Letter U with circumflex
            _glyphs.Add("Ü", @"\u00DC"); // Letter U with diaeresis
            _glyphs.Add("Ý", @"\u00DD"); // Letter Y with acute
            _glyphs.Add("Þ", @"\u00DE"); // Letter THORN
            _glyphs.Add("ß", @"\u00DF"); // Letter sharp s = ess-zed
            _glyphs.Add("à", @"\u00E0"); // Letter a with grave
            _glyphs.Add("á", @"\u00E1"); // Letter a with acute
            _glyphs.Add("â", @"\u00E2"); // Letter a with circumflex
            _glyphs.Add("ã", @"\u00E3"); // Letter a with tilde
            _glyphs.Add("ä", @"\u00E4"); // Letter a with diaeresis
            _glyphs.Add("å", @"\u00E5"); // Letter a with ring above
            _glyphs.Add("æ", @"\u00E6"); // Ligature ae
            _glyphs.Add("ç", @"\u00E7"); // Letter c with cedilla
            _glyphs.Add("è", @"\u00E8"); // Letter e with grave
            _glyphs.Add("é", @"\u00E9"); // Letter e with acute
            _glyphs.Add("ê", @"\u00EA"); // Letter e with circumflex
            _glyphs.Add("ë", @"\u00EB"); // Letter e with diaeresis
            _glyphs.Add("ì", @"\u00EC"); // Letter i with grave
            _glyphs.Add("í", @"\u00ED"); // Letter i with acute
            _glyphs.Add("î", @"\u00EE"); // Letter i with circumflex
            _glyphs.Add("ï", @"\u00EF"); // Letter i with diaeresis
            _glyphs.Add("ð", @"\u00F0"); // Letter eth
            _glyphs.Add("ñ", @"\u00F1"); // Letter n with tilde
            _glyphs.Add("ò", @"\u00F2"); // Letter o with grave
            _glyphs.Add("ó", @"\u00F3"); // Letter o with acute
            _glyphs.Add("ô", @"\u00F4"); // Letter o with circumflex
            _glyphs.Add("õ", @"\u00F5"); // Letter o with tilde
            _glyphs.Add("ö", @"\u00F6"); // Letter o with diaeresis
            _glyphs.Add("ø", @"\u00F8"); // Letter o with stroke
            _glyphs.Add("ù", @"\u00F9"); // Letter u with grave
            _glyphs.Add("ú", @"\u00FA"); // Letter u with acute
            _glyphs.Add("û", @"\u00FB"); // Letter u with circumflex
            _glyphs.Add("ü", @"\u00FC"); // Letter u with diaeresis
            _glyphs.Add("ý", @"\u00FD"); // Letter y with acute
            _glyphs.Add("þ", @"\u00FE"); // Letter thorn
            _glyphs.Add("ÿ", @"\u00FF"); // Letter y with diaeresis
            _glyphs.Add("Ā", @"\u0100"); // Uppercase a With Macron                               
            _glyphs.Add("ā", @"\u0101"); // Lowercase a With Macron                               
            _glyphs.Add("Ă", @"\u0102"); // Uppercase a With Breve                                
            _glyphs.Add("ă", @"\u0103"); // Lowercase a With Breve                                
            _glyphs.Add("Ą", @"\u0104"); // Uppercase a With Ogonek                               
            _glyphs.Add("ą", @"\u0105"); // Lowercase a With Ogonek                               
            _glyphs.Add("Ć", @"\u0106"); // Uppercase C With Acute                                
            _glyphs.Add("ć", @"\u0107"); // Lowercase C With Acute                                
            _glyphs.Add("Ĉ", @"\u0108"); // Uppercase C With Circumflex                           
            _glyphs.Add("ĉ", @"\u0109"); // Lowercase C With Circumflex                           
            _glyphs.Add("Ċ", @"\u010A"); // Uppercase C With Dot Above                            
            _glyphs.Add("ċ", @"\u010B"); // Lowercase C With Dot Above                            
            _glyphs.Add("Č", @"\u010C"); // Uppercase C With Caron                                
            _glyphs.Add("č", @"\u010D"); // Lowercase C With Caron                                
            _glyphs.Add("Ď", @"\u010E"); // Uppercase D With Caron                                
            _glyphs.Add("ď", @"\u010F"); // Lowercase D With Caron                                
            _glyphs.Add("Đ", @"\u0110"); // Uppercase D With Stroke                               
            _glyphs.Add("đ", @"\u0111"); // Lowercase D With Stroke                               
            _glyphs.Add("Ē", @"\u0112"); // Uppercase E With Macron                               
            _glyphs.Add("ē", @"\u0113"); // Lowercase E With Macron                               
            _glyphs.Add("Ĕ", @"\u0114"); // Uppercase E With Breve                                
            _glyphs.Add("ĕ", @"\u0115"); // Lowercase E With Breve                                
            _glyphs.Add("Ė", @"\u0116"); // Uppercase E With Dot Above                            
            _glyphs.Add("ė", @"\u0117"); // Lowercase E With Dot Above                            
            _glyphs.Add("Ę", @"\u0118"); // Uppercase E With Ogonek                               
            _glyphs.Add("ę", @"\u0119"); // Lowercase E With Ogonek                               
            _glyphs.Add("Ě", @"\u011A"); // Uppercase E With Caron                                
            _glyphs.Add("ě", @"\u011B"); // Lowercase E With Caron                                
            _glyphs.Add("Ĝ", @"\u011C"); // Uppercase G With Circumflex                           
            _glyphs.Add("ĝ", @"\u011D"); // Lowercase G With Circumflex                           
            _glyphs.Add("Ğ", @"\u011E"); // Uppercase G With Breve                                
            _glyphs.Add("ğ", @"\u011F"); // Lowercase G With Breve                                
            _glyphs.Add("Ġ", @"\u0120"); // Uppercase G With Dot Above                            
            _glyphs.Add("ġ", @"\u0121"); // Lowercase G With Dot Above                            
            _glyphs.Add("Ģ", @"\u0122"); // Uppercase G With Cedilla                              
            _glyphs.Add("ģ", @"\u0123"); // Lowercase G With Cedilla                              
            _glyphs.Add("Ĥ", @"\u0124"); // Uppercase H With Circumflex                           
            _glyphs.Add("ĥ", @"\u0125"); // Lowercase H With Circumflex                           
            _glyphs.Add("Ħ", @"\u0126"); // Uppercase H With Stroke                               
            _glyphs.Add("ħ", @"\u0127"); // Lowercase H With Stroke                               
            _glyphs.Add("Ĩ", @"\u0128"); // Uppercase I With Tilde                                
            _glyphs.Add("ĩ", @"\u0129"); // Lowercase I With Tilde                                
            _glyphs.Add("Ī", @"\u012A"); // Uppercase I With Macron                               
            _glyphs.Add("ī", @"\u012B"); // Lowercase I With Macron                               
            _glyphs.Add("Ĭ", @"\u012C"); // Uppercase I With Breve                                
            _glyphs.Add("ĭ", @"\u012D"); // Lowercase I With Breve                                
            _glyphs.Add("Į", @"\u012E"); // Uppercase I With Ogonek                               
            _glyphs.Add("į", @"\u012F"); // Lowercase I With Ogonek                               
            _glyphs.Add("İ", @"\u0130"); // Uppercase I With Dot Above                            
            _glyphs.Add("ı", @"\u0131"); // Lowercase Dotless I                                   
            _glyphs.Add("Ĳ", @"\u0132"); // Latin Capital Ligature Ij                             
            _glyphs.Add("ĳ", @"\u0133"); // Latin Small Ligature Ij                               
            _glyphs.Add("Ĵ", @"\u0134"); // Uppercase J With Circumflex                           
            _glyphs.Add("ĵ", @"\u0135"); // Lowercase J With Circumflex                           
            _glyphs.Add("Ķ", @"\u0136"); // Uppercase K With Cedilla                              
            _glyphs.Add("ķ", @"\u0137"); // Lowercase K With Cedilla                              
            _glyphs.Add("ĸ", @"\u0138"); // Lowercase Kra                                         
            _glyphs.Add("Ĺ", @"\u0139"); // Uppercase L With Acute                                
            _glyphs.Add("ĺ", @"\u013A"); // Lowercase L With Acute                                
            _glyphs.Add("Ļ", @"\u013B"); // Uppercase L With Cedilla                              
            _glyphs.Add("ļ", @"\u013C"); // Lowercase L With Cedilla                              
            _glyphs.Add("Ľ", @"\u013D"); // Uppercase L With Caron                                
            _glyphs.Add("ľ", @"\u013E"); // Lowercase L With Caron                                
            _glyphs.Add("Ŀ", @"\u013F"); // Uppercase L With Middle Dot                           
            _glyphs.Add("ŀ", @"\u0140"); // Lowercase L With Middle Dot                           
            _glyphs.Add("Ł", @"\u0141"); // Uppercase L With Stroke                               
            _glyphs.Add("ł", @"\u0142"); // Lowercase L With Stroke                               
            _glyphs.Add("Ń", @"\u0143"); // Uppercase N With Acute                                
            _glyphs.Add("ń", @"\u0144"); // Lowercase N With Acute                                
            _glyphs.Add("Ņ", @"\u0145"); // Uppercase N With Cedilla                              
            _glyphs.Add("ņ", @"\u0146"); // Lowercase N With Cedilla                              
            _glyphs.Add("Ň", @"\u0147"); // Uppercase N With Caron                                
            _glyphs.Add("ň", @"\u0148"); // Lowercase N With Caron                                
            _glyphs.Add("ŉ", @"\u0149"); // Lowercase N Preceded by Apostrophe                    
            _glyphs.Add("Ŋ", @"\u014A"); // Uppercase Eng                                         
            _glyphs.Add("ŋ", @"\u014B"); // Lowercase Eng                                         
            _glyphs.Add("Ō", @"\u014C"); // Uppercase O With Macron                               
            _glyphs.Add("ō", @"\u014D"); // Lowercase O With Macron                               
            _glyphs.Add("Ŏ", @"\u014E"); // Uppercase O With Breve                                
            _glyphs.Add("ŏ", @"\u014F"); // Lowercase O With Breve                                
            _glyphs.Add("Ő", @"\u0150"); // Uppercase O With Double Acute                         
            _glyphs.Add("ő", @"\u0151"); // Lowercase O With Double Acute                         
            _glyphs.Add("Œ", @"\u0152"); // Uppercase Ligature OE                                 
            _glyphs.Add("œ", @"\u0153"); // Lowercase Ligature oe                                 
            _glyphs.Add("Ŕ", @"\u0154"); // Uppercase R With Acute                                
            _glyphs.Add("ŕ", @"\u0155"); // Lowercase R With Acute                                
            _glyphs.Add("Ŗ", @"\u0156"); // Uppercase R With Cedilla                              
            _glyphs.Add("ŗ", @"\u0157"); // Lowercase R With Cedilla                              
            _glyphs.Add("Ř", @"\u0158"); // Uppercase R With Caron                                
            _glyphs.Add("ř", @"\u0159"); // Lowercase R With Caron                                
            _glyphs.Add("Ś", @"\u015A"); // Uppercase S With Acute                                
            _glyphs.Add("ś", @"\u015B"); // Lowercase S With Acute                                
            _glyphs.Add("Ŝ", @"\u015C"); // Uppercase S With Circumflex                           
            _glyphs.Add("ŝ", @"\u015D"); // Lowercase S With Circumflex                           
            _glyphs.Add("Ş", @"\u015E"); // Uppercase S With Cedilla                              
            _glyphs.Add("ş", @"\u015F"); // Lowercase S With Cedilla                              
            _glyphs.Add("Š", @"\u0160"); // Uppercase S With Caron                                
            _glyphs.Add("š", @"\u0161"); // Lowercase S With Caron
            _glyphs.Add("Ţ", @"\u0162"); // Uppercase T With Cedilla                              
            _glyphs.Add("ţ", @"\u0163"); // Lowercase T With Cedilla                              
            _glyphs.Add("Ť", @"\u0164"); // Uppercase T With Caron                                
            _glyphs.Add("ť", @"\u0165"); // Lowercase T With Caron                                
            _glyphs.Add("Ŧ", @"\u0166"); // Uppercase T With Stroke                               
            _glyphs.Add("ŧ", @"\u0167"); // Lowercase T With Stroke                               
            _glyphs.Add("Ũ", @"\u0168"); // Uppercase U With Tilde                                
            _glyphs.Add("ũ", @"\u0169"); // Lowercase U With Tilde                                
            _glyphs.Add("Ū", @"\u016A"); // Uppercase U With Macron                               
            _glyphs.Add("ū", @"\u016B"); // Lowercase U With Macron                               
            _glyphs.Add("Ŭ", @"\u016C"); // Uppercase U With Breve                                
            _glyphs.Add("ŭ", @"\u016D"); // Lowercase U With Breve                                
            _glyphs.Add("Ů", @"\u016E"); // Uppercase U With Ring Above                           
            _glyphs.Add("ů", @"\u016F"); // Lowercase U With Ring Above                           
            _glyphs.Add("Ű", @"\u0170"); // Uppercase U With Double Acute                         
            _glyphs.Add("ű", @"\u0171"); // Lowercase U With Double Acute                         
            _glyphs.Add("Ų", @"\u0172"); // Uppercase U With Ogonek                               
            _glyphs.Add("ų", @"\u0173"); // Lowercase U With Ogonek                               
            _glyphs.Add("Ŵ", @"\u0174"); // Uppercase W With Circumflex                           
            _glyphs.Add("ŵ", @"\u0175"); // Lowercase W With Circumflex                           
            _glyphs.Add("Ŷ", @"\u0176"); // Uppercase Y With Circumflex                           
            _glyphs.Add("ŷ", @"\u0177"); // Lowercase Y With Circumflex                           
            _glyphs.Add("Ÿ", @"\u0178"); // Uppercase Y With Diaeresis                            
            _glyphs.Add("Ź", @"\u0179"); // Uppercase Z With Acute                                
            _glyphs.Add("ź", @"\u017A"); // Lowercase Z With Acute                                
            _glyphs.Add("Ż", @"\u017B"); // Uppercase Z With Dot Above                            
            _glyphs.Add("ż", @"\u017C"); // Lowercase Z With Dot Above                            
            _glyphs.Add("Ž", @"\u017D"); // Uppercase Z With Caron                                
            _glyphs.Add("ž", @"\u017E"); // Lowercase Z With Caron
            _glyphs.Add("ſ", @"\u017F"); // Lowercase Long S                              
            _glyphs.Add("ƀ", @"\u0180"); // Lowercase B With Stroke                       
            _glyphs.Add("Ɖ", @"\u0189"); // Uppercase African D                           
            _glyphs.Add("Ɗ", @"\u018A"); // Uppercase D With Hook                         
            _glyphs.Add("Ƌ", @"\u018B"); // Uppercase D With Topbar                       
            _glyphs.Add("ƌ", @"\u018C"); // Lowercase D With Topbar                       
            _glyphs.Add("ƍ", @"\u018D"); // Lowercase Turned Delta                        
            _glyphs.Add("Ǝ", @"\u018E"); // Uppercase Reversed E                          
            _glyphs.Add("Ə", @"\u018F"); // Uppercase Schwa                               
            _glyphs.Add("Ɛ", @"\u0190"); // Uppercase Open E                              
            _glyphs.Add("Ƒ", @"\u0191"); // Uppercase F With Hook                         
            _glyphs.Add("ƒ", @"\u0192"); // Lowercase F With Hook                         
            _glyphs.Add("Ɠ", @"\u0193"); // Uppercase G With Hook                         
            _glyphs.Add("Ɣ", @"\u0194"); // Uppercase Gamma                               
            _glyphs.Add("ƕ", @"\u0195"); // Lowercase Hv                                  
            _glyphs.Add("Ɩ", @"\u0196"); // Uppercase Iota                                
            _glyphs.Add("Ɨ", @"\u0197"); // Uppercase I With Stroke                       
            _glyphs.Add("Ƙ", @"\u0198"); // Uppercase K With Hook                         
            _glyphs.Add("ƙ", @"\u0199"); // Lowercase K With Hook                         
            _glyphs.Add("ƚ", @"\u019A"); // Lowercase L With Bar                          
            _glyphs.Add("ƛ", @"\u019B"); // Lowercase Lambda With Stroke                  
            _glyphs.Add("Ɯ", @"\u019C"); // Uppercase Turned M                            
            _glyphs.Add("Ɲ", @"\u019D"); // Uppercase N With Left Hook                    
            _glyphs.Add("ƞ", @"\u019E"); // Lowercase N With Long Right Leg               
            _glyphs.Add("Ɵ", @"\u019F"); // Uppercase O With Middle Tilde                 
            _glyphs.Add("Ơ", @"\u01A0"); // Uppercase O With Horn                         
            _glyphs.Add("ơ", @"\u01A1"); // Lowercase O With Horn                         
            _glyphs.Add("Ƣ", @"\u01A2"); // Uppercase Oi                                  
            _glyphs.Add("ƣ", @"\u01A3"); // Lowercase Oi                                  
            _glyphs.Add("Ƥ", @"\u01A4"); // Uppercase P With Hook                         
            _glyphs.Add("ƥ", @"\u01A5"); // Lowercase P With Hook                         
            _glyphs.Add("Ʀ", @"\u01A6"); // Latin Letter Yr                               
            _glyphs.Add("Ƨ", @"\u01A7"); // Uppercase Tone Two                            
            _glyphs.Add("ƨ", @"\u01A8"); // Lowercase Tone Two                            
            _glyphs.Add("Ʃ", @"\u01A9"); // Uppercase Esh                                 
            _glyphs.Add("ƪ", @"\u01AA"); // Latin Letter Reversed Esh Loop                
            _glyphs.Add("ƫ", @"\u01AB"); // Lowercase T With Palatal Hook                 
            _glyphs.Add("Ƭ", @"\u01AC"); // Uppercase T With Hook                         
            _glyphs.Add("ƭ", @"\u01AD"); // Lowercase T With Hook                         
            _glyphs.Add("Ʈ", @"\u01AE"); // Uppercase T With Retroflex Hook               
            _glyphs.Add("Ư", @"\u01AF"); // Uppercase U With Horn                         
            _glyphs.Add("ư", @"\u01B0"); // Lowercase U With Horn                         
            _glyphs.Add("Ʊ", @"\u01B1"); // Uppercase Upsilon                             
            _glyphs.Add("Ʋ", @"\u01B2"); // Uppercase v With Hook                         
            _glyphs.Add("Ƴ", @"\u01B3"); // Uppercase Y With Hook                         
            _glyphs.Add("ƴ", @"\u01B4"); // Lowercase Y With Hook                         
            _glyphs.Add("Ƶ", @"\u01B5"); // Uppercase Z With Stroke                       
            _glyphs.Add("ƶ", @"\u01B6"); // Lowercase Z With Stroke                       
            _glyphs.Add("Ʒ", @"\u01B7"); // Uppercase Ezh                                 
            _glyphs.Add("Ƹ", @"\u01B8"); // Uppercase Ezh Reversed                        
            _glyphs.Add("ƹ", @"\u01B9"); // Lowercase Ezh Reversed                        
            _glyphs.Add("ƺ", @"\u01BA"); // Lowercase Ezh With Tail                       
            _glyphs.Add("ƻ", @"\u01BB"); // Latin Letter Two With Stroke                  
            _glyphs.Add("Ƽ", @"\u01BC"); // Uppercase Tone Five                           
            _glyphs.Add("ƽ", @"\u01BD"); // Lowercase Tone Five                           
            _glyphs.Add("ƾ", @"\u01BE"); // Latin Letter Inverted Glottal Stop With Stroke
            _glyphs.Add("ƿ", @"\u01BF"); // Latin Letter Wynn                             
            _glyphs.Add("ǀ", @"\u01C0"); // Latin Letter Dental Click                     
            _glyphs.Add("ǁ", @"\u01C1"); // Latin Letter Lateral Click                    
            _glyphs.Add("ǂ", @"\u01C2"); // Latin Letter Alveolar Click                   
            _glyphs.Add("ǃ", @"\u01C3"); // Latin Letter Retroflex Click                  
            _glyphs.Add("Ǆ", @"\u01C4"); // Uppercase Dz With Caron                       
            _glyphs.Add("ǅ", @"\u01C5"); // Uppercase D With Small Letter Z With Caron    
            _glyphs.Add("ǆ", @"\u01C6"); // Lowercase Dz With Caron                       
            _glyphs.Add("Ǉ", @"\u01C7"); // Uppercase Lj                                  
            _glyphs.Add("ǈ", @"\u01C8"); // Uppercase L With Small Letter J               
            _glyphs.Add("ǉ", @"\u01C9"); // Lowercase Lj                                  
            _glyphs.Add("Ǌ", @"\u01CA"); // Uppercase Nj                                  
            _glyphs.Add("ǋ", @"\u01CB"); // Uppercase N With Small Letter J               
            _glyphs.Add("ǌ", @"\u01CC"); // Lowercase Nj                                  
            _glyphs.Add("Ǎ", @"\u01CD"); // Uppercase a With Caron                        
            _glyphs.Add("ǎ", @"\u01CE"); // Lowercase a With Caron                        
            _glyphs.Add("Ǐ", @"\u01CF"); // Uppercase I With Caron                        
            _glyphs.Add("ǐ", @"\u01D0"); // Lowercase I With Caron                        
            _glyphs.Add("Ǒ", @"\u01D1"); // Uppercase O With Caron                        
            _glyphs.Add("ǒ", @"\u01D2"); // Lowercase O With Caron                        
            _glyphs.Add("Ǔ", @"\u01D3"); // Uppercase U With Caron                        
            _glyphs.Add("ǔ", @"\u01D4"); // Lowercase U With Caron                        
            _glyphs.Add("Ǖ", @"\u01D5"); // Uppercase U With Diaeresis and Macron         
            _glyphs.Add("ǖ", @"\u01D6"); // Lowercase U With Diaeresis and Macron         
            _glyphs.Add("Ǘ", @"\u01D7"); // Uppercase U With Diaeresis and Acute          
            _glyphs.Add("ǘ", @"\u01D8"); // Lowercase U With Diaeresis and Acute          
            _glyphs.Add("Ǚ", @"\u01D9"); // Uppercase U With Diaeresis and Caron          
            _glyphs.Add("ǚ", @"\u01DA"); // Lowercase U With Diaeresis and Caron          
            _glyphs.Add("Ǜ", @"\u01DB"); // Uppercase U With Diaeresis and Grave          
            _glyphs.Add("ǜ", @"\u01DC"); // Lowercase U With Diaeresis and Grave          
            _glyphs.Add("ǝ", @"\u01DD"); // Lowercase Turned E                            
            _glyphs.Add("Ǟ", @"\u01DE"); // Uppercase a With Diaeresis and Macron         
            _glyphs.Add("ǟ", @"\u01DF"); // Lowercase a With Diaeresis and Macron         
            _glyphs.Add("Ǡ", @"\u01E0"); // Uppercase a With Dot Above and Macron         
            _glyphs.Add("ǡ", @"\u01E1"); // Lowercase a With Dot Above and Macron         
            _glyphs.Add("Ǣ", @"\u01E2"); // Uppercase Ae With Macron                      
            _glyphs.Add("ǣ", @"\u01E3"); // Lowercase Ae With Macron                      
            _glyphs.Add("Ǥ", @"\u01E4"); // Uppercase G With Stroke                       
            _glyphs.Add("ǥ", @"\u01E5"); // Lowercase G With Stroke                       
            _glyphs.Add("Ǧ", @"\u01E6"); // Uppercase G With Caron                        
            _glyphs.Add("ǧ", @"\u01E7"); // Lowercase G With Caron                        
            _glyphs.Add("Ǩ", @"\u01E8"); // Uppercase K With Caron                        
            _glyphs.Add("ǩ", @"\u01E9"); // Lowercase K With Caron                        
            _glyphs.Add("Ǫ", @"\u01EA"); // Uppercase O With Ogonek                       
            _glyphs.Add("ǫ", @"\u01EB"); // Lowercase O With Ogonek                       
            _glyphs.Add("Ǭ", @"\u01EC"); // Uppercase O With Ogonek and Macron            
            _glyphs.Add("ǭ", @"\u01ED"); // Lowercase O With Ogonek and Macron            
            _glyphs.Add("Ǯ", @"\u01EE"); // Uppercase Ezh With Caron                      
            _glyphs.Add("ǯ", @"\u01EF"); // Lowercase Ezh With Caron                      
            _glyphs.Add("ǰ", @"\u01F0"); // Lowercase J With Caron                        
            _glyphs.Add("Ǳ", @"\u01F1"); // Uppercase Dz                                  
            _glyphs.Add("ǲ", @"\u01F2"); // Uppercase D With Small Letter Z               
            _glyphs.Add("ǳ", @"\u01F3"); // Lowercase Dz                                  
            _glyphs.Add("Ǵ", @"\u01F4"); // Uppercase G With Acute                        
            _glyphs.Add("ǵ", @"\u01F5"); // Lowercase G With Acute                        
            _glyphs.Add("Ƕ", @"\u01F6"); // Uppercase Hwair                               
            _glyphs.Add("Ƿ", @"\u01F7"); // Uppercase Wynn                                
            _glyphs.Add("Ǹ", @"\u01F8"); // Uppercase N With Grave                        
            _glyphs.Add("ǹ", @"\u01F9"); // Lowercase N With Grave                        
            _glyphs.Add("Ǻ", @"\u01FA"); // Uppercase a With Ring Above and Acute         
            _glyphs.Add("ǻ", @"\u01FB"); // Lowercase a With Ring Above and Acute         
            _glyphs.Add("Ǽ", @"\u01FC"); // Uppercase Ae With Acute                       
            _glyphs.Add("ǽ", @"\u01FD"); // Lowercase Ae With Acute                       
            _glyphs.Add("Ǿ", @"\u01FE"); // Uppercase O With Stroke and Acute             
            _glyphs.Add("ǿ", @"\u01FF"); // Lowercase O With Stroke and Acute             
            _glyphs.Add("Ȁ", @"\u0200"); // Uppercase a With Double Grave                 
            _glyphs.Add("ȁ", @"\u0201"); // Lowercase a With Double Grave                 
            _glyphs.Add("Ȃ", @"\u0202"); // Uppercase a With Inverted Breve               
            _glyphs.Add("ȃ", @"\u0203"); // Lowercase a With Inverted Breve               
            _glyphs.Add("Ȅ", @"\u0204"); // Uppercase E With Double Grave                 
            _glyphs.Add("ȅ", @"\u0205"); // Lowercase E With Double Grave                 
            _glyphs.Add("Ȇ", @"\u0206"); // Uppercase E With Inverted Breve               
            _glyphs.Add("ȇ", @"\u0207"); // Lowercase E With Inverted Breve               
            _glyphs.Add("Ȉ", @"\u0208"); // Uppercase I With Double Grave                 
            _glyphs.Add("ȉ", @"\u0209"); // Lowercase I With Double Grave                 
            _glyphs.Add("Ȋ", @"\u020A"); // Uppercase I With Inverted Breve               
            _glyphs.Add("ȋ", @"\u020B"); // Lowercase I With Inverted Breve               
            _glyphs.Add("Ȍ", @"\u020C"); // Uppercase O With Double Grave                 
            _glyphs.Add("ȍ", @"\u020D"); // Lowercase O With Double Grave                 
            _glyphs.Add("Ȏ", @"\u020E"); // Uppercase O With Inverted Breve               
            _glyphs.Add("ȏ", @"\u020F"); // Lowercase O With Inverted Breve               
            _glyphs.Add("Ȑ", @"\u0210"); // Uppercase R With Double Grave                 
            _glyphs.Add("ȑ", @"\u0211"); // Lowercase R With Double Grave                 
            _glyphs.Add("Ȓ", @"\u0212"); // Uppercase R With Inverted Breve               
            _glyphs.Add("ȓ", @"\u0213"); // Lowercase R With Inverted Breve               
            _glyphs.Add("Ȕ", @"\u0214"); // Uppercase U With Double Grave                 
            _glyphs.Add("ȕ", @"\u0215"); // Lowercase U With Double Grave                 
            _glyphs.Add("Ȗ", @"\u0216"); // Uppercase U With Inverted Breve               
            _glyphs.Add("ȗ", @"\u0217"); // Lowercase U With Inverted Breve               
            _glyphs.Add("Ș", @"\u0218"); // Uppercase S With Comma Below                  
            _glyphs.Add("ș", @"\u0219"); // Lowercase S With Comma Below                  
            _glyphs.Add("Ț", @"\u021A"); // Uppercase T With Comma Below                  
            _glyphs.Add("ț", @"\u021B"); // Lowercase T With Comma Below                  
            _glyphs.Add("Ȝ", @"\u021C"); // Uppercase Yogh                                
            _glyphs.Add("ȝ", @"\u021D"); // Lowercase Yogh                                
            _glyphs.Add("Ȟ", @"\u021E"); // Uppercase H With Caron                        
            _glyphs.Add("ȟ", @"\u021F"); // Lowercase H With Caron                        
            _glyphs.Add("Ƞ", @"\u0220"); // Uppercase N With Long Right Leg               
            _glyphs.Add("ȡ", @"\u0221"); // Lowercase D With Curl                         
            _glyphs.Add("Ȣ", @"\u0222"); // Uppercase Ou                                  
            _glyphs.Add("ȣ", @"\u0223"); // Lowercase Ou                                  
            _glyphs.Add("Ȥ", @"\u0224"); // Uppercase Z With Hook                         
            _glyphs.Add("ȥ", @"\u0225"); // Lowercase Z With Hook                         
            _glyphs.Add("Ȧ", @"\u0226"); // Uppercase a With Dot Above                    
            _glyphs.Add("ȧ", @"\u0227"); // Lowercase a With Dot Above                    
            _glyphs.Add("Ȩ", @"\u0228"); // Uppercase E With Cedilla                      
            _glyphs.Add("ȩ", @"\u0229"); // Lowercase E With Cedilla                      
            _glyphs.Add("Ȫ", @"\u022A"); // Uppercase O With Diaeresis and Macron         
            _glyphs.Add("ȫ", @"\u022B"); // Lowercase O With Diaeresis and Macron         
            _glyphs.Add("Ȭ", @"\u022C"); // Uppercase O With Tilde and Macron             
            _glyphs.Add("ȭ", @"\u022D"); // Lowercase O With Tilde and Macron             
            _glyphs.Add("Ȯ", @"\u022E"); // Uppercase O With Dot Above                    
            _glyphs.Add("ȯ", @"\u022F"); // Lowercase O With Dot Above                    
            _glyphs.Add("Ȱ", @"\u0230"); // Uppercase O With Dot Above and Macron         
            _glyphs.Add("ȱ", @"\u0231"); // Lowercase O With Dot Above and Macron         
            _glyphs.Add("Ȳ", @"\u0232"); // Uppercase Y With Macron                       
            _glyphs.Add("ȳ", @"\u0233"); // Lowercase Y With Macron                       
            _glyphs.Add("ȴ", @"\u0234"); // Lowercase L With Curl                             
            _glyphs.Add("ȵ", @"\u0235"); // Lowercase N With Curl                             
            _glyphs.Add("ȶ", @"\u0236"); // Lowercase T With Curl                             
            _glyphs.Add("ȷ", @"\u0237"); // Lowercase Dotless J                               
            _glyphs.Add("ȸ", @"\u0238"); // Lowercase Db Digraph                              
            _glyphs.Add("ȹ", @"\u0239"); // Lowercase Qp Digraph                              
            _glyphs.Add("Ⱥ", @"\u023A"); // Uppercase a With Stroke                           
            _glyphs.Add("Ȼ", @"\u023B"); // Uppercase C With Stroke                           
            _glyphs.Add("ȼ", @"\u023C"); // Lowercase C With Stroke                           
            _glyphs.Add("Ƚ", @"\u023D"); // Uppercase L With Bar                              
            _glyphs.Add("Ⱦ", @"\u023E"); // Uppercase T With Diagonal Stroke                  
            _glyphs.Add("ȿ", @"\u023F"); // Lowercase S With Swash Tail                       
            _glyphs.Add("ɀ", @"\u0240"); // Lowercase Z With Swash Tail                       
            _glyphs.Add("Ɂ", @"\u0241"); // Uppercase Glottal Stop                            
            _glyphs.Add("ɂ", @"\u0242"); // Lowercase Glottal Stop                            
            _glyphs.Add("Ƀ", @"\u0243"); // Uppercase B With Stroke                           
            _glyphs.Add("Ʉ", @"\u0244"); // Uppercase U Bar                                   
            _glyphs.Add("Ʌ", @"\u0245"); // Uppercase Turned V                                
            _glyphs.Add("Ɇ", @"\u0246"); // Uppercase E With Stroke                           
            _glyphs.Add("ɇ", @"\u0247"); // Lowercase E With Stroke                           
            _glyphs.Add("Ɉ", @"\u0248"); // Uppercase J With Stroke                           
            _glyphs.Add("ɉ", @"\u0249"); // Lowercase J With Stroke                           
            _glyphs.Add("Ɋ", @"\u024A"); // Uppercase Small Q With Hook Tail                  
            _glyphs.Add("ɋ", @"\u024B"); // Lowercase Q With Hook Tail                        
            _glyphs.Add("Ɍ", @"\u024C"); // Uppercase R With Stroke                           
            _glyphs.Add("ɍ", @"\u024D"); // Lowercase R With Stroke                           
            _glyphs.Add("Ɏ", @"\u024E"); // Uppercase Y With Stroke                           
            _glyphs.Add("ɏ", @"\u024F"); // Lowercase Y With Stroke                           
            _glyphs.Add("ɐ", @"\u0250"); // Lowercase Turned A                                
            _glyphs.Add("ɑ", @"\u0251"); // Lowercase Alpha                                   
            _glyphs.Add("ɒ", @"\u0252"); // Lowercase Turned Alpha                            
            _glyphs.Add("ɓ", @"\u0253"); // Lowercase B With Hook                             
            _glyphs.Add("ɔ", @"\u0254"); // Lowercase Open O                                  
            _glyphs.Add("ɕ", @"\u0255"); // Lowercase C With Curl                             
            _glyphs.Add("ɖ", @"\u0256"); // Lowercase D With Tail                             
            _glyphs.Add("ɗ", @"\u0257"); // Lowercase D With Hook                             
            _glyphs.Add("ɘ", @"\u0258"); // Lowercase Reversed E                              
            _glyphs.Add("ə", @"\u0259"); // Lowercase Schwa                                   
            _glyphs.Add("ɚ", @"\u025A"); // Lowercase Schwa With Hook                         
            _glyphs.Add("ɛ", @"\u025B"); // Lowercase Open E                                  
            _glyphs.Add("ɜ", @"\u025C"); // Lowercase Reversed Open E                         
            _glyphs.Add("ɝ", @"\u025D"); // Lowercase Reversed Open E With Hook               
            _glyphs.Add("ɞ", @"\u025E"); // Lowercase Closed Reversed Open E                  
            _glyphs.Add("ɟ", @"\u025F"); // Lowercase Dotless J With Stroke                   
            _glyphs.Add("ɠ", @"\u0260"); // Lowercase G With Hook                             
            _glyphs.Add("ɡ", @"\u0261"); // Lowercase Script G                                
            _glyphs.Add("ɢ", @"\u0262"); // Latin Letter Small Capital G                      
            _glyphs.Add("ɣ", @"\u0263"); // Lowercase Gamma                                   
            _glyphs.Add("ɤ", @"\u0264"); // Lowercase Rams Horn                               
            _glyphs.Add("ɥ", @"\u0265"); // Lowercase Turned H                                
            _glyphs.Add("ɦ", @"\u0266"); // Lowercase H With Hook                             
            _glyphs.Add("ɧ", @"\u0267"); // Lowercase Heng With Hook                          
            _glyphs.Add("ɨ", @"\u0268"); // Lowercase I With Stroke                           
            _glyphs.Add("ɩ", @"\u0269"); // Lowercase Iota                                    
            _glyphs.Add("ɪ", @"\u026A"); // Latin Letter Small Capital I                      
            _glyphs.Add("ɫ", @"\u026B"); // Lowercase L With Middle Tilde                     
            _glyphs.Add("ɬ", @"\u026C"); // Lowercase L With Belt                             
            _glyphs.Add("ɭ", @"\u026D"); // Lowercase L With Retroflex Hook                   
            _glyphs.Add("ɮ", @"\u026E"); // Lowercase Lezh                                    
            _glyphs.Add("ɯ", @"\u026F"); // Lowercase Turned M                                
            _glyphs.Add("ɰ", @"\u0270"); // Lowercase Turned M With Long Leg                  
            _glyphs.Add("ɱ", @"\u0271"); // Lowercase M With Hook                             
            _glyphs.Add("ɲ", @"\u0272"); // Lowercase N With Left Hook                        
            _glyphs.Add("ɳ", @"\u0273"); // Lowercase N With Retroflex Hook                   
            _glyphs.Add("ɴ", @"\u0274"); // Latin Letter Small Capital N                      
            _glyphs.Add("ɵ", @"\u0275"); // Lowercase Barred O                                
            _glyphs.Add("ɶ", @"\u0276"); // Latin Letter Small Capital Oe                     
            _glyphs.Add("ɷ", @"\u0277"); // Lowercase Closed Omega                            
            _glyphs.Add("ɸ", @"\u0278"); // Lowercase Phi                                     
            _glyphs.Add("ɹ", @"\u0279"); // Lowercase Turned R                                
            _glyphs.Add("ɺ", @"\u027A"); // Lowercase Turned R With Long Leg                  
            _glyphs.Add("ɻ", @"\u027B"); // Lowercase Turned R With Hook                      
            _glyphs.Add("ɼ", @"\u027C"); // Lowercase R With Long Leg                         
            _glyphs.Add("ɽ", @"\u027D"); // Lowercase R With Tail                             
            _glyphs.Add("ɾ", @"\u027E"); // Lowercase R With Fishhook                         
            _glyphs.Add("ɿ", @"\u027F"); // Lowercase Reversed R With Fishhook                
            _glyphs.Add("ʀ", @"\u0280"); // Latin Letter Small Capital R                      
            _glyphs.Add("ʁ", @"\u0281"); // Latin Letter Small Capital Inverted R             
            _glyphs.Add("ʂ", @"\u0282"); // Lowercase S With Hook                             
            _glyphs.Add("ʃ", @"\u0283"); // Lowercase Esh                                     
            _glyphs.Add("ʄ", @"\u0284"); // Lowercase Dotless J With Stroke and Hook          
            _glyphs.Add("ʅ", @"\u0285"); // Lowercase Squat Reversed Esh                      
            _glyphs.Add("ʆ", @"\u0286"); // Lowercase Esh With Curl                           
            _glyphs.Add("ʇ", @"\u0287"); // Lowercase Turned T                                
            _glyphs.Add("ʈ", @"\u0288"); // Lowercase T With Retroflex Hook                   
            _glyphs.Add("ʉ", @"\u0289"); // Lowercase U Bar                                   
            _glyphs.Add("ʊ", @"\u028A"); // Lowercase Upsilon                                 
            _glyphs.Add("ʋ", @"\u028B"); // Lowercase v With Hook                             
            _glyphs.Add("ʌ", @"\u028C"); // Lowercase Turned V                                
            _glyphs.Add("ʍ", @"\u028D"); // Lowercase Turned W                                
            _glyphs.Add("ʎ", @"\u028E"); // Lowercase Turned Y                                
            _glyphs.Add("ʏ", @"\u028F"); // Latin Letter Small Capital Y                      
            _glyphs.Add("ʐ", @"\u0290"); // Lowercase Z With Retroflex Hook                   
            _glyphs.Add("ʑ", @"\u0291"); // Lowercase Z With Curl                             
            _glyphs.Add("ʒ", @"\u0292"); // Lowercase Ezh                                     
            _glyphs.Add("ʓ", @"\u0293"); // Lowercase Ezh With Curl                           
            _glyphs.Add("ʔ", @"\u0294"); // Latin Letter Glottal Stop                         
            _glyphs.Add("ʕ", @"\u0295"); // Latin Letter Pharyngeal Voiced Fricative          
            _glyphs.Add("ʖ", @"\u0296"); // Latin Letter Inverted Glottal Stop                
            _glyphs.Add("ʗ", @"\u0297"); // Latin Letter Stretched C                          
            _glyphs.Add("ʘ", @"\u0298"); // Latin Letter Bilabial Click                       
            _glyphs.Add("ʙ", @"\u0299"); // Latin Letter Small Capital B                      
            _glyphs.Add("ʚ", @"\u029A"); // Lowercase Closed Open E                           
            _glyphs.Add("ʛ", @"\u029B"); // Latin Letter Small Capital G With Hook            
            _glyphs.Add("ʜ", @"\u029C"); // Latin Letter Small Capital H                      
            _glyphs.Add("ʝ", @"\u029D"); // Lowercase J With Crossed-Tail                     
            _glyphs.Add("ʞ", @"\u029E"); // Lowercase Turned K                                
            _glyphs.Add("ʟ", @"\u029F"); // Latin Letter Small Capital L                      
            _glyphs.Add("ʠ", @"\u02A0"); // Lowercase Q With Hook                             
            _glyphs.Add("ʡ", @"\u02A1"); // Latin Letter Glottal Stop With Stroke             
            _glyphs.Add("ʢ", @"\u02A2"); // Latin Letter Reversed Glottal Stop With Stroke    
            _glyphs.Add("ʣ", @"\u02A3"); // Lowercase Dz Digraph                              
            _glyphs.Add("ʤ", @"\u02A4"); // Lowercase Dezh Digraph                            
            _glyphs.Add("ʥ", @"\u02A5"); // Lowercase Dz Digraph With Curl                    
            _glyphs.Add("ʦ", @"\u02A6"); // Lowercase Ts Digraph                              
            _glyphs.Add("ʧ", @"\u02A7"); // Lowercase Tesh Digraph                            
            _glyphs.Add("ʨ", @"\u02A8"); // Lowercase Tc Digraph With Curl                    
            _glyphs.Add("ʩ", @"\u02A9"); // Lowercase Feng Digraph                            
            _glyphs.Add("ʪ", @"\u02AA"); // Lowercase Ls Digraph                              
            _glyphs.Add("ʫ", @"\u02AB"); // Lowercase Lz Digraph                              
            _glyphs.Add("ʬ", @"\u02AC"); // Lowercase Bilabial Percussive                     
            _glyphs.Add("ʭ", @"\u02AD"); // Lowercase Bidental Percussive                     
            _glyphs.Add("ʮ", @"\u02AE"); // Lowercase Turned H With Fishhook                  
            _glyphs.Add("ʯ", @"\u02AF"); // Lowercase Turned H With Fishhook and Tail         
            _glyphs.Add("à", @"\u0300"); // Grave accent
            _glyphs.Add("á", @"\u0301"); // Acute accent
            _glyphs.Add("ê", @"\u0302"); // Circumflex accent
            _glyphs.Add("ã", @"\u0303"); // Tilde
            _glyphs.Add("ā", @"\u0304"); // Macron
            _glyphs.Add("ă", @"\u0306"); // Breve
            _glyphs.Add("ż", @"\u0307"); // Dot
            _glyphs.Add("ä", @"\u0308"); // Diaeresis(umlaut)
            _glyphs.Add("ả", @"\u0309"); // Hook
            _glyphs.Add("å", @"\u030A"); // Ring
            _glyphs.Add("ő", @"\u030B"); // Double acute
            _glyphs.Add("ž", @"\u030C"); // Caron(haček)
            Debug.WriteLine($"[INFO] Glyph table contains {_glyphs.Count} keys.");
            #endregion
        }

        /// <summary>
        /// Test event for our custom <see cref="ImageComboBox"/> control.
        /// </summary>
        /// <param name="sender"><see cref="ImageComboBox"/></param>
        /// <param name="e"><see cref="ImageComboBoxItemSelectedEvent"/></param>
        void ImageComboBox_ItemSelected(object sender, ImageComboBoxItemSelectedEvent e)
        {
            UpdateStatusBar($"SelectedItem: {e.SelectedItem}   SelectedImage: {e.SelectedImage.Width},{e.SelectedImage.Height}");
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

        /// <summary>
        /// <see cref="Form"/> event.
        /// </summary>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_formPainter == null)
                _formPainter = e.Graphics; // can be used in other methods for custom drawing

            if (_backgroundImage != null && !_closing)
                e.Graphics.DrawImage(_backgroundImage, _imagePosition); //e.Graphics.DrawImage(_backgroundImage, _imagePosition.X, _imagePosition.Y, _backgroundImage.Width, _backgroundImage.Height);

            #region [TextRenderer Example]
            if (_showWarning)
            {
                var textSize = TextRenderer.MeasureText(" ** WARNING ** ", _warningFont);
                TextRenderer.DrawText(e.Graphics, " ** WARNING ** ", _warningFont, new Point(cbDelimiters.Right + 20, cbDelimiters.Top), _clrWarning, Color.Black);
            }
            #endregion
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
                        //string fileContent = File.ReadAllText(filePath, Encoding.GetEncoding(_codePage));
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
                    SwitchButtonImage(btnGenerateResx, ResxWriter.Properties.Resources.Button02);
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

                    _userValues = ReadDelimitedFile(tbFilePath.Text, _userDelimiter[0]);

                    if (_userValues.Count > 0)
                    {
                        UpdateStatusBar($"File data has been loaded.  {_userValues.Count} items total.");
                        //if (_userValues.Count > 10) { tbContents.ScrollBars = ScrollBars.Vertical; }
                        Task.Run(() => FlashButton(btnGenerateResx));
                    }
                    else
                    {
                        UpdateStatusBar($"Check your input file and try again.");
                        SwitchButtonImage(btnGenerateResx, ResxWriter.Properties.Resources.Button02);
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
                    SwitchButtonImage(btnGenerateResx, ResxWriter.Properties.Resources.Button02);
                    Logger.Instance.Write($"Could not read the file: {ex.Message}", LogLevel.Error);
                    ShowMsgBoxError($"Could not read the file.\r\n{ex.Message}", "Error");
                }
            }
            else
            {
                SwitchButtonImage(btnGenerateResx, ResxWriter.Properties.Resources.Button02);
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
                    SwitchButtonImage(btnGenerateResx, ResxWriter.Properties.Resources.Button02);
                    ShowMsgBoxError("No valid delimited values to work with from the provided file.", "Validation");
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
                                if (!string.IsNullOrEmpty($"{kvp.Key}"))
                                {
                                    if (_useMeta)
                                        resxWriter.AddMetadata(kvp.Key, kvp.Value);
                                    else
                                        resxWriter.AddResource(kvp.Key, kvp.Value);
                                }
                            }
                        }

                        // Do we want the JS-style output for Angular pages?
                        if (_outputJS)
                        {
                            var jsFile = Path.Combine(Path.GetDirectoryName(filePath), $"{Path.GetFileNameWithoutExtension(tbFilePath.Text)}.js");
                            using (var jsfs = new FileStream(jsFile, FileMode.Create))
                            {
                                using (var fileStream = new StreamWriter(jsfs, Encoding.GetEncoding(_codePage)))
                                {
                                    int idx = 0;
                                    fileStream.BaseStream.Seek(0, SeekOrigin.Begin); // Jump to the beginning of the file before writing.
                                    fileStream.WriteLine("{");
                                    foreach (var kvp in _userValues)
                                    {
                                        idx++;
                                        if (!string.IsNullOrEmpty($"{kvp.Key}"))
                                        {
                                            // If you want the HTML entity style encoding use System.Web.HttpUtility.HtmlEncode() instead of the Filter() method.
                                            if (idx == _userValues.Count)
                                                fileStream.WriteLine($"   \"{kvp.Key}\": \"{kvp.Value.Filter(_glyphs)}\"");
                                            else
                                                fileStream.WriteLine($"   \"{kvp.Key}\": \"{kvp.Value.Filter(_glyphs)}\",");
                                        }
                                    }
                                    fileStream.WriteLine("}");
                                }
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

        /// <summary>
        /// <see cref="ComboBox"/> event.
        /// </summary>
        void cbDelimiters_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cb = sender as ComboBox;
            if (cb != null)
                _userDelimiter = cb.Items[cb.SelectedIndex] as string;
        }

        /// <summary>
        /// <see cref="ComboBox"/> event.
        /// </summary>
        void cbDelimiters_TextUpdate(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cbDelimiters.Text))
            {
                _userDelimiter = cbDelimiters.Text;
                UpdateStatusBar($"Custom delimiter set to \"{_userDelimiter}\"");
            }
        }

        /// <summary>
        /// <see cref="CheckBox"/> event.
        /// </summary>
        void cbMetadata_CheckedChanged(object sender, EventArgs e)
        {
            var cb = sender as CheckBox;
            if (cb != null)
            {
                _useMeta = cb.Checked;
                if (_useMeta)
                    cb.Text = "Metadata enabled";
                else
                    cb.Text = "Resource enabled";
            }
        }

        /// <summary>
        /// <see cref="CheckBox"/> event.
        /// </summary>
        void cbJSFile_CheckedChanged(object sender, EventArgs e)
        {
            var cb = sender as CheckBox;
            if (cb != null)
            {
                _outputJS = cb.Checked;
                if (_outputJS)
                    cb.Text = "JS output enabled";
                else
                    cb.Text = "JS output disabled";
            }
        }

        /// <summary>
        /// <see cref="Button"/> event.
        /// </summary>
        void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// <see cref="Form"/> event.
        /// </summary>
        void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _closing = true;
                _timer?.Stop();
                _regMon?.Stop();

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

        /// <summary>
        /// <see cref="ListView"/> event.
        /// </summary>
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
        /// <see cref="TextBox"/> event.
        /// </summary>
        void tbCodePage_TextChanged(object sender, EventArgs e)
        {
            var tb = sender as TextBox;
            if (int.TryParse(tb.Text, out int cp))
            {
                _codePage = SettingsManager.CodePage = cp;
                UpdateStatusBar("Converted value to code page.");
            }
            else
            {
                UpdateStatusBar("Unable to convert value to code page, defaulting to CP1252.");
                _codePage = 1252;
            }
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
                #region [Don't re-enter if already active]
                if (_logProcess != null)
                    return;

                // We could also compare the current image to see if it's the "On" version.
                //if (tsmi.Image.BytewiseCompare(ResxWriter.Properties.Resources.SB_DotOn)) { return; }
                #endregion

                // Update the image to indicate the process is in use.
                UpdateMenuItemImage(tsmi, ResxWriter.Properties.Resources.SB_DotOn);

                _logProcess = Process.Start(Logger.Instance.LogPath);

                // This was not reliable.
                //process.Exited += (po, pe) => { tsmi.Image = ResxWriter.Properties.Resources.SB_DotOff; };

                Task.Run(() =>
                {
                    while (!_logProcess.HasExited && !_closing) { Thread.Sleep(500); }
                    UpdateMenuItemImage(tsmi, ResxWriter.Properties.Resources.SB_DotOff);
                    _logProcess = null;
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
                #region [Don't re-enter if already active]
                if (_settingsProcess != null)
                    return;

                // We could also compare the current image to see if it's the "On" version.
                //if (tsmi.Image.BytewiseCompare(ResxWriter.Properties.Resources.SB_DotOn)) { return; }
                #endregion

                // Update the image to indicate the process is in use.
                UpdateMenuItemImage(tsmi, ResxWriter.Properties.Resources.SB_DotOn);

                _settingsProcess = Process.Start(SettingsManager.Location);

                // This was not reliable.
                //process.Exited += (po, pe) => { tsmi.Image = ResxWriter.Properties.Resources.SB_DotOff; };

                Task.Run(() =>
                {
                    while (!_settingsProcess.HasExited && !_closing) { Thread.Sleep(500); }
                    UpdateMenuItemImage(tsmi, ResxWriter.Properties.Resources.SB_DotOff);
                    _settingsProcess = null;
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
        /// The Paint event is raised when the control is redrawn. It passes an instance of PaintEventArgs to the method
        /// that handles the Paint event. When creating a new custom control or an inherited control with a different 
        /// visual appearance, you must provide code to render the control by overriding the OnPaint method.
        /// </summary>
        void MainFormOnPaint(object sender, PaintEventArgs e)
        {
            //var frm = sender as Form;
            Graphics g = e.Graphics;

            // Place the text to the right of the ComboBox control.
            var x = cbDelimiters.Left + cbDelimiters.Width + 10;
            var y = cbDelimiters.Top + 1;

            // Draw message text.
            g.DrawString("Rendering text using the form's GDI drawing surface.", _standardFont, System.Drawing.Brushes.Wheat, new Point(x, y));
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
                //var enc = Utils.DetermineFileEncoding(filePath, Encoding.GetEncoding(1252));
                //Debug.WriteLine($"File encoding is codepage {enc.CodePage}.");

                using (StreamReader sr = new StreamReader(filePath, Encoding.GetEncoding(_codePage)))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            line = line.Replace("\"", "");
                            string[] fields = line.Split(delimiter);

                            // Ensure there are at least two fields.
                            if (fields.Length >= 2)
                            {
                                try
                                {
                                    // Avoid empty keys.
                                    if (!string.IsNullOrEmpty(fields[0]))
                                    {
                                        fieldDictionary[fields[0]] = fields[1]; // Add the first and second fields to the dictionary
                                    }
                                }
                                catch (Exception kex) // Typically a duplicate key error.
                                {
                                    UpdateStatusBar($"{kex.Message}");
                                }
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
        public void SwitchButtonImage(Button btn, Bitmap img)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => SwitchButtonImage(btn, img)));
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
                SwitchButtonImage(btn, ResxWriter.Properties.Resources.Button02);
                
                Thread.Sleep(blinkSpeed);
                //ToggleControl(btn, true);
                SwitchButtonImage(btn, ResxWriter.Properties.Resources.Button01);
            }
        }

        #region [Custom dialog message]
        void ShowMsgBoxInfo(string msg, string title)
        {
            //MessageBox.Show($"{msg}", $"{title}", MessageBoxButtons.OK, MessageBoxIcon.Information);
            frmMessage.Show($"{msg}", $"{title}", MessageLevel.Info, true, TimeSpan.FromSeconds(6));
        }
        void ShowMsgBoxSuccess(string msg, string title)
        {
            //MessageBox.Show($"{msg}", $"{title}", MessageBoxButtons.OK, MessageBoxIcon.Information);
            frmMessage.Show($"{msg}", $"{title}", MessageLevel.Success, true, TimeSpan.FromSeconds(6));
        }
        void ShowMsgBoxWarning(string msg, string title)
        {
            //MessageBox.Show($"{msg}", $"{title}", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            frmMessage.Show($"{msg}", $"{title}", MessageLevel.Warning, true, TimeSpan.Zero);
        }
        void ShowMsgBoxError(string msg, string title)
        {
            //MessageBox.Show($"{msg}", $"{title}", MessageBoxButtons.OK, MessageBoxIcon.Error);
            frmMessage.Show($"{msg}", $"{title}", MessageLevel.Error, true, TimeSpan.Zero);
        }
        #endregion

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
        /// Uses the <see cref="TextRenderer"/> to measure the text size based on the standard application font.
        /// </summary>
        Size GetApproximateTextSize(string text) => TextRenderer.MeasureText(text, _standardFont);

        /// <summary>
        /// Uses the <see cref="TextRenderer"/> to draw text based on the standard application font.
        /// </summary>
        void DrawText(string text)
        {
            if (_formPainter != null)
                TextRenderer.DrawText(_formPainter, text, _standardFont, new Point(cbDelimiters.Right + 20, cbDelimiters.Top + 2), SystemColors.Highlight, SystemColors.HighlightText);
        }

        /// <summary>
        /// Self-reference for extracting the application icon.
        /// </summary>
        /// <returns><see cref="Icon"/></returns>
        Icon GetApplicationIcon() => Utils.GetFileIcon(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".exe", true);

        /// <summary>
        /// Method for adding a right-click option to the explorer shell.
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

        /// <summary>
        /// Print out the data using all available code pages.
        /// The user can deside which looks correct.
        /// </summary>
        void TestAllEncodings()
        {
            var strFR = "Il s'agit d'un échantillon de période de dépassement à convertir à l'aide de la bibliothèque d'encodages.";
            var encs = Encoding.GetEncodings();
            foreach (var enc in encs)
            {
                Debug.WriteLine($"{enc.Name} [{enc.CodePage}]");
                Encoding test = Encoding.GetEncoding(enc.CodePage);
                var samp = test.GetBytes(strFR);
                Debug.WriteLine($"{test.GetString(samp)}");
            }
        }
    }
}
