namespace ZIKM.Server.Infrastructure
{
    internal static class Messages {
        #region Greetings
        internal const string MasterGreeting = "Welcome, Master.";
        internal const string SempaiGreeting = "Senpai!!!XD";
        internal const string KouhaiGreeting = "Sempai is waitting you)";
        internal static string UserGreeting(string name) => $"You {name}";
        #endregion

        #region Authorization
        internal const string TryAgain = "Try again";
        internal const string WrongCaptcha = "Wrong captcha code";
        internal const string Blocked = "You blocked";
        internal const string MasterBlocked = "Don't think about this";
        internal const string SempaiBlocked = "Impostor!";
        internal const string KouhaiBlocked = "Liar!!!!X|";

        internal static string NotFound(string name) => $"No {name} in data";
        #endregion
        
        #region Farewells
        internal const string MasterFarewell = "I will wait your return, Master.";
        internal const string SempaiFarewell = "Senpai! I will wait!!!";
        internal const string KouhaiFarewell = "Be carefull, my kouhai.";
        internal static string UserFarewell(string name) => $"{name} disconnect";
        #endregion

        #region Main messages

        #region Successful operations
        internal static string FolderOpened(string name) => $"Folder \"{name}\" opened";
        internal const string FolderClosed = "Folder closed";
        internal static string FolderAdded(string name) => $"Folder \"{name}\" added";
        internal static string FolderEdited(string name) => $"Folder \"{name}\" renamed";
        internal static string FolderRemoved(string name) => $"Folder \"{name}\" removed";
        internal static string FileOpened(string name) => $"File \"{name}\" opened";
        internal static string FileAdded(string name) => $"File \"{name}\" added";
        internal static string FileEdited(string name) => $"File \"{name}\" renamed";
        internal static string FileRemoved(string name) => $"File \"{name}\" removed";
        internal const string Written = "Written to file";
        internal const string Updated = "File updated";
        internal static string FileClosed(string name) => $"File {name} closed";
        #endregion

        #region Unsuccessful operations
        internal const string RootFolder = "You in home directory";
        internal static string FolderNotFound(string name) => $"Folder \"{name}\" not found";
        internal static string FileNotFound(string name) => $"File \"{name}\" not found";

        internal const string InvalidRequest = "Invalid request";
        internal const string NoAccess = "Not enough rights.";
        internal const string FileLocked = "File opened by another user.";
        #endregion

        #region Server errors
        internal const string NoFile = "File not found";
        internal const string NotOpened = "File not opened";
        internal const string ReadError = "Internal server error while reading file";
        internal const string WriteError = "Internal server error while writing file";
        internal const string EditError = "Internal server error while editing file";
        internal const string AddFileError = "Internal server error while adding file";
        internal const string EditFileError = "Internal server error while renaming file";
        internal const string RemoveFileError = "Internal server error while removing file";
        internal const string AddFolderError = "Internal server error while adding folder";
        internal const string EditFolderError = "Internal server error while renaming folder";
        internal const string RemoveFolderError = "Internal server error while removing folder";

        internal const string FileError = "Unexpected internal server error while working with file. File closed";
        internal const string SessionError = "Unexpected internal server error during session. Session ended";
        #endregion

        #endregion
    }
}