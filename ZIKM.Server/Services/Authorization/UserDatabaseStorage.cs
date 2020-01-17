using Microsoft.EntityFrameworkCore;
using System.Linq;
using ZIKM.Infrastructure;
using ZIKM.Infrastructure.DataStructures;
using ZIKM.Infrastructure.Enums;
using ZIKM.Infrastructure.Interfaces;
using ZIKM.Services.Storages.Model;

namespace ZIKM.Services.Authorization {
    /// <summary>
    /// Database user authorization storage
    /// </summary>
    internal class UserDatabaseStorage : IAuthorization {
        /// <summary>
        /// Get instance of database user authorization storage class
        /// </summary>
        public static UserDatabaseStorage Instance { get; } = new UserDatabaseStorage();

        protected StorageContext _db = new StorageContext();

        private UserDatabaseStorage() { }

        public ResponseData SingIn(string login, string password) {
            var user = _db.Users.Include(obj => obj.Passwords).FirstOrDefault(u => u.Name == login);

            if (user == null) {
                // No user in data
                Logger.ToLogAll(LogMessages.NotFound(login));
                return new ResponseData(StatusCode.BadData, Messages.NotFound(login));
            }

            var currentPassword = user.Passwords.FirstOrDefault(p => !p.IsUsed);
            if (currentPassword == null) {
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
            currentPassword.IsUsed = true;
            _db.SaveChanges();

            if (currentPassword.Password == password) {
                return new ResponseData();
            }
            else {
                if (user.Passwords.Count(p => !p.IsUsed) == 0) {
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
}
