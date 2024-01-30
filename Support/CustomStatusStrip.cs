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
    public partial class CustomStatusStrip : StatusStrip
    {
        #region [Props]
        Color _background = Color.FromArgb(20, 20, 20);
        Color _foreground = Color.FromArgb(250, 250, 250);
        Pen _pen1 = new Pen(Color.FromArgb(170, 170, 170), 2.0F);
        Pen _pen2 = new Pen(Color.FromArgb(56, 56, 56), 2.0F);
        #endregion

        /// <summary>
        /// Default Constructor
        /// </summary>
        public CustomStatusStrip()
        {
            BackColor = _background;
            ForeColor = _foreground;
            ShowItemToolTips = true; // https://learn.microsoft.com/en-us/dotnet/desktop/winforms/controls/how-to-use-tooltips-in-toolstrip-controls?view=netframeworkdesktop-4.8#to-display-a-tooltip
        }

        /// <summary>
        /// Resets the back and fore colors and redraws.
        /// </summary>
        public void Reset()
        {
            BackColor = _background;
            ForeColor = _foreground;

            Invalidate();
        }

        /// <inheritdoc/>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            var g = e.Graphics;

            using (var b = new SolidBrush(_background))
            {
                g.FillRectangle(b, ClientRectangle);
            }

            // Separator line.
            g.DrawLine(_pen1, ClientRectangle.Left, 0, ClientRectangle.Right, 0);
            g.DrawLine(_pen2, ClientRectangle.Left, 2, ClientRectangle.Right, 2);
        }
    }
}
