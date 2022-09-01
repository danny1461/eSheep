using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace eSheep
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(args.Length >= 1 ? int.Parse(args[0]) : 0, args.Length == 2 ? bool.Parse(args[1]) : false));
        }
    }
}
