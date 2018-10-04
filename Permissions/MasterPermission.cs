using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace ZIKM.Permissions{
    class MasterPermission: Permissions{
        public MasterPermission(NetworkStream stream): base(stream) { code = 0; }

        public void Session(){
            while(true){
                bool end = false;
                dynamic userData = JsonConvert.DeserializeObject("{ SessionId: \"\", Operation: \"\", Property: \"\" }");
                try{ userData = JsonConvert.DeserializeObject(Provider.GetRequest(_stream)); }
                catch (JsonReaderException){
                    Provider.SendResponse($"{{ SessionId: \"{sessionid}\", Code: -2, Message: \"Invalid request\" }}", _stream);
                    Logger.ToLogAll("Invalid request");
                    continue;
                }
                string response = "";
                int op = userData.Operation;
                operation = GetOperation(op);

                switch(operation){
                    case Operation.GetFiles:
                        var files = Directory.GetFiles(_path);
                        response = $"{{ SessionId: \"{sessionid}\", Code: 0, Message: \"{ToProperty(OnlyNames(files, true))}\" }}";
                        break;


                    case Operation.GetFolders:
                        var directories = Directory.GetDirectories(_path);
                        response = $"{{ SessionId: \"{sessionid}\", Code: 0, Message: \"{ToProperty(OnlyNames(directories, false))}\" }}";
                        break;


                    case Operation.OpenFile:
                        if (code != 0){
                            string property = userData.Property;
                            if (IsHave(Directory.GetFiles(_path), property)) {
                                FileChange(property, code, 4);
                                response = $"{{ SessionId: \"{sessionid}\", Code: 0, Message: \"File {property} closed\" }}";
                            }
                            else response = $"{{ SessionId: \"{sessionid}\",Code: 1, Message: \"File not found\" }}";
                        }
                        else response = $"{{ SessionId: \"{sessionid}\", Code: 1, Message: \"Here no files\" }}";
                        break;


                    case Operation.OpenFolder:
                        if (code == 0){
                            int property = userData.Property;
                            if (IsCorrect(property)){
                                int prop = userData.Property;
                                if (prop > 2){
                                    _path += $"/{prop}";
                                    code = prop;
                                    response = $"{{ SessionId: \"{sessionid}\", Code: 0, Message: \"Folder {prop} opened\" }}";
                                }
                                else response = $"{{ SessionId: \"{sessionid}\", Code: 2, Message: \"Not enough rights\" }}";
                            }
                            else response = $"{{ SessionId: \"{sessionid}\", Code: 1, Message: \"Here no this folder\" }}";
                        }
                        else response = $"{{ SessionId: \"{sessionid}\", Code: 1, Message: \"Here no folders\" }}";
                        break;


                    case Operation.CloseFolder:
                        if (code != 0){
                            _path.Substring(0, _path.Length - 2);
                            code = 0;
                            response = $"{{ SessionId: \"{sessionid}\", Code: 0, Message: \"Folder closed\" }}";
                        }
                        else response = $"{{ SessionId: \"{sessionid}\", Code: 1, Message: \"You in home directory\" }}";
                        break;


                    case Operation.End:
                        end = true;
                        break;

                        
                    default:
                        Provider.SendResponse($"{{ SessionId: \"{sessionid}\", Code: -1, Message: \"Invalid operation\" }}", _stream);
                        Logger.ToLogAll("Invalid operation");
                        break;
                    
                }
                if (end) break;
                Provider.SendResponse(response, _stream);
            }
        }
    }
}