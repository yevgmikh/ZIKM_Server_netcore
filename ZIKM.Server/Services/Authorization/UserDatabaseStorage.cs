using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Server.Infrastructure;
using ZIKM.Server.Infrastructure.Interfaces;
using ZIKM.Server.Services.Storages.Model;
using ZIKM.Server.Utils;

namespace ZIKM.Server.Services.Authorization {
    /// <summary>
    /// Database user authorization storage
    /// </summary>
    internal class UserDatabaseStorage : IAuthorization {

        protected StorageContext _db = IoC.GetService<StorageContext>();

        /// <summary>
        /// Compare password and hashed password
        /// </summary>
        /// <param name="password">Password</param>
        /// <param name="hashed">Hashed password</param>
        /// <returns>The result of the comparison</returns>
        private bool CheckPassword(string password, string hashed) {
            var parts = hashed.Split("&8/1");
            password = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Convert.FromBase64String(parts[2]),
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: int.Parse(parts[1]),
                numBytesRequested: 256 / 8));

            return password == parts[0];
        }

        public ResponseData SingIn(string login, string password) {
            var user = _db.Users.Include(obj => obj.Passwords).FirstOrDefault(u => u.Name == login);

            if (user == null) {
                // No user in data
                Logger.LogAll(LogMessages.NotFound(login));
                return new ResponseData(StatusCode.BadData, Messages.NotFound(login));
            }

            var currentPassword = user.Passwords.FirstOrDefault(p => !p.IsUsed);
            if (currentPassword == null) {
                #region User's spent all passwords
                switch (login) {
                    case "Master":
                        Logger.LogAll(LogMessages.MasterBlocked);
                        return new ResponseData(StatusCode.Blocked, Messages.MasterBlocked);
                    case "Senpai":
                        Logger.LogAll(LogMessages.SempaiBlocked);
                        return new ResponseData(StatusCode.Blocked, Messages.SempaiBlocked);
                    case "Kouhai":
                        Logger.LogAll(LogMessages.KouhaiBlocked);
                        return new ResponseData(StatusCode.Blocked, Messages.KouhaiBlocked);
                    default:
                        Logger.LogAll(LogMessages.Blocked(login));
                        return new ResponseData(StatusCode.Blocked, Messages.Blocked);
                }
                #endregion
            }
            currentPassword.IsUsed = true;
            _db.SaveChanges();

            if (CheckPassword(password, currentPassword.Password)) {
                return new ResponseData();
            }
            else {
                if (user.Passwords.Count(p => !p.IsUsed) == 0) {
                    // User's spent last password 
                    Logger.LogAll(LogMessages.Blocked(login));
                    return new ResponseData(StatusCode.Blocked, Messages.Blocked);
                }
                else {
                    // User's written wrong password
                    Logger.LogAll(LogMessages.WrongPassword(login));
                    return new ResponseData(StatusCode.BadData, Messages.TryAgain);
                }
            }
        }
    }
}
