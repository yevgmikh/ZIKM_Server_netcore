using ZIKM.Infrastructure;
using ZIKM.Interfaces;

namespace ZIKM.Permissions{
    class MasterPermission : Client{
        protected override string EndMessage { get; set; } = "I will wait your return, Master.";
        protected override string EndLog { get; set; } = "Master gone";

        public MasterPermission(IProvider provider, IPermissionsLevel permissions) : base(provider, permissions, 4) { }

        /// <summary>
        /// Start session for Master
        /// </summary>
        public override void StartSession(){
            Provider.SendResponse(new ResponseData(Sessionid, 0, "Welcome, Master."));
            Logger.ToLog("Master here");
            Session();
        }
    }
}