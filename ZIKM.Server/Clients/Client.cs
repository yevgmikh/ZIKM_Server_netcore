using System;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Server.Infrastructure;
using ZIKM.Server.Infrastructure.Interfaces;
using ZIKM.Server.Servers;
using ZIKM.Server.Services.Storages;
using ZIKM.Server.Utils;

namespace ZIKM.Server.Clients {
    /// <summary>
    /// <see cref="Client"/> object for working with data
    /// </summary>
    abstract class Client {
        private const string fileName = "FileName";
        private const string newName = "NewName";
        private const string fileData = "FileData";

        public static event Action<Action, Action<Exception>> HandlerActionError;
        public static event Func<Func<bool>, Func<Exception, bool>, bool> HandlerFuncBoolError;

        protected readonly Storage storage;

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
        private bool CheckSessionID(Guid userSession) {
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
        private void SendResponse(ResponseData data) {
            data.SessionId = SessionID;
            Provider.SendResponse(data);
        }

        private ResponseData FileLocked() {
            return new ResponseData(StatusCode.NoAccess, Messages.FileLocked);
        }

        private void LogException(Exception ex) {
            Logger.LogError(ex.Message);
            Logger.LogError(ex.StackTrace);
            while (ex.InnerException != null) {
                Logger.LogError(ex.InnerException.Message);
                Logger.LogError(ex.InnerException.StackTrace);
                ex = ex.InnerException;
            }
        }

        private void HandleActionException(Exception ex) {
            Logger.LogError(LogMessages.SessionError);
            LogException(ex);
            SendResponse(new ResponseData(StatusCode.SessionLost, Messages.SessionError));
        }

        private bool HandleBoolFuncException(Exception ex) {
            Logger.LogError(LogMessages.FileError);
            LogException(ex);
            SendResponse(new ResponseData(StatusCode.ServerError, Messages.FileError));
            return true;
        }

        protected void HandleActionError(Action operation) 
            => HandlerActionError(operation, HandleActionException);

        protected bool HandleFuncBoolError(Func<bool> operation) 
            => HandlerFuncBoolError(operation, HandleBoolFuncException);
        #endregion

        /// <summary>
        /// Operation for changing files
        /// </summary>
        /// <returns>Status of normal ending working with file</returns>
        protected bool FileChange() {
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
                        SendResponse(file.WriteToFile(userData.Properties[fileData]));
                        break;

                    case FileOperation.Edit:
                        #region Edit
                        if (!file.LockFile()) {
                            SendResponse(FileLocked());
                            break;
                        }

                        var edit = file.ReadFile();
                        SendResponse(edit);

                        if (edit.Code == 0){
                            // Get confirm request
                            while (!Provider.GetRequest(out userData)) { }
                            if (!CheckSessionID(userData.SessionId))
                                return false;

                            // Commit changes
                            if (userData.Operation == 3){
                                SendResponse(storage.ChangeFile(userData.Properties[fileData]));
                            }
                            else{
                                SendResponse(new ResponseData(0, "Canceled"));
                                file.UnlockFile();
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
        protected void Session() {
            HandleActionError(() => {
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
                            SendResponse(session.OpenFile(userData.Properties[fileName]));
                            // Opening file, if incorrect sessionID inside, then close session here
                            if (!HandleFuncBoolError(FileChange))
                                return;
                            break;

                        case MainOperation.AddFile:
                            SendResponse(session.AddFile(userData.Properties[fileName]));
                            break;

                        case MainOperation.EditFile:
                            SendResponse(session
                                .EditFile(userData.Properties[fileName], userData.Properties[newName]));
                            break;

                        case MainOperation.RemoveFile:
                            SendResponse(session.RemoveFile(userData.Properties[fileName]));
                            break;

                        case MainOperation.OpenFolder:
                            SendResponse(session.OpenFolder(userData.Properties[fileName]));
                            break;

                        case MainOperation.CloseFolder:
                            SendResponse(session.CloseFolder());
                            break;

                        case MainOperation.EndSession:
                            Provider.SendResponse(new ResponseData(SessionID, StatusCode.Success, EndMessage));
                            Logger.Log(EndLog);
                            return;

                        default:
                            Provider.SendResponse(new ResponseData(SessionID, StatusCode.BadRequest, $"Invalid operation"));
                            Logger.LogAll("Invalid operation");
                            break;
                    }
                }
            });
        }

        /// <summary>
        /// Start session for user
        /// </summary>
        public abstract void StartSession();
    }
}