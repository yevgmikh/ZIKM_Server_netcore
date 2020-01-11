﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Infrastructure.Interfaces;
using ZIKM.Server.Infrastructure;

namespace ZIKM.Infrastructure.Storages.Authorization
{
    public class UserAuthorizationStorage : IAuthorization
    {
        private static IAuthorization authorization;

        private readonly Dictionary<string, List<string>> passwords;

        private UserAuthorizationStorage(){
            passwords = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(new ReadOnlySpan<byte>(File.ReadAllBytes("Accounts.json")));
        }

        public static IAuthorization GetAuthorization(){
            if (authorization == null)
                authorization = new UserAuthorizationStorage();
            return authorization;
        }

        public ResponseData SingIn(string login, string password){
            if (passwords.ContainsKey(login)){
                if (passwords[login]?.Count == 0){
                    #region User's spent all passwords
                    switch (login)
                    {
                        case "Master":
                            Logger.ToLogAll("Fake master");
                            return new ResponseData(StatusCode.Blocked, Messages.MasterBlocked);
                        case "Senpai":
                            Logger.ToLogAll("Impostor");
                            return new ResponseData(StatusCode.Blocked, Messages.SempaiBlocked);
                        case "Kouhai":
                            Logger.ToLogAll("Liar");
                            return new ResponseData(StatusCode.Blocked, Messages.KouhaiBlocked);
                        default:
                            Logger.ToLogAll($"{login} blocked");
                            return new ResponseData(StatusCode.Blocked, Messages.Blocked);
                    }
                    #endregion
                }
                else{
                    if (passwords[login][0] == password){
                        passwords[login].RemoveAt(0);
                        return new ResponseData();
                    }
                    else{
                        passwords[login].RemoveAt(0);
                        if (passwords[login].Count == 1){
                            // User's spent last password 
                            Logger.ToLogAll($"{login} blocked");
                            return new ResponseData(StatusCode.Blocked, Messages.Blocked);
                        }
                        else{
                            // User's written wrong password
                            Logger.ToLogAll($"{login} errored");
                            return new ResponseData(StatusCode.BadData, Messages.TryAgain);
                        }
                    }
                }
            }
            else{
                // No user in data
                Logger.ToLogAll($"{login} not found");
                return new ResponseData(StatusCode.BadData, Messages.NotFound(login));
            }
        }
    }
}