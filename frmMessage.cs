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

    public partial class frmMessage : Form
    {
        Label messageLabel;
        Button okButton;
        PictureBox pictureBox;

        public static void Show(string message, string title, MessageLevel level = MessageLevel.Info, bool addIcon = false)
        {
            using (var customMessageBox = new frmMessage(message, title, level, addIcon))
            {
                customMessageBox.ShowDialog();
            }
        }

        public frmMessage(string message, string title, MessageLevel level, bool addIcon)
        {
            InitializeComponents(message, title, level, addIcon);
        }

        private void InitializeComponents(string message, string title, MessageLevel level, bool addIcon)
        {
            this.Text = title;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            switch (level)
            {
                case MessageLevel.Info:
                    this.BackColor = Color.FromArgb(25, 25, 25);
                    break;
                case MessageLevel.Warning:
                    this.BackColor = Color.FromArgb(60, 50, 0);
                    break;
                case MessageLevel.Success:
                    this.BackColor = Color.FromArgb(25, 45, 25);
                    break;
                case MessageLevel.Error:
                    this.BackColor = Color.FromArgb(45, 25, 25);
                    break;
            }
            this.ClientSize = new Size(400, 150);
            this.StartPosition = FormStartPosition.CenterParent;

            if (addIcon)
            {
                pictureBox = new PictureBox();
                pictureBox.Image = global::ResxWriter.Properties.Resources.App_Icon_png;
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox.Size = new Size(50, 50);
                pictureBox.Location = new Point(0, 30);
                this.Controls.Add(pictureBox);
            }

            messageLabel = new Label();
            messageLabel.ForeColor = Color.White;
            messageLabel.Font = new Font("Calibri", 14);
            messageLabel.Text = message;
            messageLabel.TextAlign = ContentAlignment.MiddleCenter;
            messageLabel.Dock = DockStyle.Fill;
            messageLabel.Padding = new Padding(20);
            this.Controls.Add(messageLabel);

            okButton = new Button();
            okButton.Text = "OK";
            okButton.TextAlign = ContentAlignment.TopCenter;
            okButton.Font = new Font("Calibri", 14);
            okButton.BackColor = Color.FromArgb(50, 50, 50);
            okButton.ForeColor = Color.White;
            okButton.MinimumSize = new Size(300, 32);
            okButton.FlatStyle = FlatStyle.Flat;
            okButton.FlatAppearance.BorderSize = 0;
            okButton.Dock = DockStyle.Bottom;
            okButton.Click += (sender, e) => this.Close();
            this.Controls.Add(okButton);
        }
    }
}
