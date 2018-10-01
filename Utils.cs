using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ZIKM{
    class Logger{
        public static void ToLog(string text){
            Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {text}");
        }
    }

    static class Sender{
        public static void SendResponse(string message, NetworkStream stream){
            var data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
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