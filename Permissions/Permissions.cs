using System;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace ZIKM.Permissions{
    class Permissions{
        FileOperation fileCode = FileOperation.Error;

        protected string sessionid = "1";
        protected string _path = "/home/yevgeniy/C#/ZIKM/Data";
        protected int code = 0;
        protected Operation operation;
        protected NetworkStream _stream;

        protected Permissions(NetworkStream stream) => _stream = stream;

        #region Helpers
        private void PermissionError() {
            Provider.SendResponse($"{{ SessionId: \"{sessionid}\", Code: 2, Message: \"Not enough rights\" }}", _stream);
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

        #region FileChange
        protected void FileChange(string name, int perStatus, int perUser)
        {
            Provider.SendResponse($"{{ SessionId: \"{sessionid}\", Code: 0, Message: \"File {name} opened\" }}", _stream);
            Logger.ToLog($"File {name} opened");
            while (true)
            {
                dynamic userData = JsonConvert.DeserializeObject("{ SessionId: \"\", Operation: \"\", Property: \"\" }");
                try{ userData = JsonConvert.DeserializeObject(Provider.GetRequest(_stream)); }
                catch (JsonReaderException){
                    Provider.SendResponse($"{{ SessionId: \"{sessionid}\", Code: -2, Message: \"Invalid request\" }}", _stream);
                    Logger.ToLogAll("Invalid request");
                    continue;
                }

                int Operation = userData.Operation;
                fileCode = GetFileOperation(Operation);

                bool ex = false;
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
                                var texts = File.ReadAllLines($"{_path}/{name}");
                                ushort property = 0;
                                if (property == 0) property = (ushort)texts.Length;
                                string text = "";
                                for (ushort i = 0; i < property; i++) text += $"{texts[i]}\n";
                                Provider.SendResponse($"{{ SessionId: \"{sessionid}\", Code: 0, Message: \"{text}\" }}", _stream);
                                Logger.ToLog($"File {name} read");
                            }
                            catch (Exception e)
                            {
                                Provider.SendResponse($"{{ SessionId: \"{sessionid}\", Code: 1, Message: \"Errorr reading{e.Message}\" }}", _stream);
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
                                using (StreamWriter writer = new StreamWriter($"{_path}/{name}", true)) writer.WriteLine(property);
                                Provider.SendResponse($"{{ SessionId: \"{sessionid}\", Code: 0, Message: \"Successfully\" }}", _stream);
                                Logger.ToLog($"Writen to file {name}");
                            }
                            catch (Exception e)
                            {
                                Provider.SendResponse($"{{ SessionId: \"{sessionid}\", Code: 1, Message: \"Error writing{e.Message}\" }}", _stream);
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
                                var text = File.ReadAllText($"{_path}/{name}");
                                Provider.SendResponse($"{{ SessionId: \"{sessionid}\", Code: 0, Message: \"{text}\" }}", _stream);
                                while (true){
                                    try{ 
                                        userData = JsonConvert.DeserializeObject(Provider.GetRequest(_stream)); 
                                        break;
                                    }
                                    catch (JsonReaderException){
                                        Provider.SendResponse($"{{ SessionId: \"{sessionid}\", Code: -2, Message: \"Invalid request\" }}", _stream);
                                        Logger.ToLogAll("Invalid request");
                                        continue;
                                    }
                                }
                                Operation = userData.Operation;
                                if (Operation == 3)
                                {
                                    string property = userData.Property;
                                    using (StreamWriter writer = new StreamWriter($"{_path}/{name}", false)) writer.WriteLine(property);
                                    Provider.SendResponse($"{{ SessionId: \"{sessionid}\", Code: 0, Message: \"Updated\" }}", _stream);
                                    Logger.ToLog($"File {name} edited");
                                }
                                else {
                                    Provider.SendResponse($"{{ SessionId: \"{sessionid}\", Code: 0, Message: \"Canceled\" }}", _stream);
                                    Logger.ToLog($"File {name} no edited");
                                }
                            }
                            catch (Exception e)
                            {
                                Provider.SendResponse($"{{ SessionId: \"{sessionid}\", Code: 1, Message: \"Error editing:{e.Message}\" }}", _stream);
                                Logger.ToLogAll($"Error while {name} edit, {e.Message}");
                            }
                        }
                        else PermissionError();
                        break;
                    default:
                        Provider.SendResponse($"{{ SessionId: \"{sessionid}\", Code: -1, Message: \"Invalid operation\" }}", _stream);
                        Logger.ToLogAll("Invalid operation");
                        break;
                }
                if (ex) break;
            }
            Logger.ToLog($"File {name} closed");
        }
        #endregion
    }
}