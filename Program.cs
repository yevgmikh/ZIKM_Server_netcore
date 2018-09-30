using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace ZIKM
{
    class Program{
        static int port = 8005;
        static void Main(string[] args){
            TcpListener server=null;
            try
            {
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                server = new TcpListener(localAddr, port);
                var passwordsBase = GetPasswords();
 
                // запуск слушателя
                server.Start();
                Console.WriteLine("Server started");
 
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
 
                    byte[] data = new byte[256];
                    StringBuilder response = new StringBuilder();
                    NetworkStream stream = client.GetStream();

                    var captcha = Captcha.Get();
                    stream.Write(captcha.Item2, 0, captcha.Item2.Length);
                    Captcha.Back(captcha.Item1);

                    do
                    {
                        int bytes = stream.Read(data, 0, data.Length);
                        response.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    dynamic userData = JsonConvert.DeserializeObject("{ User: \"\", Password: \"\", Captcha: \"\" }");
                    try{ userData = JsonConvert.DeserializeObject(response.ToString()); }
                    catch (JsonReaderException){
                        data = Encoding.UTF8.GetBytes("{ Code: -2, Message: \"Invalid request\" }");
                        stream.Write(data, 0, data.Length);
                        stream.Close();
                        client.Close();
                        continue;
                    }

                    string account = userData.User;
                    string password = userData.Password;
                    string captchaResponse = userData.Captcha;

                    if (account == null || password == null || captchaResponse == null){
                        data = Encoding.UTF8.GetBytes("{ Code: -2, Message: \"Invalid request\" }");
                        stream.Write(data, 0, data.Length);
                        stream.Close();
                        client.Close();
                        continue;
                    }

                    try{
                        if (passwordsBase[account] != null) {
                            if (passwordsBase[account].Count == 0){
                                switch (account){
                                    case "Senpai": 
                                        data = Encoding.UTF8.GetBytes("{ Code: 2, Message: \"Impostor!\" }");
                                        stream.Write(data, 0, data.Length);
                                        Console.WriteLine(DateTime.Now.ToShortTimeString() + ": Impostor");
                                        break;
                                    case "Kouhai": 
                                        data = Encoding.UTF8.GetBytes("{ Code: 2, Message: \"Liar!!!!X|\" }");
                                        stream.Write(data, 0, data.Length);
                                        Console.WriteLine(DateTime.Now.ToShortTimeString() + ": Liar");
                                        break;
                                    default: 
                                        data = Encoding.UTF8.GetBytes("{ Code: 2, Message: \"Blocked\" }");
                                        stream.Write(data, 0, data.Length);
                                        Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {account} blocked");
                                        break;
                                }
                            }
                            else{
                                if (passwordsBase[account][0] == password && captchaResponse == captcha.Item1){
                                    switch (account){
                                        case "Senpai": 
                                            data = Encoding.UTF8.GetBytes("{ Code: 0, Message: \"Senpai!!!XD\" }");
                                            stream.Write(data, 0, data.Length);
                                            Console.WriteLine(DateTime.Now.ToShortTimeString() + ": Sempai back");
                                            break;
                                        case "Kouhai": 
                                            data = Encoding.UTF8.GetBytes("{ Code: 0, Message: \"Sempai is waitting you)\" }");
                                            stream.Write(data, 0, data.Length);
                                            Console.WriteLine(DateTime.Now.ToShortTimeString() + ": Pervered kouhai here");
                                            break;
                                        default: 
                                            data = Encoding.UTF8.GetBytes($"{{ Code: 0, Message: \"You {account}\" }}");
                                            stream.Write(data, 0, data.Length);
                                            Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {account} here");
                                            break;
                                    }
                                }
                                else{
                                    if (passwordsBase[account].Count == 1){
                                        data = Encoding.UTF8.GetBytes("{ Code: -2, Message: \"You blocked\" }");
                                        stream.Write(data, 0, data.Length);
                                        Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {account} blocked");
                                    }
                                    else {
                                        data = Encoding.UTF8.GetBytes("{ Code: 1, Message: \"Try again\" }");
                                        stream.Write(data, 0, data.Length);
                                    }
                                } 
                                passwordsBase[account].RemoveAt(0);
                            }
                        }
                    }
                    catch (KeyNotFoundException){
                        data = Encoding.UTF8.GetBytes($"{{ Code: -1, Message: \"No {account} in data\" }}");
                        stream.Write(data, 0, data.Length);
                    }

                    stream.Close();
                    client.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (server != null)
                    server.Stop();
            }
        }

        private static Dictionary<string, List<string>> GetPasswords (){
            var passwords = new Dictionary<string, List<string>>();
            XDocument doc = XDocument.Load("Accounts.xml");
            foreach (var acc in doc.Element("Accounts").Elements("Account")){
                var user = new List<string>();
                foreach (var pass in acc.Element("Passwords").Elements("Password")) user.Add(pass.Value);
                passwords.Add(acc.Attribute("Name").Value, user);
                //return passwords;
            }
            return passwords;
        }
    }
}
