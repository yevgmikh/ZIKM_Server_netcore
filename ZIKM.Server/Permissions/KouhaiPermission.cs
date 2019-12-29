using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Interfaces;

namespace ZIKM.Permissions{
    class KouhaiPermission : Client{
        protected override string EndMessage { get; set; } = "Be carefull, my kouhai.";
        protected override string EndLog { get; set; } = "Pervered kouhai gone";

        public KouhaiPermission(IProvider provider) : base(provider, 2) { }

        /// <summary>
        /// Start session for kouhai
        /// </summary>
        public override void StartSession(){
            Provider.SendResponse(new ResponseData(SessionID, 0, "Sempai is waitting you)"));
            Logger.ToLog("Pervered kouhai here");
            Session();
        }
    }
}