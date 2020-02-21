using System;
using System.IO;

namespace ZIKM.Server.Utils {
    static class Logger {
        private static readonly string logSuccessPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "LogSuccsess");
        private static readonly string logAllPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "LogAll");

        static Logger() {
            Directory.CreateDirectory(logSuccessPath);
            Directory.CreateDirectory(logAllPath);
        }

        /// <summary>
        /// Write all operation 
        /// </summary>
        /// <param name="text">Log entry</param>
        /// <param name="type">Log type</param>
        private static void ToLog(string text, string type) {
            using StreamWriter writer = new StreamWriter(Path.Combine(logAllPath, $"{DateTime.Today.ToString("dd-MM-yyyy")}.log"), true);
            writer.WriteLine(DateTime.Now.ToString() + $"| {type} : {text}");
        }

        /// <summary>
        /// Write succesfull operation
        /// </summary>
        /// <param name="text">Log entry</param>
        public static void Log(string text) {
            ToLog(text, "SO");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {text}");

            using StreamWriter writer = new StreamWriter(Path.Combine(logSuccessPath, $"{DateTime.Today.ToString("dd-MM-yyyy")}.log"), true);
            writer.WriteLine(DateTime.Now.ToString() + $": {text}");
        }

        /// <summary>
        /// Write all operation
        /// </summary>
        /// <param name="text">Log entry</param>
        public static void LogAll(string text) {
            ToLog(text, "AO");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {text}");
        }

        /// <summary>
        /// Write a information log message.
        /// </summary>
        /// <param name="text">Log entry</param>
        public static void LogInformation(string text) {
            ToLog(text, "Information");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {text}");
        }

        /// <summary>
        /// Write a warning log message.
        /// </summary>
        /// <param name="text">Log entry</param>
        public static void LogWarning(string text) {
            ToLog(text, "Warning");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {text}");
        }

        /// <summary>
        /// Write a error log message.
        /// </summary>
        /// <param name="text">Log entry</param>
        public static void LogError(string text) {
            ToLog(text, "Error");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {text}");
        }

        /// <summary>
        /// Write a critical error log message.
        /// </summary>
        /// <param name="text">Log entry</param>
        public static void LogCritical(string text) {
            ToLog(text, "Critical");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {text}");
        }
    }
}