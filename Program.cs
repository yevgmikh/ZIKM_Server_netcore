using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using ZIKM.Permissions;

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
 
                    //byte[] data = new byte[256];
                    //StringBuilder response = new StringBuilder();
                    NetworkStream stream = client.GetStream();

                    var captcha = Captcha.Send(stream);

                    /*do
                    {
                        int bytes = stream.Read(data, 0, data.Length);
                        response.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);*/

                    dynamic userData = JsonConvert.DeserializeObject("{ User: \"\", Password: \"\", Captcha: \"\" }");
                    try{ userData = JsonConvert.DeserializeObject(Provider.GetRequest(stream)); }
                    catch (JsonReaderException){
                        Provider.SendResponse("{ Code: -2, Message: \"Invalid request\" }", stream);
                        stream.Close();
                        client.Close();
                        continue;
                    }

                    string account = userData.User;
                    string password = userData.Password;
                    string captchaResponse = userData.Captcha;

                    if (account == null || password == null || captchaResponse == null){
                        Provider.SendResponse("{ Code: -2, Message: \"Invalid request\" }", stream);
                        stream.Close();
                        client.Close();
                        continue;
                    }

                    try{
                        if (passwordsBase[account] != null) {
                            if (passwordsBase[account].Count == 0){
                                switch (account){
                                    case "Master":
                                        Provider.SendResponse("{ Code: 2, Message: \"Don't think about this\" }", stream);
                                        Logger.ToLog("Fake master");
                                        break;
                                    case "Senpai": 
                                        Provider.SendResponse("{ Code: 2, Message: \"Impostor!\" }", stream);
                                        Logger.ToLog("Impostor");
                                        break;
                                    case "Kouhai": 
                                        Provider.SendResponse("{ Code: 2, Message: \"Liar!!!!X|\" }", stream);
                                        Logger.ToLog("Liar");
                                        break;
                                    default: 
                                        Provider.SendResponse("{ Code: 2, Message: \"Blocked\" }", stream);
                                        Logger.ToLog($"{account} blocked");
                                        break;
                                }
                            }
                            else{
                                if (passwordsBase[account][0] == password && captchaResponse == captcha){
                                    switch (account){
                                        case "Master":
                                            Provider.SendResponse("{ Code: 0, Message: \"Welcome, Master.\" }", stream);
                                            Logger.ToLog("Master here");
                                            MasterPermission master = new MasterPermission(stream);
                                            master.Session();
                                            Provider.SendResponse("{ Code: 0, Message: \" I will wait your return, Master.\" }", stream);
                                            break;
                                        case "Senpai": 
                                            Provider.SendResponse("{ Code: 0, Message: \"Senpai!!!XD\" }", stream);
                                            Logger.ToLog("Sempai back");
                                            SenpaiPermission senpai = new SenpaiPermission(stream);
                                            senpai.Session();
                                            Provider.SendResponse("{ Code: 0, Message: \"Senpai! I will wait!!!\" }", stream);
                                            break;
                                        case "Kouhai": 
                                            Provider.SendResponse("{ Code: 0, Message: \"Sempai is waitting you)\" }", stream);
                                            Logger.ToLog("Pervered kouhai here");
                                            KouhaiPermission kouhai = new KouhaiPermission(stream);
                                            kouhai.Session();
                                            Provider.SendResponse("{ Code: 0, Message: \"Be carefull, my kouhai.\" }", stream);
                                            break;
                                        default: 
                                            Provider.SendResponse($"{{ Code: 0, Message: \"You {account}\" }}", stream);
                                            Logger.ToLog($"{account} here");
                                            UserPermission user = new UserPermission(stream);
                                            user.Session();
                                            Provider.SendResponse($"{{ Code: 0, Message: \"Bye {account}\" }}", stream);
                                            break;
                                    }
                                }
                                else{
                                    if (passwordsBase[account].Count == 1){
                                        Provider.SendResponse("{ Code: -2, Message: \"You blocked\" }", stream);
                                        Logger.ToLog($"{account} blocked");
                                    }
                                    else {
                                        Provider.SendResponse("{ Code: 1, Message: \"Try again\" }", stream);
                                        Logger.ToLog($"{account} errored");
                                    }
                                } 
                                passwordsBase[account].RemoveAt(0);
                            }
                        }
                    }
                    catch (KeyNotFoundException){
                        Provider.SendResponse($"{{ Code: -1, Message: \"No {account} in data\" }}", stream);
                        Logger.ToLog($"{account} not found");
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
