using System;
using System.IO;

namespace ZIKM{
    static class Logger{
        private static readonly string logSuccessPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "LogSuccsess");
        private static readonly string logAllPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "LogAll");
        private static readonly string logProviderPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "LogProvider");

        static Logger(){
            Directory.CreateDirectory(logSuccessPath);
            Directory.CreateDirectory(logAllPath);
            Directory.CreateDirectory(logProviderPath);
        }

        /// <summary>
        /// Write succesfull operation
        /// </summary>
        /// <param name="text">Log entry</param>
        public static void ToLog(string text){
            ToLogAll(text);
            using StreamWriter writer = new StreamWriter(Path.Combine(logSuccessPath, $"{DateTime.Today.ToLongDateString()}.log"), true);
            writer.WriteLine(DateTime.Now.ToString() + $": {text}");
        }

        /// <summary>
        /// Write all operation 
        /// </summary>
        /// <param name="text">Log entry</param>
        public static void ToLogAll(string text){
            Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {text}");
            using StreamWriter writer = new StreamWriter(Path.Combine(logAllPath, $"{DateTime.Today.ToLongDateString()}.log"), true);
            writer.WriteLine(DateTime.Now.ToString() + $": {text}");
        }

        /// <summary>
        /// Write provider comunication operation
        /// </summary>
        /// <param name="text">Log entry</param>
        public static void ToLogProvider (string text) {
            using StreamWriter writer = new StreamWriter(Path.Combine(logProviderPath, $"{DateTime.Today.ToLongDateString()}.log"), true);
            writer.WriteLine(DateTime.Now.ToString() + $": {text}");
        }
    }
}