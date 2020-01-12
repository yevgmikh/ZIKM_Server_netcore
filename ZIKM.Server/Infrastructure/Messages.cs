using System;

namespace ZIKM.Infrastructure {
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
        internal static string FolderOpened(string name) => $"Folder \"{name}\" opened";
        internal const string FolderClosed = "Folder closed";
        internal static string FileOpened(string name) => $"File \"{name}\" opened";
        internal const string Written = "Written to file";
        internal const string Updated = "File updated";
        internal static string FileClosed(string name) => $"File {name} closed";

        internal const string RootFolder = "You in home directory";
        internal static string FolderNotFound(string name) => $"Folder \"{name}\" not found";
        internal static string FileNotFound(string name) => $"File \"{name}\" not found";

        internal const string NoAccess = "Not enough rights.";

        internal const string NoFile = "File not found";
        internal const string NotOpened = "File not opened";
        internal static string ReadError(Exception ex) => $"Error reading file:{ex.Message}";
        internal static string WriteError(Exception ex) => $"Error writing file:{ex.Message}";
        internal static string EditError(Exception ex) => $"Error editing file:{ex.Message}";
        #endregion
    }
}