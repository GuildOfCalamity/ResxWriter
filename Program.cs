using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResxWriter
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the WinForm application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmMain());
        }
        static void UnhandledException(object sender, UnhandledExceptionEventArgs e) => Logger.Instance.Write($"{(Exception)e.ExceptionObject}  IsTerminating:{e.IsTerminating}", LogLevel.Error);
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e) => Logger.Instance.Write($"ThreadException:{e.Exception?.Message}", LogLevel.Error);
    }
}
