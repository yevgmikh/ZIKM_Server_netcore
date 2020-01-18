using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ZIKM.Clients;
using ZIKM.Infrastructure;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Infrastructure.Interfaces;
using ZIKM.Servers.Providers;

namespace ZIKM.Servers {
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
            while (true) {
                try {
                    provider.SendCaptcha(captcha.GetCaptcha(out string captchaCode));

                    // Read login request
                    if (!provider.GetLoginRequest(out LoginData loginData))
                        break;

                    var resault = authorization.SingIn(loginData.User, loginData.Password);
                    if (resault.Code == 0) {
                        if (loginData.Captcha == captchaCode){
                            GetClient(loginData.User, provider).StartSession();
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
                catch (IOException ex) {
                    var inner = ex.InnerException as SocketException;
                    if (inner?.ErrorCode == 10054) 
                        Logger.ToLogAll(LogMessages.Disconnected);
                    else { 
                        Logger.ToLogAll(ex.Message);
                        Logger.ToLogAll(ex.InnerException?.Message);
                    }
                    break;
                }
                catch (Exception ex) {
                    Logger.ToLogAll(ex.Message);
                    Logger.ToLogAll(ex.InnerException?.Message);
                    break;
                }
            }
            provider.Dispose();
        }

        /// <summary>
        /// Start server
        /// </summary>
        public abstract void Start();
    }
}
