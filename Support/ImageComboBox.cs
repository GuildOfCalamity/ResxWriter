using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResxWriter
{
    #region [Custom EventArgs]
    public class ImageComboBoxItemSelectedEvent : EventArgs
    {
        public string SelectedItem { get; }
        public Image SelectedImage { get; }

        public ImageComboBoxItemSelectedEvent(string selectedItem, Image selectedImage)
        {
            SelectedItem = selectedItem;
            SelectedImage = selectedImage;
        }
    }
    #endregion

    public partial class ImageComboBox : ComboBox
    {
        int hoveredIndex = -1;
        System.Windows.Forms.ImageList imageList;
        const int WM_PAINT = 0xF;
        const int WM_PAINTICON = 0x26;
        const int WM_NCPAINT = 0x85;
        const int WM_SYNCPAINT = 0x88;

        [DefaultValue(null)]
        [Description("The ImageList for the ComboBox")]
        [Category("Appearance")]
        public ImageList ImageList
        {
            get { return imageList; }
            set { imageList = value; }
        }
        
        // Define a custom event for item selection change
        public event EventHandler<ImageComboBoxItemSelectedEvent> ItemSelected;

        /// <summary>
        /// Main constructor for the control.
        /// </summary>
        /// <param name="clrFore"><see cref="System.Drawing.Color"/></param>
        /// <param name="clrBack"><see cref="System.Drawing.Color"/></param>
        /// <param name="ctrlSize"><see cref="System.Drawing.Size"/></param>
        /// <param name="imgSize"><see cref="System.Drawing.Size"/></param>
        /// <param name="ctrlFont"><see cref="System.Drawing.Font"/></param>
        public ImageComboBox(System.Drawing.Color clrFore, System.Drawing.Color clrBack, System.Drawing.Size ctrlSize, System.Drawing.Size imgSize, System.Drawing.Font ctrlFont)
        {
            this.SuspendLayout();

            //this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 19F);
            //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

            if (clrFore != null)
                this.ForeColor = clrFore;
            else
                this.ForeColor = System.Drawing.Color.White;

            if (clrBack != null)
                this.BackColor = clrBack;
            else
                this.BackColor = System.Drawing.Color.FromArgb(20, 20, 20);

            if (ctrlSize != null)
                this.Size = ctrlSize;
            else
                this.Size = new System.Drawing.Size(160, 80);

            if (ctrlFont != null)
                this.Font = ctrlFont;
            else
                this.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, (byte)0);

            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ImageComboBox";
            this.DropDownStyle = ComboBoxStyle.DropDownList;
            this.FlatStyle = FlatStyle.Flat;
            //this.FlatAppearance.BorderColor = Color.Empty; // default ComboBox does not offer appearance changes
            this.ResumeLayout(false);


            // Initialize ImageList and configure properties.
            imageList = new ImageList();
            if (imgSize != null)
                imageList.ImageSize = imgSize;
            else
                imageList.ImageSize = new Size(16, 16); // default is 16x16

            imageList.ColorDepth = ColorDepth.Depth32Bit; // Use 32-bit color depth for transparency

            this.DrawMode = DrawMode.OwnerDrawFixed;

            // Assign the ImageList to the ComboBox
            this.ImageList = imageList;

            // Subscribe to DrawItem event
            this.DrawItem += ImageComboBox_DrawItem;
            this.MouseMove += ImageComboBox_MouseMove;

            // Subscribe to SelectedIndexChanged event
            this.SelectedIndexChanged += ImageComboBox_SelectedIndexChanged;
        }

        /// <summary>
        /// Raise the custom ItemSelected event when the selected item changes.
        /// </summary>
        void ImageComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.SelectedIndex != -1)
            {
                string selectedItem = this.SelectedItem?.ToString();
                Image selectedImage = this.SelectedIndex < imageList.Images.Count ? imageList.Images[this.SelectedIndex] : null;
                ItemSelected?.Invoke(this, new ImageComboBoxItemSelectedEvent(selectedItem, selectedImage));
            }
        }

        void ImageComboBox_MouseMove(object sender, MouseEventArgs e)
        {
            //Point p1 = this.Parent.PointToScreen(this.Location);
            //Point p2 = this.PointToScreen(Point.Empty);

            int index = -1;
            int itemHeight = this.ItemHeight;

            for (int i = 0; i < this.Items.Count; i++)
            {
                Rectangle itemRect = new Rectangle(0, i * itemHeight, this.Width, itemHeight);
                if (itemRect.Contains(e.Location))
                {
                    index = i;
                    break;
                }
            }

            if (index != hoveredIndex)
            {
                hoveredIndex = index;
                this.Invalidate();
            }
        }

        void ImageComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            e.DrawBackground();

            // Get the item text
            string text = this.Items[e.Index].ToString();

            // Get the item image
            Image image = null;
            if (e.Index < imageList.Images.Count)
                image = imageList.Images[e.Index];

            // Define the text and image rectangle
            Rectangle textRect = new Rectangle(e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
            Rectangle imageRect = new Rectangle(e.Bounds.Left, e.Bounds.Top, imageList.ImageSize.Width, e.Bounds.Height);

            // Draw the image (if available)
            if (image != null)
            {
                e.Graphics.DrawImage(image, imageRect);
                textRect.X += imageList.ImageSize.Width; // Adjust text rectangle based on image width
            }

            // Draw the item text
            TextRenderer.DrawText(e.Graphics, text, e.Font, textRect, e.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            // Draw the focus rectangle if the item has focus
            e.DrawFocusRectangle();
        }

        /// <summary>
        /// Override the WndProc method to customize control appearance
        /// </summary>
        //protected override void WndProc(ref Message m)
        //{
        //    if (m.Msg == WM_NCPAINT)
        //        return; // Do nothing (suppress non-client area painting)
        //
        //    base.WndProc(ref m);
        //}
    }
}
