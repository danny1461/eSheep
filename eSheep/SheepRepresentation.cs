using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace eSheep
{
    public partial class SheepRepresentation : Form
    {
        public bool dragging = false;
        Point offset;

        public SheepRepresentation()
        {
            InitializeComponent();
            //SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.TransparencyKey = SystemColors.Control;
        }

        private void SheepRepresentation_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            offset = new Point(e.X, e.Y);
        }

        private void SheepRepresentation_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void SheepRepresentation_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                this.Location = new Point(MousePosition.X - offset.X, MousePosition.Y - offset.Y);
            }
        }
    }
}
