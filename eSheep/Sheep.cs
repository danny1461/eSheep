using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace eSheep
{
    class Sheep
    {
        public SheepManager manager;
        public static int SheepSize = 40;
        public int ID = -1;
        public bool dragging = false;

        public Rectangle BoundingBox;
        public SheepState CurrentState;
        public SheepState NextState = null;

        public SheepRepresentation window;

        public Sheep(SheepManager manager, bool offScreen)
        {
            if (offScreen)
                BoundingBox = new Rectangle(Form1.rand.Next(0, SystemInformation.VirtualScreen.Width - SheepSize),
                                        -300,
                                        SheepSize, SheepSize);
            else
                BoundingBox = new Rectangle(Form1.rand.Next(0, SystemInformation.VirtualScreen.Width - SheepSize),
                                        Form1.rand.Next(0, SystemInformation.VirtualScreen.Height - SheepSize),
                                        SheepSize, SheepSize);

            CurrentState = new IdleState(this, Form1.rand.Next(0, 2) == 0 ? -1 : 1);
            this.manager = manager;
            this.window = new SheepRepresentation();
            this.window.Show();
            this.window.Location = new Point(BoundingBox.Left, BoundingBox.Top);
            this.window.Size = new Size(SheepSize, SheepSize);
            this.ID = manager.NextSheepId;
        }
        public void Update()
        {
            do
            {
                // Handle Sheep Dragging Events
                if (window.dragging)
                {
                    if (!dragging)
                    {
                        NextState = new DraggingState(this, CurrentState.Direction);
                        dragging = true;
                    }
                    else
                        BoundingBox.Location = window.Location;
                }
                else if (dragging)
                {
                    NextState = new IdleState(this, CurrentState.Direction);
                    dragging = false;
                }
                // Switch to next state if available
                if (NextState != null)
                {
                    CurrentState = NextState;
                    NextState = null;
                    CurrentState.StartAnimation();
                }
                CurrentState.Update();
            }
            while (NextState != null);
            
            // Update our window
            window.Location = BoundingBox.Location;
            if (CurrentState.DrawSprite)
            {
                Bitmap bitmap = new Bitmap(SheepSize, SheepSize);
                Graphics g = Graphics.FromImage(bitmap);
                g.DrawImage(
                        Form1.sprites,
                        new Rectangle(0, 0, SheepSize, SheepSize),
                        new Rectangle(CurrentState.CurrentAnimation.X * 40, CurrentState.CurrentAnimation.Y * 40, 40, 40),
                        GraphicsUnit.Pixel);
                g.Dispose();
                if (CurrentState.Direction == 1)
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                window.BackgroundImage = bitmap;
            }
        }
        public void Delete()
        {
            CurrentState = null;
            window.Close();
            window.Dispose();
        }
        public void ChangeState(SheepState newState)
        {
            if (NextState != null)
                return;
            NextState = newState;
        }
        public void ForceChangeState(SheepState newState)
        {
            NextState = newState;
        }
    }
}
