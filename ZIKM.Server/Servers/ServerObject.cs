using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Server.Clients;
using ZIKM.Server.Infrastructure;
using ZIKM.Server.Infrastructure.Interfaces;
using ZIKM.Server.Utils;

namespace ZIKM.Server.Servers {
    abstract class ServerObject {
        protected readonly IAuthorization authorization;
        protected readonly ICaptcha captcha;

        protected ServerObject() {
            captcha = IoC.GetService<ICaptcha>();
            authorization = IoC.GetService<IAuthorization>();

            Client.HandlerActionError += HandleDisconnect;
            Client.HandlerFuncBoolError += HandleDisconnect;
        }

        /// <summary>
        /// Handle unexpected client's exceptions
        /// </summary>
        /// <param name="operation">Tracked operation</param>
        protected abstract void HandleErrors(Action operation);

        /// <summary>
        /// Handle unexpected client's losting connection
        /// </summary>
        /// <param name="operation">Tracked operation</param>
        /// <param name="handler">Exception handler</param>
        protected abstract void HandleDisconnect(Action operation, Action<Exception> handler);

        /// <summary>
        /// Handle unexpected client's losting connection
        /// </summary>
        /// <param name="operation">Tracked operation</param>
        /// /// <param name="handler">Exception handler</param>
        protected abstract bool HandleDisconnect(Func<bool> operation, Func<Exception, bool> handler);

        /// <summary>
        /// Start server
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Get client by user name
        /// </summary>
        /// <param name="name">User name</param>
        /// <param name="provider">Provider for communication with user</param>
        /// <returns></returns>
        private Client GetClient(string name, IProvider provider) {
            return name switch {
                "Master" => new MasterClient(provider),
                "Senpai" => new SenpaiClient(provider),
                "Kouhai" => new KouhaiClient(provider),
                _ => new UserClient(provider, name),
            };
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
        /// Start client session
        /// </summary>
        /// <param name="provider">Provider for communication with client</param>
        protected void ClientSession(IProvider provider) {
            HandleErrors(() => {
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
            });
            provider.Dispose();
        }
    }
}
