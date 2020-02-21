using System.Text.Json;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Server.Infrastructure;
using ZIKM.Server.Infrastructure.Interfaces;
using ZIKM.Server.Utils;

namespace ZIKM.Server.Servers.Providers {
    public static class ProviderExtension {
        /// <summary>
        /// Get user request
        /// </summary>
        /// <param name="provider">Provider</param>
        /// <param name="userData">User request data</param>
        /// <returns>Status of request</returns>
        public static bool GetRequest(this IProvider provider, out RequestData userData) {
            try {
                userData = provider.GetRequest();
                return true;
            }
            catch (JsonException) {
                provider.SendResponse(new ResponseData(StatusCode.BadRequest, Messages.InvalidRequest));
                Logger.LogAll(LogMessages.InvalidRequest);
                userData = new RequestData();
                return false;
            }
        }

        /// <summary>
        /// Get login request
        /// </summary>
        /// <param name="provider">Provider</param>
        /// <param name="loginData">Login request data</param>
        /// <returns>Status of request</returns>
        public static bool? GetLoginRequest(this IProvider provider, out LoginData loginData) {
            try {
                loginData = provider.GetLoginRequest();
                if (loginData.User == null || loginData.Password == null || loginData.Captcha == null) {
                    provider.SendResponse(new ResponseData(StatusCode.BadRequest, Messages.InvalidRequest));
                    Logger.LogAll(LogMessages.InvalidRequest);
                    return false;
                }
                return true;
            }
            catch (JsonException e) when (e.HResult == -2146233088) {
                return null;
            }
            catch (JsonException) {
                provider.SendResponse(new ResponseData(StatusCode.BadRequest, Messages.InvalidRequest));
                Logger.LogAll(LogMessages.InvalidRequest);
                loginData = new LoginData();
                return false;
            }
        }
    }
}
