using System;
using System.IO;

namespace ZIKM{
    class Logger{
        public static void ToLog(string text){
            
        }
    }

    static class Captcha{
        private static string path = "/home/yevgeniy/C#/ZIKM/Captchas";
        public static (string, byte[]) Get(){
            string[] files = Directory.GetFiles(path);
            var random = new Random();
            var fileInfo = new FileInfo($"{path}/{files[random.Next(0, files.Length)]}");
            string captcha = fileInfo.Name.Substring(0,5);
            File.Move($"{path}/{fileInfo.Name}" ,"/home/yevgeniy/C#/ZIKM/captcha.jpg");
            return (captcha, File.ReadAllBytes("/home/yevgeniy/C#/ZIKM/captcha.jpg"));
        }

        public static void Back(string name) => File.Move("/home/yevgeniy/C#/ZIKM/captcha.jpg", $"{path}/{name}.jpg");
    }
}