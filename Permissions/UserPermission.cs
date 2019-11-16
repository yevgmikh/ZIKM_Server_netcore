using System;
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
        /// <param name="guid">Session ID</param>
        /// <param name="name">Name of user</param>
        public UserPermission(IProvider provider, Guid guid, string name): base(provider, guid) {
            UserName = name;
            EndMessage = $"Bye {UserName}";
            EndLog = $"{UserName} disconnect";
            code = 0; 
        }

        /// <summary>
        /// Start session for user
        /// </summary>
        public override void StartSession(){
            _provider.SendResponse(new ResponseData(sessionid, 0, $"You {UserName}"));
            Logger.ToLog($"{UserName} here");
            Session(1);
        }
    }
}