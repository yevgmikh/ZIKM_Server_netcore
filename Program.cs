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
using ZIKM.Interfaces;
using System.Threading.Tasks;

namespace ZIKM{
    class Program{
        static int port = 8000;
        static Dictionary<string, List<string>> passwordsBase;

        static void Main(string[] args){
            TcpListener server=null;
            try{
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                server = new TcpListener(localAddr, port);
                passwordsBase = GetPasswords();
 
                server.Start();
                Console.WriteLine("Server started");
 
                while (true){
                    TcpClient client = server.AcceptTcpClient();
                    Task.Run(() => Process(client));
                }
            }
            catch (Exception e){
                Console.WriteLine(e.Message);
            }
            finally{
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

        /// <summary>
        /// Login and get session for client
        /// </summary>
        /// <param name="client"></param>
        private static void Process(TcpClient client){
            using (IProvider provider = new TCPProvider(client)){
                ICaptcha captcha = new PrimitiveCaptcha((TCPProvider)provider);
                var captchaCode = captcha.SendCaptcha();

                // Read login request
                LoginData userData;
                try{
                    userData = provider.GetLoginRequest();
                    if (userData.User == null || userData.Password == null || userData.Captcha == null){
                        provider.SendResponse(new ResponseData(-2, "Invalid request"));
                        Logger.ToLogAll("Invalid request");
                        return;
                    }
                }
                catch (JsonReaderException){
                    provider.SendResponse(new ResponseData(-2, "Invalid request"));
                    Logger.ToLogAll("Invalid request");
                    return;
                }

                try{
                    if (passwordsBase[userData.User] != null){
                        if (passwordsBase[userData.User].Count == 0){
                            #region User's spent all passwords
                            switch (userData.User)
                            {
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
                                    Logger.ToLogAll($"{userData.User} blocked");
                                    break;
                            }
                            #endregion
                        }
                        else{
                            if (passwordsBase[userData.User][0] == userData.Password && userData.Captcha == captchaCode){
                                #region Successfull login
                                var guid = Guid.NewGuid();
                                switch (userData.User)
                                {
                                    case "Master":
                                        MasterPermission master = new MasterPermission(provider, guid);
                                        master.StartSession();
                                        break;
                                    case "Senpai":
                                        SenpaiPermission senpai = new SenpaiPermission(provider, guid);
                                        senpai.StartSession();
                                        break;
                                    case "Kouhai":
                                        KouhaiPermission kouhai = new KouhaiPermission(provider, guid);
                                        kouhai.StartSession();
                                        break;
                                    default:
                                        UserPermission user = new UserPermission(provider, guid, userData.User);
                                        user.StartSession();
                                        break;
                                }
                                #endregion
                            }
                            else{
                                if (passwordsBase[userData.User].Count == 1){
                                    // User's spent last password 
                                    provider.SendResponse(new ResponseData(-2, "You blocked"));
                                    Logger.ToLogAll($"{userData.User} blocked");
                                }
                                else{
                                    // User's written wrong password
                                    provider.SendResponse(new ResponseData(1, "Try again"));
                                    Logger.ToLogAll($"{userData.User} errored");
                                }
                            }
                            passwordsBase[userData.User].RemoveAt(0);
                        }
                    }
                }
                catch (KeyNotFoundException){
                    // No user in data
                    provider.SendResponse(new ResponseData(-1, $"No {userData.User} in data"));
                    Logger.ToLogAll($"{userData.User} not found");
                }
            }
        }
    }
}
