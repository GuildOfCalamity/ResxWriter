using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResxWriter
{
    public class CustomPopupMenu : ContextMenuStrip
    {
        public event EventHandler MenuItemClicked;

        public CustomPopupMenu(Dictionary<string, Image> items, Font menuFont)
        {
            // Add menu items
            foreach (KeyValuePair<string, Image> item in items)
            {
                ToolStripMenuItem menuItem = new ToolStripMenuItem(item.Key);
                menuItem.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                menuItem.BackColor = System.Drawing.Color.FromArgb(40, 40, 40);
                menuItem.ForeColor = System.Drawing.Color.WhiteSmoke;
                menuItem.TextAlign = ContentAlignment.MiddleLeft;
                menuItem.Image = item.Value;
                menuItem.ImageAlign = ContentAlignment.MiddleCenter;
                menuItem.Font = menuFont;
                menuItem.Click += MenuItem_Click;
                menuItem.MouseEnter += MenuItem_MouseEnter;
                menuItem.MouseLeave += MenuItem_MouseLeave;
                this.Items.Add(menuItem);
            }
        }

        void MenuItem_MouseLeave(object sender, EventArgs e)
        {
            var mi = sender as ToolStripMenuItem;
            mi.ForeColor = System.Drawing.Color.WhiteSmoke;
            //mi.Font = new Font(mi.Font, FontStyle.Regular);
        }

        void MenuItem_MouseEnter(object sender, EventArgs e)
        {
            var mi = sender as ToolStripMenuItem;
            mi.ForeColor = System.Drawing.Color.Black;
            //mi.Font = new Font(mi.Font, FontStyle.Bold);
        }

        /// <summary>
        /// Trigger the MenuItemClicked event with the clicked menu item's text
        /// </summary>
        void MenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem)
            {
                MenuItemClicked?.Invoke(this, new MenuItemClickedEventArgs(menuItem.Text));
            }
        }
    }

    public class MenuItemClickedEventArgs : EventArgs
    {
        public string ItemText { get; }

        public MenuItemClickedEventArgs(string clickedItemText)
        {
            ItemText = clickedItemText;
        }
    }

    /// <summary>
    /// Inside frmMain...
    /// var ms1 = new MenuStrip();
    /// ms1.Renderer = new AltRenderer();
    /// this.Controls.Add(ms1);
    /// https://stackoverflow.com/questions/36767478/color-change-for-menuitem
    /// </summary>
    public class AltRenderer : ToolStripProfessionalRenderer
    {
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            Rectangle rc = new Rectangle(Point.Empty, e.Item.Size);
            Color c = e.Item.Selected ? Color.LightBlue : Color.MediumBlue;
            using (SolidBrush brush = new SolidBrush(c))
                e.Graphics.FillRectangle(brush, rc);
        }
    }

    /// <summary>
    /// Inside frmMain...
    /// var ms1 = new MenuStrip();
    /// ms1.Renderer = new ToolStripProfessionalRenderer(new ModifiedColorTable());
    /// this.Controls.Add(ms1);
    /// https://stackoverflow.com/questions/36767478/color-change-for-menuitem
    /// </summary>
    public class ModifiedColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground
        {
            get => Color.Blue;
        }

        public override Color ImageMarginGradientBegin
        {
            get => Color.Blue;
        }

        public override Color ImageMarginGradientMiddle
        {
            get => Color.Blue;
        }

        public override Color ImageMarginGradientEnd
        {
            get => Color.Blue;
        }

        public override Color MenuBorder
        {
            get => Color.Black;
        }

        public override Color MenuItemBorder
        {
            get => Color.Black;
        }

        public override Color MenuItemSelected
        {
            get => Color.Navy;
        }

        public override Color MenuStripGradientBegin
        {
            get => Color.Blue;
        }

        public override Color MenuStripGradientEnd
        {
            get => Color.Blue;
        }

        public override Color MenuItemSelectedGradientBegin
        {
            get => Color.Navy;
        }

        public override Color MenuItemSelectedGradientEnd
        {
            get => Color.Navy;
        }

        public override Color MenuItemPressedGradientBegin
        {
            get => Color.Blue;
        }

        public override Color MenuItemPressedGradientEnd
        {
            get => Color.Blue;
        }
    }
}
