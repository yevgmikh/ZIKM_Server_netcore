using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Server.Infrastructure;
using ZIKM.Server.Infrastructure.Interfaces;
using ZIKM.Server.Utils;

namespace ZIKM.Server.Clients {
    /// <summary>
    /// Master client object for working with data
    /// </summary>
    class MasterClient : Client {
        protected override string EndMessage { get; set; } = Messages.MasterFarewell;
        protected override string EndLog { get; set; } = LogMessages.MasterLoggedOut;

        /// <summary>
        /// Create master client object
        /// </summary>
        /// <param name="provider">Provider for sending data</param>
        public MasterClient(IProvider provider) : base(provider, PermissionLevel.Master, "Master") { }

        /// <summary>
        /// Start session for Master
        /// </summary>
        public override void StartSession() {
            Provider.SendResponse(new ResponseData(SessionID, StatusCode.Success, Messages.MasterGreeting));
            Logger.Log(LogMessages.MasterLoggedIn);
            Session();
        }
    }
}