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
    public partial class BounceButton : Button
    {
        Color _disableColor = Color.FromArgb(70, 70, 70);
        Color _backColor = Color.FromArgb(40, 40, 60);
        Color _foreColor = Color.FromArgb(240, 240, 250);
        int _growthFactor = 8;
        bool _isMouseOver;
        bool _isMouseDown;
        // Define a custom event for button click
        public event EventHandler ButtonClick;

        public BounceButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.TextAlign = ContentAlignment.MiddleCenter;

            // Subscribe to mouse events
            this.MouseEnter += Button_MouseEnter;
            this.MouseLeave += Button_MouseLeave;
            this.MouseDown += Button_MouseDown;
            this.MouseUp += Button_MouseUp;
            this.Click += Button_Click;

            // Set initial properties or customize as needed
            this.BackColor = _backColor;
            this.ForeColor = _foreColor;
            this.FlatAppearance.BorderColor = _foreColor;
            this.FlatAppearance.MouseOverBackColor = Utils.LightenColor(_backColor);
            this.FlatAppearance.MouseDownBackColor = Utils.DarkenColor(_backColor);

            InitializeComponent();
        }

        public BounceButton(Font font, int growthFactor, int borderSize) : this()
        {
            this.Font = font;
            _growthFactor = growthFactor;
            this.FlatAppearance.BorderSize = borderSize;
        }

        public void ColorConfig(Color backColor, Color foreColor, Color disableColor)
        {
            _disableColor = disableColor;
            this.BackColor = _backColor = backColor;
            this.ForeColor = _foreColor = foreColor;
            this.FlatAppearance.BorderColor = foreColor;
            this.FlatAppearance.MouseOverBackColor = Utils.LightenColor(_backColor);
            this.FlatAppearance.MouseDownBackColor = Utils.DarkenColor(_backColor);
        }

        public void Disable()
        {
            this.Enabled = false;
            this.ForeColor = this.FlatAppearance.BorderColor = _disableColor;
            this.BackColor = Utils.DarkenColor(_disableColor);
        }

        public void Enable()
        {
            this.Enabled = true;
            this.BackColor = _backColor;
            this.ForeColor = this.FlatAppearance.BorderColor = _foreColor;
        }

        protected override void InitLayout()
        {
            base.InitLayout();
        }

        void Button_Click(object sender, EventArgs e)
        {
            // If you needed the current location for showing a popup menu:
            //Point pt = new Point(this.Left, this.Top + this.Height);
            //pt = this.Parent.PointToScreen(pt);

            ButtonClick?.Invoke(this, EventArgs.Empty);
        }

        void Button_MouseEnter(object sender, EventArgs e)
        {
            _isMouseOver = true;
            this.ForeColor = Utils.DarkenColor(_backColor);
            GrowButton();
        }

        void Button_MouseLeave(object sender, EventArgs e)
        {
            _isMouseOver = false;
            this.ForeColor = _foreColor;
            ShrinkButton();
        }

        void Button_MouseDown(object sender, MouseEventArgs e)
        {
            _isMouseDown = true;
            this.ForeColor = Utils.LightenColor(_backColor);
            ShrinkButton();
        }

        void Button_MouseUp(object sender, MouseEventArgs e)
        {
            _isMouseDown = false;
            
            if (_isMouseOver)
            {
                this.ForeColor = Utils.DarkenColor(_backColor);
                GrowButton(); // If the mouse is still over the button after releasing the click, grow the button
            }
            else
            {
                this.ForeColor = Utils.LightenColor(_backColor);
                ShrinkButton(); // If the mouse is not over the button, shrink it back to normal size
            }
        }

        void GrowButton()
        {
            if (_isMouseDown)
            {
                // If the mouse is down, don't grow the button further
                return;
            }

            // Calculate the new size with growth centered around the button's center
            int newXSize = this.Width + _growthFactor;
            int newYSize = this.Height + (_growthFactor / 2);
            int offsetX = (newXSize - this.Width) / 2;
            int offsetY = (newYSize - this.Height) / 2;

            // Set the new size and location
            this.Size = new Size(newXSize, newYSize);
            this.Location = new Point(this.Location.X - offsetX, this.Location.Y - offsetY);
        }

        void ShrinkButton()
        {
            // Calculate the original size with shrinking centered around the button's center
            int newXSize = this.Width - _growthFactor;
            int newYSize = this.Height - (_growthFactor / 2);
            int offsetX = (this.Width - newXSize) / 2;
            int offsetY = (this.Height - newYSize) / 2;

            // Set the original size and location
            this.Size = new Size(newXSize, newYSize);
            this.Location = new Point(this.Location.X + offsetX, this.Location.Y + offsetY);
        }
    }
}
