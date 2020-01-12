using System.ComponentModel.DataAnnotations;

namespace ZIKM.Services.Storages.Model {
    public class Folder {
        [Key]
        public int Id { get; set; }
        [Required]
        public uint Permission { get; set; }
    }
}
