/*using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ZIKM
{
    class Program{
        static int port = 8005;
        static void Main(string[] args){
            //var captcha = Captcha.Get();


            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("192.168.31.19"), port);
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try{
                listenSocket.Bind(ipPoint);
                listenSocket.Listen(10);
                var passwordsBase = GetPasswords();
                Console.WriteLine("Server started");
                while(true){
                    Socket handler = listenSocket.Accept();
                    

                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    byte[] data = new byte[256];                    

                    do{
                        bytes = handler.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    } while (handler.Available>0);
                    
                    dynamic userData = JsonConvert.DeserializeObject("{ User: \"\", Password: \"\" }");
                    try{ userData = JsonConvert.DeserializeObject(builder.ToString()); }
                    catch (JsonReaderException){ 
                        handler.Send(Encoding.Unicode.GetBytes("{ Code: -2, Message: \"Invalid request\" }"));
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                        continue;
                    }
                    
                    string account = userData.User;
                    string password = userData.Password;

                    if (account == null || password == null){
                        handler.Send(Encoding.Unicode.GetBytes("{ Code: -2, Message: \"Invalid request\" }"));
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                        continue;
                    }

                    try{
                        if (passwordsBase[account] != null) {
                            if (passwordsBase[account].Count == 0){
                                switch (account){
                                    case "Senpai": 
                                        handler.Send(Encoding.Unicode.GetBytes("{ Code: 2, Message: \"Impostor!\" }"));
                                        Console.WriteLine(DateTime.Now.ToShortTimeString() + ": Impostor");
                                        break;
                                    case "Kouhai": 
                                        handler.Send(Encoding.Unicode.GetBytes("{ Code: 2, Message: \"Liar!!!!X|\" }"));
                                        Console.WriteLine(DateTime.Now.ToShortTimeString() + ": Liar");
                                        break;
                                    default: 
                                        handler.Send(Encoding.Unicode.GetBytes("{ Code: 2, Message: \"Blocked\" }"));
                                        Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {account} blocked");
                                        break;
                                }
                            }
                            else{
                                if (passwordsBase[account][0] == password){
                                    switch (account){
                                        case "Senpai": 
                                            handler.Send(Encoding.Unicode.GetBytes("{ Code: 0, Message: \"Senpai!!!XD\" }"));
                                            Console.WriteLine(DateTime.Now.ToShortTimeString() + ": Sempai back");
                                            break;
                                        case "Kouhai": 
                                            handler.Send(Encoding.Unicode.GetBytes("{ Code: 0, Message: \"Sempai is waitting you)\" }"));
                                            Console.WriteLine(DateTime.Now.ToShortTimeString() + ": Pervered kouhai here");
                                            break;
                                        default: 
                                            handler.Send(Encoding.Unicode.GetBytes($"{{ Code: 0, Message: \"You {account}\" }}"));
                                            Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {account} here");
                                            break;
                                    }
                                }
                                else{
                                    if (passwordsBase[account].Count == 1){
                                        handler.Send(Encoding.Unicode.GetBytes("{ Code: -2, Message: \"You blocked\" }"));
                                            Console.WriteLine(DateTime.Now.ToShortTimeString() + $": {account} blocked");
                                    }
                                    else handler.Send(Encoding.Unicode.GetBytes("{ Code: 1, Message: \"Try again\" }"));
                                } 
                                passwordsBase[account].RemoveAt(0);
                            }
                        }
                    }
                    catch (KeyNotFoundException){
                        handler.Send(Encoding.Unicode.GetBytes($"{{ Code: -1, Message: \"No {account} in data\" }}"));
                    }
                    
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex){
                Console.WriteLine(ex.Message);
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
}*/
