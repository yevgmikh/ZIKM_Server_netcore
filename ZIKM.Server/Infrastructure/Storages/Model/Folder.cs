using System.ComponentModel.DataAnnotations;

namespace ZIKM.Infrastructure.Storages.Model {
    public class Folder {
        [Key]
        public int Id { get; set; }
        [Required]
        public uint Permission { get; set; }
    }
}
