using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DataServer
{
    static class Program
    {
        public static Logger gLogger;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DataServer.Program.gLogger = new Logger("DS");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (gLogger != null)
                    gLogger.WriteToLog("Unhandled exception. " + e.ExceptionObject.ToString(), null);
            }
            catch
            {
            }
            finally
            {
                if (e.ExceptionObject != null)
                    MessageBox.Show(e.ExceptionObject.ToString(), "DataServer application error");
            }
        }
    }
}
