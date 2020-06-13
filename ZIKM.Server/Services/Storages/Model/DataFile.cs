using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZIKM.Server.Services.Storages.Model {
    public class DataFile {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public uint Permission { get; set; }
        [Required]
        public byte[] Data { get; set; }
    }
}
