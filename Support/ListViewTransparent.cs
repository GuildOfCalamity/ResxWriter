using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace ResxWriter
{
    /// <summary>
    /// This still needs work.
    /// https://learn.microsoft.com/en-us/windows/win32/controls/extended-list-view-styles
    /// </summary>
    public partial class ListViewTransparent : ListView
    {
        public ListViewTransparent()
        {
            // Set the control styles to support a transparent backcolor
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.Opaque, false);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            //SetStyle(ControlStyles.UserPaint, true);
            this.BackColor = Color.FromArgb(20,20,20);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                // Set the window style to support transparency
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                return cp;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs pea)
        {
            // Do not paint the background
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Override OnPaint to ensure correct redrawing
            base.OnPaint(e);

            // Manually draw the background to avoid flickering
            using (Brush brush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0014) // WM_ERASEBKGND
            {
                // Prevent background erasing to reduce flickering
                m.Result = IntPtr.Zero;
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        //protected override void OnNotifyMessage(Message m)
        //{
        //    if (m.Msg == 0x0014) // WM_ERASEBKGND
        //    {
        //        return; // Suppress the message
        //    }
        //    base.OnNotifyMessage(m);
        //}

        //protected override void InitLayout()
        //{
        //    //this.SuspendLayout();
        //    base.SetStyle(System.Windows.Forms.ControlStyles.Opaque, false);
        //    base.SetStyle(System.Windows.Forms.ControlStyles.SupportsTransparentBackColor, true);
        //    base.BackColor = Color.Transparent; //Color.FromArgb(30,20,20,20);
        //    //this.ResumeLayout(false);
        //    base.InitLayout();
        //}
        //
        //protected override void OnCreateControl()
        //{
        //    base.OnCreateControl();
        //}
        //
        //protected override void OnInvalidated(InvalidateEventArgs e)
        //{
        //    base.OnInvalidated(e);
        //
        //    var rect = e.InvalidRect;
        //
        //    #region [Custom Render]
        //    int alpha = 40;
        //    Point p1 = this.Parent.PointToScreen(this.Location);
        //    Point p2 = this.PointToScreen(Point.Empty);
        //    p2.Offset(-p1.X, -p1.Y);
        //    if (this.BackgroundImage != null)
        //        this.BackgroundImage.Dispose();
        //    this.Hide();
        //    Bitmap bmp = new Bitmap(this.Parent.Width, this.Parent.Height);
        //    this.Parent.DrawToBitmap(bmp, this.Parent.ClientRectangle);
        //    Rectangle r = this.Bounds;
        //    r.Offset(p2.X, p2.Y);
        //    bmp = bmp.Clone(r, PixelFormat.Format32bppArgb);
        //    using (Graphics g = Graphics.FromImage(bmp))
        //    {
        //        using (SolidBrush br = new SolidBrush(Color.FromArgb(alpha, this.BackColor)))
        //        {
        //            g.FillRectangle(br, this.ClientRectangle);
        //        }
        //    }
        //    this.BackgroundImage = bmp;
        //    this.Show();
        //    #endregion
        //}
        //
        //protected override void OnPaint(PaintEventArgs e)
        //{
        //    base.OnPaint(e);
        //}
    }
}
