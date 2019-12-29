using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Interfaces;

namespace ZIKM.Permissions{
    class MasterPermission : Client{
        protected override string EndMessage { get; set; } = "I will wait your return, Master.";
        protected override string EndLog { get; set; } = "Master gone";

        public MasterPermission(IProvider provider) : base(provider, 4) { }

        /// <summary>
        /// Start session for Master
        /// </summary>
        public override void StartSession(){
            Provider.SendResponse(new ResponseData(SessionID, 0, "Welcome, Master."));
            Logger.ToLog("Master here");
            Session();
        }
    }
}