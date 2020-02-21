using Microsoft.EntityFrameworkCore;
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

            if (currentPassword.Password == password) {
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
