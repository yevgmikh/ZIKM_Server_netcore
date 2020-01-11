using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Infrastructure.Interfaces;
using ZIKM.Server.Infrastructure;

namespace ZIKM.Permissions {
    class KouhaiPermission : Client {
        protected override string EndMessage { get; set; } = Messages.KouhaiFarewell;
        protected override string EndLog { get; set; } = "Pervered kouhai gone";

        public KouhaiPermission(IProvider provider) : base(provider, 2) { }

        /// <summary>
        /// Start session for kouhai
        /// </summary>
        public override void StartSession() {
            Provider.SendResponse(new ResponseData(SessionID, StatusCode.Success, Messages.KouhaiGreeting));
            Logger.ToLog("Pervered kouhai here");
            Session();
        }
    }
}