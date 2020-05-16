using System;
using ZIKM.Infrastructure.DataStructures;

namespace ZIKM.Server.Infrastructure.Interfaces {
    /// <summary>
    /// Sends data to client
    /// </summary>
    public interface IProvider : IDisposable {
        /// <summary>
        /// Exchange cryptography keys with the client
        /// </summary>
        void PrepareProtecting();
        /// <summary>
        /// Get data from client request
        /// </summary>
        /// <returns>Data of client's request</returns>
        RequestData GetRequest();
        /// <summary>
        /// Get data from client login-request
        /// </summary>
        /// <returns>Data of login request</returns>
        LoginData GetLoginRequest();
        /// <summary>
        /// Send response to client
        /// </summary>
        /// <param name="response">Response data</param>
        void SendResponse(ResponseData response);
        /// <summary>
        /// Send captcha to client
        /// </summary>
        /// <param name="fileData">Image to send</param>
        void SendCaptcha(byte[] fileData);
    }
}
