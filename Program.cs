using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using ZIKM.Permissions;
using ZIKM.Infrastructure;

namespace ZIKM
{
    class Program{
        static int port = 8000;
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
                    NetworkStream stream = client.GetStream();
                    Provider provider = new Provider(stream);

                    var captcha = Captcha.Send(stream);

                    // Read login request
                    LoginData userData; //= JsonConvert.DeserializeObject<LoginData>("{ User: \"\", Password: \"\", Captcha: \"\" }");
                    try{ 
                        userData = JsonConvert.DeserializeObject<LoginData>(provider.GetRequest());
                    }
                    catch (JsonReaderException){
                        provider.SendResponse(new ResponseData(-2, "Invalid request"));
                        Logger.ToLogAll("Invalid request");
                        stream.Close();
                        client.Close();
                        continue;
                    }

                    string account = userData.User;
                    string password = userData.Password;
                    string captchaResponse = userData.Captcha;

                    if (account == null || password == null || captchaResponse == null){
                        provider.SendResponse(new ResponseData(-2, "Invalid request"));
                        Logger.ToLogAll("Invalid request");
                        stream.Close();
                        client.Close();
                        continue;
                    }

                    try{
                        if (passwordsBase[account] != null) {
                            if (passwordsBase[account].Count == 0){
                                // User spent all passwords
                                switch (account){
                                    case "Master":
                                        provider.SendResponse(new ResponseData(2, "Don't think about this"));
                                        Logger.ToLogAll("Fake master");
                                        break;
                                    case "Senpai":
                                        provider.SendResponse(new ResponseData(2, "Impostor!"));
                                        Logger.ToLogAll("Impostor");
                                        break;
                                    case "Kouhai":
                                        provider.SendResponse(new ResponseData(2, "Liar!!!!X|"));
                                        Logger.ToLogAll("Liar");
                                        break;
                                    default:
                                        provider.SendResponse(new ResponseData(2, "Blocked"));
                                        Logger.ToLogAll($"{account} blocked");
                                        break;
                                }
                            }
                            else{
                                if (passwordsBase[account][0] == password && captchaResponse == captcha){
                                    // Successful login
                                    var guid = Guid.NewGuid();
                                    switch (account){
                                        case "Master":
                                            provider.SendResponse(new ResponseData(guid, 0, "Welcome, Master."));
                                            Logger.ToLog("Master here");
                                            MasterPermission master = new MasterPermission(provider, guid);
                                            master.StartSession();
                                            provider.SendResponse(new ResponseData(guid, 0, "I will wait your return, Master."));
                                            Logger.ToLog("Master gone");
                                            break;
                                        case "Senpai": 
                                            provider.SendResponse(new ResponseData(guid, 0, "Senpai!!!XD"));
                                            Logger.ToLog("Sempai back");
                                            SenpaiPermission senpai = new SenpaiPermission(provider, guid);
                                            senpai.StartSession();
                                            provider.SendResponse(new ResponseData(guid, 0, "Senpai! I will wait!!!"));
                                            Logger.ToLog("Sempai gone");
                                            break;
                                        case "Kouhai": 
                                            provider.SendResponse(new ResponseData(guid, 0, "Sempai is waitting you)"));
                                            Logger.ToLog("Pervered kouhai here");
                                            KouhaiPermission kouhai = new KouhaiPermission(provider, guid);
                                            kouhai.StartSession();
                                            provider.SendResponse(new ResponseData(guid, 0, "Be carefull, my kouhai."));
                                            Logger.ToLog("Pervered kouhai gone");
                                            break;
                                        default: 
                                            provider.SendResponse(new ResponseData(guid, 0, $"You {account}"));
                                            Logger.ToLog($"{account} here");
                                            UserPermission user = new UserPermission(provider, guid);
                                            user.StartSession();
                                            provider.SendResponse(new ResponseData(guid, 0, $"Bye {account}"));
                                            Logger.ToLog($"{account} disconnect");
                                            break;
                                    }
                                }
                                else{
                                    if (passwordsBase[account].Count == 1){
                                        // User's spent last password 
                                        provider.SendResponse(new ResponseData(-2, "You blocked"));
                                        Logger.ToLogAll($"{account} blocked");
                                    }
                                    else {
                                        // User's written wrong password
                                        provider.SendResponse(new ResponseData(1, "Try again"));
                                        Logger.ToLogAll($"{account} errored");
                                    }
                                } 
                                passwordsBase[account].RemoveAt(0);
                            }
                        }
                    }
                    catch (KeyNotFoundException){
                        // No user in data
                        provider.SendResponse(new ResponseData(-1, $"No {account} in data"));
                        Logger.ToLogAll($"{account} not found");
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

        /// <summary>
        /// Get users and passwords
        /// </summary>
        /// <returns>List of users with passwords</returns>
        private static Dictionary<string, List<string>> GetPasswords (){
            var passwords = new Dictionary<string, List<string>>();
            XDocument doc = XDocument.Load(Path.Combine(Directory.GetCurrentDirectory(), "Accounts.xml"));
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
