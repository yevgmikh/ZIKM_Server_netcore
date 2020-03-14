using System;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Server.Infrastructure;
using ZIKM.Server.Infrastructure.Interfaces;
using ZIKM.Server.Servers;
using ZIKM.Server.Utils;

namespace ZIKM.Server.Clients {
    /// <summary>
    /// <see cref="Client"/> object for working with data
    /// </summary>
    abstract class Client {
        protected readonly IStorage storage;

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
        /// Create <see cref="Client"/> object
        /// </summary>
        /// <param name="provider">Provider for sending data</param>
        /// <param name="level">User's permission level</param>
        /// <param name="user">User's name</param>
        protected Client(IProvider provider, PermissionLevel level, string user) {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            SessionID = Guid.NewGuid();
            storage = IoC.GetService<IStorageFactory>().GetStorage(level, user);
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
                Provider.SendResponse(new ResponseData(StatusCode.SessionLost, "SessionID incorrect. Force closing session."));
                Logger.LogAll("SessionID incorrect");
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
                        Provider.SendResponse(new ResponseData(SessionID, StatusCode.BadRequest, "Invalid operation"));
                        Logger.LogAll("Invalid operation");
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
                        Provider.SendResponse(new ResponseData(SessionID, StatusCode.BadData, EndMessage));
                        Logger.Log(EndLog);
                        return;

                    default:
                        Provider.SendResponse(new ResponseData(SessionID, StatusCode.BadRequest, $"Invalid operation"));
                        Logger.LogAll("Invalid operation");
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