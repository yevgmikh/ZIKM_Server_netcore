using ZIKM.Infrastructure;
using ZIKM.Interfaces;

namespace ZIKM.Permissions{
    class KouhaiPermission : Client{
        protected override string EndMessage { get; set; } = "Be carefull, my kouhai.";
        protected override string EndLog { get; set; } = "Pervered kouhai gone";

        public KouhaiPermission(IProvider provider, IPermissionsLevel permissions) : base(provider, permissions, 2) { }

        /// <summary>
        /// Start session for kouhai
        /// </summary>
        public override void StartSession(){
            Provider.SendResponse(new ResponseData(Sessionid, 0, "Sempai is waitting you)"));
            Logger.ToLog("Pervered kouhai here");
            Session();
        }
    }
}