using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ZIKM.Infrastructure;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Infrastructure.Interfaces;

namespace ZIKM.Services.Authorization {
    /// <summary>
    /// File user authorization storage
    /// </summary>
    internal class UserFileStorage : IAuthorization {

        private readonly Dictionary<string, List<string>> passwords;

        public UserFileStorage() {
            passwords = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(new ReadOnlySpan<byte>(File.ReadAllBytes("Accounts.json")));
        }

        public ResponseData SingIn(string login, string password) {
            if (passwords.ContainsKey(login)) {
                if (passwords[login]?.Count == 0){
                    #region User's spent all passwords
                    switch (login) {
                        case "Master":
                            Logger.ToLogAll(LogMessages.MasterBlocked);
                            return new ResponseData(StatusCode.Blocked, Messages.MasterBlocked);
                        case "Senpai":
                            Logger.ToLogAll(LogMessages.SempaiBlocked);
                            return new ResponseData(StatusCode.Blocked, Messages.SempaiBlocked);
                        case "Kouhai":
                            Logger.ToLogAll(LogMessages.KouhaiBlocked);
                            return new ResponseData(StatusCode.Blocked, Messages.KouhaiBlocked);
                        default:
                            Logger.ToLogAll(LogMessages.Blocked(login));
                            return new ResponseData(StatusCode.Blocked, Messages.Blocked);
                    }
                    #endregion
                }
                else {
                    if (passwords[login][0] == password) {
                        passwords[login].RemoveAt(0);
                        return new ResponseData();
                    }
                    else {
                        passwords[login].RemoveAt(0);
                        if (passwords[login].Count == 1) {
                            // User's spent last password 
                            Logger.ToLogAll(LogMessages.Blocked(login));
                            return new ResponseData(StatusCode.Blocked, Messages.Blocked);
                        }
                        else {
                            // User's written wrong password
                            Logger.ToLogAll(LogMessages.WrongPassword(login));
                            return new ResponseData(StatusCode.BadData, Messages.TryAgain);
                        }
                    }
                }
            }
            else {
                // No user in data
                Logger.ToLogAll(LogMessages.NotFound(login));
                return new ResponseData(StatusCode.BadData, Messages.NotFound(login));
            }
        }
    }
}
