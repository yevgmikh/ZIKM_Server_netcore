using System;

namespace ZIKM.Server.Infrastructure {
    internal static class LogMessages {
        #region Logins
        internal const string MasterLoggedIn = "Master here";
        internal const string SempaiLoggedIn = "Sempai back";
        internal const string KouhaiLoggedIn = "Pervered kouhai here";
        internal static string UserLoggedIn(string name) => $"{name} here";
        #endregion

        #region Authorization
        internal const string KeyExchange = "Keys exchanged";
        internal static string WrongPassword(string name) => $"{name} errored";
        internal static string WrongCaptcha(string name) => $"{name} wrote wrong captcha code";
        internal static string Blocked(string name) => $"{name} blocked";
        internal static string NotFound(string name) => $"{name} not found";

        internal const string MasterBlocked = "Fake master";
        internal const string SempaiBlocked = "Impostor";
        internal const string KouhaiBlocked = "Liar";

        internal const string Disconnected = "Client disconnected";
        internal const string LostConnection = "Lost connection to client";
        #endregion

        #region Logouts
        internal const string MasterLoggedOut = "Master gone";
        internal const string SempaiLoggedOut = "Sempai gone";
        internal const string KouhaiLoggedOut = "Pervered kouhai gone";
        internal static string UserLoggedOut(string name) => $"Bye {name}";
        #endregion

        #region Main messages

        #region Successful operations
        internal static string FolderOpened(string user, string name) 
            => $"{user} open folder \"{name}\"";
        internal static string FolderAdded(string user, string name) 
            => $"{user} added folder \"{name}\"";
        internal static string FolderEdited(string user, string name, string newName) 
            => $"{user} renamed folder \"{name}\" to \"{newName}\"";
        internal static string FolderRemoved(string user, string name) 
            => $"{user} removed folder \"{name}\"";
        internal static string FileOpened(string user, string name) 
            => $"{user} open file \"{name}\"";
        internal static string FileAdded(string user, string name) 
            => $"{user} added file \"{name}\"";
        internal static string FileEdited(string user, string name, string newName) 
            => $"{user} renamed file \"{name}\" to \"{newName}\"";
        internal static string FileRemoved(string user, string name) 
            => $"{user} removed file \"{name}\"";
        internal static string FileRead(string user, string name) 
            => $"File \"{name}\" readed by {user}";
        internal static string Written(string user, string name) 
            => $"{user} saved to file \"{name}\"";
        internal static string Updated(string user, string name) 
            => $"File \"{name}\" edited by {user}";
        internal static string FileClosed(string user, string name) 
            => $"{user} close file \"{name}\"";
        #endregion

        internal const string InvalidRequest = "Invalid request";
        internal static string NoAccess(string user) => $"{user} has not enough rights";
        internal static string LockedFile(string user) => $"{user} trying edit opened file";

        #region Server errors
        internal static string ReadError(string user, string name, Exception ex) 
            => $"{user} has error while reading file \"{name}\":{ex.Message}";
        internal static string WriteError(string user, string name, Exception ex) 
            => $"{user} has error while writing file \"{name}\":{ex.Message}";
        internal static string EditError(string user, string name, Exception ex) 
            => $"{user} has error while editing file \"{name}\":{ex.Message}";
        internal static string AddFileError(string user, string name, Exception ex) 
            => $"{user} has error while adding file \"{name}\":{ex.Message}";
        internal static string EditFileError(string user, string name, Exception ex) 
            => $"{user} has error while renaming file \"{name}\":{ex.Message}";
        internal static string RemoveFileError(string user, string name, Exception ex) 
            => $"{user} has error while removing file \"{name}\":{ex.Message}";
        internal static string AddFolderError(string user, string name, Exception ex) 
            => $"{user} has error while adding folder \"{name}\":{ex.Message}";
        internal static string EditFolderError(string user, string name, Exception ex) 
            => $"{user} has error while renaming folder \"{name}\":{ex.Message}";
        internal static string RemoveFolderError(string user, string name, Exception ex) 
            => $"{user} has error while removing folder \"{name}\":{ex.Message}";
        internal const string FileError = "Unexpected error while working with file. File closed";
        internal const string SessionError = "Unexpected error during session. Session ended";
        #endregion

        #endregion
    }
}
