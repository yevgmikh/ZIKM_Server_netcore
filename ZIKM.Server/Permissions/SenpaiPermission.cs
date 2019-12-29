using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Interfaces;

namespace ZIKM.Permissions{
    class SenpaiPermission : Client{
        protected override string EndMessage { get; set; } = "Senpai! I will wait!!!";
        protected override string EndLog { get; set; } = "Sempai gone";

        public SenpaiPermission(IProvider provider) : base(provider, 3) { }

        /// <summary>
        /// Start session for senpai
        /// </summary>
        public override void StartSession(){
            Provider.SendResponse(new ResponseData(SessionID, 0, "Senpai!!!XD"));
            Logger.ToLog("Sempai back");
            Session();
        }
    }
}