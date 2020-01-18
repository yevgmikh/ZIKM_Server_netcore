using ZIKM.Infrastructure;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Infrastructure.Interfaces;

namespace ZIKM.Clients {
    /// <summary>
    /// Simple user client object for working with data
    /// </summary>
    class UserClient : Client {
        public string UserName { get; private set; }
        /// <summary>
        /// Message when user disconnect
        /// </summary>
        protected override string EndMessage { get; set; }
        /// <summary>
        /// Log-data when user disconnect
        /// </summary>
        protected override string EndLog { get; set; }

        /// <summary>
        /// Create simple client object
        /// </summary>
        /// <param name="provider">Provider for sending data</param>
        /// <param name="name">Name of user</param>
        public UserClient(IProvider provider, string name): base(provider, PermissionLevel.User, name) {
            UserName = name;
            EndMessage = LogMessages.UserLoggedOut(UserName);
            EndLog = Messages.UserFarewell(UserName);
        }

        /// <summary>
        /// Start session for user
        /// </summary>
        public override void StartSession() {
            Provider.SendResponse(new ResponseData(SessionID, StatusCode.Success, Messages.UserGreeting(UserName)));
            Logger.ToLog(LogMessages.UserLoggedIn(UserName));
            Session();
        }
    }
}