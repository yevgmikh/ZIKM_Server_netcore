using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ZIKM.Server.Services.Storages.Model {
    public class User {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public uint AccessLevel { get; set; }

        public virtual List<UserPassword> Passwords { get; set; }
    }
}
