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
            this.statusText = new System.Windows.Forms.Label();
            this.tbFilePath = new System.Windows.Forms.TextBox();
            this.cbDelimiters = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.cbMetadata = new System.Windows.Forms.CheckBox();
            this.btnFileSelect = new System.Windows.Forms.Button();
            this.tbContents = new System.Windows.Forms.TextBox();
            this.panelLine = new System.Windows.Forms.Panel();
            this.statusTime = new System.Windows.Forms.Label();
            this.btnGenerateResx = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.lvContents = new System.Windows.Forms.ListView();
            this.columnHeaderKey = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // btnImport
            // 
            this.btnImport.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.btnImport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImport.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnImport.ForeColor = System.Drawing.Color.White;
            this.btnImport.Location = new System.Drawing.Point(15, 52);
            this.btnImport.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(141, 30);
            this.btnImport.TabIndex = 2;
            this.btnImport.Text = "Import file";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // statusText
            // 
            this.statusText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusText.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusText.ForeColor = System.Drawing.Color.SteelBlue;
            this.statusText.Location = new System.Drawing.Point(9, 498);
            this.statusText.Name = "statusText";
            this.statusText.Size = new System.Drawing.Size(764, 28);
            this.statusText.TabIndex = 2;
            this.statusText.Text = "...";
            // 
            // tbFilePath
            // 
            this.tbFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFilePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.tbFilePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbFilePath.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbFilePath.ForeColor = System.Drawing.Color.White;
            this.tbFilePath.Location = new System.Drawing.Point(15, 14);
            this.tbFilePath.Name = "tbFilePath";
            this.tbFilePath.Size = new System.Drawing.Size(823, 26);
            this.tbFilePath.TabIndex = 0;
            // 
            // cbDelimiters
            // 
            this.cbDelimiters.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.cbDelimiters.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbDelimiters.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbDelimiters.ForeColor = System.Drawing.Color.White;
            this.cbDelimiters.FormattingEnabled = true;
            this.cbDelimiters.Location = new System.Drawing.Point(307, 55);
            this.cbDelimiters.Name = "cbDelimiters";
            this.cbDelimiters.Size = new System.Drawing.Size(60, 27);
            this.cbDelimiters.TabIndex = 3;
            this.cbDelimiters.Text = "~";
            this.cbDelimiters.SelectedIndexChanged += new System.EventHandler(this.cbDelimiters_SelectedIndexChanged);
            this.cbDelimiters.TextUpdate += new System.EventHandler(this.cbDelimiters_TextUpdate);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(178, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 19);
            this.label1.TabIndex = 5;
            this.label1.Text = "Column delimiter:";
            // 
            // cbMetadata
            // 
            this.cbMetadata.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbMetadata.AutoSize = true;
            this.cbMetadata.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbMetadata.ForeColor = System.Drawing.Color.White;
            this.cbMetadata.Location = new System.Drawing.Point(537, 56);
            this.cbMetadata.Name = "cbMetadata";
            this.cbMetadata.Size = new System.Drawing.Size(335, 23);
            this.cbMetadata.TabIndex = 4;
            this.cbMetadata.Text = "Add each item as metadata instead of resource";
            this.cbMetadata.UseVisualStyleBackColor = true;
            this.cbMetadata.CheckedChanged += new System.EventHandler(this.cbMetadata_CheckedChanged);
            // 
            // btnFileSelect
            // 
            this.btnFileSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFileSelect.FlatAppearance.BorderSize = 0;
            this.btnFileSelect.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.btnFileSelect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFileSelect.Font = new System.Drawing.Font("Wingdings", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnFileSelect.ForeColor = System.Drawing.Color.PaleGoldenrod;
            this.btnFileSelect.Location = new System.Drawing.Point(844, 13);
            this.btnFileSelect.Margin = new System.Windows.Forms.Padding(0);
            this.btnFileSelect.Name = "btnFileSelect";
            this.btnFileSelect.Size = new System.Drawing.Size(32, 28);
            this.btnFileSelect.TabIndex = 1;
            this.btnFileSelect.Text = "1";
            this.btnFileSelect.UseVisualStyleBackColor = true;
            this.btnFileSelect.Click += new System.EventHandler(this.btnFileSelect_Click);
            // 
            // tbContents
            // 
            this.tbContents.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.tbContents.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.tbContents.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbContents.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbContents.ForeColor = System.Drawing.Color.SteelBlue;
            this.tbContents.Location = new System.Drawing.Point(15, 98);
            this.tbContents.Multiline = true;
            this.tbContents.Name = "tbContents";
            this.tbContents.Size = new System.Drawing.Size(856, 150);
            this.tbContents.TabIndex = 5;
            this.tbContents.WordWrap = false;
            // 
            // panelLine
            // 
            this.panelLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelLine.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLine.Location = new System.Drawing.Point(13, 488);
            this.panelLine.Name = "panelLine";
            this.panelLine.Size = new System.Drawing.Size(859, 2);
            this.panelLine.TabIndex = 9;
            // 
            // statusTime
            // 
            this.statusTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.statusTime.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusTime.ForeColor = System.Drawing.Color.SteelBlue;
            this.statusTime.Location = new System.Drawing.Point(779, 498);
            this.statusTime.Name = "statusTime";
            this.statusTime.Size = new System.Drawing.Size(95, 28);
            this.statusTime.TabIndex = 11;
            this.statusTime.Text = "...";
            this.statusTime.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // btnGenerateResx
            // 
            this.btnGenerateResx.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGenerateResx.FlatAppearance.BorderSize = 0;
            this.btnGenerateResx.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.btnGenerateResx.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerateResx.Image = ((System.Drawing.Image)(resources.GetObject("btnGenerateResx.Image")));
            this.btnGenerateResx.Location = new System.Drawing.Point(15, 400);
            this.btnGenerateResx.Margin = new System.Windows.Forms.Padding(0);
            this.btnGenerateResx.Name = "btnGenerateResx";
            this.btnGenerateResx.Padding = new System.Windows.Forms.Padding(0, 0, 5, 5);
            this.btnGenerateResx.Size = new System.Drawing.Size(80, 80);
            this.btnGenerateResx.TabIndex = 6;
            this.btnGenerateResx.UseVisualStyleBackColor = true;
            this.btnGenerateResx.Click += new System.EventHandler(this.btnGenerateResx_Click);
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.SteelBlue;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Image = ((System.Drawing.Image)(resources.GetObject("btnExit.Image")));
            this.btnExit.Location = new System.Drawing.Point(748, 400);
            this.btnExit.Margin = new System.Windows.Forms.Padding(0);
            this.btnExit.Name = "btnExit";
            this.btnExit.Padding = new System.Windows.Forms.Padding(0, 0, 5, 5);
            this.btnExit.Size = new System.Drawing.Size(123, 80);
            this.btnExit.TabIndex = 7;
            this.btnExit.UseVisualStyleBackColor = true;
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
            this.lvContents.Location = new System.Drawing.Point(15, 98);
            this.lvContents.Name = "lvContents";
            this.lvContents.Size = new System.Drawing.Size(856, 292);
            this.lvContents.TabIndex = 12;
            this.lvContents.UseCompatibleStateImageBehavior = false;
            this.lvContents.View = System.Windows.Forms.View.Details;
            this.lvContents.SelectedIndexChanged += new System.EventHandler(this.lvContents_SelectedIndexChanged);
            // 
            // columnHeaderKey
            // 
            this.columnHeaderKey.Text = "Key";
            this.columnHeaderKey.Width = 150;
            // 
            // columnHeaderValue
            // 
            this.columnHeaderValue.Text = "Value";
            this.columnHeaderValue.Width = 700;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.ClientSize = new System.Drawing.Size(884, 536);
            this.Controls.Add(this.lvContents);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnGenerateResx);
            this.Controls.Add(this.statusTime);
            this.Controls.Add(this.panelLine);
            this.Controls.Add(this.tbContents);
            this.Controls.Add(this.btnFileSelect);
            this.Controls.Add(this.cbMetadata);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbDelimiters);
            this.Controls.Add(this.tbFilePath);
            this.Controls.Add(this.statusText);
            this.Controls.Add(this.btnImport);
            this.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MinimumSize = new System.Drawing.Size(750, 400);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Resx Writer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Shown += new System.EventHandler(this.frmMain_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Label statusText;
        private System.Windows.Forms.TextBox tbFilePath;
        private System.Windows.Forms.ComboBox cbDelimiters;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox cbMetadata;
        private System.Windows.Forms.Button btnFileSelect;
        private System.Windows.Forms.TextBox tbContents;
        private System.Windows.Forms.Panel panelLine;
        private System.Windows.Forms.Label statusTime;
        private System.Windows.Forms.Button btnGenerateResx;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.ListView lvContents;
        private System.Windows.Forms.ColumnHeader columnHeaderKey;
        private System.Windows.Forms.ColumnHeader columnHeaderValue;
    }
}

