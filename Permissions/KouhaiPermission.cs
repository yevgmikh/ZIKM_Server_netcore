using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace ZIKM.Permissions{
    class KouhaiPermission: Permissions{
        public KouhaiPermission(NetworkStream stream, string guid): base(stream, guid) { code = 0; }

        public void Session(){
            while(true){
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
                string session = userData.SessionId;

                if (session == sessionid)
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
                                FileChange(property, code, 2);
                                response = $"{{ SessionId: \"{sessionid}\", Code: 0, Message: \"File {property} closed\" }}";
                            }
                            else response = $"{{ SessionId: \"{sessionid}\", Code: 1, Message: \"File not found\" }}";
                        }
                        else response = $"{{ SessionId: \"{sessionid}\", Code: 1, Message: \"Here no files\" }}";
                        break;


                    case Operation.OpenFolder:
                        if (code == 0){
                            int property = userData.Property;
                            if (IsCorrect(property)){
                                int prop = userData.Property;
                                if (prop < 4){
                                    _path += $"/{prop}";
                                    code = prop;
                                    response = $"{{ SessionId: \"{sessionid}\", Code: 0, Message: \"Folder {prop} opened\" }}";
                                }
                                else response = $"{{ SessionId: \"{sessionid}\", Code: 2, Message: \"Not enough rights\" }}";
                            }
                            else response = $"{{ SessionId: \"{sessionid}\" ,Code: 1, Message: \"Here no this folder\" }}";
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
                        _end = true;
                        break;

                        
                    default:
                        response = $"{{ SessionId: \"{sessionid}\", Code: -1, Message: \"Invalid operation\" }}";
                        Logger.ToLogAll("Invalid operation");
                        break;
                    
                }
                else{
                    Provider.SendResponse("{ Code: -3, Message: \"SessionID incorrect\" }", _stream);
                    Logger.ToLogAll("SessionID incorrect");
                    _end = true;
                }
                if (_end) break;
                Provider.SendResponse(response, _stream);
            }
        }
    }
}