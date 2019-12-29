using System;
using ZIKM.Infrastructure;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Infrastructure.Interfaces;
using ZIKM.Infrastructure.Providers;
using ZIKM.Infrastructure.Storages;

namespace ZIKM.Permissions{
    abstract class Client{
        #region Storage
        private IStorage storage;

        public static Storage StorageType { get; set; }
        #endregion

        protected Guid SessionID { get; set; }
        protected IProvider Provider { get; set; }

        /// <summary>
        /// Message when user disconnect
        /// </summary>
        protected abstract string EndMessage { get; set; }
        /// <summary>
        /// Log-data when user disconnect
        /// </summary>
        protected abstract string EndLog { get; set; }

        /// <summary>
        /// Create client object
        /// </summary>
        /// <param name="provider">Provider for sending data</param>
        /// <param name="level">User's permission level</param>
        protected Client(IProvider provider, int level){
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            SessionID = Guid.NewGuid();
            switch (StorageType)
            {
                case Storage.Files:
                    storage = new FileStorage(level);
                    break;
                case Storage.InternalDB:
                    throw new NotImplementedException();
                case Storage.ExternalDB:
                    throw new NotImplementedException();
            }
        }

        #region Helpers

        /// <summary>
        /// Check user session ID
        /// </summary>
        /// <param name="userSession">User session ID</param>
        /// <returns>Status of checking</returns>
        private bool CheckSessionID(Guid userSession){
            if (userSession == SessionID){
                return true;
            }
            else{
                Provider.SendResponse(new ResponseData(-3, "SessionID incorrect. Force closing session."));
                Logger.ToLogAll("SessionID incorrect");
                return false;
            }
        }

        /// <summary>
        /// Add session ID to response and send it
        /// </summary>
        /// <param name="data"></param>
        private void SendResponse(ResponseData data){
            data.SessionId = SessionID;
            Provider.SendResponse(data);
        }

        #endregion

        /// <summary>
        /// Operation for changing files
        /// </summary>
        /// <returns>Status of normal ending working with file</returns>
        protected bool FileChange(){
            IFileOperation file = storage;
            while (true){
                // Get request
                if (!Provider.GetRequest(out RequestData userData))
                    continue;

                if (!CheckSessionID(userData.SessionId))
                    return false;

                switch ((FileOperation)userData.Operation){
                    case FileOperation.Read:
                        SendResponse(file.ReadFile());
                        break;

                    case FileOperation.Write:
                        SendResponse(file.WriteToFile(userData.Property));
                        break;

                    case FileOperation.Edit:
                        #region Edit
                        var edit = file.ReadFile();
                        SendResponse(edit);

                        if (edit.Code == 0){
                            // Get confirm request
                            while (!Provider.GetRequest(out userData)) { }
                            if (!CheckSessionID(userData.SessionId))
                                return false;

                            // Commit changes
                            if (userData.Operation == 3){
                                SendResponse(storage.ChangeFile(userData.Property));
                            }
                            else{
                                SendResponse(new ResponseData(0, "Canceled"));
                            }
                        }
                        #endregion
                        break;

                    case FileOperation.Exit:
                        SendResponse(file.CloseFile());
                        return true;

                    default:
                        Provider.SendResponse(new ResponseData(SessionID, -1, "Invalid operation"));
                        Logger.ToLogAll("Invalid operation");
                        break;
                }
            }
        }

        /// <summary>
        /// Get session for working with files
        /// </summary>
        protected void Session(){
            IDirectoryOperation session = storage;
            while (true){
                // Get request
                if (!Provider.GetRequest(out RequestData userData))
                    continue;

                if (!CheckSessionID(userData.SessionId))
                    return;

                // Main operations
                switch ((MainOperation)userData.Operation){

                    case MainOperation.GetAll:
                        Provider.SendResponse(session.GetAll());
                        break;

                    case MainOperation.OpenFile:
                        SendResponse(session.OpenFile(userData.Property));
                        // Opening file, if incorrect sessionID inside, then close session here
                        if (!FileChange())
                            return;
                        break;

                    case MainOperation.OpenFolder:
                        SendResponse(session.OpenFolder(userData.Property));
                        break;

                    case MainOperation.CloseFolder:
                        SendResponse(session.CloseFolder());
                        break;

                    case MainOperation.EndSession:
                        Provider.SendResponse(new ResponseData(SessionID, 0, EndMessage));
                        Logger.ToLog(EndLog);
                        return;

                    default:
                        Provider.SendResponse(new ResponseData(SessionID, -1, $"Invalid operation"));
                        Logger.ToLogAll("Invalid operation");
                        break;
                }
            }
        }

        /// <summary>
        /// Start session for user
        /// </summary>
        public abstract void StartSession();
    }
}