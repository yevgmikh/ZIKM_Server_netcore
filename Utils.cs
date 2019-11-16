using System;
using System.IO;

namespace ZIKM{
    class Logger{
        public static void ToLog(string text){
            using (StreamWriter writer = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "LogSuccsess.log"), true)) 
                writer.WriteLine(DateTime.Now.ToString() + $": {text}");
            ToLogAll(text);
        }

        public static void ToLogAll(string text){
            Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {text}");
            using (StreamWriter writer = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "LogAll.log"), true)) 
                writer.WriteLine(DateTime.Now.ToString() + $": {text}");
        }

        public static void ToLogProvider (string text) {
            using (StreamWriter writer = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "LogProvider.log"), true)) 
                writer.WriteLine(DateTime.Now.ToString() + $": {text}");
        }
    }
}