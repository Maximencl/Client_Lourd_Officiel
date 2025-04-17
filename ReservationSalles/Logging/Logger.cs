using System;
using System.IO;
using System.Text;

namespace ReservationSalles.Services
{
    public static class Logger
    {
        private static readonly string logFile = "application.log";

        /// <summary>
        /// Ecrit un message horodaté dans un fichier "application.log".
        /// </summary>
        public static void Log(string message)
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            File.AppendAllText(logFile, line + Environment.NewLine, Encoding.UTF8);
        }
    }
}