using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Infrastructure.Interfaces;
using ZIKM.Server.Infrastructure;

namespace ZIKM.Permissions {
    class SenpaiPermission : Client {
        protected override string EndMessage { get; set; } = Messages.SempaiFarewell;
        protected override string EndLog { get; set; } = "Sempai gone";

        public SenpaiPermission(IProvider provider) : base(provider, 3) { }

        /// <summary>
        /// Start session for senpai
        /// </summary>
        public override void StartSession() {
            Provider.SendResponse(new ResponseData(SessionID, StatusCode.Success, Messages.SempaiGreeting));
            Logger.ToLog("Sempai back");
            Session();
        }
    }
}