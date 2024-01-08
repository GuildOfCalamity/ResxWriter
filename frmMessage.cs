using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace ResxWriter
{
    public enum MessageLevel
    {
        Info = 0,
        Warning = 1, 
        Success = 2,
        Error = 3,
    }

    /// <summary>
    /// A custom designer-less MessageBox replacement.
    /// </summary>
    public partial class frmMessage : Form
    {
        #region [Props]
        bool autoTab = false;
        string okText = "OK"; //"Acknowledge"
        Label messageLabel;
        Button okButton;
        PictureBox pictureBox;
        FormBorderStyle borderStyle = FormBorderStyle.FixedToolWindow; //FormBorderStyle.FixedDialog;
        Point iconLocus = new Point(16, 56);
        Size iconSize = new Size(48, 48);
        Size mainFormSize = new Size(470, 190);
        Padding withIconPadding = new Padding(65, 10, 20, 10);
        Padding withoutIconPadding = new Padding(15);
        Color clrForeText = Color.White;
        Color clrInfo = Color.FromArgb(0, 8, 48);
        Color clrWarning = Color.FromArgb(60, 50, 0);
        Color clrSuccess = Color.FromArgb(25, 45, 25);
        Color clrError = Color.FromArgb(45, 25, 25);
        Color clrBackground = Color.FromArgb(28, 28, 28);
        Color clrMouseOver = Color.FromArgb(55, 55, 55);
        #endregion

        /// <summary>
        /// The main user method to show the custom message box.
        /// </summary>
        /// <param name="message">the text to display in the center of the dialog</param>
        /// <param name="title">the text to display on the title bar</param>
        /// <param name="level"><see cref="MessageLevel"/></param>
        /// <param name="addIcon">true to add the appropriate <see cref="MessageLevel"/> icon, false to hide the icon</param>
        public static void Show(string message, string title, MessageLevel level = MessageLevel.Info, bool addIcon = false)
        {
            using (var customMessageBox = new frmMessage(message, title, level, addIcon))
            {
                customMessageBox.ShowDialog();
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        frmMessage(string message, string title, MessageLevel level, bool addIcon)
        {
            InitializeComponents(message, title, level, addIcon);
        }

        /// <summary>
        /// Replaces the standard <see cref="InitializeComponent"/>.
        /// </summary>
        void InitializeComponents(string message, string title, MessageLevel level, bool addIcon)
        {
            #region [Form Background, Icon & Others]
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

            if (addIcon)
            {
                switch (level)
                {
                    case MessageLevel.Info:
                            pictureBox = new PictureBox { Image = ResxWriter.Properties.Resources.MB_Info };
                        break;
                    case MessageLevel.Warning:
                            pictureBox = new PictureBox { Image = ResxWriter.Properties.Resources.MB_Warning };
                        break;
                    case MessageLevel.Success:
                            pictureBox = new PictureBox { Image = ResxWriter.Properties.Resources.MB_Success };
                        break;
                    case MessageLevel.Error:
                            pictureBox = new PictureBox { Image = ResxWriter.Properties.Resources.MB_Error };
                        break;
                    default: // Default will be the info style.
                            pictureBox = new PictureBox { Image = ResxWriter.Properties.Resources.MB_Info };
                        break;
                }
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

            #region [Form Close Button]
            okButton = new Button();
            okButton.TabIndex = 0;
            okButton.TabStop = true;
            okButton.Text = okText;
            okButton.TextAlign = ContentAlignment.TopCenter;
            okButton.Font = new Font("Calibri", 14);
            okButton.ForeColor = clrForeText;
            okButton.MinimumSize = new Size(300, 33);
            okButton.FlatStyle = FlatStyle.Flat;
            okButton.FlatAppearance.BorderSize = 0;
            switch (level)
            {
                case MessageLevel.Info:
                    okButton.BackColor = clrInfo;
                    okButton.FlatAppearance.MouseOverBackColor = clrMouseOver; //ColorBlend(Color.Gray, clrInfo);
                    break;
                case MessageLevel.Warning:
                    okButton.BackColor = clrWarning;
                    okButton.FlatAppearance.MouseOverBackColor = clrMouseOver; //ColorBlend(Color.Gray, clrWarning);
                    break;
                case MessageLevel.Success:
                    okButton.BackColor = clrSuccess;
                    okButton.FlatAppearance.MouseOverBackColor = clrMouseOver; //ColorBlend(Color.Gray, clrSuccess);
                    break;
                case MessageLevel.Error:
                    okButton.BackColor = clrError;
                    okButton.FlatAppearance.MouseOverBackColor = clrMouseOver; //ColorBlend(Color.Gray, clrError);
                    break;
                default:
                    okButton.BackColor = clrBackground;
                    okButton.FlatAppearance.MouseOverBackColor = clrMouseOver;
                    break;
            }
            okButton.Dock = DockStyle.Bottom;
            okButton.Click += (sender, e) => this.Close();
            this.Shown += (sender, e) => {
                this.ActiveControl = okButton;
                if (autoTab)
                    SendKeys.SendWait("{TAB}");
            };
            this.Controls.Add(okButton);
            #endregion
        }

        /// <summary>
        /// Blends the provided two colors together.
        /// </summary>
        /// <param name="foreColor">Color to blend onto the background color.</param>
        /// <param name="backColor">Color to blend the other color onto.</param>
        /// <param name="amount">How much of <paramref name="foreColor"/> to keep, on top of <paramref name="backColor"/>.</param>
        /// <returns>The blended color.</returns>
        /// <remarks>The alpha channel is not altered.</remarks>
        public Color ColorBlend(Color foreColor, Color backColor, double amount = 0.3)
        {
            byte r = (byte)(foreColor.R * amount + backColor.R * (1 - amount));
            byte g = (byte)(foreColor.G * amount + backColor.G * (1 - amount));
            byte b = (byte)(foreColor.B * amount + backColor.B * (1 - amount));
            return Color.FromArgb(r, g, b);
        }
    }
}
