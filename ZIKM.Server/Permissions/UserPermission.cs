using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Infrastructure.Interfaces;
using ZIKM.Server.Infrastructure;

namespace ZIKM.Permissions {
    class UserPermission : Client {
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
        public UserPermission(IProvider provider, string name): base(provider, 1) {
            UserName = name;
            EndMessage = $"Bye {UserName}";
            EndLog = Messages.UserFarewell(UserName);
        }

        /// <summary>
        /// Start session for user
        /// </summary>
        public override void StartSession() {
            Provider.SendResponse(new ResponseData(SessionID, StatusCode.Success, Messages.UserGreeting(UserName)));
            Logger.ToLog($"{UserName} here");
            Session();
        }
    }
}