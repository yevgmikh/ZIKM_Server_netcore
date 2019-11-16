using System;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;
using ZIKM.Infrastructure;

namespace ZIKM.Permissions{
    abstract class Permissions{
        FileOperation fileCode = FileOperation.Error;

        protected Guid sessionid;
        protected string _path = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        protected int code = 0;
        protected bool _end = false;
        protected Operation operation;
        protected Provider _provider;

        protected Permissions(Provider stream, Guid guid){
            _provider = stream;
            sessionid = guid;
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

        protected Operation GetOperation(int number){
            switch (number){
                case 1:
                    return Operation.GetFiles;
                case 2:
                    return Operation.GetFolders;
                case 3:
                    return Operation.OpenFile;
                case 4:
                    return Operation.OpenFolder;
                case 5:
                    return Operation.CloseFolder;
                case 6:
                    return Operation.End;
                default:
                    return Operation.Error;
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

        protected void FileChange(string name, int perStatus, int perUser)
        {
            _provider.SendResponse(new ResponseData(sessionid, 0, $"File {name} opened"));
            Logger.ToLog($"File {name} opened");
            while (true)
            {
                // Get request
                RequestData userData; //= JsonConvert.DeserializeObject("{ SessionId: \"\", Operation: \"\", Property: \"\" }");
                try{ userData = JsonConvert.DeserializeObject<RequestData>(_provider.GetRequest()); }
                catch (JsonReaderException){
                    _provider.SendResponse(new ResponseData(-2, "Invalid request"));
                    Logger.ToLogAll("Invalid request");
                    continue;
                }

                int Operation = userData.Operation;
                fileCode = GetFileOperation(Operation);

                bool ex = false;
                Guid session = userData.SessionId;

                if (session == sessionid)
                switch (fileCode)
                {
                    case FileOperation.Exit:
                        ex = true;
                        break;
                    case FileOperation.Read:
                        if ((perStatus + 1) == perUser || perStatus == perUser)
                        {
                            try
                            {
                                var texts = File.ReadAllLines($@"{_path}\{name}");
                                ushort property = 0;
                                if (property == 0) property = (ushort)texts.Length;
                                string text = "";
                                for (ushort i = 0; i < property; i++) text += $"{texts[i]}\n";
                                _provider.SendResponse(new ResponseData(sessionid, 0, $"{text}"));
                                Logger.ToLog($"File {name} read");
                            }
                            catch (Exception e)
                            {
                                _provider.SendResponse(new ResponseData(sessionid, 1, $"Errorr reading{e.Message}"));
                                Logger.ToLogAll($"Error while {name} read, {e.Message}");
                            }

                        }
                        else PermissionError();
                        break;
                    case FileOperation.Write:
                        if ((perStatus - 1) == perUser || perStatus == perUser)
                        {
                            try
                            {
                                string property = userData.Property;
                                using (StreamWriter writer = new StreamWriter($@"{_path}\{name}", true)) writer.WriteLine(property);
                                _provider.SendResponse(new ResponseData(sessionid, 0, "Successfully"));
                                Logger.ToLog($"Writen to file {name}");
                            }
                            catch (Exception e)
                            {
                                _provider.SendResponse(new ResponseData(sessionid, 1, $"Error writing{e.Message}"));
                                Logger.ToLogAll($"Error while {name} write, {e.Message}");
                            }
                        }
                        else PermissionError();
                        break;
                    case FileOperation.Edit:
                        if (perStatus == perUser)
                        {
                            try
                            {
                                var text = File.ReadAllText($@"{_path}\{name}");
                                _provider.SendResponse(new ResponseData(sessionid, 0, $"{text}"));
                                while (true){
                                    try{ 
                                        userData = JsonConvert.DeserializeObject<RequestData>(_provider.GetRequest()); 
                                        break;
                                    }
                                    catch (JsonReaderException){
                                        _provider.SendResponse(new ResponseData(-2, "Invalid request"));
                                        Logger.ToLogAll("Invalid request");
                                        continue;
                                    }
                                }
                                Operation = userData.Operation;
                                if (Operation == 3)
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
                            catch (Exception e)
                            {
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
        protected void Session(int permision)
        {
            while (true)
            {
                // Get request
                RequestData userData; //= JsonConvert.DeserializeObject("{ SessionId: \"\", Operation: \"\", Property: \"\" }");
                try { userData = JsonConvert.DeserializeObject<RequestData>(_provider.GetRequest()); }
                catch (JsonReaderException)
                {
                    _provider.SendResponse(new ResponseData(-2, "Invalid request"));
                    Logger.ToLogAll("Invalid request");
                    continue;
                }
                ResponseData response = new ResponseData();
                int op = userData.Operation;
                operation = GetOperation(op);
                Guid session = userData.SessionId;

                if (session == sessionid)
                    switch (operation)
                    {
                        case Operation.GetFiles:
                            var files = Directory.GetFiles(_path);
                            response = new ResponseData(sessionid, 0, $"{ToProperty(OnlyNames(files, true))}");
                            break;


                        case Operation.GetFolders:
                            var directories = Directory.GetDirectories(_path);
                            response = new ResponseData(sessionid, 0, $"{ToProperty(OnlyNames(directories, false))}");
                            break;


                        case Operation.OpenFile:
                            if (code != 0)
                            {
                                string property = userData.Property;
                                if (IsHave(Directory.GetFiles(_path), property))
                                {
                                    FileChange(property, code, permision);
                                    response = new ResponseData(sessionid, 0, $"File {property} closed");
                                }
                                else response = new ResponseData(sessionid, 1, "File not found");
                            }
                            else response = new ResponseData(sessionid, 1, "Here no files");
                            break;


                        case Operation.OpenFolder:
                            if (code == 0)
                            {
                                int property = int.Parse(userData.Property);
                                if (IsCorrect(property))
                                {
                                    int prop = int.Parse(userData.Property);
                                    if (prop <= permision - 1 && prop >= permision + 1)
                                    {
                                        _path += $"/{prop}";
                                        code = prop;
                                        response = new ResponseData(sessionid, 0, $"Folder {prop} opened");
                                    }
                                    else response = new ResponseData(sessionid, 2, "Not enough rights");
                                }
                                else response = new ResponseData(sessionid, 1, "Here no this folder");
                            }
                            else response = new ResponseData(sessionid, 1, "Here no folders");
                            break;


                        case Operation.CloseFolder:
                            if (code != 0)
                            {
                                _path.Substring(0, _path.Length - 2);
                                code = 0;
                                response = new ResponseData(sessionid, 0, "Folder closed");
                            }
                            else response = new ResponseData(sessionid, 1, "You in home directory");
                            break;


                        case Operation.End:
                            _end = true;
                            break;


                        default:
                            response = new ResponseData(sessionid, -1, $"Invalid operation");
                            Logger.ToLogAll("Invalid operation");
                            break;

                    }
                else
                {
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