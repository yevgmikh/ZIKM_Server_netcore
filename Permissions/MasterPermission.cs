using System;
using ZIKM.Infrastructure;
using ZIKM.Interfaces;

namespace ZIKM.Permissions{
    class MasterPermission : Client{
        protected override string EndMessage { get; set; } = "I will wait your return, Master.";
        protected override string EndLog { get; set; } = "Master gone";

        public MasterPermission(IProvider provider, Guid guid): base(provider, guid) { 
            code = 0; 
        }

        /// <summary>
        /// Start session for Master
        /// </summary>
        public override void StartSession(){
            _provider.SendResponse(new ResponseData(sessionid, 0, "Welcome, Master."));
            Logger.ToLog("Master here");
            Session(4);
        }
    }
}