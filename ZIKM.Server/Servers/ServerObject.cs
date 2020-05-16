using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Server.Clients;
using ZIKM.Server.Infrastructure;
using ZIKM.Server.Infrastructure.Interfaces;
using ZIKM.Server.Servers.Providers;
using ZIKM.Server.Utils;

namespace ZIKM.Server.Servers {
    abstract class ServerObject {
        protected readonly IAuthorization authorization;
        protected readonly ICaptcha captcha;

        protected ServerObject() {
            captcha = IoC.GetService<ICaptcha>();
            authorization = IoC.GetService<IAuthorization>();
        }

        /// <summary>
        /// Get IP address of local machine
        /// </summary>
        /// <returns>IP address</returns>
        protected IPAddress GetLocalIPAddress() {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(c => c.NetworkInterfaceType != NetworkInterfaceType.Loopback && c.OperationalStatus == OperationalStatus.Up)
                .Select(i => i.GetIPProperties()).First().UnicastAddresses
                .Where(c => c.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(j => j.Address).First();
        }

        /// <summary>
        /// Get client by user name
        /// </summary>
        /// <param name="name">User name</param>
        /// <param name="provider">Provider for communication with user</param>
        /// <returns></returns>
        public Client GetClient(string name, IProvider provider) {
            return name switch {
                "Master" => new MasterClient(provider),
                "Senpai" => new SenpaiClient(provider),
                "Kouhai" => new KouhaiClient(provider),
                _ => new UserClient(provider, name),
            };
        }

        /// <summary>
        /// Start client session
        /// </summary>
        /// <param name="provider">Provider for communication with client</param>
        protected void ClientSession(IProvider provider) {
            try {
                provider.PrepareProtecting();
                while (true) {
                    provider.SendCaptcha(captcha.GetCaptcha(out string captchaCode));

                    // Read login request
                    bool? result = provider.GetLoginRequest(out LoginData loginData);
                    if (!result.HasValue)
                        break;
                    if (!result.Value)
                        continue;

                    var resault = authorization.SingIn(loginData.User, loginData.Password);
                    if (resault.Code == 0) {
                        if (loginData.Captcha == captchaCode) {
                            GetClient(loginData.User, provider).StartSession();
                        }
                        else {
                            Logger.LogAll(LogMessages.WrongCaptcha(loginData.User));
                            provider.SendResponse(new ResponseData(StatusCode.BadData, Messages.WrongCaptcha));
                        }
                    }
                    else {
                        provider.SendResponse(resault);
                    }
                }
                Logger.LogAll(LogMessages.Disconnected);
            }
            catch (IOException ex) when (ex.InnerException is SocketException inner && inner.ErrorCode == 10054) {
                Logger.LogAll(LogMessages.LostConnection);
            }
            catch (Exception ex) {
                Logger.LogError(ex.Message);
                Logger.LogError(ex.InnerException?.Message);
            }
            provider.Dispose();
        }

        /// <summary>
        /// Start server
        /// </summary>
        public abstract void Start();
    }
}
