using ZIKM.Infrastructure;
using ZIKM.Interfaces;

namespace ZIKM.Permissions{
    class SenpaiPermission : Client{
        protected override string EndMessage { get; set; } = "Senpai! I will wait!!!";
        protected override string EndLog { get; set; } = "Sempai gone";

        public SenpaiPermission(IProvider provider, IPermissionsLevel permissions) : base(provider, permissions, 3) { }

        /// <summary>
        /// Start session for senpai
        /// </summary>
        public override void StartSession(){
            Provider.SendResponse(new ResponseData(Sessionid, 0, "Senpai!!!XD"));
            Logger.ToLog("Sempai back");
            Session();
        }
    }
}