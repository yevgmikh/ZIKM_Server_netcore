using ZIKM.Infrastructure;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Infrastructure.Interfaces;

namespace ZIKM.Clients {
    /// <summary>
    /// Kouhai client object for working with data
    /// </summary>
    class KouhaiClient : Client {
        protected override string EndMessage { get; set; } = Messages.KouhaiFarewell;
        protected override string EndLog { get; set; } = LogMessages.KouhaiLoggedOut;

        /// <summary>
        /// Create kouhai client object
        /// </summary>
        /// <param name="provider">Provider for sending data</param>
        public KouhaiClient(IProvider provider) : base(provider, 2, "Kouhai") { }

        /// <summary>
        /// Start session for kouhai
        /// </summary>
        public override void StartSession() {
            Provider.SendResponse(new ResponseData(SessionID, StatusCode.Success, Messages.KouhaiGreeting));
            Logger.ToLog(LogMessages.KouhaiLoggedIn);
            Session();
        }
    }
}