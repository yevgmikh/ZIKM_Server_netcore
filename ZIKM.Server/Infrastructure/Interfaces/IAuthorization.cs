using ZIKM.Infrastructure.DataStructures;

namespace ZIKM.Server.Infrastructure.Interfaces{
    internal interface IAuthorization
    {
        /// <summary>
        /// Sing in
        /// </summary>
        /// <param name="login">User name</param>
        /// <param name="password">User's password</param>
        /// <returns>Status and message of operation</returns>
        ResponseData SingIn(string login, string password);
    }
}
