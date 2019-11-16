using System;
using ZIKM.Infrastructure;

namespace ZIKM.Interfaces{
    public interface IProvider : IDisposable{
        /// <summary>
        /// Send response to client
        /// </summary>
        /// <param name="response">Response data</param>
        void SendResponse(ResponseData response);
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
    }
}
