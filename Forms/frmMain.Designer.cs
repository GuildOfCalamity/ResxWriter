using System.Windows.Forms;

namespace ResxWriter
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.btnImport = new System.Windows.Forms.Button();
            this.tbFilePath = new System.Windows.Forms.TextBox();
            this.cbDelimiters = new System.Windows.Forms.ComboBox();
            this.lblDelims = new System.Windows.Forms.Label();
            this.cbMetadata = new System.Windows.Forms.CheckBox();
            this.btnFileSelect = new System.Windows.Forms.Button();
            this.tbContents = new System.Windows.Forms.TextBox();
            this.btnGenerateResx = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.lvContents = new System.Windows.Forms.ListView();
            this.columnHeaderKey = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tbCodePage = new System.Windows.Forms.TextBox();
            this.lblCodePage = new System.Windows.Forms.Label();
            this.cbJSFile = new System.Windows.Forms.CheckBox();
            this.stbStatus = new ResxWriter.CustomStatusStrip();
            this.sbStatusPanel = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            this.openSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stbStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnImport
            // 
            this.btnImport.BackColor = System.Drawing.Color.Transparent;
            this.btnImport.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.btnImport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImport.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnImport.ForeColor = System.Drawing.Color.White;
            this.btnImport.Location = new System.Drawing.Point(22, 52);
            this.btnImport.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(141, 30);
            this.btnImport.TabIndex = 2;
            this.btnImport.Text = "Import file";
            this.btnImport.UseVisualStyleBackColor = false;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // tbFilePath
            // 
            this.tbFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.tbFilePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbFilePath.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbFilePath.ForeColor = System.Drawing.Color.White;
            this.tbFilePath.Location = new System.Drawing.Point(22, 14);
            this.tbFilePath.Name = "tbFilePath";
            this.tbFilePath.Size = new System.Drawing.Size(810, 26);
            this.tbFilePath.TabIndex = 0;
            // 
            // cbDelimiters
            // 
            this.cbDelimiters.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.cbDelimiters.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbDelimiters.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbDelimiters.ForeColor = System.Drawing.Color.White;
            this.cbDelimiters.FormattingEnabled = true;
            this.cbDelimiters.Location = new System.Drawing.Point(314, 54);
            this.cbDelimiters.Name = "cbDelimiters";
            this.cbDelimiters.Size = new System.Drawing.Size(60, 27);
            this.cbDelimiters.TabIndex = 3;
            this.cbDelimiters.Text = "~";
            this.cbDelimiters.SelectedIndexChanged += new System.EventHandler(this.cbDelimiters_SelectedIndexChanged);
            this.cbDelimiters.TextUpdate += new System.EventHandler(this.cbDelimiters_TextUpdate);
            // 
            // lblDelims
            // 
            this.lblDelims.AutoSize = true;
            this.lblDelims.BackColor = System.Drawing.Color.Transparent;
            this.lblDelims.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDelims.ForeColor = System.Drawing.Color.White;
            this.lblDelims.Location = new System.Drawing.Point(185, 58);
            this.lblDelims.Name = "lblDelims";
            this.lblDelims.Size = new System.Drawing.Size(124, 19);
            this.lblDelims.TabIndex = 5;
            this.lblDelims.Text = "Column delimiter:";
            // 
            // cbMetadata
            // 
            this.cbMetadata.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbMetadata.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbMetadata.AutoSize = true;
            this.cbMetadata.BackColor = System.Drawing.Color.Transparent;
            this.cbMetadata.FlatAppearance.BorderSize = 0;
            this.cbMetadata.FlatAppearance.CheckedBackColor = System.Drawing.Color.DodgerBlue;
            this.cbMetadata.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.cbMetadata.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbMetadata.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbMetadata.ForeColor = System.Drawing.Color.White;
            this.cbMetadata.Location = new System.Drawing.Point(749, 53);
            this.cbMetadata.Name = "cbMetadata";
            this.cbMetadata.Size = new System.Drawing.Size(128, 28);
            this.cbMetadata.TabIndex = 4;
            this.cbMetadata.Text = "Add as metadata?";
            this.cbMetadata.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbMetadata.UseVisualStyleBackColor = false;
            this.cbMetadata.CheckedChanged += new System.EventHandler(this.cbMetadata_CheckedChanged);
            // 
            // btnFileSelect
            // 
            this.btnFileSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFileSelect.BackColor = System.Drawing.Color.Transparent;
            this.btnFileSelect.FlatAppearance.BorderSize = 0;
            this.btnFileSelect.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.btnFileSelect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFileSelect.Font = new System.Drawing.Font("Wingdings", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnFileSelect.ForeColor = System.Drawing.Color.PaleGoldenrod;
            this.btnFileSelect.Location = new System.Drawing.Point(842, 13);
            this.btnFileSelect.Margin = new System.Windows.Forms.Padding(0);
            this.btnFileSelect.Name = "btnFileSelect";
            this.btnFileSelect.Size = new System.Drawing.Size(32, 28);
            this.btnFileSelect.TabIndex = 1;
            this.btnFileSelect.Text = "1";
            this.btnFileSelect.UseVisualStyleBackColor = false;
            this.btnFileSelect.Click += new System.EventHandler(this.btnFileSelect_Click);
            // 
            // tbContents
            // 
            this.tbContents.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tbContents.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.tbContents.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbContents.Enabled = false;
            this.tbContents.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbContents.ForeColor = System.Drawing.Color.SteelBlue;
            this.tbContents.Location = new System.Drawing.Point(22, 98);
            this.tbContents.Multiline = true;
            this.tbContents.Name = "tbContents";
            this.tbContents.Size = new System.Drawing.Size(496, 150);
            this.tbContents.TabIndex = 5;
            this.tbContents.Visible = false;
            this.tbContents.WordWrap = false;
            // 
            // btnGenerateResx
            // 
            this.btnGenerateResx.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGenerateResx.BackColor = System.Drawing.Color.Transparent;
            this.btnGenerateResx.FlatAppearance.BorderSize = 0;
            this.btnGenerateResx.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.btnGenerateResx.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerateResx.Image = ((System.Drawing.Image)(resources.GetObject("btnGenerateResx.Image")));
            this.btnGenerateResx.Location = new System.Drawing.Point(22, 402);
            this.btnGenerateResx.Margin = new System.Windows.Forms.Padding(0);
            this.btnGenerateResx.Name = "btnGenerateResx";
            this.btnGenerateResx.Padding = new System.Windows.Forms.Padding(0, 0, 5, 5);
            this.btnGenerateResx.Size = new System.Drawing.Size(80, 80);
            this.btnGenerateResx.TabIndex = 6;
            this.btnGenerateResx.UseVisualStyleBackColor = false;
            this.btnGenerateResx.Click += new System.EventHandler(this.btnGenerateResx_Click);
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.BackColor = System.Drawing.Color.Transparent;
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Image = ((System.Drawing.Image)(resources.GetObject("btnExit.Image")));
            this.btnExit.Location = new System.Drawing.Point(754, 402);
            this.btnExit.Margin = new System.Windows.Forms.Padding(0);
            this.btnExit.Name = "btnExit";
            this.btnExit.Padding = new System.Windows.Forms.Padding(0, 0, 5, 5);
            this.btnExit.Size = new System.Drawing.Size(123, 80);
            this.btnExit.TabIndex = 7;
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // lvContents
            // 
            this.lvContents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvContents.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.lvContents.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvContents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderKey,
            this.columnHeaderValue});
            this.lvContents.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvContents.ForeColor = System.Drawing.Color.White;
            this.lvContents.FullRowSelect = true;
            this.lvContents.HideSelection = false;
            this.lvContents.Location = new System.Drawing.Point(22, 98);
            this.lvContents.Name = "lvContents";
            this.lvContents.Size = new System.Drawing.Size(849, 290);
            this.lvContents.TabIndex = 12;
            this.lvContents.UseCompatibleStateImageBehavior = false;
            this.lvContents.View = System.Windows.Forms.View.Details;
            this.lvContents.SelectedIndexChanged += new System.EventHandler(this.lvContents_SelectedIndexChanged);
            this.lvContents.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lvContents_MouseDown);
            // 
            // columnHeaderKey
            // 
            this.columnHeaderKey.Text = "Key";
            this.columnHeaderKey.Width = 140;
            // 
            // columnHeaderValue
            // 
            this.columnHeaderValue.Text = "Value";
            this.columnHeaderValue.Width = 700;
            // 
            // tbCodePage
            // 
            this.tbCodePage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.tbCodePage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbCodePage.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbCodePage.ForeColor = System.Drawing.Color.White;
            this.tbCodePage.Location = new System.Drawing.Point(496, 54);
            this.tbCodePage.Name = "tbCodePage";
            this.tbCodePage.Size = new System.Drawing.Size(115, 27);
            this.tbCodePage.TabIndex = 14;
            this.tbCodePage.Text = "windows-1252";
            this.tbCodePage.TextChanged += new System.EventHandler(this.tbCodePage_TextChanged);
            // 
            // lblCodePage
            // 
            this.lblCodePage.AutoSize = true;
            this.lblCodePage.BackColor = System.Drawing.Color.Transparent;
            this.lblCodePage.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCodePage.ForeColor = System.Drawing.Color.White;
            this.lblCodePage.Location = new System.Drawing.Point(408, 57);
            this.lblCodePage.Name = "lblCodePage";
            this.lblCodePage.Size = new System.Drawing.Size(82, 19);
            this.lblCodePage.TabIndex = 15;
            this.lblCodePage.Text = "Code page:";
            // 
            // cbJSFile
            // 
            this.cbJSFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbJSFile.Appearance = System.Windows.Forms.Appearance.Button;
            this.cbJSFile.AutoSize = true;
            this.cbJSFile.BackColor = System.Drawing.Color.Transparent;
            this.cbJSFile.FlatAppearance.BorderSize = 0;
            this.cbJSFile.FlatAppearance.CheckedBackColor = System.Drawing.Color.DodgerBlue;
            this.cbJSFile.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.cbJSFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbJSFile.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbJSFile.ForeColor = System.Drawing.Color.White;
            this.cbJSFile.Location = new System.Drawing.Point(663, 53);
            this.cbJSFile.Name = "cbJSFile";
            this.cbJSFile.Size = new System.Drawing.Size(79, 28);
            this.cbJSFile.TabIndex = 16;
            this.cbJSFile.Text = "Ouput JS?";
            this.cbJSFile.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbJSFile.UseVisualStyleBackColor = false;
            this.cbJSFile.CheckedChanged += new System.EventHandler(this.cbJSFile_CheckedChanged);
            // 
            // stbStatus
            // 
            this.stbStatus.AutoSize = false;
            this.stbStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.stbStatus.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stbStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.stbStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sbStatusPanel,
            this.toolStripSplitButton1});
            this.stbStatus.Location = new System.Drawing.Point(0, 496);
            this.stbStatus.Name = "stbStatus";
            this.stbStatus.Size = new System.Drawing.Size(896, 29);
            this.stbStatus.TabIndex = 13;
            this.stbStatus.Text = "status";
            // 
            // sbStatusPanel
            // 
            this.sbStatusPanel.AutoSize = false;
            this.sbStatusPanel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.sbStatusPanel.DoubleClickEnabled = true;
            this.sbStatusPanel.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sbStatusPanel.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.sbStatusPanel.Image = global::ResxWriter.Properties.Resources.App_Icon_png;
            this.sbStatusPanel.Margin = new System.Windows.Forms.Padding(2, 3, 5, 2);
            this.sbStatusPanel.Name = "sbStatusPanel";
            this.sbStatusPanel.Padding = new System.Windows.Forms.Padding(4);
            this.sbStatusPanel.Size = new System.Drawing.Size(780, 24);
            this.sbStatusPanel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSplitButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSplitButton1.DropDownButtonWidth = 14;
            this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openSettingsToolStripMenuItem,
            this.openLogToolStripMenuItem});
            this.toolStripSplitButton1.Image = global::ResxWriter.Properties.Resources.App_Settings;
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Margin = new System.Windows.Forms.Padding(5, 2, 0, 0);
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.Size = new System.Drawing.Size(35, 27);
            // 
            // openSettingsToolStripMenuItem
            // 
            this.openSettingsToolStripMenuItem.BackColor = System.Drawing.Color.DarkGray;
            this.openSettingsToolStripMenuItem.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.openSettingsToolStripMenuItem.Name = "openSettingsToolStripMenuItem";
            this.openSettingsToolStripMenuItem.Size = new System.Drawing.Size(169, 24);
            this.openSettingsToolStripMenuItem.Text = "Open Settings";
            // 
            // openLogToolStripMenuItem
            // 
            this.openLogToolStripMenuItem.BackColor = System.Drawing.Color.DarkGray;
            this.openLogToolStripMenuItem.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.openLogToolStripMenuItem.Name = "openLogToolStripMenuItem";
            this.openLogToolStripMenuItem.Size = new System.Drawing.Size(169, 24);
            this.openLogToolStripMenuItem.Text = "Open Log";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(896, 525);
            this.Controls.Add(this.cbJSFile);
            this.Controls.Add(this.lblCodePage);
            this.Controls.Add(this.tbCodePage);
            this.Controls.Add(this.stbStatus);
            this.Controls.Add(this.lvContents);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnGenerateResx);
            this.Controls.Add(this.tbContents);
            this.Controls.Add(this.btnFileSelect);
            this.Controls.Add(this.cbMetadata);
            this.Controls.Add(this.lblDelims);
            this.Controls.Add(this.cbDelimiters);
            this.Controls.Add(this.tbFilePath);
            this.Controls.Add(this.btnImport);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinimumSize = new System.Drawing.Size(750, 400);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Resx Writer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Shown += new System.EventHandler(this.frmMain_Shown);
            this.SizeChanged += new System.EventHandler(this.frmMain_SizeChanged);
            this.stbStatus.ResumeLayout(false);
            this.stbStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.TextBox tbFilePath;
        private System.Windows.Forms.ComboBox cbDelimiters;
        private System.Windows.Forms.Label lblDelims;
        private System.Windows.Forms.CheckBox cbMetadata;
        private System.Windows.Forms.Button btnFileSelect;
        private System.Windows.Forms.TextBox tbContents;
        private System.Windows.Forms.Button btnGenerateResx;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.ListView lvContents;
        //private ListViewTransparent lvContents;
        private System.Windows.Forms.ColumnHeader columnHeaderKey;
        private System.Windows.Forms.ColumnHeader columnHeaderValue;
        private CustomStatusStrip stbStatus;
        private ToolStripStatusLabel sbStatusPanel;
        private ToolStripSplitButton toolStripSplitButton1;
        private ToolStripMenuItem openLogToolStripMenuItem;
        private ToolStripMenuItem openSettingsToolStripMenuItem;
        private TextBox tbCodePage;
        private Label lblCodePage;
        private CheckBox cbJSFile;
    }
}

