using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace ResxWriter
{
    /// <summary>
    /// A custom designer-less InfoBar replacement.
    /// </summary>
    public partial class frmInfoBar : Form
    {
        #region [Props]
        bool roundedForm = true;
        Label messageLabel;
        PictureBox pictureBox;
        FormBorderStyle borderStyle = FormBorderStyle.FixedToolWindow;
        Point iconLocus = new Point(16, 32);
        Size iconSize = new Size(48, 48);
        static Size mainFormSize = new Size(540, 110);
        Padding withIconPadding = new Padding(65, 10, 20, 10);
        Padding withoutIconPadding = new Padding(15);
        Color clrForeText = Color.White;
        Color clrInfo = Color.FromArgb(0, 8, 48);
        Color clrWarning = Color.FromArgb(60, 50, 0);
        Color clrSuccess = Color.FromArgb(25, 45, 25);
        Color clrError = Color.FromArgb(45, 25, 25);
        Color clrBackground = Color.FromArgb(35, 35, 35);
        Color clrMouseOver = Color.FromArgb(55, 55, 55);
        TimeSpan defaultTimeSpan = TimeSpan.FromSeconds(1.6);
        System.Windows.Forms.Timer tmrClose = null;
        #endregion

        #region [Round Border]
        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ReleaseCapture();
        #endregion

        /// <summary>
        /// The main user method to show the custom message box.
        /// </summary>
        /// <param name="message">the text to display in the center of the dialog</param>
        /// <param name="title">the text to display on the title bar</param>
        /// <param name="autoClose"><see cref="TimeSpan"/> to close the dialog, use <see cref="TimeSpan.Zero"/> or null for indefinite</param>
        /// <param name="addIcon">true to show the <see cref="MessageLevel"/> icon, false to hide the icon</param>
        public static void ShowModal(string message, string title, bool addIcon = false, TimeSpan? autoClose = null)
        {
            using (var customMessageBox = new frmInfoBar(message, title, addIcon, autoClose))
            {
                customMessageBox.ShowDialog();
            }
        }

        #region [Non-Modal Option]
        static frmInfoBar _infoBar;
        static Form _owner;
        /// <summary>
        /// This proved to be trickier than expected, the built-in .Show() method does not seem to function 
        /// properly unless it is started in another thread. However, the call to .Show() still needs to reside 
        /// on the UI thread or bad things will happen.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <param name="addIcon"></param>
        /// <param name="autoClose"></param>
        /// <param name="owner"><see cref="IWin32Window"/></param>
        /// <remarks>
        /// The <see cref="IWin32Window"/> is NOT for the show method as expected, it is used for centering the 
        /// dialog window over the parent when multiple monitors are used.
        /// </remarks>
        public static void ShowNonModal(string message, string title, bool addIcon = false, TimeSpan? autoClose = null, Form owner = null)
        {
            _owner = owner;
            _infoBar = new frmInfoBar(message, title, addIcon, autoClose);
            _infoBar.WindowState = FormWindowState.Normal;
            _infoBar.Visible = true;
            _infoBar.ShowInTaskbar = true;
            _infoBar.TopMost = true; // We don't want this being covered up if the user clicks the main window.
            Thread t = new Thread(new ThreadStart(StartThread));
            t.Start();
        }

        static void StartThread()
        {
            _infoBar.InvokeIfRequired(() =>
            {
                try
                {
                    _infoBar.Show();
                    if (_owner != null)
                    {
                        // We're wider than we are tall.
                        var top = Math.Abs(_owner.Top + (_owner.Height / 2) - mainFormSize.Height);
                        var left = Math.Abs(_owner.Left + (int)(_owner.Width / 1.35) - mainFormSize.Width);
                        _infoBar.Top = top;
                        _infoBar.Left = left;
                    }
                    else
                    {
                        // This does a respectable job of centering the dialog in single monitor use-cases.
                        // It behaves more like "center in screen" than "center in parent", but that's fine in most cases.
                        _infoBar.CenterToParent();
                    }
                }
                catch (Exception ex) { Debug.WriteLine($"[WARNING] StartThread: {ex.Message}"); }
            });
        }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        frmInfoBar(string message, string title, bool addIcon, TimeSpan? autoClose)
        {
            //var fcoll = Application.OpenForms.OfType<Form>();
            //foreach (var frm in fcoll) { /* We'll only have one form open, the Main Form. */ }

            InitializeComponents(message, title, addIcon, autoClose);
        }


        /// <summary>
        /// Replaces the standard <see cref="InitializeComponent"/>.
        /// </summary>
        void InitializeComponents(string message, string title, bool addIcon, TimeSpan? autoClose)
        {
            #region [Form Background, Icon & Border]
            this.Text = title;
            this.FormBorderStyle = borderStyle;
            this.ClientSize = mainFormSize;
            this.BackColor = clrBackground;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Icon = ResxWriter.Properties.Resources.App_Icon_ico;

            if (this.FormBorderStyle == FormBorderStyle.FixedDialog)
            {
                this.MaximizeBox = false;
                this.MinimizeBox = false;
            }

            if (roundedForm)
            {
                this.FormBorderStyle = FormBorderStyle.None; // Do not use border styles with this API, it will look weird.
                this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 13, 13));
            }

            if (addIcon)
            {
                pictureBox = new PictureBox { Image = ResxWriter.Properties.Resources.MB_Info };
                pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                pictureBox.Size = iconSize;
                pictureBox.Location = iconLocus;
                this.Controls.Add(pictureBox);
            }
            #endregion

            #region [Form Message Label]
            messageLabel = new Label();
            messageLabel.ForeColor = clrForeText;
            // Attempt to auto-size the available label area.
            // We could change this to a TextBox with a vertical scrollbar.
            if (message.Length > 600)
                messageLabel.Font = new Font("Calibri", 8);
            else if (message.Length > 500)
                messageLabel.Font = new Font("Calibri", 9);
            else if (message.Length > 400)
                messageLabel.Font = new Font("Calibri", 10);
            else if (message.Length > 300)
                messageLabel.Font = new Font("Calibri", 11);
            else if (message.Length > 200)
                messageLabel.Font = new Font("Calibri", 12);
            else if (message.Length > 100)
                messageLabel.Font = new Font("Calibri", 13);
            else // standard font size
                messageLabel.Font = new Font("Calibri", 14);
            messageLabel.Text = message;
            messageLabel.TextAlign = ContentAlignment.MiddleCenter; //ContentAlignment.MiddleLeft
            messageLabel.Dock = DockStyle.Fill;
            messageLabel.Padding = addIcon ? withIconPadding : withoutIconPadding;
            this.Controls.Add(messageLabel);
            #endregion

            //this.Shown += (sender, e) => { /* timer start could alternatively be here */  };

            if (roundedForm)
            {   // Support dragging the dialog, since the message label will
                // take up most of the real estate when there is no title bar.
                messageLabel.MouseDown += (obj, mea) =>
                {
                    if (mea.Button == MouseButtons.Left)
                    {
                        ReleaseCapture();
                        SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                    }
                };
            }

            autoClose ??= defaultTimeSpan;
            if (autoClose == TimeSpan.Zero || autoClose == TimeSpan.MinValue || autoClose == TimeSpan.MaxValue)
                autoClose = defaultTimeSpan;

            if (tmrClose == null)
            {
                tmrClose = new System.Windows.Forms.Timer();
                var overflow = (int)autoClose.Value.TotalMilliseconds;
                if (overflow == int.MinValue)
                    overflow = int.MaxValue;
                tmrClose.Interval = overflow;
                tmrClose.Tick += TimerOnTick;
                tmrClose.Start();
            }
        }

        /// <summary>
        /// Auto-close timer event.
        /// </summary>
        void TimerOnTick(object sender, EventArgs e)
        {
            if (tmrClose != null)
            {
                tmrClose.Stop();
                tmrClose.Dispose();
                try { this.Close(); }
                catch (Exception) { }
            }
        }
    }
}
