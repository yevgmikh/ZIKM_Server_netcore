namespace ZIKM.Infrastructure{
    enum MainOperation : int{
        Error = 0,
        GetFiles = 1,
        GetFolders,
        OpenFile,
        OpenFolder,
        CloseFolder,
        EndSession
    }
}
