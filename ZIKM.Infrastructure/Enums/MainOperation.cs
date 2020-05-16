namespace ZIKM.Infrastructure.Enums{
    public enum MainOperation : int{
        Error = 0,
        GetFiles = 1,
        GetFolders,
        GetAll,
        OpenFile,
        OpenFolder,
        CloseFolder,
        EndSession
    }
}
