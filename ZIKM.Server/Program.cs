using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ZIKM.Infrastructure;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ZIKM.Infrastructure.Interfaces;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Services.Captcha;
using ZIKM.Services.Providers;
using ZIKM.Services.Authorization;
using ZIKM.Clients;
using ZIKM.Services.Storages.Model;

namespace ZIKM {
    class Program {
        private static readonly IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        private static IAuthorization authorization;
        private static ICaptcha captcha;
        private static readonly Storage storage = 
            Enum.Parse<Storage>(Environment.GetEnvironmentVariable("Storage") ?? configuration["Storage"]);

        static void Main(string[] args) {
            Client.StorageType = storage;
            Logger.ToLog($"Storage type: {storage}");

            TcpListener server=null;
            try {
                SetServices();
                server = new TcpListener(GetLocalIPAddress(), 8000);
 
                server.Start();
                Logger.ToLog("Server started");
 
                while (true) {
                    TcpClient client = server.AcceptTcpClient();
                    Task.Run(() => Process(client));
                }
            }
            catch (Exception ex) {
                Logger.ToLogAll(ex.Message);
                Logger.ToLogAll(ex.InnerException?.Message);
            }
            finally {
                if (server != null)
                    server.Stop();
                Logger.ToLog("Server stoped");
            }
        }

        public static IPAddress GetLocalIPAddress() {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(c => c.NetworkInterfaceType != NetworkInterfaceType.Loopback && c.OperationalStatus == OperationalStatus.Up)
                .Select(i => i.GetIPProperties()).First().UnicastAddresses
                .Where(c => c.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(j => j.Address).First();
        }

        /// <summary>
        /// Set default services
        /// </summary>
        private static void SetServices() {
            switch (storage){
                case Storage.Files:
                    authorization = UserFileStorage.Instance;
                    captcha = SimpleCaptcha.Instance;
                    return;
                case Storage.InternalDB:
                    authorization = UserDatabaseStorage.Instance;
                    captcha = GeneratedCaptcha.Instance;
                    Logger.ToLog("Using generating captcha");
                    return;
                case Storage.ExternalDB:
                    StorageContext.Connection = configuration.GetConnectionString("StorageContext");
                    authorization = UserDatabaseStorage.Instance;
                    captcha = GeneratedCaptcha.Instance;
                    Logger.ToLog("Using generating captcha");
                    return;
            }
        }

        /// <summary>
        /// Login and start session for client
        /// </summary>
        /// <param name="client"></param>
        private static void Process(TcpClient client) {
            using IProvider provider = new TCPProvider(client);
            while (true) {
                try {
                    provider.SendCaptcha(captcha.GetCaptcha(out string captchaCode));

                    // Read login request
                    if (!provider.GetLoginRequest(out LoginData loginData))
                        return;

                    var resault = authorization.SingIn(loginData.User, loginData.Password);
                    if (resault.Code == 0) {
                        if (loginData.Captcha == captchaCode){
                            #region Successfull login
                            switch (loginData.User){
                                case "Master":
                                    new MasterClient(provider).StartSession();
                                    break;
                                case "Senpai":
                                    new SenpaiClient(provider).StartSession();
                                    break;
                                case "Kouhai":
                                    new KouhaiClient(provider).StartSession();
                                    break;
                                default:
                                    new UserClient(provider, loginData.User).StartSession();
                                    break;
                            }
                            #endregion
                        }
                        else {
                            Logger.ToLogAll(LogMessages.WrongCaptcha(loginData.User));
                            provider.SendResponse(new ResponseData(StatusCode.BadData, Messages.WrongCaptcha));
                        }
                    }
                    else {
                        provider.SendResponse(resault);
                    }
                }
                catch (Exception ex) {
                    Logger.ToLogAll(ex.Message);
                    Logger.ToLogAll(ex.InnerException?.Message);
                    return;
                }
            }
        }
    }
}
