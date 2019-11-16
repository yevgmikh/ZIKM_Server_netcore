using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ZIKM.Infrastructure;
using ZIKM.Interfaces;

namespace ZIKM.Permissions{
    abstract class Client{
        /// <summary>
        /// User's permission level
        /// </summary>
        private readonly int _level;
        private readonly string _rootPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Data");
        protected readonly Dictionary<string, int> _permissions;

        /// <summary>
        /// Permission status of file(opened folder)
        /// </summary>
        protected int Code { get; set; } = 0;
        /// <summary>
        /// Current directory
        /// </summary>
        protected string Path { get; set; } = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Data");
        protected Guid Sessionid { get; set; }
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
        /// <param name="permissions">Getting data about permissions</param>
        /// <param name="level">User's permission level</param>
        protected Client(IProvider provider, IPermissionsLevel permissions, int level){
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            Sessionid = Guid.NewGuid();
            _level = level;
            _permissions = new Dictionary<string, int>(permissions.Levels);
            if (_permissions.ContainsValue(0)){
                if (!_permissions.ContainsKey(_rootPath))
                    throw new Exception("Incorrect root path in permission level dictionary");
            }
            else
                _permissions.Add(_rootPath, 0);
        }

        #region Helpers
        /// <summary>
        /// Sending response about not enough permissions
        /// </summary>
        protected void PermissionError() {
            Provider.SendResponse(new ResponseData(Sessionid, 2, "Not enough rights."));
            Logger.ToLogAll("Not enough rights");
        }

        /// <summary>
        /// Pack names to property(string)
        /// </summary>
        /// <param name="names">Names for pack</param>
        /// <returns>Packed names to string</returns>
        protected string ToProperty(IEnumerable<string> names){
            string pack ="";
            foreach (var name in names) pack += $"{name};"; 
            return pack;
        }
        #endregion

        /// <summary>
        /// Operation for changing files
        /// </summary>
        /// <param name="name">File name</param>
        /// <returns>Status of normal ending working with file</returns>
        protected bool FileChange(string name){
            Provider.SendResponse(new ResponseData(Sessionid, 0, $"File {name} opened"));
            Logger.ToLog($"File {name} opened");
            while (true){
                // Get request
                RequestData userData;
                try{ 
                    userData = Provider.GetRequest(); 
                }
                catch (JsonReaderException){
                    Provider.SendResponse(new ResponseData(-2, "Invalid request"));
                    Logger.ToLogAll("Invalid request");
                    continue;
                }

                if (userData.SessionId == Sessionid){
                    switch ((FileOperation)userData.Operation){
                        case FileOperation.Exit:
                            Logger.ToLog($"File {name} closed");
                            return true;

                        case FileOperation.Read:
                            // Check permisions
                            if ((Code + 1) == _level || Code == _level){
                                try{
                                    var texts = File.ReadAllText(System.IO.Path.Combine(Path, $"{name}"));
                                    Provider.SendResponse(new ResponseData(Sessionid, 0, $"{texts}"));
                                    Logger.ToLog($"File {name} read");
                                }
                                catch (Exception e){
                                    Provider.SendResponse(new ResponseData(Sessionid, 1, $"Error reading{e.Message}"));
                                    Logger.ToLogAll($"Error while {name} read, {e.Message}");
                                }

                            }
                            else 
                                PermissionError();
                            break;

                        case FileOperation.Write:
                            // Check permisions
                            if ((Code - 1) == _level || Code == _level){
                                try{
                                    using (StreamWriter writer = new StreamWriter(System.IO.Path.Combine(Path, $"{name}"), true)) 
                                        writer.WriteLine(userData.Property);

                                    Provider.SendResponse(new ResponseData(Sessionid, 0, "Successfully"));
                                    Logger.ToLog($"Saved to file {name}");
                                }
                                catch (Exception e){
                                    Provider.SendResponse(new ResponseData(Sessionid, 1, $"Error writing{e.Message}"));
                                    Logger.ToLogAll($"Error while saving file {name}, {e.Message}");
                                }
                            }
                            else PermissionError();
                            break;

                        case FileOperation.Edit:
                            // Check permisions
                            if (Code == _level){
                                try{
                                    var text = File.ReadAllText(System.IO.Path.Combine(Path, $"{name}"));
                                    Provider.SendResponse(new ResponseData(Sessionid, 0, $"{text}"));
                                    while (true){
                                        try{ 
                                            userData = Provider.GetRequest(); 
                                            break;
                                        }
                                        catch (JsonReaderException){
                                            Provider.SendResponse(new ResponseData(-2, "Invalid request"));
                                            Logger.ToLogAll("Invalid request");
                                            continue;
                                        }
                                    }
                                
                                    // Commit changes
                                    if (userData.SessionId == Sessionid){
                                        if (userData.Operation == 3){
                                            using (StreamWriter writer = new StreamWriter(System.IO.Path.Combine(Path, $"{name}"), false)) 
                                                writer.WriteLine(userData.Property);

                                            Provider.SendResponse(new ResponseData(Sessionid, 0, "Updated"));
                                            Logger.ToLog($"File {name} edited");
                                        }
                                        else {
                                            Provider.SendResponse(new ResponseData(Sessionid, 0, "Canceled"));
                                            Logger.ToLog($"File {name} no edited");
                                        }
                                    }
                                    else{
                                        Provider.SendResponse(new ResponseData(-3, "SessionID incorrect. Force closing session."));
                                        Logger.ToLogAll("SessionID incorrect");
                                        return false;
                                    }
                                }
                                catch (Exception e){
                                    Provider.SendResponse(new ResponseData(Sessionid, 1, $"Error editing:{e.Message}"));
                                    Logger.ToLogAll($"Error while editing file {name}, {e.Message}");
                                }
                            }
                            else 
                                PermissionError();
                            break;
                        default:
                            Provider.SendResponse(new ResponseData(Sessionid, -1, "Invalid operation"));
                            Logger.ToLogAll("Invalid operation");
                            break;
                    }
                }
                else{
                    Provider.SendResponse(new ResponseData(-3, "SessionID incorrect. Force closing session."));
                    Logger.ToLogAll("SessionID incorrect");
                    return false;
                }
            }
        }

        /// <summary>
        /// Get session for working with files
        /// </summary>
        protected void Session(){
            while (true){
                // Get request
                RequestData userData;
                try { 
                    userData = Provider.GetRequest(); 
                }
                catch (JsonReaderException){
                    Provider.SendResponse(new ResponseData(-2, "Invalid request"));
                    Logger.ToLogAll("Invalid request");
                    continue;
                }

                // Main operations
                if (userData.SessionId == Sessionid){
                    switch ((MainOperation)userData.Operation){
                        case MainOperation.GetFiles:
                            var files = Directory.GetFiles(Path).Select(i => new FileInfo(i).Name);
                            Provider.SendResponse(new ResponseData(Sessionid, 0, $"{ToProperty(files)}"));
                            break;

                        case MainOperation.GetFolders:
                            var directories = Directory.GetDirectories(Path).Select(i => new DirectoryInfo(i).Name);
                            Provider.SendResponse(new ResponseData(Sessionid, 0, $"{ToProperty(directories)}"));
                            break;

                        case MainOperation.OpenFile:
                            // Check is not root folder
                            if (Code != 0){
                                // Check file name
                                if (Directory.GetFiles(Path).Select(i => new FileInfo(i).Name).Contains(userData.Property)){
                                    // Opening file, if incorrect sessionID inside, then close session here
                                    if (!FileChange(userData.Property))
                                        return;

                                    Provider.SendResponse(new ResponseData(Sessionid, 0, $"File {userData.Property} closed"));
                                    Logger.ToLog($"File {userData.Property} closed");
                                }
                                else
                                    Provider.SendResponse(new ResponseData(Sessionid, 1, "File not found"));
                            }
                            else
                                Provider.SendResponse(new ResponseData(Sessionid, 1, "Here no files"));
                            break;


                        case MainOperation.OpenFolder:
                            // Check is root folder
                            if (Path == _rootPath){ 
                                // Check name folder
                                if (Directory.GetDirectories(Path).Select(i => new DirectoryInfo(i).Name).Contains(userData.Property)){ 
                                    // Check permissions
                                    int code = _permissions[System.IO.Path.Combine(Path, userData.Property)];
                                    if (code >= _level - 1 && code <= _level + 1)
                                    {
                                        // Opening folder
                                        Path = System.IO.Path.Combine(Path, $"{userData.Property}");
                                        Code = code;
                                        Provider.SendResponse(new ResponseData(Sessionid, 0, $"Folder {userData.Property} opened"));
                                    }
                                    else
                                        PermissionError();
                                }
                                else 
                                    Provider.SendResponse(new ResponseData(Sessionid, 1, "Here no this folder"));
                            }
                            else 
                                Provider.SendResponse(new ResponseData(Sessionid, 1, "Here no folders"));
                            break;


                        case MainOperation.CloseFolder:
                            // Check is not root folder
                            if (Path != _rootPath){
                                // Closing folder
                                Path = Directory.GetParent(Path).FullName;
                                Code = _permissions[Path];
                                Provider.SendResponse(new ResponseData(Sessionid, 0, "Folder closed"));
                            }
                            else
                                Provider.SendResponse(new ResponseData(Sessionid, 1, "You in home directory"));
                            break;

                        case MainOperation.EndSession:
                            Provider.SendResponse(new ResponseData(Sessionid, 0, EndMessage));
                            Logger.ToLog(EndLog);
                            return;

                        default:
                            Provider.SendResponse(new ResponseData(Sessionid, -1, $"Invalid operation"));
                            Logger.ToLogAll("Invalid operation");
                            break;
                    }
                }
                else{
                    Provider.SendResponse(new ResponseData(-3, "SessionID incorrect. Force closing session."));
                    Logger.ToLogAll("SessionID incorrect");
                    return;
                }
            }
        }

        /// <summary>
        /// Start session for user
        /// </summary>
        public abstract void StartSession();
    }
}