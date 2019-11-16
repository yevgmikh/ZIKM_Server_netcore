using System;
using System.IO;
using Newtonsoft.Json;
using ZIKM.Infrastructure;
using ZIKM.Interfaces;

namespace ZIKM.Permissions{
    abstract class Client{
        FileOperation fileCode = FileOperation.Error;

        protected Guid sessionid;
        protected string _path = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        protected int code = 0;
        protected bool _end = false;
        protected MainOperation operation;
        protected IProvider _provider;

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
        /// <param name="guid">Session ID</param>
        protected Client(IProvider provider, Guid guid){
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            sessionid = guid != Guid.Empty ? guid : throw new ArgumentNullException(nameof(guid));
        }

        #region Helpers
        private void PermissionError() {
            _provider.SendResponse(new ResponseData(sessionid, 2, "Not enough rights."));
            Logger.ToLogAll("Not enough rights");
        } 

        private FileOperation GetFileOperation(int number){
            switch (number){
                case 0:
                    return FileOperation.Exit;
                case 1:
                    return FileOperation.Read;
                case 2:
                    return FileOperation.Write;
                case 3:
                    return FileOperation.Edit;
                default:
                    return FileOperation.Error;
            }
        }

        protected bool IsCorrect(int i){
            var a = new int[]{ 1, 2, 3, 4 };
            foreach (var b in a){
                if (i == b) return true;
            }
            return false;
        }

        protected bool IsHave(string[] list, string name){
            
            foreach (var n in OnlyNames(list, true)){
                if (n == name) return true;
            }
            return false;
        }

        protected string[] OnlyNames(string[] names, bool isFile){
            var list = new string[names.Length];
            for (var i = 0; i < names.Length; i++) {
                if (isFile) list[i] = names[i].Substring(30);
                else list[i] = names[i].Substring(28);
            }
            return list;
        }

        protected string ToProperty(string[] names){
            string pack ="";
            foreach (var name in names) pack += $"{name} "; 
            return pack;
        }
        #endregion

        /// <summary>
        /// Operation for changing files
        /// </summary>
        /// <param name="name">File name</param>
        /// <param name="status">Permission status of file(opened folder)</param>
        /// <param name="user">User's permission</param>
        protected void FileChange(string name, int status, int user){
            _provider.SendResponse(new ResponseData(sessionid, 0, $"File {name} opened"));
            Logger.ToLog($"File {name} opened");
            while (true){
                // Get request
                RequestData userData;
                try{ 
                    userData = _provider.GetRequest(); 
                }
                catch (JsonReaderException){
                    _provider.SendResponse(new ResponseData(-2, "Invalid request"));
                    Logger.ToLogAll("Invalid request");
                    continue;
                }

                fileCode = (FileOperation)userData.Operation;
                bool ex = false;

                if (userData.SessionId == sessionid)
                switch (fileCode){
                    case FileOperation.Exit:
                        ex = true;
                        break;
                    case FileOperation.Read:
                        if ((status + 1) == user || status == user){
                            try{
                                var texts = File.ReadAllLines($@"{_path}\{name}");
                                ushort property = 0;
                                if (property == 0) property = (ushort)texts.Length;
                                string text = "";
                                for (ushort i = 0; i < property; i++) text += $"{texts[i]}\n";
                                _provider.SendResponse(new ResponseData(sessionid, 0, $"{text}"));
                                Logger.ToLog($"File {name} read");
                            }
                            catch (Exception e){
                                _provider.SendResponse(new ResponseData(sessionid, 1, $"Errorr reading{e.Message}"));
                                Logger.ToLogAll($"Error while {name} read, {e.Message}");
                            }

                        }
                        else PermissionError();
                        break;
                    case FileOperation.Write:
                        if ((status - 1) == user || status == user){
                            try{
                                string property = userData.Property;
                                using (StreamWriter writer = new StreamWriter($@"{_path}\{name}", true)) writer.WriteLine(property);
                                _provider.SendResponse(new ResponseData(sessionid, 0, "Successfully"));
                                Logger.ToLog($"Writen to file {name}");
                            }
                            catch (Exception e){
                                _provider.SendResponse(new ResponseData(sessionid, 1, $"Error writing{e.Message}"));
                                Logger.ToLogAll($"Error while {name} write, {e.Message}");
                            }
                        }
                        else PermissionError();
                        break;
                    case FileOperation.Edit:
                        if (status == user){
                            try{
                                var text = File.ReadAllText($@"{_path}\{name}");
                                _provider.SendResponse(new ResponseData(sessionid, 0, $"{text}"));
                                while (true){
                                    try{ 
                                        userData = _provider.GetRequest(); 
                                        break;
                                    }
                                    catch (JsonReaderException){
                                        _provider.SendResponse(new ResponseData(-2, "Invalid request"));
                                        Logger.ToLogAll("Invalid request");
                                        continue;
                                    }
                                }
                                
                                if (userData.Operation == 3)
                                {
                                    string property = userData.Property;
                                    using (StreamWriter writer = new StreamWriter($@"{_path}\{name}", false)) writer.WriteLine(property);
                                    _provider.SendResponse(new ResponseData(sessionid, 0, "Updated"));
                                    Logger.ToLog($"File {name} edited");
                                }
                                else {
                                    _provider.SendResponse(new ResponseData(sessionid, 0, "Canceled"));
                                    Logger.ToLog($"File {name} no edited");
                                }
                            }
                            catch (Exception e){
                                _provider.SendResponse(new ResponseData(sessionid, 1, $"Error editing:{e.Message}"));
                                Logger.ToLogAll($"Error while {name} edit, {e.Message}");
                            }
                        }
                        else PermissionError();
                        break;
                    default:
                        _provider.SendResponse(new ResponseData(sessionid, -1, "Invalid operation"));
                        Logger.ToLogAll("Invalid operation");
                        break;
                }
                else{
                    _provider.SendResponse(new ResponseData(-3, "SessionID incorrect"));
                    Logger.ToLogAll("SessionID incorrect");
                    ex = true;
                    _end = true;
                }
                if (ex) break;
            }
            Logger.ToLog($"File {name} closed");
        }

        /// <summary>
        /// Get session for working with files
        /// </summary>
        /// <param name="permision">User rights level</param>
        protected void Session(int permision){
            while (true){
                // Get request
                RequestData userData;
                try { 
                    userData = _provider.GetRequest(); 
                }
                catch (JsonReaderException){
                    _provider.SendResponse(new ResponseData(-2, "Invalid request"));
                    Logger.ToLogAll("Invalid request");
                    continue;
                }
                ResponseData response = new ResponseData();
                //operation = GetOperation(userData.Operation);

                if (userData.SessionId == sessionid)
                    switch ((MainOperation)userData.Operation){
                        case MainOperation.GetFiles:
                            var files = Directory.GetFiles(_path);
                            response = new ResponseData(sessionid, 0, $"{ToProperty(OnlyNames(files, true))}");
                            break;


                        case MainOperation.GetFolders:
                            var directories = Directory.GetDirectories(_path);
                            response = new ResponseData(sessionid, 0, $"{ToProperty(OnlyNames(directories, false))}");
                            break;


                        case MainOperation.OpenFile:
                            if (code != 0){
                                string property = userData.Property;
                                if (IsHave(Directory.GetFiles(_path), property)){
                                    FileChange(property, code, permision);
                                    response = new ResponseData(sessionid, 0, $"File {property} closed");
                                }
                                else response = new ResponseData(sessionid, 1, "File not found");
                            }
                            else response = new ResponseData(sessionid, 1, "Here no files");
                            break;


                        case MainOperation.OpenFolder:
                            if (code == 0){
                                int property = int.Parse(userData.Property);
                                if (IsCorrect(property)){
                                    int prop = int.Parse(userData.Property);
                                    if (prop <= permision - 1 && prop >= permision + 1){
                                        _path = Path.Combine(_path, $"{prop}");
                                        code = prop;
                                        response = new ResponseData(sessionid, 0, $"Folder {prop} opened");
                                    }
                                    else response = new ResponseData(sessionid, 2, "Not enough rights");
                                }
                                else response = new ResponseData(sessionid, 1, "Here no this folder");
                            }
                            else response = new ResponseData(sessionid, 1, "Here no folders");
                            break;


                        case MainOperation.CloseFolder:
                            if (code != 0){
                                _path.Substring(0, _path.Length - 2);
                                code = 0;
                                response = new ResponseData(sessionid, 0, "Folder closed");
                            }
                            else response = new ResponseData(sessionid, 1, "You in home directory");
                            break;


                        case MainOperation.End:
                            response = new ResponseData(sessionid, 0, EndMessage);
                            Logger.ToLog(EndLog);
                            _end = true;
                            break;


                        default:
                            response = new ResponseData(sessionid, -1, $"Invalid operation");
                            Logger.ToLogAll("Invalid operation");
                            break;

                    }
                else{
                    _provider.SendResponse(new ResponseData(-3, "SessionID incorrect"));
                    Logger.ToLogAll("SessionID incorrect");
                    _end = true;
                }
                if (_end) break;
                _provider.SendResponse(response);
            }
        }

        /// <summary>
        /// Start session for user
        /// </summary>
        public abstract void StartSession();
    }
}