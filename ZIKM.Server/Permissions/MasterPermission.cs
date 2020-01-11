using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Infrastructure.Interfaces;
using ZIKM.Server.Infrastructure;

namespace ZIKM.Permissions {
    class MasterPermission : Client {
        protected override string EndMessage { get; set; } = Messages.MasterFarewell;
        protected override string EndLog { get; set; } = "Master gone";

        public MasterPermission(IProvider provider) : base(provider, 4) { }

        /// <summary>
        /// Start session for Master
        /// </summary>
        public override void StartSession() {
            Provider.SendResponse(new ResponseData(SessionID, StatusCode.Success, Messages.MasterGreeting));
            Logger.ToLog("Master here");
            Session();
        }
    }
}