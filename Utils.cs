using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace ZIKM{
    class Logger{
        public static void ToLog(string text){
            using (StreamWriter writer = new StreamWriter("/home/yevgeniy/C#/ZIKM/Logs/LogSuccsess.log", true)) 
                writer.WriteLine(DateTime.Now.ToString() + $": {text}");
            ToLogAll(text);
        }

        public static void ToLogAll(string text){
            Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {text}");
            using (StreamWriter writer = new StreamWriter("/home/yevgeniy/C#/ZIKM/Logs/LogAll.log", true)) 
                writer.WriteLine(DateTime.Now.ToString() + $": {text}");
        }

        public static void ToLogProvider (string text) {
            using (StreamWriter writer = new StreamWriter("/home/yevgeniy/C#/ZIKM/Logs/LogProvider.log", true)) 
                writer.WriteLine(DateTime.Now.ToString() + $": {text}");
        }
    }

    static class Provider{
        static byte[] _data;
        static StringBuilder _response;

        public static void SendResponse(string message, NetworkStream stream){
            _data = Encoding.UTF8.GetBytes(message);
            stream.Write(_data, 0, _data.Length);
            Logger.ToLogProvider(message);
        }

        public static string GetRequest(NetworkStream stream){
            _data = new byte[100000];
            _response = new StringBuilder();
            int bytes = stream.Read(_data, 0, _data.Length);
            _response.Append(Encoding.UTF8.GetString(_data, 0, bytes));
            Logger.ToLogProvider(_response.ToString());
            return _response.ToString();
        }
    }

    static class Captcha{
        private static string _path = "/home/yevgeniy/C#/ZIKM/Captchas";
        public static string Send(NetworkStream stream){
            string[] files = Directory.GetFiles(_path);
            var random = new Random();
            var fileInfo = new FileInfo(files[random.Next(0, files.Length)]);
            string captcha = fileInfo.Name.Substring(0,5);
            File.Move($"{_path}/{fileInfo.Name}" ,"/home/yevgeniy/C#/ZIKM/captcha.jpg");
            var data = File.ReadAllBytes("/home/yevgeniy/C#/ZIKM/captcha.jpg");
            stream.Write(data, 0, data.Length);
            File.Move("/home/yevgeniy/C#/ZIKM/captcha.jpg", $"{_path}/{captcha}.jpg");
            return captcha;
        }
    }

    enum Operation{
        Error = 0,
        GetFiles = 1,
        GetFolders,
        OpenFile,
        OpenFolder,
        CloseFolder,
        End
    }

    enum FileOperation
    {
        Error = -1,
        Exit = 0,
        Read = 1,
        Write,
        Edit
    }
}