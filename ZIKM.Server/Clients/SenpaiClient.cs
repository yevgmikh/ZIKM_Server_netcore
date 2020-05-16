using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Server.Infrastructure;
using ZIKM.Server.Infrastructure.Interfaces;
using ZIKM.Server.Utils;

namespace ZIKM.Server.Clients {
    /// <summary>
    /// Sempai client object for working with data
    /// </summary>
    class SenpaiClient : Client {
        protected override string EndMessage { get; set; } = Messages.SempaiFarewell;
        protected override string EndLog { get; set; } = LogMessages.SempaiLoggedOut;

        /// <summary>
        /// Create senpai client object
        /// </summary>
        /// <param name="provider">Provider for sending data</param>
        public SenpaiClient(IProvider provider) : base(provider, PermissionLevel.Senpai, "Senpai") { }

        /// <summary>
        /// Start session for senpai
        /// </summary>
        public override void StartSession() {
            Provider.SendResponse(new ResponseData(SessionID, StatusCode.Success, Messages.SempaiGreeting));
            Logger.Log(LogMessages.SempaiLoggedIn);
            Session();
        }
    }
}