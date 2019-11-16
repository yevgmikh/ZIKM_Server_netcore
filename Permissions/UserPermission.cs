using ZIKM.Infrastructure;
using ZIKM.Interfaces;

namespace ZIKM.Permissions{
    class UserPermission : Client{
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
        /// <param name="permissions">Getting data about permissions</param>
        /// <param name="name">Name of user</param>
        public UserPermission(IProvider provider, IPermissionsLevel permissions, string name): base(provider, permissions, 1) {
            UserName = name;
            EndMessage = $"Bye {UserName}";
            EndLog = $"{UserName} disconnect";
        }

        /// <summary>
        /// Start session for user
        /// </summary>
        public override void StartSession(){
            Provider.SendResponse(new ResponseData(Sessionid, 0, $"You {UserName}"));
            Logger.ToLog($"{UserName} here");
            Session();
        }
    }
}