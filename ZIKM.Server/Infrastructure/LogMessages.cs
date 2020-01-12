using System;

namespace ZIKM.Infrastructure {
    internal static class LogMessages {
        #region Logins
        internal const string MasterLoggedIn = "Master here";
        internal const string SempaiLoggedIn = "Sempai back";
        internal const string KouhaiLoggedIn = "Pervered kouhai here";
        internal static string UserLoggedIn(string name) => $"{name} here";
        #endregion

        #region Authorization
        internal static string WrongPassword(string name) => $"{name} errored";
        internal static string WrongCaptcha(string name) => $"{name} wrote wrong captcha code";
        internal static string Blocked(string name) => $"{name} blocked";
        internal static string NotFound(string name) => $"{name} not found";

        internal const string MasterBlocked = "Fake master";
        internal const string SempaiBlocked = "Impostor";
        internal const string KouhaiBlocked = "Liar";
        #endregion

        #region Logouts
        internal const string MasterLoggedOut = "Master gone";
        internal const string SempaiLoggedOut = "Sempai gone";
        internal const string KouhaiLoggedOut = "Pervered kouhai gone";
        internal static string UserLoggedOut(string name) => $"Bye {name}";
        #endregion

        #region Main messages
        internal static string FolderOpened(string user, string name) => $"{user} open folder \"{name}\"";
        internal static string FileOpened(string user, string name) => $"{user} open file \"{name}\"";
        internal static string FileRead(string user, string name) => $"File \"{name}\" readed by {user}";
        internal static string Written(string user, string name) => $"{user} saved to file \"{name}\"";
        internal static string Updated(string user, string name) => $"File \"{name}\" edited by {user}";
        internal static string FileClosed(string user, string name) => $"{user} close file \"{name}\"";

        internal static string NoAccess(string user) => $"{user} has not enough rights";

        internal static string ReadError(string user, string name, Exception ex) => $"{user} has error while reading file \"{name}\":{ex.Message}";
        internal static string WriteError(string user, string name, Exception ex) => $"{user} has error while writing file \"{name}\":{ex.Message}";
        internal static string EditError(string user, string name, Exception ex) => $"{user} has error while editing file \"{name}\":{ex.Message}";
        #endregion
    }
}
