using System;
using System.Windows.Forms;

namespace PAT.Main
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] files)
        {
            Application.SetCompatibleTextRenderingDefault(false);
            Application.EnableVisualStyles();
            Application.DoEvents(); 
            Application.Run(new FormMain());
        }
    }
}