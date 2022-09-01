using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;

namespace eSheep
{
    public partial class Form1 : Form
    {
        public static Image sprites;
        public static Random rand = new Random();
        public bool silent = false;

        SheepManager manager = new SheepManager();
        Thread processor;
        bool running = false;

        public Form1(int SheepCount, bool silent)
        {
            InitializeComponent();
            String[] res = GetType().Assembly.GetManifestResourceNames();
            foreach (String r in res)
            {
                if (r.Contains("StraySheep"))
                {
                    sprites = new Bitmap(GetType().Assembly.GetManifestResourceStream(r));
                    break;
                }
            }
            this.silent = silent;
            if (SheepCount > 0)
            {
                this.Opacity = 0;
                this.ShowInTaskbar = false;
                this.WindowState = FormWindowState.Minimized;
                numericUpDown1.Value = SheepCount;
                button1_Click(null, null);
            }
        }
        public void ProcessSheep()
        {
            int cnt = (int)numericUpDown1.Value;
            if (!silent)
            {
                for (int x = 0; x < cnt; x++)
                    manager.AddSheep();
            }
            else
            {
                manager.AddSheep(true);
                timer1.Enabled = true;
            }
            while (running)
            {
                lock (manager)
                {
                    manager.Update();
                }
                Thread.Sleep(50);
            }
            for (int x = 0; x < cnt; x++)
                manager.RemoveSheep();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "Start")
            {
                button1.Text = "Stop";
                running = true;
                processor = new Thread(new ThreadStart(this.ProcessSheep));
                processor.Start();
            }
            else
            {
                running = false;
                button1.Text = "Start";
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            running = false;
            while (processor != null && processor.IsAlive)
                Thread.Sleep(50);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lock (manager)
            {
                manager.AddSheep(true);
                timer1.Interval = rand.Next(1000, 5000);
                if (manager.sheep.Count == numericUpDown1.Value)
                    timer1.Enabled = false;
            }
        }
    }
}
