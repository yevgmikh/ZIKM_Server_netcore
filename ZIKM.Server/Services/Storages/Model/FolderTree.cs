using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZIKM.Server.Services.Storages.Model {
    public class FolderTree {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public int? FolderId { get; set; }
        public virtual Folder Folder { get; set; }

        public int? ParentFolderId { get; set; }
        public virtual Folder ParentFolder { get; set; }

        public int? FileId { get; set; }
        public virtual DataFile File { get; set; }

        public FolderTree() { }

        public FolderTree(string name, Folder parentFolder, Folder folder) {
            Name = name;
            ParentFolder = parentFolder;
            Folder = folder;
        }

        public FolderTree(string name, Folder parentFolder, DataFile file) {
            Name = name;
            ParentFolder = parentFolder;
            File = file;
        }
    }
}
