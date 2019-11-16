using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using ZIKM.Infrastructure;

namespace ZIKM{
    class Logger{
        public static void ToLog(string text){
            using (StreamWriter writer = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), @"Logs\LogSuccsess.log"), true)) 
                writer.WriteLine(DateTime.Now.ToString() + $": {text}");
            ToLogAll(text);
        }

        public static void ToLogAll(string text){
            Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {text}");
            using (StreamWriter writer = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), @"Logs\LogAll.log"), true)) 
                writer.WriteLine(DateTime.Now.ToString() + $": {text}");
        }

        public static void ToLogProvider (string text) {
            using (StreamWriter writer = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), @"Logs\LogProvider.log"), true)) 
                writer.WriteLine(DateTime.Now.ToString() + $": {text}");
        }
    }

    class Provider{
        private byte[] _data;
        private StringBuilder _response;
        private readonly NetworkStream stream;

        public Provider(NetworkStream stream)
        {
            this.stream = stream;
        }

        public void SendResponse(ResponseData response){
            string JsonString = JsonConvert.SerializeObject(response);
            _data = Encoding.UTF8.GetBytes(JsonString);
            stream.Write(_data, 0, _data.Length);
            Logger.ToLogProvider(JsonString);
        }

        public string GetRequest(){
            _data = new byte[100000];
            _response = new StringBuilder();
            int bytes = stream.Read(_data, 0, _data.Length);
            _response.Append(Encoding.UTF8.GetString(_data, 0, bytes));
            Logger.ToLogProvider(_response.ToString());
            return _response.ToString();
        }
    }

    static class Captcha{
        private static string _path = Path.Combine(Directory.GetCurrentDirectory(), "Captchas");
        public static string Send(NetworkStream stream){
            string[] files = Directory.GetFiles(_path);
            var random = new Random();
            var fileInfo = new FileInfo(files[random.Next(0, files.Length)]);
            string captcha = fileInfo.Name.Substring(0,5);
            File.Move($@"{_path}]{fileInfo.Name}" , Path.Combine(Directory.GetCurrentDirectory(), "captcha.jpg"));
            var data = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory(), "captcha.jpg"));
            stream.Write(data, 0, data.Length);
            File.Move(Path.Combine(Directory.GetCurrentDirectory(), "captcha.jpg"), $@"{_path}\{captcha}.jpg");
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