using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ZIKM.Permissions;
using ZIKM.Infrastructure;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ZIKM.Infrastructure.Interfaces;
using ZIKM.Infrastructure.Providers;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Storages.Authorization;

namespace ZIKM{
    class Program{
        private static IAuthorization authorization;
        private static readonly Storage storage = Enum.Parse<Storage>(new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()["Storage"]);

        static void Main(string[] args){
            Client.StorageType = storage;
            Logger.ToLog($"Storage type: {storage}");

            TcpListener server=null;
            try{
                server = new TcpListener(GetLocalIPAddress(), 8000);
                GetPasswords();
 
                server.Start();
                Logger.ToLog("Server started");
 
                while (true){
                    TcpClient client = server.AcceptTcpClient();
                    Task.Run(() => Process(client));
                }
            }
            catch (Exception ex){
                Logger.ToLogAll(ex.Message);
                Logger.ToLogAll(ex.StackTrace);
            }
            finally{
                if (server != null)
                    server.Stop();
                Logger.ToLog("Server stoped");
            }
        }

        public static IPAddress GetLocalIPAddress(){
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(c => c.NetworkInterfaceType != NetworkInterfaceType.Loopback && c.OperationalStatus == OperationalStatus.Up)
                .Select(i => i.GetIPProperties()).First().UnicastAddresses
                .Where(c => c.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(j => j.Address).First();
        }

        /// <summary>
        /// Get users and passwords to password base
        /// </summary>
        /// <returns>List of users with passwords</returns>
        private static void GetPasswords(){
            switch (storage){
                case Storage.Files:
                    authorization = UserAuthorizationStorage.GetAuthorization();
                    return;
                case Storage.InternalDB:
                    throw new NotImplementedException();
                case Storage.ExternalDB:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Login and start session for client
        /// </summary>
        /// <param name="client"></param>
        private static void Process(TcpClient client){
            using IProvider provider = new TCPProvider(client);
            ICaptcha captcha = new PrimitiveCaptcha((TCPProvider)provider);
            while (true){
                try{
                    var captchaCode = captcha.SendCaptcha();

                    // Read login request
                    if (!provider.GetLoginRequest(out LoginData loginData))
                        return;

                    var resault = authorization.SingIn(loginData.User, loginData.Password);
                    if (resault.Code == 0){
                        if (loginData.Captcha == captchaCode){
                            #region Successfull login
                            switch (loginData.User){
                                case "Master":
                                    new MasterPermission(provider).StartSession();
                                    break;
                                case "Senpai":
                                    new SenpaiPermission(provider).StartSession();
                                    break;
                                case "Kouhai":
                                    new KouhaiPermission(provider).StartSession();
                                    break;
                                default:
                                    new UserPermission(provider, loginData.User).StartSession();
                                    break;
                            }
                            #endregion
                        }
                        else{
                            Logger.ToLogAll("Wrong captcha code");
                            provider.SendResponse(new ResponseData(1, "Wrong captcha code"));
                        }
                    }
                    else{
                        provider.SendResponse(resault);
                    }
                }
                catch (Exception ex){
                    Logger.ToLogAll(ex.Message);
                    Logger.ToLogAll(ex.StackTrace);
                    return;
                }
            }
        }
    }
}
